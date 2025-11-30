using Facturacion.Shared.Enums;

namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para documentos recibidos pendientes de enviar Mensaje Receptor
/// </summary>
public class DocumentosPendientesMR
{
    /// <summary>
    /// ID del documento
    /// </summary>
    public Guid DocumentoId { get; set; }

    /// <summary>
    /// Clave del documento (50 dígitos)
    /// </summary>
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Número consecutivo del documento
    /// </summary>
    public string NumeroConsecutivo { get; set; } = null!;

    /// <summary>
    /// Tipo de documento
    /// </summary>
    public DocumentoTipo TipoDocumento { get; set; }

    /// <summary>
    /// Nombre del tipo de documento (para display)
    /// </summary>
    public string TipoDocumentoNombre { get; set; } = null!;

    /// <summary>
    /// Nombre del emisor (proveedor)
    /// </summary>
    public string EmisorNombre { get; set; } = null!;

    /// <summary>
    /// Número de identificación del emisor
    /// </summary>
    public string? EmisorNumeroIdentificacion { get; set; }

    /// <summary>
    /// Fecha de emisión del documento
    /// </summary>
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Total de venta del documento
    /// </summary>
    public decimal TotalVenta { get; set; }

    /// <summary>
    /// Moneda del documento
    /// </summary>
    public TipoMoneda Moneda { get; set; }

    /// <summary>
    /// Días restantes para enviar el MR (límite 8 días calendario)
    /// Valor negativo indica que ya venció el plazo
    /// </summary>
    public int DiasParaVencer { get; set; }

    /// <summary>
    /// Indica si el plazo para enviar MR ya venció
    /// </summary>
    public bool PlazoVencido { get; set; }

    /// <summary>
    /// Fecha límite para enviar el MR (8 días desde emisión)
    /// </summary>
    public DateTime FechaLimite { get; set; }
}
