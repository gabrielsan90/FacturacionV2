namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para estadísticas de cobranza
/// </summary>
public class EstadisticasCobranzaDTO
{
    public int TotalDocumentosPendientes { get; set; }
    public decimal TotalPorCobrar { get; set; }
    public int DocumentosVencidos { get; set; }
    public decimal MontoVencido { get; set; }
    public int DocumentosAlDia { get; set; }
    public decimal MontoAlDia { get; set; }
    public decimal PromedioVencimiento { get; set; }  // En días
    public List<CobranzaPorClienteDTO> TopClientesDeudores { get; set; } = new List<CobranzaPorClienteDTO>();
}

/// <summary>
/// DTO para cobranza por cliente
/// </summary>
public class CobranzaPorClienteDTO
{
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = null!;
    public string? ClienteIdentificacion { get; set; }
    public int CantidadDocumentos { get; set; }
    public decimal TotalPendiente { get; set; }
    public int DiasPromedioVencido { get; set; }
}
