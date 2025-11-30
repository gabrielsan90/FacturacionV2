using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio de validación de documentos electrónicos según Hacienda v4.4
/// Recomendación 8: Validación de totales
/// Recomendación 1 y 2: Validaciones frontend y backend
/// </summary>
public interface IValidacionDocumentoService
{
    /// <summary>
    /// Valida que los totales del documento sean correctos
    /// </summary>
    /// <param name="documento">Documento a validar</param>
    /// <returns>Resultado de la validación con errores si existen</returns>
    Task<ValidacionResultado> ValidarTotalesAsync(Documento documento);

    /// <summary>
    /// Valida que una línea de detalle tenga cálculos correctos
    /// </summary>
    /// <param name="detalle">Línea de detalle a validar</param>
    /// <returns>Resultado de la validación</returns>
    ValidacionResultado ValidarLineaDetalle(DocumentoDetalle detalle);

    /// <summary>
    /// Valida que los campos obligatorios estén presentes según el tipo de documento
    /// </summary>
    /// <param name="documento">Documento a validar</param>
    /// <returns>Resultado de la validación</returns>
    ValidacionResultado ValidarCamposObligatorios(Documento documento);

    /// <summary>
    /// Valida el formato de la cédula según el tipo de identificación
    /// </summary>
    /// <param name="tipoIdentificacion">Tipo de identificación</param>
    /// <param name="numero">Número de identificación</param>
    /// <returns>Resultado de la validación</returns>
    ValidacionResultado ValidarIdentificacion(TipoIdentificacion tipoIdentificacion, string numero);

    /// <summary>
    /// Valida que el documento cumpla con todas las reglas de Hacienda v4.4
    /// </summary>
    /// <param name="documento">Documento a validar</param>
    /// <returns>Resultado de la validación completa</returns>
    Task<ValidacionResultado> ValidarDocumentoCompletoAsync(Documento documento);

    /// <summary>
    /// Valida los campos específicos de información de referencia para NC/ND/REP
    /// </summary>
    /// <param name="documento">Documento a validar</param>
    /// <returns>Resultado de la validación</returns>
    ValidacionResultado ValidarInformacionReferencia(Documento documento);

    /// <summary>
    /// Valida que el código CAByS sea válido (13 dígitos, existe en catálogo)
    /// </summary>
    /// <param name="codigoCAByS">Código a validar</param>
    /// <returns>Resultado de la validación</returns>
    Task<ValidacionResultado> ValidarCodigoCABySAsync(string codigoCAByS);
}

/// <summary>
/// Resultado de una validación
/// </summary>
public class ValidacionResultado
{
    public bool EsValido { get; set; }
    public List<string> Errores { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();

    public static ValidacionResultado Exito() => new() { EsValido = true };

    public static ValidacionResultado Error(string mensaje) => new()
    {
        EsValido = false,
        Errores = new List<string> { mensaje }
    };

    public static ValidacionResultado Error(List<string> mensajes) => new()
    {
        EsValido = false,
        Errores = mensajes
    };

    public void AgregarError(string mensaje)
    {
        EsValido = false;
        Errores.Add(mensaje);
    }

    public void AgregarAdvertencia(string mensaje)
    {
        Advertencias.Add(mensaje);
    }

    public void Combinar(ValidacionResultado otro)
    {
        if (!otro.EsValido)
            EsValido = false;
        Errores.AddRange(otro.Errores);
        Advertencias.AddRange(otro.Advertencias);
    }
}
