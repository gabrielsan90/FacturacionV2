using Facturacion.Backend.Helpers;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facturacion.Backend.Services.Implementations.PdfDocuments;

/// <summary>
/// PDF document generator for Cotización (quotation/estimate)
/// Standalone class — does not extend BasePdfDocument (which requires Documento entity)
/// </summary>
public class CotizacionPdfDocument : IDocument
{
    private readonly Cotizacion _cotizacion;

    // Colors (same palette as BasePdfDocument)
    private static readonly string PrimaryColor = "#1a5276";
    private static readonly string SecondaryColor = "#2980b9";
    private static readonly string HeaderBgColor = "#ecf0f1";
    private static readonly string AlternateRowColor = "#f8f9fa";
    private static readonly string BorderColor = "#dee2e6";
    private static readonly string TextColor = "#2c3e50";
    private static readonly string LightTextColor = "#7f8c8d";

    // Font sizes
    private const float TitleFontSize = 14;
    private const float SubtitleFontSize = 11;
    private const float NormalFontSize = 9;
    private const float SmallFontSize = 8;
    private const float TinyFontSize = 7;

    public CotizacionPdfDocument(Cotizacion cotizacion)
    {
        _cotizacion = cotizacion ?? throw new ArgumentNullException(nameof(cotizacion));
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Cotización - {_cotizacion.Numero}",
        Author = _cotizacion.Empresa?.RazonSocial ?? "Sistema de Facturación",
        Subject = "Cotización / Presupuesto",
        Creator = "Sistema de Facturación Electrónica"
    };

