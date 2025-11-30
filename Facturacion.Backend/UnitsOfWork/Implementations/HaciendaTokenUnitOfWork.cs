using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;

namespace Facturacion.Backend.UnitsOfWork.Implementations;

/// <summary>
/// Implementación del Unit of Work para tokens de Hacienda
/// </summary>
public class HaciendaTokenUnitOfWork : IHaciendaTokenUnitOfWork
{
    private readonly IHaciendaTokenRepository _repository;

    public HaciendaTokenUnitOfWork(IHaciendaTokenRepository repository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<HaciendaToken>> GetTokenActivoAsync(Guid empresaId, string ambiente)
    {
        return await _repository.GetTokenActivoAsync(empresaId, ambiente);
    }

    public async Task<ActionResponse<IEnumerable<HaciendaToken>>> GetTokensPorEmpresaAsync(Guid empresaId)
    {
        return await _repository.GetTokensPorEmpresaAsync(empresaId);
    }

    public async Task<ActionResponse<HaciendaToken>> AddAsync(HaciendaToken token)
    {
        return await _repository.AddAsync(token);
    }

    public async Task<ActionResponse<HaciendaToken>> UpdateAsync(HaciendaToken token)
    {
        return await _repository.UpdateAsync(token);
    }

    public async Task<ActionResponse<bool>> InvalidarTokenAsync(Guid tokenId)
    {
        return await _repository.InvalidarTokenAsync(tokenId);
    }

    public async Task<ActionResponse<bool>> InvalidarTokensEmpresaAsync(Guid empresaId, string ambiente)
    {
        return await _repository.InvalidarTokensEmpresaAsync(empresaId, ambiente);
    }

    public async Task<ActionResponse<int>> LimpiarTokensExpiradosAsync(int diasAntiguedad = 30)
    {
        return await _repository.LimpiarTokensExpiradosAsync(diasAntiguedad);
    }
}
