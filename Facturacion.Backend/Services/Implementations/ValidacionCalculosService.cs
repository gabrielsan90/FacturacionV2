using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Servicio para validar cálculos de documentos antes de enviarlos a Hacienda
/// NUEVO v4.4 - M8: Validación de cálculos previo al envío
/// </summary>
public class ValidacionCalculosService : IValidacionCalculosService
{
    private readonly DataContext _context;
    private const decimal TOLERANCIA = 0.00001m; // Tolerancia para comparaciones decimales
    private const int MAX_DECIMALES = 5; // Máximo 5 decimales según Hacienda

    public ValidacionCalculosService(DataContext context)
    {
        _context = context;
    }

    public async Task<ValidacionCalculosResultado> ValidarDocumentoAsync(Documento documento)
    {
        var resultado = new ValidacionCalculosResultado { EsValido = true };

        // Cargar detalles si no están cargados
        if (documento.Detalles == null || !documento.Detalles.Any())
        {
            documento.Detalles = await _context.Set<DocumentoDetalle>()
                .Include(d => d.Impuestos)
                .Include(d => d.Descuentos)
                .Where(d => d.DocumentoId == documento.Id)
                .ToListAsync();
        }

        // Validar cada línea de detalle
        foreach (var detalle in documento.Detalles)
        {
            var resultadoLinea = ValidarLineaDetalle(detalle);
            if (!resultadoLinea.EsValido)
            {
                resultado.EsValido = false;
                foreach (var error in resultadoLinea.Errores)
                {
                    resultado.AgregarError($"Línea {detalle.NumeroLinea}: {error}");
                }
            }
            foreach (var advertencia in resultadoLinea.Advertencias)
            {
                resultado.AgregarAdvertencia($"Línea {detalle.NumeroLinea}: {advertencia}");
            }
        }

        // Validar totales del documento
        ValidarTotalesDocumento(documento, resultado);

        return resultado;
    }

    public ValidacionCalculosResultado ValidarLineaDetalle(DocumentoDetalle detalle)
    {
        var resultado = new ValidacionCalculosResultado { EsValido = true };

        // 1. Verificar que SubTotal = Cantidad × PrecioUnitario
        var montoTotalEsperado = detalle.Cantidad * detalle.PrecioUnitario;
        if (Math.Abs(detalle.MontoTotal - montoTotalEsperado) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"MontoTotal incorrecto. Esperado: {montoTotalEsperado:F5}, " +
                $"Actual: {detalle.MontoTotal:F5}. " +
                $"Debe ser Cantidad ({detalle.Cantidad:F3}) × PrecioUnitario ({detalle.PrecioUnitario:F5})");
        }