    public void Compose(IDocumentContainer container)
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
            col.Item().Row(row =>
            {
                // Left — Company info
                row.RelativeItem(2).Column(left =>
                {
                    left.Item().Height(50).Width(150).Background(Colors.White)
                        .Border(1).BorderColor(BorderColor)
                        .AlignCenter().AlignMiddle()
                        .Text(_cotizacion.Empresa?.NombreComercial ?? _cotizacion.Empresa?.RazonSocial ?? "EMPRESA")
                        .FontSize(SmallFontSize).Bold().FontColor(PrimaryColor);

                    left.Item().Height(5);

                    left.Item().Text(_cotizacion.Empresa?.RazonSocial ?? "")
                        .FontSize(SubtitleFontSize).Bold().FontColor(TextColor);

                    if (!string.IsNullOrWhiteSpace(_cotizacion.Empresa?.NombreComercial) &&
                        _cotizacion.Empresa.NombreComercial != _cotizacion.Empresa.RazonSocial)
                    {
                        left.Item().Text(_cotizacion.Empresa.NombreComercial)
                            .FontSize(NormalFontSize).FontColor(LightTextColor);
                    }

                    left.Item().Text($"Cédula: {PdfFormatHelper.FormatIdentificacion(_cotizacion.Empresa?.NumeroIdentificacion, _cotizacion.Empresa?.TipoIdentificacion)}")
                        .FontSize(NormalFontSize).FontColor(TextColor);
                });

                row.ConstantItem(20);

                // Right — Cotización type box
                row.RelativeItem(1).Border(1).BorderColor(PrimaryColor).Background(HeaderBgColor).Padding(10).Column(right =>
                {
                    right.Item().AlignCenter().Text("COTIZACIÓN")
                        .FontSize(TitleFontSize).Bold().FontColor(PrimaryColor);

                    right.Item().Height(5);

                    right.Item().AlignCenter().Text($"No. {_cotizacion.Numero}")
                        .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                    right.Item().Height(3);

                    right.Item().AlignCenter().Text($"Fecha: {PdfFormatHelper.FormatFecha(_cotizacion.FechaEmision)}")
                        .FontSize(NormalFontSize).FontColor(TextColor);

                    right.Item().Height(3);

                    var estadoText = _cotizacion.Estado switch
                    {
                        EstadoCotizacion.Borrador => "BORRADOR",
                        EstadoCotizacion.Enviada => "ENVIADA",
                        EstadoCotizacion.Aprobada => "APROBADA",
                        EstadoCotizacion.Rechazada => "RECHAZADA",
                        EstadoCotizacion.Convertida => "CONVERTIDA",
                        EstadoCotizacion.Vencida => "VENCIDA",
                        _ => _cotizacion.Estado.ToString().ToUpper()
                    };
                    var estadoColor = _cotizacion.Estado switch
                    {
                        EstadoCotizacion.Aprobada => "#28a745",
                        EstadoCotizacion.Rechazada or EstadoCotizacion.Vencida => "#dc3545",
                        EstadoCotizacion.Enviada => "#17a2b8",
                        EstadoCotizacion.Convertida => "#007bff",
                        _ => "#6c757d"
                    };
                    right.Item().AlignCenter().Text(estadoText)
                        .FontSize(TinyFontSize).Bold().FontColor(estadoColor);
                });
            });
        });
    }

    private void ComposePageContent(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Emisor & Receptor side by side
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(ComposeEmisor);
                row.ConstantItem(10);
                row.RelativeItem().Element(ComposeReceptor);
            });

            col.Item().Height(10);

            // Conditions
            col.Item().Element(ComposeCondiciones);

            col.Item().Height(10);

            // Line items
            col.Item().Element(ComposeLineItems);

            col.Item().Height(10);

            // Bottom: notes + totals
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(leftCol =>
                {
                    // Validity info
                    leftCol.Item().Border(1).BorderColor(BorderColor).Column(validez =>
                    {
                        validez.Item().Background(SecondaryColor).Padding(5)
                            .Text("VIGENCIA").FontSize(SmallFontSize).Bold().FontColor(Colors.White);

                        validez.Item().Padding(8).Column(vc =>
                        {
                            vc.Item().Text($"Válida hasta: {PdfFormatHelper.FormatFecha(_cotizacion.FechaVencimiento)}")
                                .FontSize(NormalFontSize).FontColor(TextColor);

                            var diasRestantes = (_cotizacion.FechaVencimiento - DateTime.Today).Days;
                            if (diasRestantes > 0)
                            {
                                vc.Item().Text($"{diasRestantes} días restantes")
                                    .FontSize(SmallFontSize).FontColor("#28a745");
                            }
                            else
                            {
                                vc.Item().Text("Vencida")
                                    .FontSize(SmallFontSize).Bold().FontColor("#dc3545");
                            }
                        });
                    });

                    leftCol.Item().Height(5);

                    // Notes
                    if (!string.IsNullOrWhiteSpace(_cotizacion.Notas))
                    {
                        leftCol.Item().Border(1).BorderColor(BorderColor).Column(notas =>
                        {
                            notas.Item().Background(HeaderBgColor).Padding(5)
                                .Text("NOTAS / CONDICIONES").FontSize(SmallFontSize).Bold().FontColor(TextColor);

                            notas.Item().Padding(8).Text(_cotizacion.Notas)
                                .FontSize(SmallFontSize).FontColor(TextColor);
                        });
                    }
                });

                row.ConstantItem(15);

                // Right column — Totals
                row.ConstantItem(220).Element(ComposeTotals);
            });
        });
    }

    private void ComposeEmisor(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("EMISOR").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            col.Item().Padding(8).Column(content =>
            {
                content.Item().Text(_cotizacion.Empresa?.RazonSocial ?? "N/A")
                    .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                if (!string.IsNullOrWhiteSpace(_cotizacion.Empresa?.NombreComercial))
                {
                    content.Item().Text($"Nombre Comercial: {_cotizacion.Empresa.NombreComercial}")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }

                content.Item().Text($"Identificación: {PdfFormatHelper.GetTipoIdentificacion(_cotizacion.Empresa?.TipoIdentificacion)} - {PdfFormatHelper.FormatIdentificacion(_cotizacion.Empresa?.NumeroIdentificacion, _cotizacion.Empresa?.TipoIdentificacion)}")
                    .FontSize(SmallFontSize).FontColor(TextColor);

                if (_cotizacion.Empresa != null && !string.IsNullOrWhiteSpace(_cotizacion.Empresa.OtrasSenas))
                {
                    content.Item().Text($"Dirección: {PdfFormatHelper.TruncarTexto(_cotizacion.Empresa.OtrasSenas, 100)}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                var email = _cotizacion.Empresa?.Emails?.FirstOrDefault()?.DireccionEmail;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    content.Item().Text($"Email: {email}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                var telefono = _cotizacion.Empresa?.Telefonos?.FirstOrDefault()?.NumeroTelefono;
                if (!string.IsNullOrWhiteSpace(telefono))
                {
                    content.Item().Text($"Teléfono: {telefono}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }
            });
        });
    }

    private void ComposeReceptor(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("CLIENTE").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            col.Item().Padding(8).Column(content =>
            {
                var nombre = _cotizacion.ClienteNombre ?? _cotizacion.Cliente?.Nombre ?? "N/A";
                content.Item().Text(nombre)
                    .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                if (!string.IsNullOrWhiteSpace(_cotizacion.ClienteNumeroIdentificacion))
                {
                    content.Item().Text($"Identificación: {PdfFormatHelper.GetTipoIdentificacion(_cotizacion.ClienteTipoIdentificacion)} - {PdfFormatHelper.FormatIdentificacion(_cotizacion.ClienteNumeroIdentificacion, _cotizacion.ClienteTipoIdentificacion)}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                if (!string.IsNullOrWhiteSpace(_cotizacion.ClienteEmail))
                {
                    content.Item().Text($"Email: {_cotizacion.ClienteEmail}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                if (!string.IsNullOrWhiteSpace(_cotizacion.ClienteTelefono))
                {
                    content.Item().Text($"Teléfono: {_cotizacion.ClienteTelefono}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }
            });
        });
    }

    private void ComposeCondiciones(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Padding(8).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Condición de Venta:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(PdfFormatHelper.GetCondicionVenta(_cotizacion.CondicionVenta))
                    .FontSize(NormalFontSize).FontColor(TextColor);

                if (_cotizacion.PlazoCreditoDias.HasValue && _cotizacion.PlazoCreditoDias > 0)
                {
                    col.Item().Text($"Plazo: {_cotizacion.PlazoCreditoDias} días")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Medio de Pago:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(PdfFormatHelper.GetMedioPago(_cotizacion.MedioPago))
                    .FontSize(NormalFontSize).FontColor(TextColor);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Moneda:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(_cotizacion.Moneda.ToString())
                    .FontSize(NormalFontSize).FontColor(TextColor);

                if (_cotizacion.TipoCambio.HasValue && _cotizacion.TipoCambio > 0 && _cotizacion.Moneda != TipoMoneda.CRC)
                {
                    col.Item().Text($"T.C.: {_cotizacion.TipoCambio:N5}")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Vigencia:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text($"{PdfFormatHelper.FormatFecha(_cotizacion.FechaEmision)} - {PdfFormatHelper.FormatFecha(_cotizacion.FechaVencimiento)}")
                    .FontSize(NormalFontSize).FontColor(TextColor);
            });
        });
    }

    private void ComposeLineItems(IContainer container)
    {
        container.Column(col =>
        {
            // Table header
            col.Item().Border(1).BorderColor(BorderColor).Background(PrimaryColor).Padding(5).Row(row =>
            {
                row.ConstantItem(25).AlignCenter().Text("#").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.ConstantItem(70).AlignCenter().Text("Código").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.RelativeItem(3).Text("Descripción").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.ConstantItem(45).AlignCenter().Text("Cant.").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.ConstantItem(30).AlignCenter().Text("UM").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.ConstantItem(65).AlignRight().Text("Precio").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.ConstantItem(55).AlignRight().Text("Desc.").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.ConstantItem(55).AlignRight().Text("Imp.").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
                row.ConstantItem(70).AlignRight().Text("Total").FontSize(SmallFontSize).Bold().FontColor(Colors.White);
            });

            // Table rows
            var detalles = _cotizacion.Detalles?.OrderBy(d => d.NumeroLinea).ToList() ?? new List<CotizacionDetalle>();
            var isAlternate = false;

            foreach (var detalle in detalles)
            {
                var bgColor = isAlternate ? Color.FromHex(AlternateRowColor) : Colors.White;

                col.Item().Border(1).BorderColor(BorderColor).Background(bgColor).Padding(4).Row(row =>
                {
                    row.ConstantItem(25).AlignCenter().Text(detalle.NumeroLinea.ToString())
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(70).AlignCenter().Text(PdfFormatHelper.TruncarTexto(detalle.Codigo ?? "", 13))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.RelativeItem(3).Text(PdfFormatHelper.TruncarTexto(detalle.Descripcion, 60))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(45).AlignCenter().Text(PdfFormatHelper.FormatCantidad(detalle.Cantidad))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(30).AlignCenter().Text(detalle.UnidadMedida ?? "UN")
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(65).AlignRight().Text(PdfFormatHelper.FormatMoneda(detalle.PrecioUnitario, _cotizacion.Moneda))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(55).AlignRight().Text(detalle.MontoDescuento > 0 ? PdfFormatHelper.FormatMoneda(detalle.MontoDescuento, _cotizacion.Moneda) : "-")
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(55).AlignRight().Text(detalle.TotalImpuestos > 0 ? PdfFormatHelper.FormatMoneda(detalle.TotalImpuestos, _cotizacion.Moneda) : "-")
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(70).AlignRight().Text(PdfFormatHelper.FormatMoneda(detalle.TotalLinea, _cotizacion.Moneda))
                        .FontSize(TinyFontSize).Bold().FontColor(TextColor);
                });

                isAlternate = !isAlternate;
            }
        });
    }

    private void ComposeTotals(IContainer container)
    {
        container.Border(1).BorderColor(PrimaryColor).Column(col =>
        {
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("RESUMEN").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            col.Item().Padding(8).Column(content =>
            {
                ComposeTotalRow(content, "Subtotal:", _cotizacion.Subtotal);

                if (_cotizacion.TotalDescuentos > 0)
                {
                    ComposeTotalRow(content, "Total Descuentos:", -_cotizacion.TotalDescuentos, true);
                }

                if (_cotizacion.TotalImpuestos > 0)
                {
                    ComposeTotalRow(content, "Total Impuestos:", _cotizacion.TotalImpuestos);
                }

                // Separator
                content.Item().Height(5);
                content.Item().BorderBottom(2).BorderColor(PrimaryColor);
                content.Item().Height(5);

                // Grand total
                content.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("TOTAL:")
                        .FontSize(SubtitleFontSize).Bold().FontColor(PrimaryColor);
                    row.ConstantItem(10);
                    row.ConstantItem(100).AlignRight().Text(PdfFormatHelper.FormatMoneda(_cotizacion.Total, _cotizacion.Moneda))
                        .FontSize(SubtitleFontSize).Bold().FontColor(PrimaryColor);
                });
            });
        });
    }

    private void ComposeTotalRow(ColumnDescriptor content, string label, decimal amount, bool isNegative = false)
    {
        content.Item().Row(row =>
        {
            row.RelativeItem().AlignRight().Text(label)
                .FontSize(SmallFontSize).FontColor(LightTextColor);
            row.ConstantItem(10);

            var displayAmount = isNegative
                ? $"({PdfFormatHelper.FormatMoneda(Math.Abs(amount), _cotizacion.Moneda)})"
                : PdfFormatHelper.FormatMoneda(amount, _cotizacion.Moneda);
            var color = isNegative ? "#dc3545" : TextColor;

            row.ConstantItem(100).AlignRight().Text(displayAmount)
                .FontSize(SmallFontSize).FontColor(Color.FromHex(color));
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Column(col =>
        {
            col.Item().BorderTop(1).BorderColor(BorderColor).PaddingTop(5);

            col.Item().PaddingBottom(3).AlignCenter()
                .Text("Este documento es una cotización y no tiene validez fiscal.")
                .FontSize(TinyFontSize).Bold().FontColor(TextColor);

            col.Item().Row(row =>
            {
                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("Generado: ").FontSize(TinyFontSize).FontColor(LightTextColor);
                    text.Span(PdfFormatHelper.FormatFechaHora(FechaCostaRicaHelper.Ahora)).FontSize(TinyFontSize).FontColor(LightTextColor);
                });

                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.Span("Cotización - Documento no fiscal").FontSize(TinyFontSize).FontColor(LightTextColor);
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(TinyFontSize).FontColor(LightTextColor);
                    text.CurrentPageNumber().FontSize(TinyFontSize).FontColor(LightTextColor);
                    text.Span(" de ").FontSize(TinyFontSize).FontColor(LightTextColor);
                    text.TotalPages().FontSize(TinyFontSize).FontColor(LightTextColor);
                });
            });
        });
    }
}
