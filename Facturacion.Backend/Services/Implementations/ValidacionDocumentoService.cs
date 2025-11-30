using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Entities.Catalogos;
using Facturacion.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Services.Implementations;

/// <summary>
/// Implementación del servicio de validación de documentos según Hacienda v4.4
/// </summary>
public class ValidacionDocumentoService : IValidacionDocumentoService
{
    private readonly DataContext _context;
    private const decimal TOLERANCIA_DECIMAL = 0.01m;

    public ValidacionDocumentoService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Valida que los totales del documento sean correctos
    /// </summary>
    public async Task<ValidacionResultado> ValidarTotalesAsync(Documento documento)
    {
        var resultado = new ValidacionResultado { EsValido = true };

        // Cargar detalles si no están cargados
        if (!documento.Detalles.Any())
        {
            documento.Detalles = await _context.DocumentoDetalles
                .Where(d => d.DocumentoId == documento.Id)
                .Include(d => d.Impuestos)
                .Include(d => d.Descuentos)
                .ToListAsync();
        }

        // Validar cada línea de detalle
        foreach (var detalle in documento.Detalles)
        {
            var validacionLinea = ValidarLineaDetalle(detalle);
            resultado.Combinar(validacionLinea);
        }

        // Calcular totales esperados
        decimal totalServiciosGravados = 0;
        decimal totalServiciosExentos = 0;
        decimal totalServiciosExonerados = 0;
        decimal totalMercanciasGravadas = 0;
        decimal totalMercanciasExentas = 0;
        decimal totalMercanciasExoneradas = 0;
        decimal totalDescuentos = 0;
        decimal totalImpuestos = 0;

        foreach (var detalle in documento.Detalles)
        {
            var montoNeto = detalle.MontoTotal - detalle.MontoDescuento;
            var impuestoLinea = detalle.Impuestos?.Sum(i => i.MontoImpuesto) ?? 0;

            // Determinar si es servicio basándose en el producto asociado o código CAByS
            // Por simplicidad, si tiene impuesto > 0, es gravado; si tiene exoneración, es exonerado; caso contrario es exento
            var tieneExoneracion = detalle.Impuestos?.Any(i => i.TieneExoneracion) ?? false;

            // Clasificar el monto según el tipo (mercancías por defecto)
            if (tieneExoneracion)
            {
                totalMercanciasExoneradas += montoNeto;
            }
            else if (impuestoLinea > 0)
            {
                totalMercanciasGravadas += montoNeto;
            }
            else
            {
                totalMercanciasExentas += montoNeto;
            }

            totalDescuentos += detalle.MontoDescuento;
            totalImpuestos += impuestoLinea;
        }

        // Validar totales de servicios
        if (Math.Abs(documento.TotalServiciosGravados - totalServiciosGravados) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Servicios Gravados incorrecto. Esperado: {totalServiciosGravados:N2}, Informado: {documento.TotalServiciosGravados:N2}");

        if (Math.Abs(documento.TotalServiciosExentos - totalServiciosExentos) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Servicios Exentos incorrecto. Esperado: {totalServiciosExentos:N2}, Informado: {documento.TotalServiciosExentos:N2}");

        if (Math.Abs(documento.TotalServiciosExonerados - totalServiciosExonerados) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Servicios Exonerados incorrecto. Esperado: {totalServiciosExonerados:N2}, Informado: {documento.TotalServiciosExonerados:N2}");

        // Validar totales de mercancías
        if (Math.Abs(documento.TotalMercanciasGravadas - totalMercanciasGravadas) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Mercancías Gravadas incorrecto. Esperado: {totalMercanciasGravadas:N2}, Informado: {documento.TotalMercanciasGravadas:N2}");

        if (Math.Abs(documento.TotalMercanciasExentas - totalMercanciasExentas) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Mercancías Exentas incorrecto. Esperado: {totalMercanciasExentas:N2}, Informado: {documento.TotalMercanciasExentas:N2}");

        if (Math.Abs(documento.TotalMercanciasExoneradas - totalMercanciasExoneradas) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Mercancías Exoneradas incorrecto. Esperado: {totalMercanciasExoneradas:N2}, Informado: {documento.TotalMercanciasExoneradas:N2}");

        // Validar totales generales
        var totalGravadoEsperado = totalServiciosGravados + totalMercanciasGravadas;
        if (Math.Abs(documento.TotalGravado - totalGravadoEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Gravado incorrecto. Esperado: {totalGravadoEsperado:N2}, Informado: {documento.TotalGravado:N2}");

        var totalExentoEsperado = totalServiciosExentos + totalMercanciasExentas;
        if (Math.Abs(documento.TotalExento - totalExentoEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Exento incorrecto. Esperado: {totalExentoEsperado:N2}, Informado: {documento.TotalExento:N2}");

        var totalExoneradoEsperado = totalServiciosExonerados + totalMercanciasExoneradas;
        if (Math.Abs(documento.TotalExonerado - totalExoneradoEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Exonerado incorrecto. Esperado: {totalExoneradoEsperado:N2}, Informado: {documento.TotalExonerado:N2}");

        // Validar descuentos e impuestos
        if (Math.Abs(documento.TotalDescuentos - totalDescuentos) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Descuentos incorrecto. Esperado: {totalDescuentos:N2}, Informado: {documento.TotalDescuentos:N2}");

        if (Math.Abs(documento.TotalImpuestos - totalImpuestos) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Impuestos incorrecto. Esperado: {totalImpuestos:N2}, Informado: {documento.TotalImpuestos:N2}");

        // Validar subtotal y total de venta
        var subtotalEsperado = totalGravadoEsperado + totalExentoEsperado + totalExoneradoEsperado;
        if (Math.Abs(documento.Subtotal - subtotalEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Subtotal incorrecto. Esperado: {subtotalEsperado:N2}, Informado: {documento.Subtotal:N2}");

        var totalVentaEsperado = subtotalEsperado + totalImpuestos + documento.TotalOtrosCargos - (documento.IVADevuelto ?? 0);
        if (Math.Abs(documento.TotalVenta - totalVentaEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Total Venta incorrecto. Esperado: {totalVentaEsperado:N2}, Informado: {documento.TotalVenta:N2}");

        return resultado;
    }

    /// <summary>
    /// Valida que una línea de detalle tenga cálculos correctos
    /// </summary>
    public ValidacionResultado ValidarLineaDetalle(DocumentoDetalle detalle)
    {
        var resultado = new ValidacionResultado { EsValido = true };

        // Validar cantidad positiva
        if (detalle.Cantidad <= 0)
            resultado.AgregarError($"Línea {detalle.NumeroLinea}: La cantidad debe ser mayor a cero");

        // Validar precio unitario positivo
        if (detalle.PrecioUnitario < 0)
            resultado.AgregarError($"Línea {detalle.NumeroLinea}: El precio unitario no puede ser negativo");

        // Validar cálculo de monto total (Cantidad * PrecioUnitario)
        var montoTotalEsperado = detalle.Cantidad * detalle.PrecioUnitario;
        if (Math.Abs(detalle.MontoTotal - montoTotalEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Línea {detalle.NumeroLinea}: Monto total incorrecto. Esperado: {montoTotalEsperado:N5}, Informado: {detalle.MontoTotal:N5}");

        // Validar que el descuento no exceda el monto total
        if (detalle.MontoDescuento > detalle.MontoTotal)
            resultado.AgregarError($"Línea {detalle.NumeroLinea}: El descuento no puede ser mayor al monto total");

        // Validar subtotal (MontoTotal - Descuento)
        var subtotalEsperado = detalle.MontoTotal - detalle.MontoDescuento;
        if (Math.Abs(detalle.Subtotal - subtotalEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Línea {detalle.NumeroLinea}: Subtotal incorrecto. Esperado: {subtotalEsperado:N5}, Informado: {detalle.Subtotal:N5}");

        // Validar impuestos
        if (detalle.Impuestos != null)
        {
            foreach (var impuesto in detalle.Impuestos)
            {
                // El monto del impuesto = MontoBase * (Tarifa/100)
                var montoImpuestoEsperado = impuesto.MontoBase * (impuesto.Tarifa / 100);
                if (Math.Abs(impuesto.MontoImpuesto - montoImpuestoEsperado) > TOLERANCIA_DECIMAL)
                    resultado.AgregarError($"Línea {detalle.NumeroLinea}: Monto de impuesto incorrecto. Esperado: {montoImpuestoEsperado:N5}, Informado: {impuesto.MontoImpuesto:N5}");
            }
        }

        // Validar monto total de línea (Subtotal + Impuestos)
        var impuestoTotal = detalle.Impuestos?.Sum(i => i.MontoImpuesto) ?? 0;
        var montoTotalLineaEsperado = detalle.Subtotal + impuestoTotal;
        if (Math.Abs(detalle.MontoTotalLinea - montoTotalLineaEsperado) > TOLERANCIA_DECIMAL)
            resultado.AgregarError($"Línea {detalle.NumeroLinea}: Monto total de línea incorrecto. Esperado: {montoTotalLineaEsperado:N5}, Informado: {detalle.MontoTotalLinea:N5}");

        return resultado;
    }

    /// <summary>
    /// Valida que los campos obligatorios estén presentes según el tipo de documento
    /// </summary>
    public ValidacionResultado ValidarCamposObligatorios(Documento documento)
    {
        var resultado = new ValidacionResultado { EsValido = true };

        // Campos obligatorios para todos los documentos
        if (string.IsNullOrWhiteSpace(documento.Clave))
            resultado.AgregarError("La clave numérica es obligatoria");

        if (documento.Clave?.Length != 50)
            resultado.AgregarError("La clave numérica debe tener exactamente 50 dígitos");

        if (string.IsNullOrWhiteSpace(documento.NumeroConsecutivo))
            resultado.AgregarError("El número consecutivo es obligatorio");

        if (string.IsNullOrWhiteSpace(documento.ActividadEconomica))
            resultado.AgregarError("La actividad económica es obligatoria");

        if (string.IsNullOrWhiteSpace(documento.CondicionVenta))
            resultado.AgregarError("La condición de venta es obligatoria");

        if (string.IsNullOrWhiteSpace(documento.MedioPago))
            resultado.AgregarError("El medio de pago es obligatorio");

        // Validar plazo de crédito si la condición es "Crédito" (02)
        if (documento.CondicionVenta == "02" && !documento.PlazoCreditoDias.HasValue)
            resultado.AgregarError("El plazo de crédito es obligatorio cuando la condición de venta es 'Crédito'");

        // Validar tipo de cambio si la moneda no es CRC
        if (documento.Moneda != TipoMoneda.CRC && (!documento.TipoCambio.HasValue || documento.TipoCambio <= 0))
            resultado.AgregarError("El tipo de cambio es obligatorio cuando la moneda no es Colones");

        // Validaciones específicas por tipo de documento
        switch (documento.TipoDocumento)
        {
            case DocumentoTipo.FacturaElectronica:
                ValidarFacturaElectronica(documento, resultado);
                break;

            case DocumentoTipo.TiqueteElectronico:
                // El receptor es opcional en TE
                break;

            case DocumentoTipo.NotaCreditoElectronica:
            case DocumentoTipo.NotaDebitoElectronica:
                ValidarNotaCreditoDebito(documento, resultado);
                break;

            case DocumentoTipo.FacturaElectronicaCompra:
                ValidarFacturaElectronicaCompra(documento, resultado);
                break;

            case DocumentoTipo.FacturaElectronicaExportacion:
                ValidarFacturaElectronicaExportacion(documento, resultado);
                break;

            case DocumentoTipo.ReciboElectronicoPago:
                ValidarReciboElectronicoPago(documento, resultado);
                break;
        }

        return resultado;
    }

    private void ValidarFacturaElectronica(Documento documento, ValidacionResultado resultado)
    {
        // El receptor es obligatorio en FE
        if (string.IsNullOrWhiteSpace(documento.ReceptorNumeroIdentificacion))
            resultado.AgregarError("La identificación del receptor es obligatoria en Factura Electrónica");

        if (string.IsNullOrWhiteSpace(documento.ReceptorNombre))
            resultado.AgregarError("El nombre del receptor es obligatorio en Factura Electrónica");

        // Ubicación del receptor es obligatoria (excepto Extranjero)
        if (documento.ReceptorTipoIdentificacion != TipoIdentificacion.Extranjera)
        {
            if (!documento.ReceptorProvincia.HasValue || documento.ReceptorProvincia <= 0)
                resultado.AgregarError("La ubicación del receptor es obligatoria en Factura Electrónica");
        }
    }

    private void ValidarNotaCreditoDebito(Documento documento, ValidacionResultado resultado)
    {
        // La información de referencia es obligatoria
        var validacionRef = ValidarInformacionReferencia(documento);
        resultado.Combinar(validacionRef);

        // El tipo de cambio es obligatorio en NC/ND en v4.4
        if (documento.Moneda != TipoMoneda.CRC && !documento.TipoCambio.HasValue)
            resultado.AgregarError("El tipo de cambio es obligatorio en Notas de Crédito/Débito");
    }

    private void ValidarFacturaElectronicaCompra(Documento documento, ValidacionResultado resultado)
    {
        // La actividad económica del receptor es obligatoria en FEC
        if (string.IsNullOrWhiteSpace(documento.ReceptorActividadEconomica))
            resultado.AgregarError("La actividad económica del receptor es obligatoria en Factura Electrónica de Compra");
    }

    private void ValidarFacturaElectronicaExportacion(Documento documento, ValidacionResultado resultado)
    {
        // La identificación del receptor es obligatoria en FEE (cambio v4.4)
        if (string.IsNullOrWhiteSpace(documento.ReceptorNumeroIdentificacion))
            resultado.AgregarError("La identificación del receptor es obligatoria en Factura Electrónica de Exportación");

        // No debe tener exoneraciones
        if (documento.Detalles.Any(d => d.Impuestos != null && d.Impuestos.Any(i => i.TieneExoneracion)))
            resultado.AgregarError("Las exoneraciones no son permitidas en Factura Electrónica de Exportación");
    }

    private void ValidarReciboElectronicoPago(Documento documento, ValidacionResultado resultado)
    {
        // El receptor es obligatorio en REP
        if (string.IsNullOrWhiteSpace(documento.ReceptorNumeroIdentificacion))
            resultado.AgregarError("La identificación del receptor es obligatoria en Recibo Electrónico de Pago");

        // La información de referencia es obligatoria (factura que se está pagando)
        var validacionRef = ValidarInformacionReferencia(documento);
        resultado.Combinar(validacionRef);

        // REP no debe tener líneas de detalle
        if (documento.Detalles.Any())
            resultado.AgregarError("El Recibo Electrónico de Pago no debe tener líneas de detalle");
    }

    /// <summary>
    /// Valida el formato de la cédula según el tipo de identificación
    /// </summary>
    public ValidacionResultado ValidarIdentificacion(TipoIdentificacion tipoIdentificacion, string numero)
    {
        var resultado = new ValidacionResultado { EsValido = true };

        if (string.IsNullOrWhiteSpace(numero))
        {
            resultado.AgregarError("El número de identificación es obligatorio");
            return resultado;
        }

        // Limpiar el número (quitar espacios y guiones)
        numero = numero.Replace("-", "").Replace(" ", "").Trim();

        switch (tipoIdentificacion)
        {
            case TipoIdentificacion.Fisica:
                if (!numero.All(char.IsDigit) || numero.Length != 9)
                    resultado.AgregarError("La cédula física debe tener exactamente 9 dígitos");
                break;

            case TipoIdentificacion.Juridica:
                if (!numero.All(char.IsDigit) || numero.Length != 10)
                    resultado.AgregarError("La cédula jurídica debe tener exactamente 10 dígitos");
                break;

            case TipoIdentificacion.DIMEX:
                if (!numero.All(char.IsDigit) || numero.Length < 11 || numero.Length > 12)
                    resultado.AgregarError("El DIMEX debe tener entre 11 y 12 dígitos");
                break;

            case TipoIdentificacion.NITE:
                if (!numero.All(char.IsDigit) || numero.Length != 10)
                    resultado.AgregarError("El NITE debe tener exactamente 10 dígitos");
                break;

            case TipoIdentificacion.Pasaporte:
            case TipoIdentificacion.Extranjera:
                // Puede tener hasta 20 caracteres
                if (numero.Length > 20)
                    resultado.AgregarError("La identificación de extranjero no puede exceder 20 caracteres");
                break;
        }

        return resultado;
    }

    /// <summary>
    /// Valida que el documento cumpla con todas las reglas de Hacienda v4.4
    /// </summary>
    public async Task<ValidacionResultado> ValidarDocumentoCompletoAsync(Documento documento)
    {
        var resultado = new ValidacionResultado { EsValido = true };

        // 1. Validar campos obligatorios
        var camposObligatorios = ValidarCamposObligatorios(documento);
        resultado.Combinar(camposObligatorios);

        // 2. Validar totales
        var totales = await ValidarTotalesAsync(documento);
        resultado.Combinar(totales);

        // 3. Validar identificación del receptor (si aplica)
        if (!string.IsNullOrEmpty(documento.ReceptorNumeroIdentificacion) && documento.ReceptorTipoIdentificacion.HasValue)
        {
            var identificacion = ValidarIdentificacion(documento.ReceptorTipoIdentificacion.Value, documento.ReceptorNumeroIdentificacion);
            resultado.Combinar(identificacion);
        }

        // 4. Validar códigos CAByS en líneas (si no es REP)
        if (documento.TipoDocumento != DocumentoTipo.ReciboElectronicoPago)
        {
            foreach (var detalle in documento.Detalles)
            {
                if (!string.IsNullOrEmpty(detalle.CodigoCabys))
                {
                    var cabys = await ValidarCodigoCABySAsync(detalle.CodigoCabys);
                    if (!cabys.EsValido)
                    {
                        resultado.AgregarError($"Línea {detalle.NumeroLinea}: {string.Join(", ", cabys.Errores)}");
                    }
                }
            }
        }

        // 5. Validar fecha de emisión
        if (documento.FechaEmision > DateTime.Now.AddMinutes(5))
            resultado.AgregarError("La fecha de emisión no puede ser futura");

        // 6. Validar que tenga al menos una línea de detalle (excepto REP)
        if (documento.TipoDocumento != DocumentoTipo.ReciboElectronicoPago && !documento.Detalles.Any())
            resultado.AgregarError("El documento debe tener al menos una línea de detalle");

        return resultado;
    }

    /// <summary>
    /// Valida los campos específicos de información de referencia para NC/ND/REP
    /// </summary>
    public ValidacionResultado ValidarInformacionReferencia(Documento documento)
    {
        var resultado = new ValidacionResultado { EsValido = true };

        if (!documento.Referencias.Any())
        {
            resultado.AgregarError("La información de referencia es obligatoria para este tipo de documento");
            return resultado;
        }

        foreach (var referencia in documento.Referencias)
        {
            if (string.IsNullOrWhiteSpace(referencia.TipoDocumentoReferenciado))
                resultado.AgregarError("El tipo de documento de referencia es obligatorio");

            if (string.IsNullOrWhiteSpace(referencia.NumeroDocumentoReferenciado))
                resultado.AgregarError("El número de referencia es obligatorio");

            if (referencia.FechaEmisionDocumentoReferenciado == default)
                resultado.AgregarError("La fecha de emisión del documento de referencia es obligatoria");

            // CodigoReferencia es un enum, siempre tiene valor
            // Solo validar que no sea el valor por defecto si es necesario

            if (string.IsNullOrWhiteSpace(referencia.RazonReferencia))
                resultado.AgregarError("La razón de la referencia es obligatoria");
        }

        return resultado;
    }

    /// <summary>
    /// Valida que el código CAByS sea válido
    /// </summary>
    public async Task<ValidacionResultado> ValidarCodigoCABySAsync(string codigoCAByS)
    {
        var resultado = new ValidacionResultado { EsValido = true };

        if (string.IsNullOrWhiteSpace(codigoCAByS))
        {
            resultado.AgregarError("El código CAByS es obligatorio");
            return resultado;
        }

        // Validar formato (13 dígitos)
        if (codigoCAByS.Length != 13 || !codigoCAByS.All(char.IsDigit))
        {
            resultado.AgregarError("El código CAByS debe tener exactamente 13 dígitos");
            return resultado;
        }

        // Validar que exista en el catálogo
        var existe = await _context.Set<CAByS>()
            .AnyAsync(c => c.Codigo == codigoCAByS && c.Activo);

        if (!existe)
        {
            resultado.AgregarAdvertencia($"El código CAByS {codigoCAByS} no se encontró en el catálogo. Verifique que sea correcto.");
        }

        return resultado;
    }
}
