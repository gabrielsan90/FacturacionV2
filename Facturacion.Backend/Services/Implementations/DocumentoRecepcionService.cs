using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para gestionar la recepción de documentos electrónicos de proveedores
/// Implementa el workflow completo de recepción según Hacienda v4.4
/// </summary>
public class DocumentoRecepcionService : IDocumentoRecepcionService
{
    private readonly DataContext _context;
    private readonly IXmlParserService _xmlParserService;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IProveedorRepository _proveedorRepository;

    public DocumentoRecepcionService(
        DataContext context,
        IXmlParserService xmlParserService,
        IDocumentoRepository documentoRepository,
        IProveedorRepository proveedorRepository)
    {
        _context = context;
        _xmlParserService = xmlParserService;
        _documentoRepository = documentoRepository;
        _proveedorRepository = proveedorRepository;
    }

    /// <inheritdoc/>
    public async Task<ResultadoRecepcion> RecibirDocumentoAsync(string xmlContent, Guid empresaId)
    {
        var resultado = new ResultadoRecepcion();

        try
        {
            // 1. Validar que el XML no esté vacío
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "El contenido XML no puede estar vacío";
                resultado.Errores.Add("XML vacío o nulo");
                return resultado;
            }

            // 2. Validar estructura XML
            var esValido = await _xmlParserService.ValidarEstructuraXmlAsync(xmlContent);
            if (!esValido)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "El XML no tiene una estructura válida";
                resultado.Errores.Add("Estructura XML inválida");
                return resultado;
            }

