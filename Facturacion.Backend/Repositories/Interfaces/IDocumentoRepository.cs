using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;

namespace Facturacion.Backend.Repositories.Interfaces;

public interface IDocumentoRepository
{
    Task<Documento?> GetAsync(Guid id);
    Task<Documento?> GetWithDetallesAsync(Guid id);
    Task<IEnumerable<Documento>> GetByEmpresaAsync(Guid empresaId);
    Task<(IEnumerable<object> Data, int TotalCount)> GetByEmpresaPagedAsync(
        Guid empresaId,
        int skip,
        int take,
        Guid? sucursalId = null,
        Guid? terminalId = null,
        EstadoDocumento? estado = null,
        DocumentoTipo? tipoDocumento = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        Ambiente? ambiente = null,
        bool? esDocumentoRecibido = null);
    Task<IEnumerable<Documento>> GetBySucursalAsync(Guid sucursalId);
    Task<IEnumerable<Documento>> GetByTerminalAsync(Guid terminalId);
    Task<IEnumerable<Documento>> GetByClienteAsync(Guid clienteId);
    Task<Documento?> GetByClaveAsync(string clave);
    Task<Documento?> GetByConsecutivoAsync(Guid empresaId, string numeroConsecutivo);
    Task<IEnumerable<Documento>> GetByFechaRangoAsync(Guid empresaId, DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<Documento>> GetByEstadoAsync(Guid empresaId, EstadoDocumento estado);
    Task<IEnumerable<Documento>> GetPendientesEnvioAsync(Guid empresaId);
    Task<Documento> AddAsync(Documento documento);
    Task UpdateAsync(Documento documento);
    Task DeleteAsync(Guid id, string userId);

    // Métodos para documentos recibidos (compras)
    Task<IEnumerable<Documento>> GetDocumentosRecibidosAsync(Guid empresaId);
    Task<IEnumerable<Documento>> GetDocumentosPendientesMRAsync(Guid empresaId);
    Task<bool> ExisteDocumentoPorClaveAsync(string clave, Guid empresaId);

    // Diagnóstico - incluye documentos eliminados
    Task<IEnumerable<Documento>> GetAllByEmpresaIncludingDeletedAsync(Guid empresaId);
}
