namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipos de documentos electrónicos según Hacienda v4.4
/// Resolución MH-DGT-RES-0027-2024
/// </summary>
public enum DocumentoTipo
{
    /// <summary>
    /// 01 - Factura Electrónica
    /// </summary>
    FacturaElectronica = 1,

    /// <summary>
    /// 02 - Nota de Débito Electrónica
    /// </summary>
    NotaDebitoElectronica = 2,

    /// <summary>
    /// 03 - Nota de Crédito Electrónica
    /// </summary>
    NotaCreditoElectronica = 3,

    /// <summary>
    /// 04 - Tiquete Electrónico
    /// </summary>
    TiqueteElectronico = 4,

    /// <summary>
    /// 05 - Nota de Débito Electrónica de Compra
    /// </summary>
    NotaDebitoElectronicaCompra = 5,

    /// <summary>
    /// 06 - Nota de Crédito Electrónica de Compra
    /// </summary>
    NotaCreditoElectronicaCompra = 6,

    /// <summary>
    /// 07 - Comprobante de Compra Electrónico (para compras a proveedores extranjeros)
    /// </summary>
    ComprobanteCompraElectronico = 7,

    /// <summary>
    /// 08 - Factura Electrónica de Compra
    /// </summary>
    FacturaElectronicaCompra = 8,

    /// <summary>
    /// 09 - Factura Electrónica de Exportación
    /// </summary>
    FacturaElectronicaExportacion = 9,

    /// <summary>
    /// 10 - Recibo Electrónico de Pago (REP)
    /// NUEVO en v4.4 - Registra pagos recibidos sobre ventas a crédito
    /// </summary>
    ReciboElectronicoPago = 10
}
