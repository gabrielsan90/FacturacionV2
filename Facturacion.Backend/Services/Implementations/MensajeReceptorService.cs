using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio orquestador para Mensajes Receptor (MR)
/// Implementa el flujo completo según Hacienda v4.4
/// </summary>
public class MensajeReceptorService : IMensajeReceptorService
{
    private readonly DataContext _context;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IDocumentoReceptorMensajeRepository _mensajeRepository;
    private readonly IXmlGeneradorService _xmlGeneradorService;
    private readonly IFirmaDigitalService _firmaDigitalService;
    private readonly IHaciendaApiService _haciendaApiService;

    // Constantes para tipos de documento MR
    private const string TIPO_DOC_MR_ACEPTACION = "05";
    private const string TIPO_DOC_MR_PARCIAL = "06";
    private const string TIPO_DOC_MR_RECHAZO = "07";

    public MensajeReceptorService(
        DataContext context,
        IDocumentoRepository documentoRepository,
        IDocumentoReceptorMensajeRepository mensajeRepository,
        IXmlGeneradorService xmlGeneradorService,
        IFirmaDigitalService firmaDigitalService,
        IHaciendaApiService haciendaApiService)
    {
        _context = context;
        _documentoRepository = documentoRepository;
        _mensajeRepository = mensajeRepository;
        _xmlGeneradorService = xmlGeneradorService;
        _firmaDigitalService = firmaDigitalService;
        _haciendaApiService = haciendaApiService;
    }

    /// <inheritdoc/>
    public async Task<ResultadoMR> GenerarYEnviarMensajeAsync(
        Guid documentoOriginalId,
        TipoMensajeReceptor tipoMensaje,
        CodigoMensajeReceptor codigoMensaje,
        string? detalle = null,
        decimal? montoAceptado = null,
        decimal? montoImpuestoAceptado = null)
    {
        var resultado = new ResultadoMR();

        try
        {
            // 1. Validar que se puede enviar MR
            var (puedeEnviar, razon) = await ValidarMensajeReceptorAsync(documentoOriginalId);
            if (!puedeEnviar)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = razon ?? "No se puede enviar el mensaje receptor";
                resultado.Errores.Add(razon ?? "Validación fallida");
                return resultado;
            }

            // 2. Obtener el documento original con todas sus relaciones
            var documentoOriginal = await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.Proveedor)
                .Include(d => d.Sucursal)
                .Include(d => d.Terminal)
                .FirstOrDefaultAsync(d => d.Id == documentoOriginalId);

