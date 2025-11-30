using System.ComponentModel;

namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipo de producto/servicio en el sistema
/// </summary>
public enum TipoProducto
{
    [Description("Producto")]
    Producto = 0,

    [Description("Servicio")]
    Servicio = 1
}
