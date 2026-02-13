using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Facturacion.Backend.Repositories.Implementations;

public class ExtractoBancarioRepository : IExtractoBancarioRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ExtractoBancarioRepository> _logger;

    public ExtractoBancarioRepository(DataContext context, ILogger<ExtractoBancarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResponse<ExtractoBancario>> GetAsync(Guid id)
    {
        try
        {
            var extracto = await _context.ExtractosBancarios
                .Include(e => e.Empresa)
                .Include(e => e.CuentaBancaria)
                .Include(e => e.Lineas)
                .Include(e => e.ConciliacionBancaria)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extracto == null)
            {
                return new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado"
                };
            }

            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = true,
                Result = extracto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener extracto bancario {Id}", id);
            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var extractos = await _context.ExtractosBancarios
                .Include(e => e.CuentaBancaria)
                .Where(e => e.EmpresaId == empresaId)
                .OrderByDescending(e => e.FechaFin)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = true,
                Result = extractos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener extractos de empresa {EmpresaId}", empresaId);
            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetByCuentaBancariaAsync(Guid cuentaBancariaId)
    {
        try
        {
            var extractos = await _context.ExtractosBancarios
                .Include(e => e.CuentaBancaria)
                .Where(e => e.CuentaBancariaId == cuentaBancariaId)
                .OrderByDescending(e => e.FechaFin)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = true,
                Result = extractos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener extractos de cuenta bancaria");
            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            var extractos = await _context.ExtractosBancarios
                .Include(e => e.CuentaBancaria)
                .Where(e => e.EmpresaId == empresaId &&
                           (e.Estado == EstadosExtractoBancario.Pendiente ||
                            e.Estado == EstadosExtractoBancario.Parcial))
                .OrderBy(e => e.FechaFin)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = true,
                Result = extractos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener extractos pendientes");
            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<ExtractoBancario>>> GetByFechaRangeAsync(Guid cuentaBancariaId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var extractos = await _context.ExtractosBancarios
                .Include(e => e.CuentaBancaria)
                .Where(e => e.CuentaBancariaId == cuentaBancariaId &&
                           e.FechaFin >= fechaInicio &&
                           e.FechaInicio <= fechaFin)
                .OrderBy(e => e.FechaInicio)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = true,
                Result = extractos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener extractos por rango de fechas");
            return new ActionResponse<IEnumerable<ExtractoBancario>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ExtractoBancario>> AddAsync(ExtractoBancario extracto)
    {
        try
        {
            // Verificar que la cuenta bancaria existe
            var cuentaBancaria = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.Id == extracto.CuentaBancariaId && !c.IsDeleted);

            if (cuentaBancaria == null)
            {
                return new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "La cuenta bancaria no existe o está eliminada"
                };
            }

            extracto.FechaCreacion = FechaCostaRicaHelper.Ahora;
            extracto.FechaImportacion = FechaCostaRicaHelper.Ahora;
            extracto.Estado = EstadosExtractoBancario.Pendiente;

            // Calcular totales si no están establecidos
            if (extracto.Lineas != null && extracto.Lineas.Any())
            {
                extracto.TotalCreditos = extracto.Lineas.Sum(l => l.Credito);
                extracto.TotalDebitos = extracto.Lineas.Sum(l => l.Debito);
                extracto.CantidadTransacciones = extracto.Lineas.Count;
            }

            _context.ExtractosBancarios.Add(extracto);
            await _context.SaveChangesAsync();

            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = true,
                Result = extracto,
                Message = "Extracto bancario creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear extracto bancario");
            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ExtractoBancario>> UpdateAsync(ExtractoBancario extracto)
    {
        try
        {
            var existente = await _context.ExtractosBancarios
                .Include(e => e.Lineas)
                .FirstOrDefaultAsync(e => e.Id == extracto.Id);

            if (existente == null)
            {
                return new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado"
                };
            }

            // No permitir modificar extractos conciliados o cerrados
            if (existente.Estado == EstadosExtractoBancario.Conciliado ||
                existente.Estado == EstadosExtractoBancario.Cerrado)
            {
                return new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "No se puede modificar un extracto que ya ha sido conciliado o cerrado"
                };
            }

            extracto.FechaModificacion = FechaCostaRicaHelper.Ahora;
            extracto.FechaCreacion = existente.FechaCreacion;
            extracto.CreadoPorId = existente.CreadoPorId;
            extracto.FechaImportacion = existente.FechaImportacion;

            // Recalcular totales si las líneas cambiaron
            if (extracto.Lineas != null && extracto.Lineas.Any())
            {
                extracto.TotalCreditos = extracto.Lineas.Sum(l => l.Credito);
                extracto.TotalDebitos = extracto.Lineas.Sum(l => l.Debito);
                extracto.CantidadTransacciones = extracto.Lineas.Count;
            }

            _context.Entry(existente).CurrentValues.SetValues(extracto);

            // Actualizar líneas del extracto
            if (extracto.Lineas != null)
            {
                // Eliminar líneas que ya no están
                var lineasAEliminar = existente.Lineas!
                    .Where(l => !extracto.Lineas.Any(nl => nl.Id == l.Id))
                    .ToList();

                foreach (var linea in lineasAEliminar)
                {
                    _context.LineasExtractoBancario.Remove(linea);
                }

                // Agregar o actualizar líneas
                foreach (var linea in extracto.Lineas)
                {
                    linea.ExtractoBancarioId = extracto.Id;

                    if (linea.Id == Guid.Empty)
                    {
                        // Nueva línea
                        existente.Lineas!.Add(linea);
                    }
                    else
                    {
                        // Actualizar línea existente
                        var lineaExistente = existente.Lineas!.FirstOrDefault(l => l.Id == linea.Id);
                        if (lineaExistente != null)
                        {
                            _context.Entry(lineaExistente).CurrentValues.SetValues(linea);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = true,
                Result = extracto,
                Message = "Extracto bancario actualizado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar extracto bancario");
            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<ExtractoBancario>> MarcarComoConciliadoAsync(Guid id, Guid conciliacionId)
    {
        try
        {
            var extracto = await _context.ExtractosBancarios.FindAsync(id);

            if (extracto == null)
            {
                return new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado"
                };
            }

            if (extracto.Estado == EstadosExtractoBancario.Cerrado)
            {
                return new ActionResponse<ExtractoBancario>
                {
                    WasSuccess = false,
                    Message = "No se puede conciliar un extracto cerrado"
                };
            }

            extracto.Estado = EstadosExtractoBancario.Conciliado;
            extracto.ConciliacionBancariaId = conciliacionId;
            extracto.FechaModificacion = FechaCostaRicaHelper.Ahora;

            await _context.SaveChangesAsync();

            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = true,
                Result = extracto,
                Message = "Extracto marcado como conciliado"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar extracto como conciliado");
            return new ActionResponse<ExtractoBancario>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string usuarioId)
    {
        try
        {
            var extracto = await _context.ExtractosBancarios
                .Include(e => e.Lineas)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extracto == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Extracto bancario no encontrado"
                };
            }

            // No permitir eliminar extractos conciliados o cerrados
            if (extracto.Estado == EstadosExtractoBancario.Conciliado ||
                extracto.Estado == EstadosExtractoBancario.Cerrado)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar un extracto que ya ha sido conciliado o cerrado"
                };
            }

            // Eliminar las líneas del extracto
            if (extracto.Lineas != null && extracto.Lineas.Any())
            {
                _context.LineasExtractoBancario.RemoveRange(extracto.Lineas);
            }

            // Eliminar el extracto
            _context.ExtractosBancarios.Remove(extracto);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Extracto bancario eliminado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar extracto bancario");
            return new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }
}
