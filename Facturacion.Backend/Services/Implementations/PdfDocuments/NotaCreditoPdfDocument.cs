using Facturacion.Backend.Helpers;
using Facturacion.Shared.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facturacion.Backend.Services.Implementations.PdfDocuments;

/// <summary>
/// PDF document generator for Nota de Crédito Electrónica (NC - tipo 03)
/// Requires reference to original document
/// </summary>
public class NotaCreditoPdfDocument : BasePdfDocument
{
    // Override colors for NC (orange theme)
    private new static readonly string PrimaryColor = "#e67e22";
    private new static readonly string SecondaryColor = "#f39c12";

    public NotaCreditoPdfDocument(Documento documento) : base(documento)
    {
    }

    public override DocumentMetadata GetMetadata()
    {
        return new DocumentMetadata
        {
            Title = $"Nota de Crédito Electrónica - {Documento.NumeroConsecutivo}",
            Author = Documento.Empresa?.RazonSocial ?? "Sistema de Facturación",
            Subject = "Nota de Crédito Electrónica Costa Rica - Hacienda v4.4",
            Keywords = "nota de crédito, electrónica, costa rica, hacienda",
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
            col.Item().Element(ComposeHeaderNC);
            col.Item().Height(10);
            col.Item().Element(ComposeClaveSection);
        });
    }

    private void ComposeHeaderNC(IContainer container)
    {
        container.Row(row =>
        {
            // Left side - Company logo and info
            row.RelativeItem(2).Column(col =>
            {
                col.Item().Height(50).Width(150).Background(Colors.White).Border(1).BorderColor(BorderColor)
                    .AlignCenter().AlignMiddle()
                    .Text(Documento.Empresa?.NombreComercial ?? Documento.Empresa?.RazonSocial ?? "EMPRESA")
                    .FontSize(SmallFontSize).Bold().FontColor(PrimaryColor);

                col.Item().Height(5);

                col.Item().Text(Documento.Empresa?.RazonSocial ?? "")
                    .FontSize(SubtitleFontSize).Bold().FontColor(TextColor);

                if (!string.IsNullOrWhiteSpace(Documento.Empresa?.NombreComercial) &&
                    Documento.Empresa.NombreComercial != Documento.Empresa.RazonSocial)
                {
                    col.Item().Text(Documento.Empresa.NombreComercial)
                        .FontSize(NormalFontSize).FontColor(LightTextColor);
                }

                col.Item().Text($"Cédula: {PdfFormatHelper.FormatIdentificacion(Documento.Empresa?.NumeroIdentificacion, Documento.Empresa?.TipoIdentificacion)}")
                    .FontSize(NormalFontSize).FontColor(TextColor);
            });

            row.ConstantItem(20);

            // Right side - Document type with orange theme
            row.RelativeItem(1).Border(2).BorderColor(PrimaryColor).Background("#fff5eb").Padding(10).Column(col =>
            {
                col.Item().AlignCenter().Text("NOTA DE CRÉDITO")
                    .FontSize(SubtitleFontSize).Bold().FontColor(PrimaryColor);

                col.Item().AlignCenter().Text("ELECTRÓNICA")
                    .FontSize(NormalFontSize).Bold().FontColor(PrimaryColor);

                col.Item().Height(5);

                col.Item().AlignCenter().Text($"No. {Documento.NumeroConsecutivo}")
                    .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                col.Item().Height(3);

                col.Item().AlignCenter().Text($"Fecha: {PdfFormatHelper.FormatFecha(Documento.FechaEmision)}")
                    .FontSize(NormalFontSize).FontColor(TextColor);
            });
        });
    }

    private void ComposePageContent(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Important: Reference section first for NC
            if (Documento.Referencias?.Any() == true)
            {
                col.Item().Element(ComposeReferenciasDestacado);
                col.Item().Height(10);
            }

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

            // Bottom section
            col.Item().Row(row =>
            {
                // Left column
                row.RelativeItem().Column(leftCol =>
                {
                    leftCol.Item().Element(ComposeOtrosCargos);
                    leftCol.Item().Height(5);
                    leftCol.Item().Element(ComposeObservaciones);
                });

                row.ConstantItem(15);

                // Right column - Totals
                row.ConstantItem(220).Element(ComposeTotalsNC);
            });

            col.Item().Height(10);

            // Hacienda status
            col.Item().Element(ComposeEstadoHacienda);
        });
    }

    /// <summary>
    /// Compose highlighted reference section for NC
    /// </summary>
    private void ComposeReferenciasDestacado(IContainer container)
    {
        var referencias = Documento.Referencias?.ToList();
        if (referencias == null || !referencias.Any())
            return;

        container.Border(2).BorderColor(PrimaryColor).Background("#fff5eb").Column(col =>
        {
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("DOCUMENTO DE REFERENCIA (Documento que se está afectando)")
                .FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            foreach (var referencia in referencias)
            {
                col.Item().Padding(8).Column(refCol =>
                {
                    refCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Tipo de Documento: {referencia.TipoDocumentoReferenciado}")
                            .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                        row.RelativeItem().Text($"Número: {referencia.NumeroDocumentoReferenciado}")
                            .FontSize(NormalFontSize).Bold().FontColor(PrimaryColor);
                    });

                    refCol.Item().Text($"Fecha de Emisión: {PdfFormatHelper.FormatFecha(referencia.FechaEmisionDocumentoReferenciado)}")
                        .FontSize(SmallFontSize).FontColor(TextColor);

                    refCol.Item().Text($"Código de Referencia: {PdfFormatHelper.GetTipoReferencia(referencia.CodigoReferencia)}")
                        .FontSize(SmallFontSize).FontColor(TextColor);

                    refCol.Item().Text($"Razón: {referencia.RazonReferencia}")
                        .FontSize(SmallFontSize).Bold().FontColor(PrimaryColor);
                });
            }
        });
    }

    /// <summary>
    /// Compose totals section with NC styling
    /// </summary>
    private void ComposeTotalsNC(IContainer container)
    {
        container.Border(2).BorderColor(PrimaryColor).Column(col =>
        {
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("RESUMEN - NOTA DE CRÉDITO").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            col.Item().Padding(8).Column(content =>
            {
                if (Documento.TotalGravado > 0)
                    ComposeTotalRowNC(content, "Total Gravado:", Documento.TotalGravado);

                if (Documento.TotalExento > 0)
                    ComposeTotalRowNC(content, "Total Exento:", Documento.TotalExento);

                if (Documento.TotalExonerado > 0)
                    ComposeTotalRowNC(content, "Total Exonerado:", Documento.TotalExonerado);

                ComposeTotalRowNC(content, "Subtotal:", Documento.Subtotal);

                if (Documento.TotalDescuentos > 0)
                    ComposeTotalRowNC(content, "Total Descuentos:", Documento.TotalDescuentos);

                if (Documento.TotalImpuestos > 0)
                    ComposeTotalRowNC(content, "Total Impuestos:", Documento.TotalImpuestos);

                content.Item().Height(5);
                content.Item().BorderBottom(2).BorderColor(PrimaryColor);
                content.Item().Height(5);

                // Emphasize this is a credit (TotalComprobante = TotalVenta + TotalOtrosCargos - IVADevuelto)
                var totalComprobante = Documento.TotalVenta + Documento.TotalOtrosCargos - (Documento.IVADevuelto ?? 0);

                content.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("TOTAL A ACREDITAR:")
                        .FontSize(SubtitleFontSize).Bold().FontColor(PrimaryColor);
                    row.ConstantItem(10);
                    row.ConstantItem(100).AlignRight().Text(PdfFormatHelper.FormatMoneda(totalComprobante, Documento.Moneda))
                        .FontSize(SubtitleFontSize).Bold().FontColor(PrimaryColor);
                });
            });
        });
    }

    private void ComposeTotalRowNC(ColumnDescriptor content, string label, decimal amount)
    {
        content.Item().Row(row =>
        {
            row.RelativeItem().AlignRight().Text(label)
                .FontSize(SmallFontSize).FontColor(LightTextColor);
            row.ConstantItem(10);
            row.ConstantItem(100).AlignRight().Text(PdfFormatHelper.FormatMoneda(amount, Documento.Moneda))
                .FontSize(SmallFontSize).FontColor(TextColor);
        });
    }
}
