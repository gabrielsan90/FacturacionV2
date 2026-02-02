using Facturacion.Backend.Helpers;
using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

/// <summary>
/// Implementación del repositorio para gestionar tokens de Hacienda
/// </summary>
public class HaciendaTokenRepository : IHaciendaTokenRepository
{
    private readonly DataContext _context;

    public HaciendaTokenRepository(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el token activo más reciente de una empresa para un ambiente específico
    /// </summary>
    public async Task<ActionResponse<HaciendaToken>> GetTokenActivoAsync(Guid empresaId, string ambiente)
    {
        try
        {
            var token = await _context.Set<HaciendaToken>()
                .Include(t => t.Empresa)
                .Where(t => t.EmpresaId == empresaId && t.Ambiente == ambiente && t.Activo)
                .OrderByDescending(t => t.FechaCreacion)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (token == null)
            {
                return new ActionResponse<HaciendaToken>
                {
                    WasSuccess = false,
                    Message = $"No se encontró un token activo para la empresa en ambiente {ambiente}"
                };
            }

            return new ActionResponse<HaciendaToken>
            {
                WasSuccess = true,
                Result = token
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<HaciendaToken>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Obtiene todos los tokens de una empresa (activos e inactivos)
    /// </summary>
    public async Task<ActionResponse<IEnumerable<HaciendaToken>>> GetTokensPorEmpresaAsync(Guid empresaId)
    {
        try
        {
            var tokens = await _context.Set<HaciendaToken>()
                .Include(t => t.Empresa)
                .Where(t => t.EmpresaId == empresaId)
                .OrderByDescending(t => t.FechaCreacion)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<HaciendaToken>>
            {
                WasSuccess = true,
                Result = tokens
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<HaciendaToken>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Agrega un nuevo token
    /// </summary>
    public async Task<ActionResponse<HaciendaToken>> AddAsync(HaciendaToken token)
    {
        try
        {
            _context.Set<HaciendaToken>().Add(token);
            await _context.SaveChangesAsync();

            return new ActionResponse<HaciendaToken>
            {
                WasSuccess = true,
                Result = token
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<HaciendaToken>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Actualiza un token existente
    /// </summary>
    public async Task<ActionResponse<HaciendaToken>> UpdateAsync(HaciendaToken token)
    {
        try
        {
            _context.Set<HaciendaToken>().Update(token);
            await _context.SaveChangesAsync();

            return new ActionResponse<HaciendaToken>
            {
                WasSuccess = true,
                Result = token
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<HaciendaToken>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Invalida (marca como inactivo) un token específico
    /// </summary>
    public async Task<ActionResponse<bool>> InvalidarTokenAsync(Guid tokenId)
    {
        try
        {
            var token = await _context.Set<HaciendaToken>()
                .FirstOrDefaultAsync(t => t.Id == tokenId);

            if (token == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Token no encontrado"
                };
            }

            token.Activo = false;
            token.FechaActualizacion = FechaCostaRicaHelper.Ahora;
            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Invalida todos los tokens activos de una empresa para un ambiente
    /// </summary>
    public async Task<ActionResponse<bool>> InvalidarTokensEmpresaAsync(Guid empresaId, string ambiente)
    {
        try
        {
            var tokens = await _context.Set<HaciendaToken>()
                .Where(t => t.EmpresaId == empresaId && t.Ambiente == ambiente && t.Activo)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Activo = false;
                token.FechaActualizacion = FechaCostaRicaHelper.Ahora;
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = $"{tokens.Count} tokens invalidados"
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// Elimina tokens expirados (limpieza)
    /// </summary>
    public async Task<ActionResponse<int>> LimpiarTokensExpiradosAsync(int diasAntiguedad = 30)
    {
        try
        {
            var fechaLimite = FechaCostaRicaHelper.Ahora.AddDays(-diasAntiguedad);

            var tokensExpirados = await _context.Set<HaciendaToken>()
                .Where(t => !t.Activo && t.FechaExpiracionRefreshToken < fechaLimite)
                .ToListAsync();

            _context.Set<HaciendaToken>().RemoveRange(tokensExpirados);
            await _context.SaveChangesAsync();

            return new ActionResponse<int>
            {
                WasSuccess = true,
                Result = tokensExpirados.Count,
                Message = $"{tokensExpirados.Count} tokens eliminados"
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<int>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
