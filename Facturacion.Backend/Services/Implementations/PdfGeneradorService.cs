using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Implementations.PdfDocuments;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Service for generating PDF documents for electronic invoices
/// Generates PDFs on-demand without storage (in-memory generation)
/// </summary>
public class PdfGeneradorService : IPdfGeneradorService
{
    private readonly DataContext _context;
    private readonly ILogger<PdfGeneradorService> _logger;

    public PdfGeneradorService(DataContext context, ILogger<PdfGeneradorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Generate PDF for a document by its ID
    /// </summary>
    public async Task<ActionResponse<byte[]>> GenerarPdfAsync(Guid documentoId)
    {
        try
        {
            _logger.LogInformation("Generating PDF for document {DocumentoId}", documentoId);

            // Load document with all required relationships
            var documento = await CargarDocumentoCompletoAsync(documentoId);

            if (documento == null)
            {
                _logger.LogWarning("Document {DocumentoId} not found", documentoId);
                return new ActionResponse<byte[]>
                {
                    WasSuccess = false,
                    Message = $"Documento con ID {documentoId} no encontrado."
                };
            }

            return await GenerarPdfAsync(documento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for document {DocumentoId}", documentoId);
            return new ActionResponse<byte[]>
            {
                WasSuccess = false,
                Message = $"Error al generar PDF: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generate PDF for a pre-loaded document
    /// </summary>
    public async Task<ActionResponse<byte[]>> GenerarPdfAsync(Documento documento)
    {
        try
        {
            // Ensure all relationships are loaded
            if (documento.Empresa == null || documento.Detalles == null || !documento.Detalles.Any())
            {
                documento = await CargarDocumentoCompletoAsync(documento.Id) ?? documento;
            }

            // Create appropriate PDF document based on type
            BasePdfDocument pdfDocument = documento.TipoDocumento switch
            {
                DocumentoTipo.FacturaElectronica => new FacturaElectronicaPdfDocument(documento),
                DocumentoTipo.TiqueteElectronico => new TiqueteElectronicoPdfDocument(documento),
                DocumentoTipo.NotaCreditoElectronica => new NotaCreditoPdfDocument(documento),
                DocumentoTipo.NotaDebitoElectronica => new NotaDebitoPdfDocument(documento),
                DocumentoTipo.FacturaElectronicaExportacion => new FacturaElectronicaPdfDocument(documento), // Use FE template for now
                DocumentoTipo.FacturaElectronicaCompra => new FacturaElectronicaPdfDocument(documento), // Use FE template for now
                DocumentoTipo.NotaCreditoElectronicaCompra => new NotaCreditoPdfDocument(documento), // Use NC template
                DocumentoTipo.NotaDebitoElectronicaCompra => new NotaDebitoPdfDocument(documento), // Use ND template
                DocumentoTipo.ComprobanteElectronicoCompra => new FacturaElectronicaPdfDocument(documento), // Use FE template for now
                DocumentoTipo.ReciboElectronicoPago => new FacturaElectronicaPdfDocument(documento), // Use FE template for now
                _ => throw new NotSupportedException($"Tipo de documento {documento.TipoDocumento} no soportado para generación de PDF.")
            };

            // Generate PDF bytes
            var pdfBytes = pdfDocument.GeneratePdf();

            _logger.LogInformation("PDF generated successfully for document {DocumentoId}, size: {Size} bytes",
                documento.Id, pdfBytes.Length);

            return new ActionResponse<byte[]>
            {
                WasSuccess = true,
                Result = pdfBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for document {DocumentoId}", documento.Id);
            return new ActionResponse<byte[]>
            {
                WasSuccess = false,
                Message = $"Error al generar PDF: {ex.Message}"
            };
        }
    }

    #region Private Methods

    /// <summary>
    /// Load document with all required relationships for PDF generation
    /// </summary>
    private async Task<Documento?> CargarDocumentoCompletoAsync(Guid documentoId)
    {
        var documento = await _context.Set<Documento>()
            .Include(d => d.Empresa)
                .ThenInclude(e => e!.Telefonos)
            .Include(d => d.Empresa)
                .ThenInclude(e => e!.Emails)
            .Include(d => d.Sucursal)
            .Include(d => d.Terminal)
            .Include(d => d.Cliente)
                .ThenInclude(c => c!.Telefonos)
            .Include(d => d.Cliente)
                .ThenInclude(c => c!.Emails)
            .Include(d => d.Proveedor)
            .FirstOrDefaultAsync(d => d.Id == documentoId && !d.IsDeleted);

        if (documento == null)
            return null;

        // Load Detalles with nested collections
        var detalles = await _context.Set<DocumentoDetalle>()
            .Include(d => d.Impuestos)
            .Include(d => d.Descuentos)
            .Include(d => d.NumerosVIN)
            .Include(d => d.UnidadMedida)
            .Include(d => d.Producto)
            .Where(d => d.DocumentoId == documentoId && !d.IsDeleted)
            .OrderBy(d => d.NumeroLinea)
            .ToListAsync();

        documento.Detalles = detalles;

        // Load Referencias
        var referencias = await _context.Set<DocumentoReferencia>()
            .Where(r => r.DocumentoId == documentoId && !r.IsDeleted)
            .ToListAsync();

        documento.Referencias = referencias;

        // Load Medios de Pago
        var mediosPago = await _context.Set<DocumentoMedioPago>()
            .Where(m => m.DocumentoId == documentoId && !m.IsDeleted)
            .ToListAsync();

        documento.MediosPago = mediosPago;

        // Load Otros Cargos
        var otrosCargos = await _context.Set<DocumentoOtroCargo>()
            .Where(o => o.DocumentoId == documentoId && !o.IsDeleted)
            .ToListAsync();

        documento.OtrosCargos = otrosCargos;

        // Load Otra Información
        var otraInformacion = await _context.Set<DocumentoOtraInformacion>()
            .Where(o => o.DocumentoId == documentoId && !o.IsDeleted)
            .ToListAsync();

        documento.OtraInformacion = otraInformacion;

        // Load Exportación if applicable
        if (documento.TipoDocumento == DocumentoTipo.FacturaElectronicaExportacion)
        {
            documento.Exportacion = await _context.Set<DocumentoExportacion>()
                .FirstOrDefaultAsync(e => e.DocumentoId == documentoId);
        }

        return documento;
    }

    #endregion
}