        // 2. Verificar que Subtotal = MontoTotal - Descuentos
        var descuentoTotal = detalle.Descuentos?.Sum(d => d.MontoDescuento) ?? 0;
        if (Math.Abs(detalle.MontoDescuento - descuentoTotal) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"MontoDescuento incorrecto. Esperado: {descuentoTotal:F5}, " +
                $"Actual: {detalle.MontoDescuento:F5}");
        }

        var subtotalEsperado = detalle.MontoTotal - descuentoTotal;
        if (Math.Abs(detalle.Subtotal - subtotalEsperado) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"Subtotal incorrecto. Esperado: {subtotalEsperado:F5}, " +
                $"Actual: {detalle.Subtotal:F5}. " +
                $"Debe ser MontoTotal ({detalle.MontoTotal:F5}) - Descuentos ({descuentoTotal:F5})");
        }

        // 3. Verificar impuestos
        if (detalle.Impuestos != null && detalle.Impuestos.Any())
        {
            var baseImponible = detalle.BaseImponible ?? detalle.Subtotal;
            decimal impuestoTotalCalculado = 0;

            foreach (var impuesto in detalle.Impuestos)
            {
                // Verificar que MontoImpuesto = BaseImponible × (Tarifa/100)
                var montoImpuestoEsperado = baseImponible * (impuesto.Tarifa / 100);

                // Aplicar FactorIVA si está presente (v4.4)
                if (impuesto.FactorIVA.HasValue && impuesto.FactorIVA.Value > 0)
                {
                    montoImpuestoEsperado *= impuesto.FactorIVA.Value;
                }

                if (Math.Abs(impuesto.MontoImpuesto - montoImpuestoEsperado) > TOLERANCIA)
                {
                    resultado.AgregarError(
                        $"MontoImpuesto incorrecto en impuesto {impuesto.CodigoImpuesto}. " +
                        $"Esperado: {montoImpuestoEsperado:F5}, Actual: {impuesto.MontoImpuesto:F5}. " +
                        $"Debe ser BaseImponible ({baseImponible:F5}) × Tarifa ({impuesto.Tarifa:F2}%) " +
                        $"{(impuesto.FactorIVA.HasValue ? $"× FactorIVA ({impuesto.FactorIVA.Value:F4})" : "")}");
                }

                impuestoTotalCalculado += impuesto.MontoImpuesto;
            }

            // Verificar que el total de impuestos coincida
            if (Math.Abs(detalle.MontoImpuesto - impuestoTotalCalculado) > TOLERANCIA)
            {
                resultado.AgregarError(
                    $"MontoImpuesto total incorrecto. Esperado: {impuestoTotalCalculado:F5}, " +
                    $"Actual: {detalle.MontoImpuesto:F5}");
            }
        }

        // 4. Verificar que TotalLinea = Subtotal - Descuentos + Impuestos
        var totalLineaEsperado = detalle.Subtotal + detalle.MontoImpuesto;
        if (Math.Abs(detalle.MontoTotalLinea - totalLineaEsperado) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"MontoTotalLinea incorrecto. Esperado: {totalLineaEsperado:F5}, " +
                $"Actual: {detalle.MontoTotalLinea:F5}. " +
                $"Debe ser Subtotal ({detalle.Subtotal:F5}) + Impuestos ({detalle.MontoImpuesto:F5})");
        }

        // 5. Verificar máximo 5 decimales en montos
        ValidarMaximoDecimales(detalle, resultado);

        return resultado;
    }

    private void ValidarTotalesDocumento(Documento documento, ValidacionCalculosResultado resultado)
    {
        // Calcular subtotal como suma de todas las líneas
        var subtotalCalculado = documento.Detalles.Sum(d => d.Subtotal);
        if (Math.Abs(documento.Subtotal - subtotalCalculado) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"Subtotal del documento incorrecto. Esperado: {subtotalCalculado:F5}, " +
                $"Actual: {documento.Subtotal:F5}. " +
                $"Debe ser la suma de los Subtotales de todas las líneas");
        }

        // Calcular total de descuentos
        var descuentosCalculados = documento.Detalles.Sum(d => d.MontoDescuento);
        if (Math.Abs(documento.TotalDescuentos - descuentosCalculados) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"TotalDescuentos incorrecto. Esperado: {descuentosCalculados:F5}, " +
                $"Actual: {documento.TotalDescuentos:F5}");
        }

        // Calcular total de impuestos
        var impuestosCalculados = documento.Detalles.Sum(d => d.MontoImpuesto);
        if (Math.Abs(documento.TotalImpuestos - impuestosCalculados) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"TotalImpuestos incorrecto. Esperado: {impuestosCalculados:F5}, " +
                $"Actual: {documento.TotalImpuestos:F5}");
        }

        // Verificar TotalVenta según Hacienda v4.4
        // TotalVenta = TotalGravado + TotalExento + TotalExonerado (usando MontoTotal, ANTES de descuentos)
        var totalVentaEsperado = documento.TotalGravado + documento.TotalExento + documento.TotalExonerado;
        if (Math.Abs(documento.TotalVenta - totalVentaEsperado) > TOLERANCIA)
        {
            resultado.AgregarError(
                $"TotalVenta incorrecto. Esperado: {totalVentaEsperado:F5}, " +
                $"Actual: {documento.TotalVenta:F5}. " +
                $"Debe ser TotalGravado + TotalExento + TotalExonerado");
        }

        // Verificar TotalComprobante según Hacienda v4.4
        // TotalComprobante = TotalVentaNeta + TotalImpuesto + TotalOtrosCargos - IVADevuelto
        var totalVentaNeta = documento.TotalVenta - documento.TotalDescuentos;
        var totalComprobanteEsperado = totalVentaNeta + documento.TotalImpuestos + documento.TotalOtrosCargos - (documento.IVADevuelto ?? 0m);
        var totalComprobanteActual = documento.Detalles.Sum(d => d.MontoTotalLinea) - documento.TotalDescuentos + documento.TotalOtrosCargos;
        // Nota: TotalComprobante no se valida aquí porque se calcula en XmlGeneradorService

        // Validar máximo 5 decimales en totales del documento
        ValidarMaximoDecimalesDocumento(documento, resultado);
    }

    private void ValidarMaximoDecimales(DocumentoDetalle detalle, ValidacionCalculosResultado resultado)
    {
        if (ContarDecimales(detalle.PrecioUnitario) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"PrecioUnitario tiene más de {MAX_DECIMALES} decimales");
        }

        if (ContarDecimales(detalle.MontoTotal) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"MontoTotal tiene más de {MAX_DECIMALES} decimales");
        }

        if (ContarDecimales(detalle.Subtotal) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"Subtotal tiene más de {MAX_DECIMALES} decimales");
        }

        if (ContarDecimales(detalle.MontoImpuesto) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"MontoImpuesto tiene más de {MAX_DECIMALES} decimales");
        }

        if (ContarDecimales(detalle.MontoTotalLinea) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"MontoTotalLinea tiene más de {MAX_DECIMALES} decimales");
        }
    }

    private void ValidarMaximoDecimalesDocumento(Documento documento, ValidacionCalculosResultado resultado)
    {
        if (ContarDecimales(documento.Subtotal) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"Subtotal del documento tiene más de {MAX_DECIMALES} decimales");
        }

        if (ContarDecimales(documento.TotalImpuestos) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"TotalImpuestos tiene más de {MAX_DECIMALES} decimales");
        }

        if (ContarDecimales(documento.TotalVenta) > MAX_DECIMALES)
        {
            resultado.AgregarAdvertencia($"TotalVenta tiene más de {MAX_DECIMALES} decimales");
        }
    }

    private int ContarDecimales(decimal valor)
    {
        var texto = valor.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var partes = texto.Split('.');
        return partes.Length > 1 ? partes[1].TrimEnd('0').Length : 0;
    }
}
