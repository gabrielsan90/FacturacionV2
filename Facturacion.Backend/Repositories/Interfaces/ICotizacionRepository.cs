using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestión de Cotizaciones
/// </summary>
public interface ICotizacionRepository
{
    /// <summary>
    /// Obtiene una cotización por su ID
    /// </summary>
    Task<ActionResponse<Cotizacion>> GetAsync(Guid id);

    /// <summary>
    /// Obtiene una cotización con sus detalles por su ID
    /// </summary>
    Task<ActionResponse<Cotizacion>> GetWithDetallesAsync(Guid id);

    /// <summary>
    /// Obtiene todas las cotizaciones de una empresa
    /// </summary>
    Task<ActionResponse<IEnumerable<Cotizacion>>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene todas las cotizaciones de un cliente
    /// </summary>
    Task<ActionResponse<IEnumerable<Cotizacion>>> GetByClienteAsync(Guid clienteId);

    /// <summary>
    /// Obtiene cotizaciones por estado
    /// </summary>
    Task<ActionResponse<IEnumerable<Cotizacion>>> GetByEstadoAsync(Guid empresaId, EstadoCotizacion estado);

    /// <summary>
    /// Obtiene cotizaciones vencidas (fecha de vencimiento superada y no aprobadas)
    /// </summary>
    Task<ActionResponse<IEnumerable<Cotizacion>>> GetVencidasAsync(Guid empresaId);

    /// <summary>
    /// Crea una nueva cotización
    /// </summary>
    Task<ActionResponse<Cotizacion>> AddAsync(Cotizacion cotizacion);

    /// <summary>
    /// Actualiza una cotización
    /// </summary>
    Task<ActionResponse<Cotizacion>> UpdateAsync(Cotizacion cotizacion);

    /// <summary>
    /// Elimina una cotización (soft delete)
    /// </summary>
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId);

    /// <summary>
    /// Aprueba una cotización
    /// </summary>
    Task<ActionResponse<bool>> AprobarAsync(Guid id, string usuarioId);

    /// <summary>
    /// Convierte una cotización a factura (documento)
    /// </summary>
    Task<ActionResponse<bool>> ConvertirAFacturaAsync(Guid id, Guid documentoId, string usuarioId);
}
