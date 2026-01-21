using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de lógica de negocio para documentos electrónicos
/// </summary>
public class DocumentoService : IDocumentoService
{
    private readonly DataContext _context;
    private readonly ILogger<DocumentoService> _logger;
    private readonly IConsecutivoService _consecutivoService;
    private readonly IClaveGeneradorService _claveGenerador;

    public DocumentoService(
        DataContext context,
        ILogger<DocumentoService> logger,
        IConsecutivoService consecutivoService,
        IClaveGeneradorService claveGenerador)
    {
        _context = context;
        _logger = logger;
        _consecutivoService = consecutivoService;
        _claveGenerador = claveGenerador;
    }

    /// <summary>
    /// Genera el número consecutivo para un documento
    /// DEPRECADO: Usar ConsecutivoService.GenerarYAsignarConsecutivoAsync con ambiente
    /// Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
    /// XXX = Código de sucursal (3 dígitos)
    /// YYYYY = Código de terminal (5 dígitos)
    /// ZZ = Tipo de documento (2 dígitos)
    /// AAAAAAAAAA = Consecutivo (10 dígitos)
    /// </summary>
    [Obsolete("Usar ConsecutivoService.GenerarYAsignarConsecutivoAsync con parámetro ambiente en su lugar")]
    public async Task<string> GenerarNumeroConsecutivoAsync(Guid terminalId, string tipoDocumento)
    {
        // Obtener terminal para obtener la empresa y su ambiente configurado
        var terminal = await _context.Set<Terminal>()
            .Include(t => t.Sucursal)
                .ThenInclude(s => s!.Empresa)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == terminalId);

        if (terminal?.Sucursal?.Empresa == null)
        {
            throw new InvalidOperationException($"No se pudo determinar la empresa del terminal {terminalId}");
        }

        var ambiente = terminal.Sucursal.Empresa.Ambiente;

        // Convertir string a enum
        DocumentoTipo tipoDocumentoEnum;
        switch (tipoDocumento)
        {
            case "01":
                tipoDocumentoEnum = DocumentoTipo.FacturaElectronica;
                break;
            case "02":
                tipoDocumentoEnum = DocumentoTipo.NotaDebitoElectronica;
                break;
            case "03":
                tipoDocumentoEnum = DocumentoTipo.NotaCreditoElectronica;
                break;
            case "04":
                tipoDocumentoEnum = DocumentoTipo.TiqueteElectronico;
                break;
            case "08":
                tipoDocumentoEnum = DocumentoTipo.FacturaElectronicaCompra;
                break;
            case "09":
                tipoDocumentoEnum = DocumentoTipo.FacturaElectronicaExportacion;
                break;
            default:
                throw new InvalidOperationException($"Tipo de documento no soportado: {tipoDocumento}");
        }

