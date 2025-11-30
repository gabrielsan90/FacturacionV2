namespace Facturacion.Shared.DTOs;

/// <summary>
/// DTO con estadísticas de gastos por periodo
/// </summary>
public class EstadisticasGastosDTO
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal TotalPendiente { get; set; }
    public int CantidadGastos { get; set; }
    public int GastosAprobados { get; set; }
    public int GastosPendientesAprobacion { get; set; }

    public List<GastoPorCategoriaDTO> GastosPorCategoria { get; set; } = new();
    public List<GastoPorProveedorDTO> GastosPorProveedor { get; set; } = new();
    public List<GastoPorMesDTO> GastosPorMes { get; set; } = new();
}

public class GastoPorCategoriaDTO
{
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = null!;
    public decimal Total { get; set; }
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

public class GastoPorProveedorDTO
{
    public Guid? ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = null!;
    public decimal Total { get; set; }
    public int Cantidad { get; set; }
}

public class GastoPorMesDTO
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string MesNombre { get; set; } = null!;
    public decimal Total { get; set; }
    public int Cantidad { get; set; }
}
