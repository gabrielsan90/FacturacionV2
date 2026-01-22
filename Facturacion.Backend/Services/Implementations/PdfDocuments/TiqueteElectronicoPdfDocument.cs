using Facturacion.Backend.Helpers;
using Facturacion.Shared.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facturacion.Backend.Services.Implementations.PdfDocuments;

/// <summary>
/// PDF document generator for Tiquete Electrónico (TE - tipo 04)
/// Similar to Factura but receptor is optional (consumidor final)
/// </summary>
public class TiqueteElectronicoPdfDocument : BasePdfDocument
{
    public TiqueteElectronicoPdfDocument(Documento documento) : base(documento)
    {
    }

    public override DocumentMetadata GetMetadata()
    {
        return new DocumentMetadata
        {
            Title = $"Tiquete Electrónico - {Documento.NumeroConsecutivo}",
            Author = Documento.Empresa?.RazonSocial ?? "Sistema de Facturación",
            Subject = "Tiquete Electrónico Costa Rica - Hacienda v4.4",
            Keywords = "tiquete, electrónico, costa rica, hacienda",
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
                row.RelativeItem().Element(ComposeReceptorTiquete);
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
                // Left column - Other charges, Observations
                row.RelativeItem().Column(leftCol =>
                {
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

    /// <summary>
    /// Compose receptor section for Tiquete - simplified for consumidor final
    /// </summary>
    private void ComposeReceptorTiquete(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            // Header
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("RECEPTOR").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            // Content
            col.Item().Padding(8).Column(content =>
            {
                var tieneReceptor = !string.IsNullOrWhiteSpace(Documento.ReceptorNombre) ||
                                    !string.IsNullOrWhiteSpace(Documento.ReceptorNumeroIdentificacion);

                if (tieneReceptor)
                {
                    var nombre = Documento.ReceptorNombre ?? "Consumidor Final";
                    var tipoId = Documento.ReceptorTipoIdentificacion;
                    var numeroId = Documento.ReceptorNumeroIdentificacion;

                    content.Item().Text(nombre)
                        .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                    if (!string.IsNullOrWhiteSpace(numeroId))
                    {
                        content.Item().Text($"Identificación: {PdfFormatHelper.GetTipoIdentificacion(tipoId)} - {PdfFormatHelper.FormatIdentificacion(numeroId, tipoId)}")
                            .FontSize(SmallFontSize).FontColor(TextColor);
                    }

                    if (!string.IsNullOrWhiteSpace(Documento.ReceptorEmails))
                    {
                        content.Item().Text($"Email: {Documento.ReceptorEmails}")
                            .FontSize(SmallFontSize).FontColor(TextColor);
                    }
                }
                else
                {
                    content.Item().Text("CONSUMIDOR FINAL")
                        .FontSize(NormalFontSize).Bold().FontColor(LightTextColor);

                    content.Item().Text("(Receptor no identificado)")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }
            });
        });
    }
}
