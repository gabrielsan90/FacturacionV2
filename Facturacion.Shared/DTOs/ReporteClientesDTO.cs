namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO para reporte de actividad de clientes
/// </summary>
public class ReporteClientesDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public List<ReporteClientesDetalleDTO> Detalles { get; set; } = new List<ReporteClientesDetalleDTO>();
}