            if (documentoOriginal == null)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Documento original no encontrado";
                resultado.Errores.Add($"No se encontró el documento con ID: {documentoOriginalId}");
                return resultado;
            }

            // 3. Validar datos según el tipo de mensaje
            if (!ValidarDatosMensaje(tipoMensaje, codigoMensaje, detalle, montoAceptado, montoImpuestoAceptado, resultado))
            {
                return resultado;
            }

            // 4. Generar clave de 50 dígitos para el MR
            var tipoDocMR = ObtenerTipoDocumentoMR(tipoMensaje);
            var claveMR = await GenerarClaveMRAsync(documentoOriginal, tipoDocMR);

            // 5. Generar consecutivo para el MR
            var consecutivoMR = await GenerarConsecutivoMRAsync(documentoOriginal, tipoDocMR);

            // 6. Crear entidad DocumentoReceptorMensaje
            var mensaje = new DocumentoReceptorMensaje
            {
                Id = Guid.NewGuid(),
                DocumentoOriginalId = documentoOriginalId,
                ClaveMensaje = claveMR,
                NumeroConsecutivo = consecutivoMR,
                TipoMensaje = (int)tipoMensaje,
                FechaEmision = DateTime.Now,
                CodigoMensaje = FormatearCodigoMensaje(codigoMensaje),
                DetalleMensaje = detalle,
                MontoTotalAceptado = montoAceptado,
                MontoTotalImpuestoAceptado = montoImpuestoAceptado,
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            // 7. Generar XML del MR
            var xmlGenerado = await _xmlGeneradorService.GenerarMensajeReceptorAsync(mensaje, documentoOriginal);
            mensaje.XmlGenerado = xmlGenerado;

            // 8. Firmar digitalmente el XML
            var certificado = await _firmaDigitalService.ObtenerCertificadoAsync(documentoOriginal.EmpresaId);
            var xmlFirmado = await _firmaDigitalService.FirmarXmlAsync(xmlGenerado, certificado);
            mensaje.XmlFirmado = xmlFirmado;
            mensaje.FechaFirma = DateTime.UtcNow;

            // 9. Obtener credenciales de Hacienda de la empresa
            var empresa = await _context.Set<Empresa>()
                .FirstOrDefaultAsync(e => e.Id == documentoOriginal.EmpresaId);

            if (empresa == null)
                throw new InvalidOperationException("Empresa no encontrada");

            // 10. Enviar a Hacienda usando el método general de envío
            var ambiente = empresa.Ambiente == Ambiente.Produccion ? "prod" : "stag";
            var respuestaHacienda = await _haciendaApiService.EnviarDocumentoAsync(
                claveMR,
                xmlFirmado,
                empresa.UsuarioHacienda ?? "",
                empresa.ClaveHacienda ?? "",
                ambiente);

            // 10. Actualizar mensaje con respuesta de Hacienda
            mensaje.FechaEnvioHacienda = DateTime.UtcNow;
            var mensajesHacienda = string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Detalle ?? m.Mensaje ?? ""));
            mensaje.MensajeHacienda = mensajesHacienda;

            var exitoso = respuestaHacienda.IndEstado?.ToLower() == "aceptado";
            mensaje.Estado = exitoso ? "Aceptado" : "Rechazado";

            if (exitoso)
            {
                mensaje.FechaRespuestaHacienda = DateTime.UtcNow;
            }

            // 11. Guardar en base de datos
            await _mensajeRepository.AddAsync(mensaje);

            // 12. Preparar resultado
            resultado.Exitoso = exitoso;
            resultado.Mensaje = exitoso
                ? "Mensaje receptor enviado y aceptado por Hacienda"
                : $"Mensaje enviado pero rechazado por Hacienda: {mensajesHacienda}";
            resultado.ClaveMensaje = claveMR;
            resultado.TipoMensaje = tipoMensaje;
            resultado.CodigoMensaje = codigoMensaje;
            resultado.Estado = mensaje.Estado;
            resultado.MensajeId = mensaje.Id;
            resultado.ClaveDocumentoOriginal = documentoOriginal.Clave;
            resultado.FechaEnvio = mensaje.FechaEnvioHacienda;
            resultado.XmlGenerado = xmlGenerado;
            resultado.XmlFirmado = xmlFirmado;
            resultado.RespuestaHacienda = mensajesHacienda;

            if (!exitoso)
            {
                resultado.Errores.Add(mensajesHacienda);
            }

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"Error al generar el mensaje receptor: {ex.Message}";
            resultado.Errores.Add(ex.Message);
            return resultado;
        }
    }

    /// <inheritdoc/>
    public async Task<(bool puedeEnviar, string? razon)> ValidarMensajeReceptorAsync(Guid documentoOriginalId)
    {
        // 1. Verificar que el documento existe
        var documento = await _documentoRepository.GetAsync(documentoOriginalId);
        if (documento == null)
        {
            return (false, "Documento no encontrado");
        }

        // 2. Verificar que sea un documento recibido
        if (!documento.EsDocumentoRecibido)
        {
            return (false, "El documento no es un documento recibido. Solo se puede enviar MR para documentos recibidos de proveedores");
        }

        // 3. Verificar que no se haya enviado ya un MR
        var existeMR = await _mensajeRepository.ExisteMensajeParaDocumentoAsync(documentoOriginalId);
        if (existeMR)
        {
            return (false, "Ya se ha enviado un Mensaje Receptor para este documento");
        }

        // 4. Verificar plazo de 8 días calendario
        var diasTranscurridos = (DateTime.Now - documento.FechaEmision).Days;
        if (diasTranscurridos > 8)
        {
            return (false, $"El plazo de 8 días calendario para enviar el MR ha vencido ({diasTranscurridos} días transcurridos)");
        }

        // 5. Verificar que el documento sea de un tipo que permita MR
        var tiposPermitidos = new[] {
            DocumentoTipo.FacturaElectronica,
            DocumentoTipo.NotaDebitoElectronica,
            DocumentoTipo.NotaCreditoElectronica,
            DocumentoTipo.TiqueteElectronico,
            DocumentoTipo.FacturaElectronicaExportacion
        };

        if (!tiposPermitidos.Contains(documento.TipoDocumento))
        {
            return (false, $"El tipo de documento {documento.TipoDocumento} no permite enviar Mensaje Receptor");
        }

        return (true, null);
    }

    /// <inheritdoc/>
    public async Task<List<DocumentosPendientesMR>> ObtenerDocumentosPendientesMRAsync(Guid empresaId)
    {
        var documentos = await _documentoRepository.GetDocumentosPendientesMRAsync(empresaId);

        var resultado = new List<DocumentosPendientesMR>();

        foreach (var doc in documentos)
        {
            var fechaLimite = doc.FechaEmision.AddDays(8);
            var diasParaVencer = (fechaLimite - DateTime.Now).Days;

            resultado.Add(new DocumentosPendientesMR
            {
                DocumentoId = doc.Id,
                Clave = doc.Clave,
                NumeroConsecutivo = doc.NumeroConsecutivo,
                TipoDocumento = doc.TipoDocumento,
                TipoDocumentoNombre = ObtenerNombreTipoDocumento(doc.TipoDocumento),
                EmisorNombre = doc.Proveedor?.Nombre ?? "Proveedor desconocido",
                EmisorNumeroIdentificacion = doc.Proveedor?.NumeroIdentificacion,
                FechaEmision = doc.FechaEmision,
                TotalVenta = doc.TotalVenta,
                Moneda = doc.Moneda,
                DiasParaVencer = diasParaVencer,
                PlazoVencido = diasParaVencer < 0,
                FechaLimite = fechaLimite
            });
        }

        return resultado.OrderBy(d => d.DiasParaVencer).ToList();
    }

    /// <inheritdoc/>
    public async Task<ResultadoMR> ReenviarMensajeAsync(Guid mensajeId)
    {
        var resultado = new ResultadoMR();

        try
        {
            var mensaje = await _mensajeRepository.GetAsync(mensajeId);
            if (mensaje == null)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Mensaje receptor no encontrado";
                return resultado;
            }

            if (string.IsNullOrWhiteSpace(mensaje.XmlFirmado))
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "El mensaje no tiene XML firmado para reenviar";
                return resultado;
            }

            var documentoOriginal = await _context.Documentos
                .FirstOrDefaultAsync(d => d.Id == mensaje.DocumentoOriginalId);

            if (documentoOriginal == null)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Documento original no encontrado";
                return resultado;
            }

            // Obtener credenciales de Hacienda de la empresa
            var empresa = await _context.Set<Empresa>()
                .FirstOrDefaultAsync(e => e.Id == documentoOriginal.EmpresaId);

            if (empresa == null)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Empresa no encontrada";
                return resultado;
            }

            // Reenviar a Hacienda
            var ambiente = empresa.Ambiente == Ambiente.Produccion ? "prod" : "stag";
            var respuestaHacienda = await _haciendaApiService.EnviarDocumentoAsync(
                mensaje.ClaveMensaje,
                mensaje.XmlFirmado,
                empresa.UsuarioHacienda ?? "",
                empresa.ClaveHacienda ?? "",
                ambiente);

            // Actualizar estado
            mensaje.FechaEnvioHacienda = DateTime.UtcNow;
            var mensajesHacienda = string.Join("; ", respuestaHacienda.Mensajes.Select(m => m.Detalle ?? m.Mensaje ?? ""));
            mensaje.MensajeHacienda = mensajesHacienda;

            var exitoso = respuestaHacienda.IndEstado?.ToLower() == "aceptado";
            mensaje.Estado = exitoso ? "Aceptado" : "Rechazado";

            if (exitoso)
            {
                mensaje.FechaRespuestaHacienda = DateTime.UtcNow;
            }

            await _mensajeRepository.UpdateAsync(mensaje);

            resultado.Exitoso = exitoso;
            resultado.Mensaje = exitoso
                ? "Mensaje reenviado y aceptado por Hacienda"
                : $"Mensaje reenviado pero rechazado: {mensajesHacienda}";
            resultado.ClaveMensaje = mensaje.ClaveMensaje;
            resultado.Estado = mensaje.Estado;
            resultado.RespuestaHacienda = mensajesHacienda;

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = $"Error al reenviar el mensaje: {ex.Message}";
            resultado.Errores.Add(ex.Message);
            return resultado;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ResultadoMR>> ObtenerMensajesPorDocumentoAsync(Guid documentoOriginalId)
    {
        var mensajes = await _mensajeRepository.GetByDocumentoOriginalAsync(documentoOriginalId);

        return mensajes.Select(m => new ResultadoMR
        {
            Exitoso = m.Estado == "Aceptado",
            Mensaje = m.MensajeHacienda ?? "Sin mensaje",
            ClaveMensaje = m.ClaveMensaje,
            TipoMensaje = (TipoMensajeReceptor)m.TipoMensaje,
            CodigoMensaje = ParsearCodigoMensaje(m.CodigoMensaje),
            Estado = m.Estado,
            MensajeId = m.Id,
            ClaveDocumentoOriginal = m.DocumentoOriginal?.Clave,
            FechaEnvio = m.FechaEnvioHacienda,
            XmlGenerado = m.XmlGenerado,
            XmlFirmado = m.XmlFirmado,
            RespuestaHacienda = m.MensajeHacienda
        }).ToList();
    }

    #region Métodos Privados

    private bool ValidarDatosMensaje(
        TipoMensajeReceptor tipoMensaje,
        CodigoMensajeReceptor codigoMensaje,
        string? detalle,
        decimal? montoAceptado,
        decimal? montoImpuestoAceptado,
        ResultadoMR resultado)
    {
        // Validar aceptación parcial
        if (tipoMensaje == TipoMensajeReceptor.AceptacionParcial)
        {
            if (!montoAceptado.HasValue || !montoImpuestoAceptado.HasValue)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Para aceptación parcial debe especificar montos aceptados";
                resultado.Errores.Add("Montos requeridos para aceptación parcial");
                return false;
            }

            if (string.IsNullOrWhiteSpace(detalle))
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Para aceptación parcial debe especificar el detalle";
                resultado.Errores.Add("Detalle requerido para aceptación parcial");
                return false;
            }
        }

        // Validar rechazo
        if (tipoMensaje == TipoMensajeReceptor.Rechazo)
        {
            if (string.IsNullOrWhiteSpace(detalle))
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Para rechazo debe especificar el motivo en el detalle";
                resultado.Errores.Add("Detalle requerido para rechazo");
                return false;
            }
        }

        // Validar concordancia entre tipo de mensaje y código
        if (tipoMensaje == TipoMensajeReceptor.Aceptacion && codigoMensaje != CodigoMensajeReceptor.Aceptado)
        {
            resultado.Exitoso = false;
            resultado.Mensaje = "Para aceptación total, el código debe ser 'Aceptado'";
            resultado.Errores.Add("Código de mensaje inválido para el tipo");
            return false;
        }

        return true;
    }

    private string ObtenerTipoDocumentoMR(TipoMensajeReceptor tipoMensaje)
    {
        return tipoMensaje switch
        {
            TipoMensajeReceptor.Aceptacion => TIPO_DOC_MR_ACEPTACION,
            TipoMensajeReceptor.AceptacionParcial => TIPO_DOC_MR_PARCIAL,
            TipoMensajeReceptor.Rechazo => TIPO_DOC_MR_RECHAZO,
            _ => TIPO_DOC_MR_ACEPTACION
        };
    }

    private async Task<string> GenerarClaveMRAsync(Documento documentoOriginal, string tipoDocMR)
    {
        // Generar clave de 50 dígitos para el MR
        // Formato: CCPPPPDDDDDDDDDDSSSTTTNNNNNNNNNNNNNNNNNNNNSSSSSSSSC
        // CC = País (506)
        // PPPP = Día + Mes + Año (ddmmyy)
        // DDDDDDDDDD = Cédula del receptor del doc original (nosotros)
        // SSS = Situación (1=Normal)
        // TTT = Tipo de documento MR (05, 06, 07)
        // NNNNNNNNNNNNNNNNNNNN = Consecutivo (20 dígitos)
        // SSSSSSSS = Código de seguridad (8 dígitos)
        // C = Dígito verificador

        var fecha = DateTime.Now;
        var dia = fecha.Day.ToString("D2");
        var mes = fecha.Month.ToString("D2");
        var anio = fecha.Year.ToString().Substring(2, 2);

        var cedula = documentoOriginal.ReceptorNumeroIdentificacion?.PadLeft(12, '0').Substring(0, 12) ?? "000000000000";
        var situacion = "1"; // Normal
        var tipo = tipoDocMR.PadLeft(3, '0');

        // Generar consecutivo de 20 dígitos
        var consecutivo = Guid.NewGuid().ToString("N").Substring(0, 20);

        // Código de seguridad de 8 dígitos
        var random = new Random();
        var codigoSeguridad = random.Next(10000000, 99999999).ToString();

        // Construir clave sin dígito verificador
        var claveSinDV = $"506{dia}{mes}{anio}{cedula}{situacion}{tipo}{consecutivo}{codigoSeguridad}";

        // Calcular dígito verificador (algoritmo ATV)
        var digitoVerificador = CalcularDigitoVerificador(claveSinDV);

        return claveSinDV + digitoVerificador;
    }

    private async Task<string> GenerarConsecutivoMRAsync(Documento documentoOriginal, string tipoDocMR)
    {
        // Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
        // XXX = Código sucursal (3 dígitos)
        // YYYYY = Código terminal (5 dígitos)
        // ZZ = Tipo de documento MR (05, 06, 07)
        // AAAAAAAAAA = Consecutivo (10 dígitos)

        var sucursal = documentoOriginal.Sucursal?.Codigo ?? "001";
        var terminal = documentoOriginal.Terminal?.Codigo ?? "00001";

        // Obtener el último consecutivo para este tipo de MR
        var ultimoMensaje = await _context.Set<DocumentoReceptorMensaje>()
            .Where(m => m.NumeroConsecutivo.Contains($"-{tipoDocMR}-"))
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        int numeroConsecutivo = 1;
        if (ultimoMensaje != null)
        {
            var partes = ultimoMensaje.NumeroConsecutivo.Split('-');
            if (partes.Length == 4 && int.TryParse(partes[3], out int ultimo))
            {
                numeroConsecutivo = ultimo + 1;
            }
        }

        return $"{sucursal.PadLeft(3, '0')}-{terminal.PadLeft(5, '0')}-{tipoDocMR}-{numeroConsecutivo:D10}";
    }

    private string FormatearCodigoMensaje(CodigoMensajeReceptor codigo)
    {
        return ((int)codigo).ToString("D2");
    }

    private CodigoMensajeReceptor ParsearCodigoMensaje(string codigo)
    {
        return int.TryParse(codigo, out int value)
            ? (CodigoMensajeReceptor)value
            : CodigoMensajeReceptor.Aceptado;
    }

    private string ObtenerNombreTipoDocumento(DocumentoTipo tipo)
    {
        return tipo switch
        {
            DocumentoTipo.FacturaElectronica => "Factura Electrónica",
            DocumentoTipo.NotaDebitoElectronica => "Nota de Débito",
            DocumentoTipo.NotaCreditoElectronica => "Nota de Crédito",
            DocumentoTipo.TiqueteElectronico => "Tiquete Electrónico",
            DocumentoTipo.FacturaElectronicaExportacion => "Factura de Exportación",
            _ => tipo.ToString()
        };
    }

    private int CalcularDigitoVerificador(string clave)
    {
        // Algoritmo básico de dígito verificador (módulo 10)
        int suma = 0;
        for (int i = 0; i < clave.Length; i++)
        {
            int digito = int.Parse(clave[i].ToString());
            suma += digito * ((i % 2) + 1);
        }
        int dv = (10 - (suma % 10)) % 10;
        return dv;
    }

    #endregion
}
