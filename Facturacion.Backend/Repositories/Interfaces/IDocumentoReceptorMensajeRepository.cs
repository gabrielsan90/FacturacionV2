using Facturacion.Shared.Entities;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestionar Mensajes Receptor (MR)
/// </summary>
public interface IDocumentoReceptorMensajeRepository
{
    /// <summary>
    /// Obtiene un mensaje por su ID
    /// </summary>
    Task<DocumentoReceptorMensaje?> GetAsync(Guid id);

    /// <summary>
    /// Obtiene todos los mensajes asociados a un documento original
    /// </summary>
    Task<IEnumerable<DocumentoReceptorMensaje>> GetByDocumentoOriginalAsync(Guid documentoOriginalId);

    /// <summary>
    /// Obtiene un mensaje por su clave de 50 dígitos
    /// </summary>
    Task<DocumentoReceptorMensaje?> GetByClaveMensajeAsync(string claveMensaje);

    /// <summary>
    /// Verifica si ya existe un mensaje receptor para un documento
    /// </summary>
    Task<bool> ExisteMensajeParaDocumentoAsync(Guid documentoOriginalId);

    /// <summary>
    /// Obtiene todos los mensajes de una empresa
    /// </summary>
    Task<IEnumerable<DocumentoReceptorMensaje>> GetByEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Obtiene mensajes pendientes de envío a Hacienda
    /// </summary>
    Task<IEnumerable<DocumentoReceptorMensaje>> GetPendientesEnvioAsync(Guid empresaId);

    /// <summary>
    /// Agrega un nuevo mensaje
    /// </summary>
    Task<DocumentoReceptorMensaje> AddAsync(DocumentoReceptorMensaje mensaje);

    /// <summary>
    /// Actualiza un mensaje existente
    /// </summary>
    Task UpdateAsync(DocumentoReceptorMensaje mensaje);

    /// <summary>
    /// Elimina (soft delete) un mensaje
    /// </summary>
    Task DeleteAsync(Guid id, string userId);
}
