namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para el XML de respuesta de Hacienda (MensajeHacienda)
/// Este XML viene codificado en Base64 dentro del campo "respuesta-xml" de la respuesta JSON
/// </summary>
public class HaciendaMensajeRespuestaXml
{
    /// <summary>
    /// Clave numérica del documento (50 dígitos)
    /// </summary>
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Nombre del emisor del documento
    /// </summary>
    public string NombreEmisor { get; set; } = null!;

    /// <summary>
    /// Tipo de identificación del emisor
    /// 01=Cédula Física, 02=Cédula Jurídica, 03=DIMEX, 04=NITE
    /// </summary>
    public string TipoIdentificacionEmisor { get; set; } = null!;

    /// <summary>
    /// Número de cédula/identificación del emisor
    /// </summary>
    public string NumeroCedulaEmisor { get; set; } = null!;

    /// <summary>
    /// Código del mensaje de Hacienda
    /// 1 = Aceptado
    /// 2 = Aceptado Parcialmente
    /// 3 = Rechazado
    /// </summary>
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Descripción detallada del resultado
    /// Ejemplo: "Documento aceptado correctamente"
    /// </summary>
    public string DetalleMensaje { get; set; } = null!;

    /// <summary>
    /// Monto total de impuestos del documento
    /// </summary>
    public decimal? MontoTotalImpuesto { get; set; }

    /// <summary>
    /// Total general del documento (factura)
    /// </summary>
    public decimal? TotalFactura { get; set; }

    /// <summary>
    /// Indica si el documento fue aceptado (Mensaje == "1")
    /// </summary>
    public bool EsAceptado => Mensaje == "1";

    /// <summary>
    /// Indica si el documento fue aceptado parcialmente (Mensaje == "2")
    /// </summary>
    public bool EsAceptadoParcial => Mensaje == "2";

    /// <summary>
    /// Indica si el documento fue rechazado (Mensaje == "3")
    /// </summary>
    public bool EsRechazado => Mensaje == "3";

    /// <summary>
    /// Retorna el estado en formato texto legible
    /// </summary>
    public string EstadoTexto => Mensaje switch
    {
        "1" => "Aceptado",
        "2" => "Aceptado Parcialmente",
        "3" => "Rechazado",
        _ => "Desconocido"
    };
}
