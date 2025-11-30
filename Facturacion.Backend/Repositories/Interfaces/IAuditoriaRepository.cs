using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestión de auditoría del sistema
/// </summary>
public interface IAuditoriaRepository
{
    /// <summary>
    /// Obtiene registros de auditoría por empresa
    /// </summary>
    Task<IEnumerable<Auditoria>> GetByEmpresaAsync(
        Guid empresaId,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? tabla = null,
        string? accion = null,
        int limite = 100);

    /// <summary>
    /// Obtiene registros de auditoría por usuario
    /// </summary>
    Task<IEnumerable<Auditoria>> GetByUsuarioAsync(
        string usuarioId,
        Guid? empresaId = null,
        int limite = 100);

    /// <summary>
    /// Obtiene registros de auditoría de un registro específico
    /// </summary>
    Task<IEnumerable<Auditoria>> GetByRegistroAsync(
        string tabla,
        string registroId,
        Guid? empresaId = null);

    /// <summary>
    /// Obtiene una auditoría por ID
    /// </summary>
    Task<Auditoria?> GetAsync(Guid id);

    /// <summary>
    /// Crea un nuevo registro de auditoría
    /// </summary>
    Task<Auditoria> AddAsync(Auditoria auditoria);

    /// <summary>
    /// Elimina registros antiguos (limpieza)
    /// </summary>
    Task EliminarAntiguosAsync(int diasRetener = 365);
}
