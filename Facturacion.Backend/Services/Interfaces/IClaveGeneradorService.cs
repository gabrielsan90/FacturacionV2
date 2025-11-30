using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Services.Interfaces;

/// <summary>
/// Servicio para generar la Clave numérica de 50 dígitos según formato Hacienda v4.4
/// </summary>
public interface IClaveGeneradorService
{
    /// <summary>
    /// Genera la clave numérica de 50 dígitos para un documento
    /// </summary>
    /// <param name="documento">Documento para el cual generar la clave</param>
    /// <param name="situacion">Situación del documento: 1=Normal, 2=Contingencia, 3=Sin internet</param>
    /// <returns>Clave de 50 dígitos</returns>
    Task<string> GenerarClaveAsync(Documento documento, int situacion = 1);

    /// <summary>
    /// Valida que una clave tenga el formato correcto
    /// </summary>
    /// <param name="clave">Clave a validar</param>
    /// <returns>True si la clave es válida</returns>
    bool ValidarClave(string clave);

    /// <summary>
    /// Genera el código de seguridad de 8 dígitos aleatorios
    /// </summary>
    /// <returns>Código de seguridad de 8 dígitos</returns>
    string GenerarCodigoSeguridad();
}
