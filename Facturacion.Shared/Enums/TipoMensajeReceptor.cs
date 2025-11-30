using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipo de Mensaje Receptor según Hacienda v4.4
/// Resolución MH-DGT-RES-0027-2024
/// </summary>
public enum TipoMensajeReceptor
{
    /// <summary>
    /// Aceptación total del documento recibido
    /// </summary>
    [Display(Name = "Aceptación")]
    Aceptacion = 1,

    /// <summary>
    /// Aceptación parcial del documento (ejemplo: pago parcial)
    /// Requiere montos aceptados
    /// </summary>
    [Display(Name = "Aceptación Parcial")]
    AceptacionParcial = 2,

    /// <summary>
    /// Rechazo total del documento
    /// Requiere código de rechazo y detalle
    /// </summary>
    [Display(Name = "Rechazo")]
    Rechazo = 3
}
