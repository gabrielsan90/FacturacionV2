namespace Facturacion.Shared.DTOs;

public class ReporteRetencionesDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalRetencionRenta { get; set; }
    public decimal TotalRetencionIVA { get; set; }
    public decimal TotalBaseImponible { get; set; }
    public int CantidadDocumentos { get; set; }
    public int CantidadProveedores { get; set; }
    public List<RetencionDetalleDTO> Detalles { get; set; } = new();
}

public class RetencionDetalleDTO
{
    public string ProveedorNombre { get; set; } = "";
    public string ProveedorIdentificacion { get; set; } = "";
    public string TipoIdentificacion { get; set; } = "";
    public decimal BaseImponible { get; set; }
    public decimal TarifaRetencion { get; set; }
    public decimal MontoRetencion { get; set; }
    public decimal MontoRetencionIVA { get; set; }
    public int CantidadDocumentos { get; set; }
}
