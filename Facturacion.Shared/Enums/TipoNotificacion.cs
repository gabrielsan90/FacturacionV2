namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipos de notificaciones del sistema
/// Incluye alertas de Hacienda, certificados, pagos, etc.
/// </summary>
public enum TipoNotificacion
{
    /// <summary>
    /// Documento aceptado por Hacienda (verde)
    /// </summary>
    DocumentoAceptado = 1,

    /// <summary>
    /// Documento rechazado por Hacienda (rojo)
    /// </summary>
    DocumentoRechazado = 2,

    /// <summary>
    /// Documento pendiente de envío a Hacienda
    /// </summary>
    DocumentoPendiente = 3,

    /// <summary>
    /// Pago recibido de cliente
    /// </summary>
    PagoRecibido = 4,

    /// <summary>
    /// Pago pendiente de recibir
    /// </summary>
    PagoPendiente = 5,

    /// <summary>
    /// Stock bajo en inventario (amarillo)
    /// </summary>
    InventarioBajo = 6,

    /// <summary>
    /// Gasto aprobado
    /// </summary>
    GastoAprobado = 7,

    /// <summary>
    /// Gasto rechazado
    /// </summary>
    GastoRechazado = 8,

    /// <summary>
    /// Gasto requiere aprobación
    /// </summary>
    GastoRequiereAprobacion = 9,

    /// <summary>
    /// Nuevo usuario registrado
    /// </summary>
    NuevoUsuario = 10,

    /// <summary>
    /// Notificación del sistema
    /// </summary>
    Sistema = 11,

    /// <summary>
    /// Advertencia general (amarillo)
    /// </summary>
    Advertencia = 12,

    /// <summary>
    /// Error del sistema (rojo)
    /// </summary>
    Error = 13,

    /// <summary>
    /// Certificado digital próximo a vencer (amarillo)
    /// Recomendación 11 de Hacienda v4.4
    /// </summary>
    CertificadoPorVencer = 14,

    /// <summary>
    /// Certificado digital vencido (rojo)
    /// </summary>
    CertificadoVencido = 15,

    /// <summary>
    /// Error en envío a Hacienda (rojo)
    /// </summary>
    ErrorEnvioHacienda = 16,

    /// <summary>
    /// Documento en contingencia
    /// </summary>
    DocumentoContingencia = 17,

    /// <summary>
    /// Consecutivo próximo a agotarse (amarillo)
    /// </summary>
    ConsecutivoPorAgotar = 18,

    /// <summary>
    /// Nueva versión disponible del sistema
    /// </summary>
    ActualizacionDisponible = 19,

    /// <summary>
    /// Recordatorio de REP pendiente (ventas a crédito)
    /// </summary>
    REPPendiente = 20
}