        // Delegar al nuevo servicio con ambiente
        return await _consecutivoService.GenerarYAsignarConsecutivoAsync(terminalId, tipoDocumentoEnum, ambiente);
    }

    /// <summary>
    /// Obtiene el siguiente número consecutivo sin incrementarlo
    /// DEPRECADO: Usar ConsecutivoService.ObtenerSiguienteConsecutivoAsync con ambiente
    /// </summary>
    [Obsolete("Usar ConsecutivoService.ObtenerSiguienteConsecutivoAsync con parámetro ambiente en su lugar")]
    public async Task<string> ObtenerSiguienteConsecutivoAsync(Guid terminalId, string tipoDocumento)
    {
        // Obtener terminal para obtener la empresa y su ambiente configurado
        var terminal = await _context.Set<Terminal>()
            .Include(t => t.Sucursal)
                .ThenInclude(s => s!.Empresa)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == terminalId);

        if (terminal?.Sucursal?.Empresa == null)
        {
            throw new InvalidOperationException($"No se pudo determinar la empresa del terminal {terminalId}");
        }

        var ambiente = terminal.Sucursal.Empresa.Ambiente;

        // Convertir string a enum
        DocumentoTipo tipoDocumentoEnum;
        switch (tipoDocumento)
        {
            case "01":
                tipoDocumentoEnum = DocumentoTipo.FacturaElectronica;
                break;
            case "02":
                tipoDocumentoEnum = DocumentoTipo.NotaDebitoElectronica;
                break;
            case "03":
                tipoDocumentoEnum = DocumentoTipo.NotaCreditoElectronica;
                break;
            case "04":
                tipoDocumentoEnum = DocumentoTipo.TiqueteElectronico;
                break;
            case "08":
                tipoDocumentoEnum = DocumentoTipo.FacturaElectronicaCompra;
                break;
            case "09":
                tipoDocumentoEnum = DocumentoTipo.FacturaElectronicaExportacion;
                break;
            default:
                throw new InvalidOperationException($"Tipo de documento no soportado: {tipoDocumento}");
        }

        // Delegar al nuevo servicio con ambiente
        return await _consecutivoService.ObtenerSiguienteConsecutivoAsync(terminalId, tipoDocumentoEnum, ambiente);
    }

    /// <summary>
    /// Calcula todos los totales del documento basándose en sus detalles
    /// </summary>
    public void CalcularTotales(Documento documento)
    {
        if (documento.Detalles == null || !documento.Detalles.Any())
        {
            // Inicializar todos los totales en cero
            documento.TotalServiciosGravados = 0;
            documento.TotalServiciosExentos = 0;
            documento.TotalServiciosExonerados = 0;
            documento.TotalMercanciasGravadas = 0;
            documento.TotalMercanciasExentas = 0;
            documento.TotalMercanciasExoneradas = 0;
            documento.TotalGravado = 0;
            documento.TotalExento = 0;
            documento.TotalExonerado = 0;
            documento.Subtotal = 0;
            documento.TotalDescuentos = 0;
            documento.TotalImpuestos = 0;
            documento.TotalOtrosCargos = 0;
            documento.TotalVenta = 0;
            return;
        }

        // Calcular totales de cada línea de detalle
        foreach (var detalle in documento.Detalles)
        {
            CalcularTotalesDetalle(detalle);
        }

        // Inicializar totales
        decimal totalServiciosGravados = 0;
        decimal totalServiciosExentos = 0;
        decimal totalServiciosExonerados = 0;
        decimal totalMercanciasGravadas = 0;
        decimal totalMercanciasExentas = 0;
        decimal totalMercanciasExoneradas = 0;

        // Sumar por categoría (asumiendo que si tiene ProductoId, es mercancía; si no, es servicio)
        foreach (var detalle in documento.Detalles)
        {
            var esMercancia = detalle.ProductoId.HasValue;
            var tieneImpuestos = detalle.Impuestos?.Any() ?? false;

            if (tieneImpuestos)
            {
                if (esMercancia)
                    totalMercanciasGravadas += detalle.Subtotal;
                else
                    totalServiciosGravados += detalle.Subtotal;
            }
            else
            {
                // TODO: Distinguir entre exento y exonerado según la configuración del impuesto
                if (esMercancia)
                    totalMercanciasExentas += detalle.Subtotal;
                else
                    totalServiciosExentos += detalle.Subtotal;
            }
        }

        documento.TotalServiciosGravados = Math.Round(totalServiciosGravados, 5);
        documento.TotalServiciosExentos = Math.Round(totalServiciosExentos, 5);
        documento.TotalServiciosExonerados = Math.Round(totalServiciosExonerados, 5);
        documento.TotalMercanciasGravadas = Math.Round(totalMercanciasGravadas, 5);
        documento.TotalMercanciasExentas = Math.Round(totalMercanciasExentas, 5);
        documento.TotalMercanciasExoneradas = Math.Round(totalMercanciasExoneradas, 5);

        documento.TotalGravado = Math.Round(totalServiciosGravados + totalMercanciasGravadas, 5);
        documento.TotalExento = Math.Round(totalServiciosExentos + totalMercanciasExentas, 5);
        documento.TotalExonerado = Math.Round(totalServiciosExonerados + totalMercanciasExoneradas, 5);

        // Subtotal antes de descuentos e impuestos
        documento.Subtotal = Math.Round(documento.Detalles.Sum(d => d.MontoTotal), 5);

        // Descuentos a nivel de línea
        var descuentosLineas = documento.Detalles.Sum(d => d.MontoDescuento);

        // Descuentos a nivel de documento
        var descuentosDocumento = documento.Descuentos?.Sum(d => d.MontoDescuento) ?? 0;

        documento.TotalDescuentos = Math.Round(descuentosLineas + descuentosDocumento, 5);

        // Total de impuestos
        documento.TotalImpuestos = Math.Round(documento.Detalles.Sum(d => d.MontoImpuesto), 5);

        // Total de otros cargos (si los hay)
        // documento.TotalOtrosCargos se mantiene como está

        // Total de venta = Subtotal - Descuentos + Impuestos + Otros Cargos
        documento.TotalVenta = Math.Round(
            documento.Subtotal - documento.TotalDescuentos + documento.TotalImpuestos + documento.TotalOtrosCargos,
            5);
    }

    /// <summary>
    /// Calcula los totales de una línea de detalle
    /// </summary>
    private void CalcularTotalesDetalle(DocumentoDetalle detalle)
    {
        // Monto total = Cantidad * Precio Unitario
        detalle.MontoTotal = Math.Round(detalle.Cantidad * detalle.PrecioUnitario, 5);

        // Calcular descuentos
        detalle.MontoDescuento = Math.Round(detalle.Descuentos?.Sum(d => d.MontoDescuento) ?? 0, 5);

        // Subtotal = Monto Total - Descuentos
        detalle.Subtotal = Math.Round(detalle.MontoTotal - detalle.MontoDescuento, 5);

        // Base imponible (normalmente igual al subtotal)
        detalle.BaseImponible = detalle.Subtotal;

        // Calcular impuestos
        if (detalle.Impuestos != null && detalle.Impuestos.Any())
        {
            foreach (var impuesto in detalle.Impuestos)
            {
                // Calcular monto del impuesto basado en la tarifa
                impuesto.MontoImpuesto = Math.Round(detalle.Subtotal * (impuesto.Tarifa / 100m), 5);
            }

            detalle.MontoImpuesto = Math.Round(detalle.Impuestos.Sum(i => i.MontoImpuesto), 5);
        }
        else
        {
            detalle.MontoImpuesto = 0;
        }

        detalle.ImpuestoNeto = detalle.MontoImpuesto;

        // Monto total de la línea = Subtotal + Impuestos
        detalle.MontoTotalLinea = Math.Round(detalle.Subtotal + detalle.MontoImpuesto, 5);
    }

    /// <summary>
    /// Valida que un documento cumpla con todas las reglas de negocio v4.4
    /// </summary>
    public async Task<List<string>> ValidarDocumentoAsync(Documento documento)
    {
        var errores = new List<string>();

        // Validar que tenga detalles
        if (documento.Detalles == null || !documento.Detalles.Any())
        {
            errores.Add("El documento debe tener al menos una línea de detalle");
        }

        // Validar actividad económica del emisor
        if (string.IsNullOrWhiteSpace(documento.ActividadEconomica))
        {
            errores.Add("La actividad económica del emisor es obligatoria");
        }
        else if (documento.ActividadEconomica.Length != 6)
        {
            errores.Add("La actividad económica debe tener 6 dígitos (CIIU4)");
        }

        // Validar receptor según tipo de documento
        if (documento.TipoDocumento != DocumentoTipo.TiqueteElectronico)
        {
            // Para facturas, NC, ND se requiere receptor
            if (string.IsNullOrWhiteSpace(documento.ReceptorNombre))
            {
                errores.Add("El nombre del receptor es obligatorio para este tipo de documento");
            }

            if (string.IsNullOrWhiteSpace(documento.ReceptorNumeroIdentificacion))
            {
                errores.Add("La identificación del receptor es obligatoria para este tipo de documento");
            }

            // NUEVO v4.4: Actividad económica del receptor obligatoria en Facturas Electrónicas
            if (documento.TipoDocumento == DocumentoTipo.FacturaElectronica)
            {
                if (string.IsNullOrWhiteSpace(documento.ReceptorActividadEconomica))
                {
                    errores.Add("La actividad económica del receptor es obligatoria para Facturas Electrónicas (v4.4)");
                }
                else if (documento.ReceptorActividadEconomica.Length != 6)
                {
                    errores.Add("La actividad económica del receptor debe tener 6 dígitos (CIIU4)");
                }
            }
        }

        // Validar que tenga ClienteId O ProveedorId (no ambos, no ninguno)
        if (!documento.ClienteId.HasValue && !documento.ProveedorId.HasValue &&
            documento.TipoDocumento != DocumentoTipo.TiqueteElectronico)
        {
            errores.Add("El documento debe tener un Cliente o un Proveedor asociado");
        }

        if (documento.ClienteId.HasValue && documento.ProveedorId.HasValue)
        {
            errores.Add("El documento no puede tener Cliente y Proveedor al mismo tiempo");
        }

        // Validar referencias para NC y ND
        if (documento.TipoDocumento == DocumentoTipo.NotaCreditoElectronica ||
            documento.TipoDocumento == DocumentoTipo.NotaDebitoElectronica)
        {
            if (documento.Referencias == null || !documento.Referencias.Any())
            {
                errores.Add("Las Notas de Crédito y Débito requieren al menos una referencia al documento original");
            }
        }

        // Validar plazo de crédito si condición de venta es crédito
        if (documento.CondicionVenta == "02" && !documento.PlazoCreditoDias.HasValue)
        {
            errores.Add("El plazo de crédito es obligatorio cuando la condición de venta es 'Crédito'");
        }

        // Validar tipo de cambio si la moneda no es CRC
        if (documento.Moneda != TipoMoneda.CRC && !documento.TipoCambio.HasValue)
        {
            errores.Add("El tipo de cambio es obligatorio cuando la moneda no es Colones (CRC)");
        }

        // Validar totales
        if (documento.TotalVenta <= 0)
        {
            errores.Add("El total del documento debe ser mayor a cero");
        }

        // Validar detalles
        if (documento.Detalles != null)
        {
            var detallesList = documento.Detalles.ToList();
            for (int i = 0; i < detallesList.Count; i++)
            {
                var detalle = detallesList[i];

                if (string.IsNullOrWhiteSpace(detalle.Descripcion))
                {
                    errores.Add($"La línea {i + 1} debe tener una descripción");
                }

                if (detalle.Cantidad <= 0)
                {
                    errores.Add($"La línea {i + 1} debe tener una cantidad mayor a cero");
                }

                if (detalle.PrecioUnitario < 0)
                {
                    errores.Add($"La línea {i + 1} tiene un precio unitario negativo");
                }

                // Validar CAByS (obligatorio desde 01/06/2025)
                if (string.IsNullOrWhiteSpace(detalle.CodigoCabys))
                {
                    errores.Add($"La línea {i + 1} debe tener código CAByS (obligatorio desde 01/06/2025)");
                }
                else if (detalle.CodigoCabys.Length != 13)
                {
                    errores.Add($"La línea {i + 1} tiene un código CAByS inválido (debe tener 13 dígitos)");
                }

                // Validar campos específicos de productos farmacéuticos
                if (!string.IsNullOrWhiteSpace(detalle.NumeroRegistroMedicamento))
                {
                    if (string.IsNullOrWhiteSpace(detalle.FormaFarmaceutica))
                    {
                        errores.Add($"La línea {i + 1} es un medicamento y requiere la forma farmacéutica");
                    }
                }
            }
        }

        // Validar información de exportación si es FEE
        if (documento.TipoDocumento == DocumentoTipo.FacturaElectronicaExportacion)
        {
            if (documento.Exportacion == null)
            {
                errores.Add("Las Facturas de Exportación requieren información de exportación");
            }
        }

        return errores;
    }

    /// <summary>
    /// Convierte un DTO de creación en una entidad Documento
    /// </summary>
    public async Task<Documento> CrearDocumentoDesdeDTO(CreateDocumentoDTO dto, string userId)
    {
        // Obtener la empresa para determinar el ambiente configurado
        var empresa = await _context.Set<Empresa>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == dto.EmpresaId && !e.IsDeleted);

        if (empresa == null)
        {
            throw new InvalidOperationException($"Empresa con ID {dto.EmpresaId} no encontrada");
        }

        var documento = new Documento
        {
            Id = Guid.NewGuid(),
            EmpresaId = dto.EmpresaId,
            SucursalId = dto.SucursalId,
            TerminalId = dto.TerminalId,
            TipoDocumento = dto.TipoDocumento,
            Estado = EstadoDocumento.Borrador,
            Ambiente = empresa.Ambiente, // Usar el ambiente configurado en la empresa
            ActividadEconomica = dto.ActividadEconomica,
            FechaEmision = dto.FechaEmision ?? FechaCostaRicaHelper.ObtenerAhoraCostaRica(),
            ClienteId = dto.ClienteId,
            ProveedorId = dto.ProveedorId,
            ReceptorTipoIdentificacion = dto.ReceptorTipoIdentificacion,
            ReceptorNumeroIdentificacion = dto.ReceptorNumeroIdentificacion,
            ReceptorNombre = dto.ReceptorNombre,
            ReceptorNombreComercial = dto.ReceptorNombreComercial,
            ReceptorActividadEconomica = dto.ReceptorActividadEconomica,
            ReceptorProvincia = dto.ReceptorProvincia,
            ReceptorCanton = dto.ReceptorCanton,
            ReceptorDistrito = dto.ReceptorDistrito,
            ReceptorBarrio = dto.ReceptorBarrio,
            ReceptorOtrasSenas = dto.ReceptorOtrasSenas,
            ReceptorEmails = dto.ReceptorEmails,
            ReceptorTelefono = dto.ReceptorTelefono,
            CondicionVenta = dto.CondicionVenta,
            PlazoCreditoDias = dto.PlazoCreditoDias,
            MedioPago = dto.MedioPago,
            Moneda = dto.Moneda,
            TipoCambio = dto.TipoCambio,
            Observaciones = dto.Observaciones,
            EsContingencia = dto.EsContingencia,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreacionId = userId
        };

        // Generar número consecutivo usando el servicio dedicado con el ambiente de la empresa
        documento.NumeroConsecutivo = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
            dto.TerminalId,
            dto.TipoDocumento,
            empresa.Ambiente);

        // Generar clave numérica de 50 dígitos
        // Situación: 1=Normal, 2=Contingencia, 3=Sin Internet
        int situacion = dto.EsContingencia ? 2 : 1;
        documento.Clave = await _claveGenerador.GenerarClaveAsync(documento, situacion);

        // Agregar detalles
        int numeroLinea = 1;
        foreach (var detalleDTO in dto.Detalles)
        {
            var detalle = new DocumentoDetalle
            {
                Id = Guid.NewGuid(),
                DocumentoId = documento.Id,
                NumeroLinea = numeroLinea++,
                ProductoId = detalleDTO.ProductoId,
                TipoCodigo = detalleDTO.TipoCodigo,
                Codigo = detalleDTO.Codigo,
                CodigoCabys = detalleDTO.CodigoCabys,
                Descripcion = detalleDTO.Descripcion,
                Cantidad = detalleDTO.Cantidad,
                UnidadMedidaId = detalleDTO.UnidadMedidaId,
                UnidadMedidaComercial = detalleDTO.UnidadMedidaComercial,
                PrecioUnitario = detalleDTO.PrecioUnitario,
                NumeroPartidaArancelaria = detalleDTO.NumeroPartidaArancelaria,
                NumeroRegistroMedicamento = detalleDTO.NumeroRegistroMedicamento,
                FormaFarmaceutica = detalleDTO.FormaFarmaceutica,
                NumeroVIN = detalleDTO.NumeroVIN,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = userId
            };

            // Agregar descuentos de la línea
            if (detalleDTO.Descuentos != null)
            {
                foreach (var descDTO in detalleDTO.Descuentos)
                {
                    detalle.Descuentos.Add(new DocumentoDetalleDescuento
                    {
                        Id = Guid.NewGuid(),
                        DocumentoDetalleId = detalle.Id,
                        NaturalezaDescuento = descDTO.Naturaleza,
                        MontoDescuento = descDTO.Monto,
                        FechaCreacion = DateTime.UtcNow,
                        UsuarioCreacionId = userId
                    });
                }
            }

            // Agregar impuestos de la línea
            if (detalleDTO.Impuestos != null)
            {
                foreach (var impDTO in detalleDTO.Impuestos)
                {
                    // TODO: ImpuestoId y CodigoImpuesto deben obtenerse del catálogo usando CodigoTarifa
                    detalle.Impuestos.Add(new DocumentoDetalleImpuesto
                    {
                        Id = Guid.NewGuid(),
                        DocumentoDetalleId = detalle.Id,
                        ImpuestoId = 1, // TODO: Obtener del catálogo usando CodigoTarifa
                        CodigoImpuesto = "01", // TODO: Obtener del catálogo usando CodigoTarifa
                        CodigoTarifa = impDTO.CodigoTarifa,
                        Tarifa = impDTO.Tarifa,
                        MontoBase = 0, // Se calculará luego
                        MontoImpuesto = 0, // Se calculará luego
                        FactorIVADevuelto = impDTO.FactorIVA,
                        FechaCreacion = DateTime.UtcNow,
                        UsuarioCreacionId = userId
                    });
                }
            }

            documento.Detalles.Add(detalle);
        }

        // Agregar descuentos a nivel de documento
        if (dto.Descuentos != null)
        {
            foreach (var descDTO in dto.Descuentos)
            {
                documento.Descuentos.Add(new DocumentoDescuento
                {
                    Id = Guid.NewGuid(),
                    DocumentoId = documento.Id,
                    NaturalezaDescuento = descDTO.Naturaleza,
                    MontoDescuento = descDTO.Monto,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId
                });
            }
        }

        // Agregar referencias
        if (dto.Referencias != null)
        {
            foreach (var refDTO in dto.Referencias)
            {
                documento.Referencias.Add(new DocumentoReferencia
                {
                    Id = Guid.NewGuid(),
                    DocumentoId = documento.Id,
                    TipoDocumentoReferenciado = ((int)refDTO.TipoReferencia).ToString("D2"),
                    NumeroDocumentoReferenciado = refDTO.NumeroDocumento,
                    FechaEmisionDocumentoReferenciado = refDTO.FechaDocumento ?? DateTime.UtcNow,
                    CodigoReferencia = string.IsNullOrWhiteSpace(refDTO.CodigoReferencia)
                        ? TipoReferenciaDocumento.AnulaDocumentoReferencia
                        : (TipoReferenciaDocumento)int.Parse(refDTO.CodigoReferencia),
                    RazonReferencia = refDTO.Razon,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId
                });
            }
        }

        // Agregar medios de pago
        if (dto.MediosPago != null)
        {
            foreach (var mpDTO in dto.MediosPago)
            {
                documento.MediosPago.Add(new DocumentoMedioPago
                {
                    Id = Guid.NewGuid(),
                    DocumentoId = documento.Id,
                    CodigoMedioPago = mpDTO.CodigoMedioPago,
                    Monto = mpDTO.Monto,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId
                });
            }
        }

        // Agregar otra información
        if (dto.OtraInformacion != null)
        {
            foreach (var oiDTO in dto.OtraInformacion)
            {
                documento.OtraInformacion.Add(new DocumentoOtraInformacion
                {
                    Id = Guid.NewGuid(),
                    DocumentoId = documento.Id,
                    Clave = oiDTO.Clave,
                    Valor = oiDTO.Valor,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId
                });
            }
        }

        // Agregar información de exportación
        if (dto.Exportacion != null && dto.TipoDocumento == DocumentoTipo.FacturaElectronicaExportacion)
        {
            documento.Exportacion = new DocumentoExportacion
            {
                Id = Guid.NewGuid(),
                DocumentoId = documento.Id,
                NombreComprador = dto.Exportacion.NombreComprador,
                Direccion = dto.Exportacion.DireccionComprador,
                Pais = dto.Exportacion.CodigoPaisDestino,
                Incoterm = dto.Exportacion.IncotermVenta,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = userId
            };
        }

        // Calcular totales
        CalcularTotales(documento);

        return documento;
    }

    /// <summary>
    /// Actualiza un documento existente con datos del DTO
    /// </summary>
    public async Task ActualizarDocumentoDesdeDTO(Documento documento, UpdateDocumentoDTO dto, string userId)
    {
        // Actualizar campos básicos
        if (dto.SucursalId.HasValue)
            documento.SucursalId = dto.SucursalId.Value;

        if (dto.TerminalId.HasValue)
            documento.TerminalId = dto.TerminalId.Value;

        if (!string.IsNullOrWhiteSpace(dto.ActividadEconomica))
            documento.ActividadEconomica = dto.ActividadEconomica;

        if (dto.FechaEmision.HasValue)
            documento.FechaEmision = dto.FechaEmision.Value;

        documento.ClienteId = dto.ClienteId;
        documento.ProveedorId = dto.ProveedorId;
        documento.ReceptorTipoIdentificacion = dto.ReceptorTipoIdentificacion;
        documento.ReceptorNumeroIdentificacion = dto.ReceptorNumeroIdentificacion;
        documento.ReceptorNombre = dto.ReceptorNombre;
        documento.ReceptorNombreComercial = dto.ReceptorNombreComercial;
        documento.ReceptorActividadEconomica = dto.ReceptorActividadEconomica;
        documento.ReceptorProvincia = dto.ReceptorProvincia;
        documento.ReceptorCanton = dto.ReceptorCanton;
        documento.ReceptorDistrito = dto.ReceptorDistrito;
        documento.ReceptorBarrio = dto.ReceptorBarrio;
        documento.ReceptorOtrasSenas = dto.ReceptorOtrasSenas;
        documento.ReceptorEmails = dto.ReceptorEmails;
        documento.ReceptorTelefono = dto.ReceptorTelefono;

        if (!string.IsNullOrWhiteSpace(dto.CondicionVenta))
            documento.CondicionVenta = dto.CondicionVenta;

        documento.PlazoCreditoDias = dto.PlazoCreditoDias;

        if (!string.IsNullOrWhiteSpace(dto.MedioPago))
            documento.MedioPago = dto.MedioPago;

        if (dto.Moneda.HasValue)
            documento.Moneda = dto.Moneda.Value;

        documento.TipoCambio = dto.TipoCambio;
        documento.Observaciones = dto.Observaciones;

        documento.FechaModificacion = DateTime.UtcNow;
        documento.UsuarioModificacionId = userId;

        // TODO: Actualizar detalles, descuentos, referencias, etc.
        // Esto requiere lógica adicional para comparar y actualizar colecciones

        // Recalcular totales
        CalcularTotales(documento);
    }

    /// <summary>
    /// Incrementa el contador de consecutivos del terminal para un tipo de documento específico
    /// </summary>
    public async Task<bool> IncrementarConsecutivoTerminalAsync(Guid terminalId, string tipoDocumento = "01")
    {
        var consecutivoEntity = await _context.Consecutivos
            .FirstOrDefaultAsync(c => c.TerminalId == terminalId &&
                                     c.TipoDocumento == tipoDocumento &&
                                     c.Activo &&
                                     !c.IsDeleted);

        if (consecutivoEntity == null)
            return false;

        if (consecutivoEntity.NumeroActual >= consecutivoEntity.NumeroFin)
            return false;

        consecutivoEntity.NumeroActual++;
        consecutivoEntity.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Verifica si un terminal ha alcanzado su límite de consecutivos para un tipo de documento
    /// </summary>
    public async Task<bool> TerminalAlcanzoLimiteAsync(Guid terminalId, string tipoDocumento = "01")
    {
        var consecutivoEntity = await _context.Consecutivos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TerminalId == terminalId &&
                                     c.TipoDocumento == tipoDocumento &&
                                     c.Activo &&
                                     !c.IsDeleted);

        if (consecutivoEntity == null)
            return true; // No existe, consideramos que alcanzó el límite

        return consecutivoEntity.NumeroActual >= consecutivoEntity.NumeroFin;
    }

    /// <summary>
    /// Obtiene el código de tipo de documento en formato de 2 dígitos
    /// DEPRECADO: Usar ConsecutivoService.ObtenerCodigoTipoDocumento
    /// </summary>
    [Obsolete("Usar ConsecutivoService.ObtenerCodigoTipoDocumento en su lugar")]
    public string ObtenerCodigoTipoDocumento(DocumentoTipo tipoDocumento)
    {
        return _consecutivoService.ObtenerCodigoTipoDocumento(tipoDocumento);
    }
}
