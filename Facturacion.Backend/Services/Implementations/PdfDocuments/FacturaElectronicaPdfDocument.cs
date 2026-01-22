using Facturacion.Backend.Helpers;
using Facturacion.Shared.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facturacion.Backend.Services.Implementations.PdfDocuments;

/// <summary>
/// PDF document generator for Factura Electrónica (FE - tipo 01)
/// </summary>
public class FacturaElectronicaPdfDocument : BasePdfDocument
{
    public FacturaElectronicaPdfDocument(Documento documento) : base(documento)
    {
    }

    public override DocumentMetadata GetMetadata()
    {
        return new DocumentMetadata
        {
            Title = $"Factura Electrónica - {Documento.NumeroConsecutivo}",
            Author = Documento.Empresa?.RazonSocial ?? "Sistema de Facturación",
            Subject = "Factura Electrónica Costa Rica - Hacienda v4.4",
            Keywords = "factura, electrónica, costa rica, hacienda",
            Creator = "Sistema de Facturación Electrónica"
        };
    }

    public override void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(NormalFontSize).FontColor(TextColor));

            page.Header().Element(ComposePageHeader);
            page.Content().Element(ComposePageContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposePageHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(ComposeHeader);
            col.Item().Height(10);
            col.Item().Element(ComposeClaveSection);
        });
    }

    private void ComposePageContent(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Emisor y Receptor side by side
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(ComposeEmisor);
                row.ConstantItem(10);
                row.RelativeItem().Element(ComposeReceptor);
            });

            col.Item().Height(10);

            // Sale conditions
            col.Item().Element(ComposeCondiciones);

            col.Item().Height(10);

            // Line items table
            col.Item().Element(ComposeLineItems);

            col.Item().Height(10);

            // Bottom section with references, other charges, and totals
            col.Item().Row(row =>
            {
                // Left column - References, Other charges, Observations
                row.RelativeItem().Column(leftCol =>
                {
                    leftCol.Item().Element(ComposeReferencias);
                    leftCol.Item().Height(5);
                    leftCol.Item().Element(ComposeOtrosCargos);
                    leftCol.Item().Height(5);
                    leftCol.Item().Element(ComposeObservaciones);
                });

                row.ConstantItem(15);

                // Right column - Totals
                row.ConstantItem(220).Element(ComposeTotals);
            });

            col.Item().Height(10);

            // Hacienda status
            col.Item().Element(ComposeEstadoHacienda);
        });
    }
}
