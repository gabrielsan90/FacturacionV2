namespace Facturacion.Shared.Enums;

/// <summary>
/// Tipos de documentos electrónicos según Hacienda v4.4
/// Resolución MH-DGT-RES-0027-2024
/// Los valores del enum corresponden EXACTAMENTE a los códigos de Hacienda
/// </summary>
public enum DocumentoTipo
{
    /// <summary>
    /// 01 - Factura Electrónica
    /// Documento principal para ventas B2B y B2C con crédito fiscal
    /// </summary>
    FacturaElectronica = 1,

    /// <summary>
    /// 02 - Nota de Débito Electrónica
    /// Para ajustes al alza: intereses, cargos adicionales
    /// Requiere referencia a documento original
    /// </summary>
    NotaDebitoElectronica = 2,

    /// <summary>
    /// 03 - Nota de Crédito Electrónica
    /// Para anulaciones, devoluciones, descuentos posteriores
    /// Requiere referencia a documento original
    /// </summary>
    NotaCreditoElectronica = 3,

    /// <summary>
    /// 04 - Tiquete Electrónico
    /// Para ventas a consumidor final sin crédito fiscal
    /// Receptor es opcional
    /// </summary>
    TiqueteElectronico = 4,

    /// <summary>
    /// 05 - Factura Electrónica de Exportación
    /// Para ventas de bienes/servicios al exterior
    /// Requiere partida arancelaria obligatoria
    /// </summary>
    FacturaElectronicaExportacion = 5,

    /// <summary>
    /// 06 - Factura Electrónica de Compra (Autofacturación)
    /// Emisor genera factura en nombre del proveedor
    /// Para compras a proveedores no emisores electrónicos
    /// </summary>
    FacturaElectronicaCompra = 6,

    /// <summary>
    /// 07 - Nota de Crédito Electrónica de Compra
    /// Nota de crédito para documentos de compra (tipo 06)
    /// </summary>
    NotaCreditoElectronicaCompra = 7,

    /// <summary>
    /// 08 - Nota de Débito Electrónica de Compra
    /// Nota de débito para documentos de compra (tipo 06)
    /// </summary>
    NotaDebitoElectronicaCompra = 8,

    /// <summary>
    /// 09 - Comprobante Electrónico de Compra
    /// Para compras a proveedores extranjeros (importación de servicios)
    /// </summary>
    ComprobanteElectronicoCompra = 9,

    /// <summary>
    /// 10 - Recibo Electrónico de Pago (REP)
    /// NUEVO en v4.4 - Registra pagos recibidos sobre ventas a crédito
    /// Obligatorio para ventas al Estado a crédito
    /// Referencia a factura(s) original(es)
    /// </summary>
    ReciboElectronicoPago = 10
}
