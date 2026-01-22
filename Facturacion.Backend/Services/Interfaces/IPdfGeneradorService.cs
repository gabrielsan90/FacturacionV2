using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Service interface for PDF generation of electronic documents
/// Generates PDFs on-demand without storage
/// </summary>
public interface IPdfGeneradorService
{
    /// <summary>
    /// Generate PDF for a document by its ID
    /// Loads the document with all required relationships
    /// </summary>
    /// <param name="documentoId">Document ID</param>
    /// <returns>PDF bytes wrapped in ActionResponse</returns>
    Task<ActionResponse<byte[]>> GenerarPdfAsync(Guid documentoId);

    /// <summary>
    /// Generate PDF for a pre-loaded document
    /// Document must have all relationships loaded
    /// </summary>
    /// <param name="documento">Document with loaded relationships</param>
    /// <returns>PDF bytes wrapped in ActionResponse</returns>
    Task<ActionResponse<byte[]>> GenerarPdfAsync(Documento documento);
}
