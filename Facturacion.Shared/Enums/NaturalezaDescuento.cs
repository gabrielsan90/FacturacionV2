namespace Facturacion.Shared.Enums;

/// <summary>
/// Naturaleza/Tipo de descuento según Hacienda v4.4
/// 11 códigos específicos para clasificar descuentos
/// Obligatorio cuando hay descuento en el documento
/// </summary>
public enum NaturalezaDescuento
{
    /// <summary>
    /// 01 - Descuento comercial por volumen
    /// </summary>
    DescuentoVolumen = 1,

    /// <summary>
    /// 02 - Descuento estacional (temporada)
    /// </summary>
    DescuentoEstacional = 2,

    /// <summary>
    /// 03 - Descuento comercial
    /// </summary>
    DescuentoComercial = 3,

    /// <summary>
    /// 04 - Bonificación
    /// </summary>
    Bonificacion = 4,

    /// <summary>
    /// 05 - Regalía
    /// </summary>
    Regalia = 5,

    /// <summary>
    /// 06 - Descuento promocional
    /// </summary>
    DescuentoPromocional = 6,

    /// <summary>
    /// 07 - Descuento por pronto pago
    /// </summary>
    DescuentoProntoPago = 7,

    /// <summary>
    /// 08 - Descuento por cliente frecuente
    /// </summary>
    DescuentoClienteFrecuente = 8,

    /// <summary>
    /// 09 - Descuento por liquidación
    /// </summary>
    DescuentoLiquidacion = 9,

    /// <summary>
    /// 10 - Descuento por defecto o imperfección
    /// </summary>
    DescuentoDefecto = 10,

    /// <summary>
    /// 99 - Otros (requiere especificar en NaturalezaDescuentoOtro)
    /// </summary>
    Otros = 99
}
