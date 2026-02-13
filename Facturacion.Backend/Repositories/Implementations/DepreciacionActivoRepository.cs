using Facturacion.Backend.Data;
using Facturacion.Backend.Helpers;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class DepreciacionActivoRepository : IDepreciacionActivoRepository
{
    private readonly DataContext _context;

    public DepreciacionActivoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<DepreciacionActivo>> GetAsync(Guid id)
    {
        try
        {
            var depreciacion = await _context.DepreciacionesActivo
                .Include(d => d.ActivoFijo)
                    .ThenInclude(a => a!.CategoriaActivo)
                .Include(d => d.UsuarioCreacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (depreciacion == null)
            {
                return new ActionResponse<DepreciacionActivo>
                {
                    WasSuccess = false,
                    Message = "Depreciación no encontrada"
                };
            }

            return new ActionResponse<DepreciacionActivo>
            {
                WasSuccess = true,
                Result = depreciacion
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<DepreciacionActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<DepreciacionActivo>>> GetByActivoAsync(Guid activoFijoId)
    {
        try
        {
            var depreciaciones = await _context.DepreciacionesActivo
                .Include(d => d.UsuarioCreacion)
                .Where(d => d.ActivoFijoId == activoFijoId && d.Estado != "REV")
                .OrderByDescending(d => d.Anio)
                .ThenByDescending(d => d.Mes)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<DepreciacionActivo>>
            {
                WasSuccess = true,
                Result = depreciaciones
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<DepreciacionActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<DepreciacionActivo>>> GetByPeriodoAsync(Guid empresaId, int anio, int mes)
    {
        try
        {
            var depreciaciones = await _context.DepreciacionesActivo
                .Include(d => d.ActivoFijo)
                    .ThenInclude(a => a!.CategoriaActivo)
                .Include(d => d.ActivoFijo)
                    .ThenInclude(a => a!.Sucursal)
                .Include(d => d.UsuarioCreacion)
                .Where(d => d.ActivoFijo!.EmpresaId == empresaId &&
                           d.Anio == anio &&
                           d.Mes == mes &&
                           d.Estado != "REV")
                .OrderBy(d => d.ActivoFijo!.Codigo)
                .AsNoTracking()
                .ToListAsync();

            return new ActionResponse<IEnumerable<DepreciacionActivo>>
            {
                WasSuccess = true,
                Result = depreciaciones
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<DepreciacionActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<decimal>> CalcularDepreciacionMensualAsync(Guid activoFijoId)
    {
        try
        {
            var activo = await _context.ActivosFijos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == activoFijoId && !a.IsDeleted);

            if (activo == null)
            {
                return new ActionResponse<decimal>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            // Validar que el activo esté activo
            if (activo.Estado != "ACT")
            {
                return new ActionResponse<decimal>
                {
                    WasSuccess = false,
                    Message = "El activo no está en estado activo"
                };
            }

            // Validar vida útil
            if (activo.VidaUtilAnios <= 0)
            {
                return new ActionResponse<decimal>
                {
                    WasSuccess = false,
                    Message = "La vida útil del activo debe ser mayor a cero"
                };
            }

            // Fórmula de Línea Recta: (ValorOriginal - ValorResidual) / (VidaUtilAnios * 12)
            decimal valorDepreciable = activo.ValorOriginal - activo.ValorResidual;
            decimal depreciacionMensual = valorDepreciable / (activo.VidaUtilAnios * 12);

            // Validar que no se exceda el límite de depreciación
            decimal depreciacionRestante = valorDepreciable - activo.DepreciacionAcumulada;

            if (depreciacionRestante <= 0)
            {
                return new ActionResponse<decimal>
                {
                    WasSuccess = false,
                    Message = "El activo ya está completamente depreciado"
                };
            }

            // Si la depreciación mensual excede lo que queda, ajustar al restante
            if (depreciacionMensual > depreciacionRestante)
            {
                depreciacionMensual = depreciacionRestante;
            }

            return new ActionResponse<decimal>
            {
                WasSuccess = true,
                Result = Math.Round(depreciacionMensual, 2)
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<decimal>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<DepreciacionActivo>>> GenerarDepreciacionesMasivasAsync(Guid empresaId, int anio, int mes)
    {
        try
        {
            // Validar período
            if (mes < 1 || mes > 12)
            {
                return new ActionResponse<IEnumerable<DepreciacionActivo>>
                {
                    WasSuccess = false,
                    Message = "El mes debe estar entre 1 y 12"
                };
            }

            // Validar que no existan depreciaciones para el período
            var existeDepreciacionPeriodo = await _context.DepreciacionesActivo
                .AnyAsync(d => d.ActivoFijo!.EmpresaId == empresaId &&
                              d.Anio == anio &&
                              d.Mes == mes &&
                              d.Estado != "REV");

            if (existeDepreciacionPeriodo)
            {
                return new ActionResponse<IEnumerable<DepreciacionActivo>>
                {
                    WasSuccess = false,
                    Message = $"Ya existen depreciaciones registradas para el período {anio}-{mes:D2}"
                };
            }

            // Obtener activos activos que deban depreciar
            var activos = await _context.ActivosFijos
                .Where(a => a.EmpresaId == empresaId &&
                           !a.IsDeleted &&
                           a.Estado == "ACT" &&
                           a.VidaUtilAnios > 0 &&
                           a.MetodoDepreciacion == "LR") // Solo línea recta por ahora
                .ToListAsync();

            if (!activos.Any())
            {
                return new ActionResponse<IEnumerable<DepreciacionActivo>>
                {
                    WasSuccess = false,
                    Message = "No se encontraron activos activos para depreciar"
                };
            }

            var depreciacionesGeneradas = new List<DepreciacionActivo>();
            var fechaDepreciacion = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));

            foreach (var activo in activos)
            {
                // Calcular si el activo ya está completamente depreciado
                decimal valorDepreciable = activo.ValorOriginal - activo.ValorResidual;
                decimal depreciacionRestante = valorDepreciable - activo.DepreciacionAcumulada;

                if (depreciacionRestante <= 0)
                {
                    continue; // Saltar activos completamente depreciados
                }

                // Verificar fecha de inicio de depreciación
                if (activo.FechaInicioDepreciacion.HasValue && fechaDepreciacion < activo.FechaInicioDepreciacion.Value)
                {
                    continue; // No ha iniciado su período de depreciación
                }

                // Calcular depreciación mensual (Línea Recta)
                decimal depreciacionMensual = valorDepreciable / (activo.VidaUtilAnios * 12);

                // Ajustar si excede el restante
                if (depreciacionMensual > depreciacionRestante)
                {
                    depreciacionMensual = depreciacionRestante;
                }

                var depreciacion = new DepreciacionActivo
                {
                    Id = Guid.NewGuid(),
                    ActivoFijoId = activo.Id,
                    Fecha = fechaDepreciacion,
                    Anio = anio,
                    Mes = mes,
                    MontoDepreciacion = Math.Round(depreciacionMensual, 2),
                    DepreciacionAcumuladaAnterior = activo.DepreciacionAcumulada,
                    DepreciacionAcumuladaNueva = activo.DepreciacionAcumulada + Math.Round(depreciacionMensual, 2),
                    ValorLibrosAnterior = activo.ValorOriginal - activo.DepreciacionAcumulada,
                    ValorLibrosNuevo = activo.ValorOriginal - (activo.DepreciacionAcumulada + Math.Round(depreciacionMensual, 2)),
                    Estado = "CAL",
                    Contabilizado = false,
                    FechaCreacion = FechaCostaRicaHelper.Ahora
                };

                _context.DepreciacionesActivo.Add(depreciacion);

                // Actualizar el activo
                activo.DepreciacionAcumulada += Math.Round(depreciacionMensual, 2);
                activo.UltimaDepreciacion = fechaDepreciacion;

                // Si está completamente depreciado, cambiar estado
                if (activo.DepreciacionAcumulada >= valorDepreciable)
                {
                    activo.Estado = "DEP";
                }

                depreciacionesGeneradas.Add(depreciacion);
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<IEnumerable<DepreciacionActivo>>
            {
                WasSuccess = true,
                Result = depreciacionesGeneradas,
                Message = $"Se generaron {depreciacionesGeneradas.Count} depreciaciones para el período {anio}-{mes:D2}"
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<DepreciacionActivo>>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<DepreciacionActivo>> AddAsync(DepreciacionActivo depreciacion)
    {
        try
        {
            // Validar que el activo exista
            var activo = await _context.ActivosFijos.FindAsync(depreciacion.ActivoFijoId);

            if (activo == null)
            {
                return new ActionResponse<DepreciacionActivo>
                {
                    WasSuccess = false,
                    Message = "Activo fijo no encontrado"
                };
            }

            // Validar que no exceda el valor depreciable
            decimal valorDepreciable = activo.ValorOriginal - activo.ValorResidual;
            decimal nuevaDepreciacionAcumulada = activo.DepreciacionAcumulada + depreciacion.MontoDepreciacion;

            if (nuevaDepreciacionAcumulada > valorDepreciable)
            {
                return new ActionResponse<DepreciacionActivo>
                {
                    WasSuccess = false,
                    Message = $"La depreciación excede el valor depreciable. Máximo permitido: {valorDepreciable - activo.DepreciacionAcumulada:N2}"
                };
            }

            depreciacion.FechaCreacion = FechaCostaRicaHelper.Ahora;
            depreciacion.Estado = "APL";
            depreciacion.DepreciacionAcumuladaAnterior = activo.DepreciacionAcumulada;
            depreciacion.DepreciacionAcumuladaNueva = nuevaDepreciacionAcumulada;
            depreciacion.ValorLibrosAnterior = activo.ValorOriginal - activo.DepreciacionAcumulada;
            depreciacion.ValorLibrosNuevo = activo.ValorOriginal - nuevaDepreciacionAcumulada;

            _context.DepreciacionesActivo.Add(depreciacion);

            // Actualizar activo
            activo.DepreciacionAcumulada = nuevaDepreciacionAcumulada;
            activo.UltimaDepreciacion = depreciacion.Fecha;

            if (nuevaDepreciacionAcumulada >= valorDepreciable)
            {
                activo.Estado = "DEP";
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<DepreciacionActivo>
            {
                WasSuccess = true,
                Result = depreciacion
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<DepreciacionActivo>
            {
                WasSuccess = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> ReversarDepreciacionAsync(Guid id, string usuarioId)
    {
        try
        {
            var depreciacion = await _context.DepreciacionesActivo
                .Include(d => d.ActivoFijo)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (depreciacion == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Depreciación no encontrada"
                };
            }

            if (depreciacion.Estado == "REV")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "La depreciación ya está reversada"
                };
            }

            if (depreciacion.Contabilizado)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede reversar una depreciación contabilizada"
                };
            }

            // Reversar en el activo
            if (depreciacion.ActivoFijo != null)
            {
                depreciacion.ActivoFijo.DepreciacionAcumulada -= depreciacion.MontoDepreciacion;

                // Si estaba depreciado totalmente, volver a activo
                if (depreciacion.ActivoFijo.Estado == "DEP")
                {
                    depreciacion.ActivoFijo.Estado = "ACT";
                }
            }

            // Marcar como reversada
            depreciacion.Estado = "REV";
            depreciacion.Observaciones = $"Reversada por usuario {usuarioId} el {FechaCostaRicaHelper.Ahora:dd/MM/yyyy HH:mm}";

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true,
                Message = "Depreciación reversada correctamente"
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
}
