using Facturacion.Backend.Helpers;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Facturacion.Backend.Services.Implementations.PdfDocuments;

/// <summary>
/// Base abstract class for all electronic document PDFs
/// Provides shared components and styling for Costa Rica Hacienda v4.4 documents
/// </summary>
public abstract class BasePdfDocument : IDocument
{
    protected readonly Documento Documento;

    // Colors
    protected static readonly string PrimaryColor = "#1a5276";
    protected static readonly string SecondaryColor = "#2980b9";
    protected static readonly string HeaderBgColor = "#ecf0f1";
    protected static readonly string AlternateRowColor = "#f8f9fa";
    protected static readonly string BorderColor = "#dee2e6";
    protected static readonly string TextColor = "#2c3e50";
    protected static readonly string LightTextColor = "#7f8c8d";

    // Font sizes
    protected const float TitleFontSize = 14;
    protected const float SubtitleFontSize = 11;
    protected const float NormalFontSize = 9;
    protected const float SmallFontSize = 8;
    protected const float TinyFontSize = 7;

    protected BasePdfDocument(Documento documento)
    {
        Documento = documento ?? throw new ArgumentNullException(nameof(documento));
    }

    public abstract DocumentMetadata GetMetadata();
    public abstract void Compose(IDocumentContainer container);

    #region Shared Components

    /// <summary>
    /// Compose the document header with company logo and document type
    /// </summary>
    protected void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            // Left side - Company logo and info
            row.RelativeItem(2).Column(col =>
            {
                // Company logo placeholder (can be replaced with actual logo)
                col.Item().Height(50).Width(150).Background(Colors.White).Border(1).BorderColor(BorderColor)
                    .AlignCenter().AlignMiddle()
                    .Text(Documento.Empresa?.NombreComercial ?? Documento.Empresa?.RazonSocial ?? "EMPRESA")
                    .FontSize(SmallFontSize).Bold().FontColor(PrimaryColor);

                col.Item().Height(5);

                // Company name (Razon Social)
                col.Item().Text(Documento.Empresa?.RazonSocial ?? "")
                    .FontSize(SubtitleFontSize).Bold().FontColor(TextColor);

                // Company commercial name if different
                if (!string.IsNullOrWhiteSpace(Documento.Empresa?.NombreComercial) &&
                    Documento.Empresa.NombreComercial != Documento.Empresa.RazonSocial)
                {
                    col.Item().Text(Documento.Empresa.NombreComercial)
                        .FontSize(NormalFontSize).FontColor(LightTextColor);
                }

                // Company ID
                col.Item().Text($"Cédula: {PdfFormatHelper.FormatIdentificacion(Documento.Empresa?.NumeroIdentificacion, Documento.Empresa?.TipoIdentificacion)}")
                    .FontSize(NormalFontSize).FontColor(TextColor);
            });

            // Spacer
            row.ConstantItem(20);

