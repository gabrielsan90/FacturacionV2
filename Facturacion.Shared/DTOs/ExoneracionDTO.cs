namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para respuesta de validación de exoneración
/// API: https://api.hacienda.go.cr/fe/ex
/// </summary>
public class ExoneracionDTO
{
    /// <summary>
    /// Número de documento de autorización de exoneración
    /// </summary>
    public string NumeroDocumento { get; set; } = null!;

    /// <summary>
    /// Nombre de la institución que emite la exoneración
    /// Ejemplo: Ministerio de Hacienda
    /// </summary>
    public string NombreInstitucion { get; set; } = null!;

    /// <summary>
    /// Fecha de emisión del documento de exoneración
    /// </summary>
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Fecha de vencimiento de la exoneración
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Porcentaje de exoneración
    /// Rango: 0-100
    /// </summary>
    public decimal PorcentajeExoneracion { get; set; }

    /// <summary>
    /// Tipo de documento
    /// 01 = Compras Autorizadas
    /// 02 = Ventas Exentas de IVA
    /// 03 = Otros
    /// </summary>
    public string TipoDocumento { get; set; } = null!;

    /// <summary>
    /// Identificación del beneficiario de la exoneración
    /// </summary>
    public string? IdentificacionBeneficiario { get; set; }

    /// <summary>
    /// Nombre del beneficiario de la exoneración
    /// </summary>
    public string? NombreBeneficiario { get; set; }

    /// <summary>
    /// Indica si la exoneración está vigente
    /// </summary>
    public bool Vigente { get; set; }

    /// <summary>
    /// Razón por la cual no está vigente (si aplica)
    /// </summary>
    public string? RazonNoVigente { get; set; }
}

/// <summary>
/// DTO para validación de exoneración
/// </summary>
public class ExoneracionValidacionDTO
{
    /// <summary>
    /// Número de documento de autorización
    /// </summary>
    public string NumeroDocumento { get; set; } = null!;

    /// <summary>
    /// Nombre de la institución emisora
    /// </summary>
    public string NombreInstitucion { get; set; } = null!;

    /// <summary>
    /// Fecha en la que se desea validar la exoneración
    /// Por defecto es la fecha actual
    /// </summary>
    public DateTime? FechaValidacion { get; set; }
}

/// <summary>
/// DTO para respuesta de validación de exoneración
/// </summary>
public class ExoneracionValidacionRespuestaDTO
{
    /// <summary>
    /// Indica si la exoneración es válida
    /// </summary>
    public bool EsValida { get; set; }

    /// <summary>
    /// Mensaje de validación
    /// </summary>
    public string Mensaje { get; set; } = null!;

    /// <summary>
    /// Datos de la exoneración (si es válida)
    /// </summary>
    public ExoneracionDTO? Exoneracion { get; set; }
}
