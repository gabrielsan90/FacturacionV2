using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Enums;

/// <summary>
/// Códigos de Mensaje Receptor según Hacienda v4.4
/// Códigos oficiales del catálogo de mensajes
/// Resolución MH-DGT-RES-0027-2024
/// </summary>
public enum CodigoMensajeReceptor
{
    /// <summary>
    /// Código 01: Aceptado - Documento aceptado completamente
    /// Usado con TipoMensajeReceptor.Aceptacion
    /// </summary>
    [Display(Name = "Aceptado")]
    Aceptado = 1,

    /// <summary>
    /// Código 02: Aceptación Parcial - Pago Parcial
    /// Usado con TipoMensajeReceptor.AceptacionParcial
    /// Requiere especificar montos aceptados
    /// </summary>
    [Display(Name = "Aceptación Parcial - Pago Parcial")]
    AceptacionParcialPagoParcial = 2,

    /// <summary>
    /// Código 03: Rechazo - Comprobante no corresponde con la transacción
    /// Usado con TipoMensajeReceptor.Rechazo
    /// </summary>
    [Display(Name = "Rechazo - Comprobante no corresponde")]
    RechazoNoCorresponde = 3,

    /// <summary>
    /// Código 04: Rechazo - Proveedor no autorizado
    /// Usado con TipoMensajeReceptor.Rechazo
    /// </summary>
    [Display(Name = "Rechazo - Proveedor no autorizado")]
    RechazoProveedorNoAutorizado = 4,

    /// <summary>
    /// Código 05: Rechazo - Comprobante rechazado por el Receptor
    /// Usado con TipoMensajeReceptor.Rechazo
    /// Razón genérica de rechazo
    /// </summary>
    [Display(Name = "Rechazo - Comprobante rechazado por el Receptor")]
    RechazoComprobante = 5
}
