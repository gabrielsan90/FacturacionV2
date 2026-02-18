namespace Facturacion.Shared.DTOs;

/// <summary>
/// Resultado de la lectura de correos electrónicos con XML adjuntos
/// </summary>
public class ResultadoLecturaCorreo
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = null!;
    public int TotalEmailsLeidos { get; set; }
    public int TotalXmlEncontrados { get; set; }
    public int TotalProcesados { get; set; }
    public int TotalErrores { get; set; }
    public List<ResultadoRecepcion> Resultados { get; set; } = new();
}