            // Right side - Document type and number
            row.RelativeItem(1).Border(1).BorderColor(PrimaryColor).Background(HeaderBgColor).Padding(10).Column(col =>
            {
                var tipoColor = PdfFormatHelper.GetColorTipoDocumento(Documento.TipoDocumento);

                col.Item().AlignCenter().Text(PdfFormatHelper.GetNombreTipoDocumento(Documento.TipoDocumento))
                    .FontSize(SubtitleFontSize).Bold().FontColor(tipoColor);

                col.Item().Height(5);

                col.Item().AlignCenter().Text($"No. {Documento.NumeroConsecutivo}")
                    .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                col.Item().Height(3);

                col.Item().AlignCenter().Text($"Fecha: {PdfFormatHelper.FormatFecha(Documento.FechaEmision)}")
                    .FontSize(NormalFontSize).FontColor(TextColor);

                // Environment indicator
                var ambienteTexto = Documento.Ambiente == Ambiente.Produccion ? "PRODUCCIÓN" : "PRUEBAS";
                var ambienteColor = Documento.Ambiente == Ambiente.Produccion ? "#28a745" : "#ffc107";
                col.Item().Height(3);
                col.Item().AlignCenter().Text(ambienteTexto)
                    .FontSize(TinyFontSize).Bold().FontColor(ambienteColor);
            });
        });
    }

    /// <summary>
    /// Compose the clave section
    /// </summary>
    protected void ComposeClaveSection(IContainer container)
    {
        container.Background(HeaderBgColor).Padding(5).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Clave:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(Documento.Clave ?? "").FontSize(SmallFontSize).FontColor(TextColor);
            });
        });
    }

    /// <summary>
    /// Compose the emisor (sender) section
    /// </summary>
    protected void ComposeEmisor(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            // Header
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("EMISOR").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            // Content
            col.Item().Padding(8).Column(content =>
            {
                content.Item().Text(Documento.Empresa?.RazonSocial ?? "N/A")
                    .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                if (!string.IsNullOrWhiteSpace(Documento.Empresa?.NombreComercial))
                {
                    content.Item().Text($"Nombre Comercial: {Documento.Empresa.NombreComercial}")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }

                content.Item().Text($"Identificación: {PdfFormatHelper.GetTipoIdentificacion(Documento.Empresa?.TipoIdentificacion)} - {PdfFormatHelper.FormatIdentificacion(Documento.Empresa?.NumeroIdentificacion, Documento.Empresa?.TipoIdentificacion)}")
                    .FontSize(SmallFontSize).FontColor(TextColor);

                content.Item().Text($"Actividad Económica: {Documento.ActividadEconomica}")
                    .FontSize(SmallFontSize).FontColor(TextColor);

                // Address from Empresa
                if (Documento.Empresa != null && !string.IsNullOrWhiteSpace(Documento.Empresa.OtrasSenas))
                {
                    content.Item().Text($"Dirección: {PdfFormatHelper.TruncarTexto(Documento.Empresa.OtrasSenas, 100)}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                // Email and phone from Empresa
                var email = Documento.Empresa?.Emails?.FirstOrDefault()?.DireccionEmail;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    content.Item().Text($"Email: {email}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                var telefono = Documento.Empresa?.Telefonos?.FirstOrDefault()?.NumeroTelefono;
                if (!string.IsNullOrWhiteSpace(telefono))
                {
                    content.Item().Text($"Teléfono: {telefono}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }
            });
        });
    }

    /// <summary>
    /// Compose the receptor (receiver) section
    /// </summary>
    protected void ComposeReceptor(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            // Header
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("RECEPTOR").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            // Content
            col.Item().Padding(8).Column(content =>
            {
                // Use stored receptor data (historical snapshot)
                var nombre = Documento.ReceptorNombre ?? Documento.Cliente?.Nombre ?? "Consumidor Final";
                var nombreComercial = Documento.ReceptorNombreComercial ?? Documento.Cliente?.NombreComercial;
                var tipoId = Documento.ReceptorTipoIdentificacion ?? Documento.Cliente?.TipoIdentificacion;
                var numeroId = Documento.ReceptorNumeroIdentificacion ?? Documento.Cliente?.NumeroIdentificacion;

                content.Item().Text(nombre)
                    .FontSize(NormalFontSize).Bold().FontColor(TextColor);

                if (!string.IsNullOrWhiteSpace(nombreComercial) && nombreComercial != nombre)
                {
                    content.Item().Text($"Nombre Comercial: {nombreComercial}")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }

                if (!string.IsNullOrWhiteSpace(numeroId))
                {
                    content.Item().Text($"Identificación: {PdfFormatHelper.GetTipoIdentificacion(tipoId)} - {PdfFormatHelper.FormatIdentificacion(numeroId, tipoId)}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                if (!string.IsNullOrWhiteSpace(Documento.ReceptorActividadEconomica))
                {
                    content.Item().Text($"Actividad Económica: {Documento.ReceptorActividadEconomica}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                // Address
                if (!string.IsNullOrWhiteSpace(Documento.ReceptorOtrasSenas))
                {
                    content.Item().Text($"Dirección: {PdfFormatHelper.TruncarTexto(Documento.ReceptorOtrasSenas, 100)}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                // Email
                if (!string.IsNullOrWhiteSpace(Documento.ReceptorEmails))
                {
                    content.Item().Text($"Email: {Documento.ReceptorEmails}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }

                // Phone
                if (!string.IsNullOrWhiteSpace(Documento.ReceptorTelefono))
                {
                    content.Item().Text($"Teléfono: {Documento.ReceptorTelefono}")
                        .FontSize(SmallFontSize).FontColor(TextColor);
                }
            });
        });
    }

    /// <summary>
    /// Compose the sale conditions section
    /// </summary>
    protected void ComposeCondiciones(IContainer container)
    {
        container.Border(1).BorderColor(BorderColor).Padding(8).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Condición de Venta:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(PdfFormatHelper.GetCondicionVenta(Documento.CondicionVenta))
                    .FontSize(NormalFontSize).FontColor(TextColor);

                if (Documento.PlazoCreditoDias.HasValue && Documento.PlazoCreditoDias > 0)
                {
                    col.Item().Text($"Plazo: {Documento.PlazoCreditoDias} días")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Medio de Pago:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(PdfFormatHelper.GetMedioPago(Documento.MedioPago))
                    .FontSize(NormalFontSize).FontColor(TextColor);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Moneda:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(Documento.Moneda.ToString())
                    .FontSize(NormalFontSize).FontColor(TextColor);

                if (Documento.TipoCambio.HasValue && Documento.TipoCambio > 0 && Documento.Moneda != TipoMoneda.CRC)
                {
                    col.Item().Text($"T.C.: {Documento.TipoCambio:N5}")
                        .FontSize(SmallFontSize).FontColor(LightTextColor);
                }
            });

            // Order number if exists
            if (!string.IsNullOrWhiteSpace(Documento.NumeroOrdenCompra))
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Orden de Compra:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                    col.Item().Text(Documento.NumeroOrdenCompra)
                        .FontSize(NormalFontSize).FontColor(TextColor);
                });
            }
        });
    }

    /// <summary>
    /// Compose the line items table
    /// </summary>
    protected void ComposeLineItems(IContainer container)
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
            var detalles = Documento.Detalles?.OrderBy(d => d.NumeroLinea).ToList() ?? new List<DocumentoDetalle>();
            var isAlternate = false;

            foreach (var detalle in detalles)
            {
                var bgColor = isAlternate ? Color.FromHex(AlternateRowColor) : Colors.White;

                col.Item().Border(1).BorderColor(BorderColor).Background(bgColor).Padding(4).Row(row =>
                {
                    row.ConstantItem(25).AlignCenter().Text(detalle.NumeroLinea.ToString())
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(70).AlignCenter().Text(PdfFormatHelper.TruncarTexto(detalle.CodigoCabys ?? detalle.Codigo ?? "", 13))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.RelativeItem(3).Text(PdfFormatHelper.TruncarTexto(detalle.Descripcion, 60))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(45).AlignCenter().Text(PdfFormatHelper.FormatCantidad(detalle.Cantidad))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(30).AlignCenter().Text(detalle.UnidadMedida?.Codigo ?? "UN")
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(65).AlignRight().Text(PdfFormatHelper.FormatMoneda(detalle.PrecioUnitario, Documento.Moneda))
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(55).AlignRight().Text(detalle.MontoDescuento > 0 ? PdfFormatHelper.FormatMoneda(detalle.MontoDescuento, Documento.Moneda) : "-")
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(55).AlignRight().Text(detalle.MontoImpuesto > 0 ? PdfFormatHelper.FormatMoneda(detalle.MontoImpuesto, Documento.Moneda) : "-")
                        .FontSize(TinyFontSize).FontColor(TextColor);

                    row.ConstantItem(70).AlignRight().Text(PdfFormatHelper.FormatMoneda(detalle.MontoTotalLinea, Documento.Moneda))
                        .FontSize(TinyFontSize).Bold().FontColor(TextColor);
                });

                isAlternate = !isAlternate;
            }
        });
    }

    /// <summary>
    /// Compose the references section (for NC, ND)
    /// </summary>
    protected void ComposeReferencias(IContainer container)
    {
        var referencias = Documento.Referencias?.ToList();
        if (referencias == null || !referencias.Any())
            return;

        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(SecondaryColor).Padding(5)
                .Text("DOCUMENTOS DE REFERENCIA").FontSize(SmallFontSize).Bold().FontColor(Colors.White);

            foreach (var referencia in referencias)
            {
                col.Item().Padding(5).Row(row =>
                {
                    row.RelativeItem().Column(refCol =>
                    {
                        refCol.Item().Text($"Tipo: {referencia.TipoDocumentoReferenciado} - No: {referencia.NumeroDocumentoReferenciado}")
                            .FontSize(SmallFontSize).FontColor(TextColor);

                        refCol.Item().Text($"Fecha: {PdfFormatHelper.FormatFecha(referencia.FechaEmisionDocumentoReferenciado)} - Código: {PdfFormatHelper.GetTipoReferencia(referencia.CodigoReferencia)}")
                            .FontSize(TinyFontSize).FontColor(LightTextColor);

                        refCol.Item().Text($"Razón: {referencia.RazonReferencia}")
                            .FontSize(TinyFontSize).FontColor(LightTextColor);
                    });
                });
            }
        });
    }

    /// <summary>
    /// Compose the other charges section
    /// </summary>
    protected void ComposeOtrosCargos(IContainer container)
    {
        var otrosCargos = Documento.OtrosCargos?.Where(o => !o.IsDeleted).ToList();
        if (otrosCargos == null || !otrosCargos.Any())
            return;

        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(SecondaryColor).Padding(5)
                .Text("OTROS CARGOS").FontSize(SmallFontSize).Bold().FontColor(Colors.White);

            foreach (var cargo in otrosCargos)
            {
                col.Item().Padding(5).Row(row =>
                {
                    row.RelativeItem(2).Text($"{PdfFormatHelper.GetTipoOtroCargo(cargo.TipoDocumento)}: {cargo.Detalle ?? ""}")
                        .FontSize(SmallFontSize).FontColor(TextColor);

                    row.RelativeItem().AlignRight().Text(PdfFormatHelper.FormatMoneda(cargo.Monto, Documento.Moneda))
                        .FontSize(SmallFontSize).Bold().FontColor(TextColor);
                });
            }
        });
    }

    /// <summary>
    /// Compose the totals section
    /// </summary>
    protected void ComposeTotals(IContainer container)
    {
        container.Border(1).BorderColor(PrimaryColor).Column(col =>
        {
            col.Item().Background(PrimaryColor).Padding(5)
                .Text("RESUMEN").FontSize(NormalFontSize).Bold().FontColor(Colors.White);

            col.Item().Padding(8).Column(content =>
            {
                // Gravado/Exento/Exonerado breakdown
                if (Documento.TotalGravado > 0)
                {
                    ComposeTotalRow(content, "Total Gravado:", Documento.TotalGravado);
                }

                if (Documento.TotalExento > 0)
                {
                    ComposeTotalRow(content, "Total Exento:", Documento.TotalExento);
                }

                if (Documento.TotalExonerado > 0)
                {
                    ComposeTotalRow(content, "Total Exonerado:", Documento.TotalExonerado);
                }

                // Subtotal (TotalVentaNeta = after discounts, before taxes)
                var subtotalAfterDiscounts = Documento.TotalGravado + Documento.TotalExento + Documento.TotalExonerado - Documento.TotalDescuentos;
                ComposeTotalRow(content, "Subtotal:", subtotalAfterDiscounts);

                // Discounts
                if (Documento.TotalDescuentos > 0)
                {
                    ComposeTotalRow(content, "Total Descuentos:", -Documento.TotalDescuentos, true);
                }

                // Taxes
                if (Documento.TotalImpuestos > 0)
                {
                    ComposeTotalRow(content, "Total Impuestos:", Documento.TotalImpuestos);
                }

                // IVA Devuelto
                if (Documento.IVADevuelto.HasValue && Documento.IVADevuelto > 0)
                {
                    ComposeTotalRow(content, "IVA Devuelto:", -Documento.IVADevuelto.Value, true);
                }

                // Other charges
                if (Documento.TotalOtrosCargos > 0)
                {
                    ComposeTotalRow(content, "Total Otros Cargos:", Documento.TotalOtrosCargos);
                }

                // Separator
                content.Item().Height(5);
                content.Item().BorderBottom(2).BorderColor(PrimaryColor);
                content.Item().Height(5);

                // Grand total per Hacienda v4.4 spec:
                // TotalVentaNeta = TotalGravado + TotalExento + TotalExonerado - TotalDescuentos
                // TotalComprobante = TotalVentaNeta + TotalImpuestos + TotalOtrosCargos - IVADevuelto
                var totalVentaNeta = Documento.TotalGravado + Documento.TotalExento + Documento.TotalExonerado - Documento.TotalDescuentos;
                var totalComprobante = totalVentaNeta + Documento.TotalImpuestos + Documento.TotalOtrosCargos - (Documento.IVADevuelto ?? 0);

                content.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("TOTAL COMPROBANTE:")
                        .FontSize(SubtitleFontSize).Bold().FontColor(PrimaryColor);
                    row.ConstantItem(10);
                    row.ConstantItem(100).AlignRight().Text(PdfFormatHelper.FormatMoneda(totalComprobante, Documento.Moneda))
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

            var displayAmount = isNegative ? $"({PdfFormatHelper.FormatMoneda(Math.Abs(amount), Documento.Moneda)})" : PdfFormatHelper.FormatMoneda(amount, Documento.Moneda);
            var color = isNegative ? "#dc3545" : TextColor;

            row.ConstantItem(100).AlignRight().Text(displayAmount)
                .FontSize(SmallFontSize).FontColor(Color.FromHex(color));
        });
    }

    /// <summary>
    /// Compose the Hacienda status section
    /// </summary>
    protected void ComposeEstadoHacienda(IContainer container)
    {
        var (estadoNombre, estadoColor) = PdfFormatHelper.GetEstado(Documento.Estado);

        container.Border(1).BorderColor(BorderColor).Padding(8).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Estado Hacienda:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                col.Item().Text(estadoNombre).FontSize(NormalFontSize).Bold().FontColor(estadoColor);

                if (!string.IsNullOrWhiteSpace(Documento.MensajeHacienda))
                {
                    col.Item().Text(PdfFormatHelper.TruncarTexto(Documento.MensajeHacienda, 80))
                        .FontSize(TinyFontSize).FontColor(LightTextColor);
                }
            });

            if (Documento.FechaRespuestaHacienda.HasValue)
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Fecha Respuesta:").FontSize(SmallFontSize).Bold().FontColor(LightTextColor);
                    col.Item().Text(PdfFormatHelper.FormatFechaHora(Documento.FechaRespuestaHacienda.Value))
                        .FontSize(NormalFontSize).FontColor(TextColor);
                });
            }
        });
    }

    /// <summary>
    /// Compose the observations section
    /// </summary>
    protected void ComposeObservaciones(IContainer container)
    {
        if (string.IsNullOrWhiteSpace(Documento.Observaciones))
            return;

        container.Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(HeaderBgColor).Padding(5)
                .Text("OBSERVACIONES").FontSize(SmallFontSize).Bold().FontColor(TextColor);

            col.Item().Padding(8).Text(Documento.Observaciones)
                .FontSize(SmallFontSize).FontColor(TextColor);
        });
    }

    /// <summary>
    /// Compose the footer with page numbers and authorization
    /// </summary>
    protected void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Column(col =>
        {
            col.Item().BorderTop(1).BorderColor(BorderColor).PaddingTop(5);

            // Authorization text
            col.Item().PaddingBottom(3).AlignCenter().Text("Autorizada mediante resolución Nº MH-DGT-RES-0027-2024 del 13 de noviembre del 2024")
                .FontSize(TinyFontSize).Bold().FontColor(TextColor);

            col.Item().Row(row =>
            {
                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("Generado: ").FontSize(TinyFontSize).FontColor(LightTextColor);
                    text.Span(PdfFormatHelper.FormatFechaHora(DateTime.Now)).FontSize(TinyFontSize).FontColor(LightTextColor);
                });

                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.Span("Documento electrónico - Hacienda v4.4").FontSize(TinyFontSize).FontColor(LightTextColor);
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

    #endregion
}
