using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.Repositories.Interfaces;

/// <summary>
/// Repositorio para gestionar tokens de autenticación con Hacienda
/// </summary>
public interface IHaciendaTokenRepository
{
    /// <summary>
    /// Obtiene el token activo más reciente de una empresa para un ambiente específico
    /// </summary>
    Task<ActionResponse<HaciendaToken>> GetTokenActivoAsync(Guid empresaId, string ambiente);

    /// <summary>
    /// Obtiene todos los tokens de una empresa (activos e inactivos)
    /// </summary>
    Task<ActionResponse<IEnumerable<HaciendaToken>>> GetTokensPorEmpresaAsync(Guid empresaId);

    /// <summary>
    /// Agrega un nuevo token
    /// </summary>
    Task<ActionResponse<HaciendaToken>> AddAsync(HaciendaToken token);

    /// <summary>
    /// Actualiza un token existente
    /// </summary>
    Task<ActionResponse<HaciendaToken>> UpdateAsync(HaciendaToken token);

    /// <summary>
    /// Invalida (marca como inactivo) un token específico
    /// </summary>
    Task<ActionResponse<bool>> InvalidarTokenAsync(Guid tokenId);

    /// <summary>
    /// Invalida todos los tokens activos de una empresa para un ambiente
    /// </summary>
    Task<ActionResponse<bool>> InvalidarTokensEmpresaAsync(Guid empresaId, string ambiente);

    /// <summary>
    /// Elimina tokens expirados (limpieza)
    /// </summary>
    Task<ActionResponse<int>> LimpiarTokensExpiradosAsync(int diasAntiguedad = 30);
}
