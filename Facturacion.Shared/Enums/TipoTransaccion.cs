namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipos de transacción según Hacienda v4.4
/// NUEVO en v4.4 - Obligatorio en LineaDetalle
/// Clasifica el tipo de operación de cada línea
/// </summary>
public enum TipoTransaccion
{
    /// <summary>
    /// 01 - Venta de bienes muebles e inmuebles
    /// </summary>
    VentaBienes = 1,

    /// <summary>
    /// 02 - Venta de servicios
    /// </summary>
    VentaServicios = 2,

    /// <summary>
    /// 03 - Importación de bienes
    /// </summary>
    ImportacionBienes = 3,

    /// <summary>
    /// 04 - Importación de servicios
    /// </summary>
    ImportacionServicios = 4,

    /// <summary>
    /// 05 - Exportación de bienes
    /// </summary>
    ExportacionBienes = 5,

    /// <summary>
    /// 06 - Exportación de servicios
    /// </summary>
    ExportacionServicios = 6,

    /// <summary>
    /// 07 - Compras locales de bienes
    /// </summary>
    ComprasLocalesBienes = 7,

    /// <summary>
    /// 08 - Compras locales de servicios
    /// </summary>
    ComprasLocalesServicios = 8,

    /// <summary>
    /// 09 - Devolución de mercancías
    /// </summary>
    DevolucionMercancias = 9,

    /// <summary>
    /// 10 - Descuentos y bonificaciones
    /// </summary>
    DescuentosBonificaciones = 10,

    /// <summary>
    /// 11 - Anticipos
    /// </summary>
    Anticipos = 11,

    /// <summary>
    /// 12 - Arrendamiento
    /// </summary>
    Arrendamiento = 12,

    /// <summary>
    /// 13 - Otros
    /// </summary>
    Otros = 13
}
