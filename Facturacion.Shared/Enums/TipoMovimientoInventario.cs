using System.ComponentModel;

namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipos de movimiento de inventario
/// Los movimientos de tipo Compra, AjusteEntrada, TrasladoEntrada y DevolucionCliente aumentan el inventario
/// Los movimientos de tipo Venta, AjusteSalida, TrasladoSalida, DevolucionProveedor y Merma disminuyen el inventario
/// </summary>
public enum TipoMovimientoInventario
{
    [Description("Compra de Proveedor")]
    Compra = 1,

    [Description("Venta a Cliente")]
    Venta = 2,

    [Description("Ajuste de Entrada")]
    AjusteEntrada = 3,

    [Description("Ajuste de Salida")]
    AjusteSalida = 4,

    [Description("Traslado Entrada")]
    TrasladoEntrada = 5,

    [Description("Traslado Salida")]
    TrasladoSalida = 6,

    [Description("Devolución de Cliente")]
    DevolucionCliente = 7,

    [Description("Devolución a Proveedor")]
    DevolucionProveedor = 8,

    [Description("Merma o Pérdida")]
    Merma = 9
}
