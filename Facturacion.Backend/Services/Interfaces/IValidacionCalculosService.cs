using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para validar cálculos de documentos antes de enviarlos a Hacienda
/// NUEVO v4.4 - M8: Validación de cálculos previo al envío
/// </summary>
public interface IValidacionCalculosService
{
    /// <summary>
    /// Valida todos los cálculos de un documento
    /// </summary>
    /// <param name="documento">Documento a validar</param>
    /// <returns>Resultado de la validación con lista de errores si los hay</returns>
    Task<ValidacionCalculosResultado> ValidarDocumentoAsync(Documento documento);

    /// <summary>
    /// Valida los cálculos de una línea de detalle específica
    /// </summary>
    /// <param name="detalle">Línea de detalle a validar</param>
    /// <returns>Resultado de la validación con lista de errores si los hay</returns>
    ValidacionCalculosResultado ValidarLineaDetalle(DocumentoDetalle detalle);
}

/// <summary>
/// Resultado de la validación de cálculos
/// </summary>
public class ValidacionCalculosResultado
{
    public bool EsValido { get; set; }
    public List<string> Errores { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();

    public void AgregarError(string error)
    {
        EsValido = false;
        Errores.Add(error);
    }

    public void AgregarAdvertencia(string advertencia)
    {
        Advertencias.Add(advertencia);
    }
}
