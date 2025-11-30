namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para respuesta de XML de un documento
/// </summary>
public class DocumentoXMLDTO
{
    public string Xml { get; set; } = null!;
    public string Clave { get; set; } = null!;
}