            // 3. Verificar que sea un documento recibible (FE, ND, NC, TE, FEE)
            if (!_xmlParserService.EsDocumentoRecibible(xmlContent))
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "El documento no es de un tipo que se pueda recibir";
                resultado.Errores.Add("Tipo de documento no válido para recepción");
                return resultado;
            }

            // 4. Parsear el XML
            DocumentoRecibido docRecibido;
            try
            {
                docRecibido = await _xmlParserService.ParseXmlAsync(xmlContent);
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error al parsear el XML: {ex.Message}";
                resultado.Errores.Add(ex.Message);
                return resultado;
            }

            // 5. Verificar que no sea duplicado
            var existeDuplicado = await VerificarDuplicadoAsync(docRecibido.Clave, empresaId);
            if (existeDuplicado)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "El documento ya fue recibido anteriormente";
                resultado.Errores.Add($"Documento duplicado con clave: {docRecibido.Clave}");
                resultado.Clave = docRecibido.Clave;
                return resultado;
            }

            // 6. Obtener la empresa
            var empresa = await _context.Set<Empresa>()
                .FirstOrDefaultAsync(e => e.Id == empresaId);

            if (empresa == null)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Empresa no encontrada";
                resultado.Errores.Add($"No se encontró la empresa con ID: {empresaId}");
                return resultado;
            }

            // 7. Verificar que el receptor del documento coincida con la empresa
            if (!string.IsNullOrWhiteSpace(docRecibido.ReceptorNumeroIdentificacion) &&
                docRecibido.ReceptorNumeroIdentificacion != empresa.NumeroIdentificacion)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "El documento no está dirigido a esta empresa";
                resultado.Errores.Add($"El receptor del documento ({docRecibido.ReceptorNumeroIdentificacion}) no coincide con la empresa ({empresa.NumeroIdentificacion})");
                return resultado;
            }

            // 8. Buscar o crear el proveedor
            var proveedor = await BuscarOCrearProveedorAsync(docRecibido, empresaId);

            // 9. Obtener sucursal y terminal por defecto de la empresa
            var sucursal = await _context.Set<Sucursal>()
                .FirstOrDefaultAsync(s => s.EmpresaId == empresaId && !s.IsDeleted);

            var terminal = sucursal != null
                ? await _context.Set<Terminal>()
                    .FirstOrDefaultAsync(t => t.SucursalId == sucursal.Id && !t.IsDeleted)
                : null;

            if (sucursal == null || terminal == null)
            {
                resultado.Advertencias.Add("No se encontró sucursal o terminal por defecto. El documento se guardará sin esta información.");
            }

            // 10. Crear el documento en la base de datos
            var documento = new Documento
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                SucursalId = sucursal?.Id ?? empresaId, // Usar empresaId como fallback
                TerminalId = terminal?.Id ?? empresaId, // Usar empresaId como fallback
                ProveedorId = proveedor.Id,

                // Datos del documento
                Clave = docRecibido.Clave,
                NumeroConsecutivo = docRecibido.NumeroConsecutivo,
                TipoDocumento = docRecibido.TipoDocumento,
                FechaEmision = docRecibido.FechaEmision,
                EsDocumentoRecibido = true, // IMPORTANTE: Marcar como documento recibido
                Estado = EstadoDocumento.Pendiente, // Estado inicial - pendiente de enviar MR

                // Actividad económica
                ActividadEconomica = docRecibido.EmisorActividadEconomica ?? "000000",

                // Receptor (nosotros)
                ReceptorTipoIdentificacion = docRecibido.ReceptorTipoIdentificacion,
                ReceptorNumeroIdentificacion = docRecibido.ReceptorNumeroIdentificacion,
                ReceptorNombre = docRecibido.ReceptorNombre,
                ReceptorActividadEconomica = docRecibido.ReceptorActividadEconomica,

                // Condición de venta y medio de pago
                CondicionVenta = docRecibido.CondicionVenta,
                PlazoCreditoDias = docRecibido.PlazoCreditoDias,
                MedioPago = docRecibido.MedioPago,

                // Moneda y tipo de cambio
                Moneda = docRecibido.Moneda,
                TipoCambio = docRecibido.TipoCambio,

                // Totales
                TotalGravado = docRecibido.TotalGravado,
                TotalExento = docRecibido.TotalExento,
                TotalExonerado = docRecibido.TotalExonerado,
                Subtotal = docRecibido.Subtotal,
                TotalDescuentos = docRecibido.TotalDescuentos,
                TotalImpuestos = docRecibido.TotalImpuestos,
                TotalVenta = docRecibido.TotalVenta,

                // Observaciones
                Observaciones = docRecibido.Observaciones,

                // XML original
                XmlGenerado = xmlContent,

                // Audit
                FechaCreacion = DateTime.UtcNow,
                IsDeleted = false
            };

            // 11. Crear los detalles del documento
            foreach (var detalleRecibido in docRecibido.Detalles)
            {
                var detalle = new DocumentoDetalle
                {
                    Id = Guid.NewGuid(),
                    DocumentoId = documento.Id,
                    NumeroLinea = detalleRecibido.NumeroLinea,
                    Codigo = detalleRecibido.Codigo,
                    CodigoCabys = detalleRecibido.CodigoCabys,
                    Descripcion = detalleRecibido.Descripcion,
                    UnidadMedidaComercial = detalleRecibido.UnidadMedida ?? "Unid",
                    Cantidad = detalleRecibido.Cantidad,
                    PrecioUnitario = detalleRecibido.PrecioUnitario,
                    MontoTotal = detalleRecibido.MontoTotal,
                    MontoDescuento = detalleRecibido.MontoDescuento,
                    Subtotal = detalleRecibido.Subtotal,
                    MontoImpuesto = detalleRecibido.MontoImpuesto,
                    MontoTotalLinea = detalleRecibido.MontoTotalLinea,
                    FechaCreacion = DateTime.UtcNow
                };

                documento.Detalles.Add(detalle);
            }

            // 12. Guardar en la base de datos
            await _documentoRepository.AddAsync(documento);

            // 13. Retornar resultado exitoso
            resultado.Exitoso = true;
            resultado.Mensaje = "Documento recibido exitosamente";
            resultado.DocumentoId = documento.Id;
            resultado.Clave = documento.Clave;
            resultado.EmisorNombre = proveedor.Nombre;
            resultado.TotalVenta = documento.TotalVenta;
            resultado.TipoDocumento = documento.TipoDocumento.ToString();
            resultado.FechaEmision = documento.FechaEmision;

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"Error al recibir el documento: {ex.Message}";
            resultado.Errores.Add(ex.Message);
            return resultado;
        }
    }

    /// <inheritdoc/>
    public async Task<Documento?> BuscarDocumentoPorClaveAsync(string clave)
    {
        return await _documentoRepository.GetByClaveAsync(clave);
    }

    /// <inheritdoc/>
    public async Task<bool> VerificarDuplicadoAsync(string clave, Guid empresaId)
    {
        return await _documentoRepository.ExisteDocumentoPorClaveAsync(clave, empresaId);
    }

    /// <inheritdoc/>
    public async Task<ResultadoRecepcion> ValidarXmlAsync(string xmlContent)
    {
        var resultado = new ResultadoRecepcion();

        try
        {
            // Validar estructura
            var esValido = await _xmlParserService.ValidarEstructuraXmlAsync(xmlContent);
            if (!esValido)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Estructura XML inválida";
                resultado.Errores.Add("El XML no cumple con la estructura requerida");
                return resultado;
            }

            // Verificar que sea recibible
            if (!_xmlParserService.EsDocumentoRecibible(xmlContent))
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Tipo de documento no válido para recepción";
                resultado.Errores.Add("Solo se pueden recibir documentos tipo FE, ND, NC, TE o FEE");
                return resultado;
            }

            // Parsear para obtener información
            var docRecibido = await _xmlParserService.ParseXmlAsync(xmlContent);

            resultado.Exitoso = true;
            resultado.Mensaje = "XML válido";
            resultado.Clave = docRecibido.Clave;
            resultado.EmisorNombre = docRecibido.EmisorNombre;
            resultado.TotalVenta = docRecibido.TotalVenta;
            resultado.TipoDocumento = docRecibido.TipoDocumento.ToString();
            resultado.FechaEmision = docRecibido.FechaEmision;

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"Error al validar el XML: {ex.Message}";
            resultado.Errores.Add(ex.Message);
            return resultado;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Documento>> ObtenerDocumentosRecibidosAsync(Guid empresaId)
    {
        return await _documentoRepository.GetDocumentosRecibidosAsync(empresaId);
    }

    #region Métodos Privados

    /// <summary>
    /// Busca un proveedor por su número de identificación o lo crea si no existe
    /// </summary>
    private async Task<Proveedor> BuscarOCrearProveedorAsync(DocumentoRecibido docRecibido, Guid empresaId)
    {
        // Buscar proveedor existente
        var proveedorExistente = await _context.Set<Proveedor>()
            .FirstOrDefaultAsync(p =>
                p.NumeroIdentificacion == docRecibido.EmisorNumeroIdentificacion &&
                p.EmpresaId == empresaId &&
                !p.IsDeleted);

        if (proveedorExistente != null)
        {
            return proveedorExistente;
        }

        // Crear nuevo proveedor
        var nuevoProveedor = new Proveedor
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            TipoIdentificacion = docRecibido.EmisorTipoIdentificacion,
            NumeroIdentificacion = docRecibido.EmisorNumeroIdentificacion,
            Nombre = docRecibido.EmisorNombre,
            NombreComercial = docRecibido.EmisorNombreComercial,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Set<Proveedor>().Add(nuevoProveedor);
        await _context.SaveChangesAsync();

        return nuevoProveedor;
    }

    #endregion
}
