using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Helpers;

/// <summary>
/// Helper para crear notificaciones de manera simplificada
/// Provee métodos estáticos para los tipos de notificaciones más comunes
/// </summary>
public static class NotificacionHelper
{
    /// <summary>
    /// Crea una notificación de documento aceptado por Hacienda
    /// </summary>
    public static async Task CrearNotificacionDocumentoAceptado(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string numeroConsecutivo,
        Guid documentoId)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.DocumentoAceptado,
            Titulo = "Documento Aceptado",
            Mensaje = $"El documento {numeroConsecutivo} fue aceptado por Hacienda",
            EntidadRelacionadaId = documentoId,
            TipoEntidad = "Documento",
            UrlAccion = $"/DocumentosElectronicos/Documentos?id={documentoId}",
            Importante = false
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de documento rechazado por Hacienda
    /// </summary>
    public static async Task CrearNotificacionDocumentoRechazado(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string numeroConsecutivo,
        string motivoRechazo,
        Guid documentoId)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.DocumentoRechazado,
            Titulo = "Documento Rechazado",
            Mensaje = $"El documento {numeroConsecutivo} fue rechazado por Hacienda: {motivoRechazo}",
            EntidadRelacionadaId = documentoId,
            TipoEntidad = "Documento",
            UrlAccion = $"/DocumentosElectronicos/Documentos?id={documentoId}",
            Importante = true
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de documento pendiente de envío
    /// </summary>
    public static async Task CrearNotificacionDocumentoPendiente(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string numeroConsecutivo,
        Guid documentoId)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.DocumentoPendiente,
            Titulo = "Documento Pendiente de Envío",
            Mensaje = $"El documento {numeroConsecutivo} está pendiente de envío a Hacienda",
            EntidadRelacionadaId = documentoId,
            TipoEntidad = "Documento",
            UrlAccion = $"/DocumentosElectronicos/Documentos?id={documentoId}",
            Importante = true
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de inventario bajo
    /// </summary>
    public static async Task CrearNotificacionInventarioBajo(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string nombreProducto,
        decimal cantidadActual,
        decimal stockMinimo,
        Guid inventarioId)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.InventarioBajo,
            Titulo = "Stock Bajo",
            Mensaje = $"El producto {nombreProducto} tiene stock bajo ({cantidadActual} unidades, mínimo: {stockMinimo})",
            EntidadRelacionadaId = inventarioId,
            TipoEntidad = "Inventario",
            UrlAccion = $"/Stock/Inventario?id={inventarioId}",
            Importante = true
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de error en envío a Hacienda
    /// </summary>
    public static async Task CrearNotificacionErrorEnvioHacienda(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string numeroConsecutivo,
        string mensajeError,
        Guid documentoId)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.ErrorEnvioHacienda,
            Titulo = "Error en Envío a Hacienda",
            Mensaje = $"Error al enviar {numeroConsecutivo}: {mensajeError}",
            EntidadRelacionadaId = documentoId,
            TipoEntidad = "Documento",
            UrlAccion = $"/DocumentosElectronicos/Documentos?id={documentoId}",
            Importante = true,
            FechaExpiracion = DateTime.Now.AddDays(7) // Expira en 7 días
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de certificado próximo a vencer
    /// </summary>
    public static async Task CrearNotificacionCertificadoPorVencer(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string nombreEmpresa,
        DateTime fechaVencimiento,
        int diasRestantes)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.CertificadoPorVencer,
            Titulo = "Certificado Digital Próximo a Vencer",
            Mensaje = $"El certificado digital de {nombreEmpresa} vence en {diasRestantes} días ({fechaVencimiento:dd/MM/yyyy})",
            TipoEntidad = "Empresa",
            UrlAccion = "/Configuracion/Empresas",
            Importante = true,
            FechaExpiracion = fechaVencimiento.AddDays(7) // Expira 7 días después del vencimiento
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de certificado vencido
    /// </summary>
    public static async Task CrearNotificacionCertificadoVencido(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string nombreEmpresa,
        DateTime fechaVencimiento)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.CertificadoVencido,
            Titulo = "Certificado Digital Vencido",
            Mensaje = $"El certificado digital de {nombreEmpresa} venció el {fechaVencimiento:dd/MM/yyyy}. No se pueden emitir documentos.",
            TipoEntidad = "Empresa",
            UrlAccion = "/Configuracion/Empresas",
            Importante = true
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de consecutivo próximo a agotarse
    /// </summary>
    public static async Task CrearNotificacionConsecutivoPorAgotar(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string tipoDocumento,
        int consecutivosRestantes,
        Guid consecutivoId)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.ConsecutivoPorAgotar,
            Titulo = "Consecutivo Próximo a Agotarse",
            Mensaje = $"El consecutivo de {tipoDocumento} se está agotando. Quedan {consecutivosRestantes} números disponibles.",
            EntidadRelacionadaId = consecutivoId,
            TipoEntidad = "Consecutivo",
            UrlAccion = "/Configuracion/Empresas",
            Importante = true
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de REP pendiente
    /// </summary>
    public static async Task CrearNotificacionREPPendiente(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string numeroFactura,
        DateTime fechaVencimiento,
        Guid documentoId)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.REPPendiente,
            Titulo = "REP Pendiente",
            Mensaje = $"Recuerde emitir el REP para la factura {numeroFactura} antes del {fechaVencimiento:dd/MM/yyyy}",
            EntidadRelacionadaId = documentoId,
            TipoEntidad = "Documento",
            UrlAccion = $"/Pagos/RecibosPago?facturaId={documentoId}",
            Importante = false,
            FechaExpiracion = fechaVencimiento.AddDays(7)
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea una notificación de sistema genérica
    /// </summary>
    public static async Task CrearNotificacionSistema(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        string usuarioId,
        string titulo,
        string mensaje,
        bool importante = false,
        string? urlAccion = null)
    {
        var dto = new CrearNotificacionDTO
        {
            EmpresaId = empresaId,
            UsuarioId = usuarioId,
            TipoNotificacion = TipoNotificacion.Sistema,
            Titulo = titulo,
            Mensaje = mensaje,
            UrlAccion = urlAccion,
            Importante = importante
        };

        await unitOfWork.CreateAsync(dto);
    }

    /// <summary>
    /// Crea notificaciones para múltiples usuarios
    /// </summary>
    public static async Task CrearNotificacionParaMultiplesUsuarios(
        INotificacionUnitOfWork unitOfWork,
        Guid empresaId,
        IEnumerable<string> usuarioIds,
        TipoNotificacion tipo,
        string titulo,
        string mensaje,
        string? urlAccion = null,
        bool importante = false)
    {
        foreach (var usuarioId in usuarioIds)
        {
            var dto = new CrearNotificacionDTO
            {
                EmpresaId = empresaId,
                UsuarioId = usuarioId,
                TipoNotificacion = tipo,
                Titulo = titulo,
                Mensaje = mensaje,
                UrlAccion = urlAccion,
                Importante = importante
            };

            await unitOfWork.CreateAsync(dto);
        }
    }
}
