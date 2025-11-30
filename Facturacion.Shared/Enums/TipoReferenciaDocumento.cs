namespace Facturacion.Shared.Enums;

/// <summary>
/// Códigos de tipo de referencia para Notas de Débito y Crédito (Hacienda v4.4)
/// </summary>
public enum TipoReferenciaDocumento
{
    /// <summary>
    /// 01 - Anula Documento de Referencia
    /// </summary>
    AnulaDocumentoReferencia = 1,

    /// <summary>
    /// 02 - Corrige el texto del Documento de Referencia
    /// </summary>
    CorrigeTexto = 2,

    /// <summary>
    /// 03 - Corrige el monto del Documento de Referencia
    /// </summary>
    CorrigeMonto = 3,

    /// <summary>
    /// 04 - Referencia a otro documento
    /// </summary>
    ReferenciaOtroDocumento = 4,

    /// <summary>
    /// 05 - Sustituye comprobante provisional por contingencia
    /// </summary>
    SustituyeComprobanteProvisional = 5,

    /// <summary>
    /// 99 - Otros
    /// </summary>
    Otros = 99
}
