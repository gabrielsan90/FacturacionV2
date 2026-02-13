using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class PlantillasAsientoController : ControllerBase
{
    private readonly IPlantillaAsientoRepository _repository;
    private readonly DataContext _context;
    private readonly ILogger<PlantillasAsientoController> _logger;

    public PlantillasAsientoController(
        IPlantillaAsientoRepository repository,
        DataContext context,
        ILogger<PlantillasAsientoController> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    #region DTOs

    /// <summary>
    /// DTO for creating/updating a PlantillaAsiento with lines
    /// </summary>
    public class PlantillaAsientoDTO
    {
        public Guid? Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string TipoAsiento { get; set; } = "DIA";
        public string? ConceptoTemplate { get; set; }
        public bool Activo { get; set; } = true;
        public List<PlantillaAsientoLineaDTO> Lineas { get; set; } = new();
    }

    /// <summary>
    /// DTO for template lines
    /// </summary>
    public class PlantillaAsientoLineaDTO
    {
        public Guid? Id { get; set; }
        public Guid CuentaContableId { get; set; }
        public int Orden { get; set; }
        public string? Descripcion { get; set; }
        public string TipoMonto { get; set; } = "FIJO";
        public decimal MontoDebeFijo { get; set; }
        public decimal MontoHaberFijo { get; set; }
        public string? NaturalezaVariable { get; set; }
        public decimal? Porcentaje { get; set; }
    }

    /// <summary>
    /// DTO for list display
    /// </summary>
    internal class PlantillaAsientoListDTO
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string TipoAsiento { get; set; } = null!;
        public string TipoAsientoDescripcion { get; set; } = null!;
        public bool Activo { get; set; }
        public int CantidadLineas { get; set; }
        public DateTime? UltimaUtilizacion { get; set; }
        public int VecesUtilizada { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? CreadoPorNombre { get; set; }
    }

    /// <summary>
    /// DTO for detail view with lines
    /// </summary>
    internal class PlantillaAsientoDetailDTO
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string TipoAsiento { get; set; } = null!;
        public string TipoAsientoDescripcion { get; set; } = null!;
        public string? ConceptoTemplate { get; set; }
        public bool Activo { get; set; }
        public DateTime? UltimaUtilizacion { get; set; }
        public int VecesUtilizada { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? CreadoPorNombre { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? ModificadoPorNombre { get; set; }
        public List<PlantillaAsientoLineaDetailDTO> Lineas { get; set; } = new();
    }

    /// <summary>
    /// DTO for line detail display
    /// </summary>
    internal class PlantillaAsientoLineaDetailDTO
    {
        public Guid Id { get; set; }
        public int Orden { get; set; }
        public Guid CuentaContableId { get; set; }
        public string CuentaCodigo { get; set; } = null!;
        public string CuentaNombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string TipoMonto { get; set; } = null!;
        public string TipoMontoDescripcion { get; set; } = null!;
        public decimal MontoDebeFijo { get; set; }
        public decimal MontoHaberFijo { get; set; }
        public string? NaturalezaVariable { get; set; }
        public decimal? Porcentaje { get; set; }
    }

    /// <summary>
    /// DTO for using (executing) a template
    /// </summary>
    public class UsarPlantillaDTO
    {
        public decimal MontoVariable { get; set; }
        public DateTime Fecha { get; set; }
        public Guid PeriodoContableId { get; set; }
        public string? Concepto { get; set; }
    }

    /// <summary>
    /// Response DTO for template usage
    /// </summary>
    internal class UsarPlantillaResponseDTO
    {
        public Guid AsientoContableId { get; set; }
        public int NumeroAsiento { get; set; }
        public string Mensaje { get; set; } = null!;
    }

    #endregion

    #region GET Endpoints

    /// <summary>
    /// GET /api/plantillasasiento/empresa/{empresaId}
    /// Get all templates for an empresa
    /// </summary>
    [HttpGet("empresa/{empresaId}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var action = await _repository.GetByEmpresaAsync(empresaId);

            if (!action.WasSuccess)
            {
                _logger.LogError("Error retrieving plantillas asiento for empresa {EmpresaId}: {Message}", empresaId, action.Message);
                return BadRequest(new ActionResponse<IEnumerable<PlantillaAsientoListDTO>>
                {
                    WasSuccess = false,
                    Message = action.Message
                });
            }

            var plantillasList = action.Result!.Select(p => new PlantillaAsientoListDTO
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                TipoAsiento = p.TipoAsiento,
                TipoAsientoDescripcion = p.TipoAsientoDescripcion,
                Activo = p.Activo,
                CantidadLineas = p.CantidadLineas,
                UltimaUtilizacion = p.UltimaUtilizacion,
                VecesUtilizada = p.VecesUtilizada,
                FechaCreacion = p.FechaCreacion,
                CreadoPorNombre = p.CreadoPor?.Email
            }).ToList();

            return Ok(new ActionResponse<IEnumerable<PlantillaAsientoListDTO>>
            {
                WasSuccess = true,
                Result = plantillasList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plantillas asiento for empresa {EmpresaId}", empresaId);
            return BadRequest(new ActionResponse<IEnumerable<PlantillaAsientoListDTO>>
            {
                WasSuccess = false,
                Message = $"Error al obtener las plantillas de asiento: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// GET /api/plantillasasiento/{id}
    /// Get single template with lines
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        try
        {
            var action = await _repository.GetByIdWithLinesAsync(id);

            if (!action.WasSuccess)
            {
                return NotFound(new ActionResponse<PlantillaAsientoDetailDTO>
                {
                    WasSuccess = false,
                    Message = action.Message
                });
            }

            var plantilla = action.Result!;

            var plantillaDetail = new PlantillaAsientoDetailDTO
            {
                Id = plantilla.Id,
                EmpresaId = plantilla.EmpresaId,
                Codigo = plantilla.Codigo,
                Nombre = plantilla.Nombre,
                Descripcion = plantilla.Descripcion,
                TipoAsiento = plantilla.TipoAsiento,
                TipoAsientoDescripcion = plantilla.TipoAsientoDescripcion,
                ConceptoTemplate = plantilla.ConceptoTemplate,
                Activo = plantilla.Activo,
                UltimaUtilizacion = plantilla.UltimaUtilizacion,
                VecesUtilizada = plantilla.VecesUtilizada,
                FechaCreacion = plantilla.FechaCreacion,
                CreadoPorNombre = plantilla.CreadoPor?.Email,
                FechaModificacion = plantilla.FechaModificacion,
                ModificadoPorNombre = plantilla.ModificadoPor?.Email,
                Lineas = plantilla.Lineas?.Select(l => new PlantillaAsientoLineaDetailDTO
                {
                    Id = l.Id,
                    Orden = l.Orden,
                    CuentaContableId = l.CuentaContableId,
                    CuentaCodigo = l.CuentaContable?.Codigo ?? "",
                    CuentaNombre = l.CuentaContable?.Nombre ?? "",
                    Descripcion = l.Descripcion,
                    TipoMonto = l.TipoMonto,
                    TipoMontoDescripcion = l.TipoMontoDescripcion,
                    MontoDebeFijo = l.MontoDebeFijo,
                    MontoHaberFijo = l.MontoHaberFijo,
                    NaturalezaVariable = l.NaturalezaVariable,
                    Porcentaje = l.Porcentaje
                }).ToList() ?? new List<PlantillaAsientoLineaDetailDTO>()
            };

            return Ok(new ActionResponse<PlantillaAsientoDetailDTO>
            {
                WasSuccess = true,
                Result = plantillaDetail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plantilla asiento {Id}", id);
            return BadRequest(new ActionResponse<PlantillaAsientoDetailDTO>
            {
                WasSuccess = false,
                Message = $"Error al obtener la plantilla de asiento: {ex.Message}"
            });
        }
    }

    #endregion

    #region POST/PUT/DELETE Endpoints

    /// <summary>
    /// POST /api/plantillasasiento
    /// Create template with lines
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] PlantillaAsientoDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Validate empresa exists (complex validation - keep in controller)
            var empresaExists = await _context.Empresas
                .AnyAsync(e => e.Id == dto.EmpresaId && !e.IsDeleted);

            if (!empresaExists)
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "La empresa especificada no existe"
                });
            }

            // Validate unique codigo per empresa using repository
            var codigoExistsAction = await _repository.CodigoExistsAsync(dto.EmpresaId, dto.Codigo.ToUpper());
            if (codigoExistsAction.WasSuccess && codigoExistsAction.Result)
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = $"Ya existe una plantilla con el código '{dto.Codigo}' para esta empresa"
                });
            }

            // Validate template has at least one line
            if (dto.Lineas == null || !dto.Lineas.Any())
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "La plantilla debe tener al menos una línea"
                });
            }

            // Validate all accounts exist and accept movements (complex validation - keep in controller)
            var cuentaIds = dto.Lineas.Select(l => l.CuentaContableId).Distinct().ToList();
            var cuentas = await _context.CuentasContables
                .Where(c => cuentaIds.Contains(c.Id) && !c.IsDeleted)
                .ToListAsync();

            if (cuentas.Count != cuentaIds.Count)
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "Una o más cuentas contables no existen"
                });
            }

            var cuentasNoAceptan = cuentas.Where(c => !c.AceptaMovimientos).ToList();
            if (cuentasNoAceptan.Any())
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = $"Las siguientes cuentas no aceptan movimientos: {string.Join(", ", cuentasNoAceptan.Select(c => c.Codigo))}"
                });
            }

            // Validate line configurations (business logic validation - keep in controller)
            var validationError = ValidateLineasConfiguration(dto.Lineas);
            if (validationError != null)
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = validationError
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Create PlantillaAsiento
            var plantilla = new PlantillaAsiento
            {
                Id = Guid.NewGuid(),
                EmpresaId = dto.EmpresaId,
                Codigo = dto.Codigo.ToUpper(),
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                TipoAsiento = dto.TipoAsiento.ToUpper(),
                ConceptoTemplate = dto.ConceptoTemplate,
                Activo = dto.Activo,
                VecesUtilizada = 0,
                CreadoPorId = userId
            };

            // Create lines
            var lineas = dto.Lineas.Select(l => new PlantillaAsientoLinea
            {
                Id = Guid.NewGuid(),
                PlantillaAsientoId = plantilla.Id,
                CuentaContableId = l.CuentaContableId,
                Orden = l.Orden,
                Descripcion = l.Descripcion,
                TipoMonto = l.TipoMonto.ToUpper(),
                MontoDebeFijo = l.MontoDebeFijo,
                MontoHaberFijo = l.MontoHaberFijo,
                NaturalezaVariable = l.NaturalezaVariable?.ToUpper(),
                Porcentaje = l.Porcentaje
            }).ToList();

            // Save to database using repository
            var action = await _repository.AddAsync(plantilla, lineas);

            if (!action.WasSuccess)
            {
                _logger.LogError("Error creating plantilla asiento: {Message}", action.Message);
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = action.Message
                });
            }

            _logger.LogInformation("Plantilla asiento {Codigo} created by user {UserId}", plantilla.Codigo, userId);

            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plantilla asiento");
            return BadRequest(new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = $"Error al crear la plantilla de asiento: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// PUT /api/plantillasasiento/{id}
    /// Update template with lines
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] PlantillaAsientoDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Validate unique codigo per empresa (excluding current) using repository
            var codigoExistsAction = await _repository.CodigoExistsAsync(dto.EmpresaId, dto.Codigo.ToUpper(), id);
            if (codigoExistsAction.WasSuccess && codigoExistsAction.Result)
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = $"Ya existe otra plantilla con el código '{dto.Codigo}' para esta empresa"
                });
            }

            // Validate template has at least one line
            if (dto.Lineas == null || !dto.Lineas.Any())
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "La plantilla debe tener al menos una línea"
                });
            }

            // Validate all accounts exist and accept movements (complex validation - keep in controller)
            var cuentaIds = dto.Lineas.Select(l => l.CuentaContableId).Distinct().ToList();
            var cuentas = await _context.CuentasContables
                .Where(c => cuentaIds.Contains(c.Id) && !c.IsDeleted)
                .ToListAsync();

            if (cuentas.Count != cuentaIds.Count)
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = "Una o más cuentas contables no existen"
                });
            }

            var cuentasNoAceptan = cuentas.Where(c => !c.AceptaMovimientos).ToList();
            if (cuentasNoAceptan.Any())
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = $"Las siguientes cuentas no aceptan movimientos: {string.Join(", ", cuentasNoAceptan.Select(c => c.Codigo))}"
                });
            }

            // Validate line configurations (business logic validation - keep in controller)
            var validationError = ValidateLineasConfiguration(dto.Lineas);
            if (validationError != null)
            {
                return BadRequest(new ActionResponse<PlantillaAsiento>
                {
                    WasSuccess = false,
                    Message = validationError
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update plantilla properties
            var plantilla = new PlantillaAsiento
            {
                Id = id,
                EmpresaId = dto.EmpresaId,
                Codigo = dto.Codigo.ToUpper(),
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                TipoAsiento = dto.TipoAsiento.ToUpper(),
                ConceptoTemplate = dto.ConceptoTemplate,
                Activo = dto.Activo,
                ModificadoPorId = userId
            };

            // Create new lines
            var lineas = dto.Lineas.Select(l => new PlantillaAsientoLinea
            {
                Id = Guid.NewGuid(),
                PlantillaAsientoId = plantilla.Id,
                CuentaContableId = l.CuentaContableId,
                Orden = l.Orden,
                Descripcion = l.Descripcion,
                TipoMonto = l.TipoMonto.ToUpper(),
                MontoDebeFijo = l.MontoDebeFijo,
                MontoHaberFijo = l.MontoHaberFijo,
                NaturalezaVariable = l.NaturalezaVariable?.ToUpper(),
                Porcentaje = l.Porcentaje
            }).ToList();

            // Update using repository
            var action = await _repository.UpdateAsync(plantilla, lineas);

            if (!action.WasSuccess)
            {
                if (action.Message == "Plantilla de asiento no encontrada")
                {
                    return NotFound(action);
                }

                _logger.LogError("Error updating plantilla asiento {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Plantilla asiento {Codigo} updated by user {UserId}", plantilla.Codigo, userId);

            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plantilla asiento {Id}", id);
            return BadRequest(new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = $"Error al actualizar la plantilla de asiento: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// DELETE /api/plantillasasiento/{id}
    /// Soft delete template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var action = await _repository.DeleteAsync(id);

            if (!action.WasSuccess)
            {
                if (action.Message == "Plantilla de asiento no encontrada")
                {
                    return NotFound(action);
                }

                _logger.LogError("Error deleting plantilla asiento {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Plantilla asiento {Id} deleted by user {UserId}", id, userId);

            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plantilla asiento {Id}", id);
            return BadRequest(new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = $"Error al eliminar la plantilla de asiento: {ex.Message}"
            });
        }
    }

    #endregion

    #region Action Endpoints

    /// <summary>
    /// POST /api/plantillasasiento/{id}/toggle-activo
    /// Toggle active status
    /// </summary>
    [HttpPost("{id}/toggle-activo")]
    public async Task<IActionResult> ToggleActivoAsync(Guid id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var action = await _repository.ToggleActivoAsync(id, userId);

            if (!action.WasSuccess)
            {
                if (action.Message == "Plantilla de asiento no encontrada")
                {
                    return NotFound(action);
                }

                _logger.LogError("Error toggling activo for plantilla asiento {Id}: {Message}", id, action.Message);
                return BadRequest(action);
            }

            _logger.LogInformation("Plantilla asiento {Codigo} activo toggled to {Activo} by user {UserId}",
                action.Result!.Codigo, action.Result.Activo, userId);

            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling activo for plantilla asiento {Id}", id);
            return BadRequest(new ActionResponse<PlantillaAsiento>
            {
                WasSuccess = false,
                Message = $"Error al cambiar el estado de la plantilla: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// POST /api/plantillasasiento/{id}/usar
    /// Create journal entry from template
    /// </summary>
    [HttpPost("{id}/usar")]
    public async Task<IActionResult> UsarAsync(Guid id, [FromBody] UsarPlantillaDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Load template with lines using repository
            var action = await _repository.GetByIdForUseAsync(id);

            if (!action.WasSuccess)
            {
                return NotFound(new ActionResponse<UsarPlantillaResponseDTO>
                {
                    WasSuccess = false,
                    Message = action.Message
                });
            }

            var plantilla = action.Result!;

            // Verify template is active
            if (!plantilla.Activo)
            {
                return BadRequest(new ActionResponse<UsarPlantillaResponseDTO>
                {
                    WasSuccess = false,
                    Message = "La plantilla está inactiva y no puede ser utilizada"
                });
            }

            // Verify template has lines
            if (plantilla.Lineas == null || !plantilla.Lineas.Any())
            {
                return BadRequest(new ActionResponse<UsarPlantillaResponseDTO>
                {
                    WasSuccess = false,
                    Message = "La plantilla no tiene líneas configuradas"
                });
            }

            // Verify period exists and is open
            var periodo = await _context.PeriodosContables
                .FirstOrDefaultAsync(p => p.Id == dto.PeriodoContableId && !p.IsDeleted);

            if (periodo == null)
            {
                return BadRequest(new ActionResponse<UsarPlantillaResponseDTO>
                {
                    WasSuccess = false,
                    Message = "El período contable no existe"
                });
            }

            if (periodo.Estado != "ABT")
            {
                return BadRequest(new ActionResponse<UsarPlantillaResponseDTO>
                {
                    WasSuccess = false,
                    Message = "El período contable está cerrado. No se pueden crear asientos."
                });
            }

            // Verify date is within period range
            if (dto.Fecha < periodo.FechaInicio || dto.Fecha > periodo.FechaFin)
            {
                return BadRequest(new ActionResponse<UsarPlantillaResponseDTO>
                {
                    WasSuccess = false,
                    Message = $"La fecha del asiento debe estar dentro del período {periodo.PeriodoNombre} ({periodo.FechaInicio:dd/MM/yyyy} - {periodo.FechaFin:dd/MM/yyyy})"
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Generate next entry number
            var nuevoNumero = periodo.UltimoNumeroAsiento + 1;

            // Calculate movements from template lines
            var movimientos = new List<MovimientoContable>();
            var numeroLinea = 1;

            foreach (var linea in plantilla.Lineas)
            {
                decimal debe = 0;
                decimal haber = 0;

                switch (linea.TipoMonto)
                {
                    case "FIJO":
                        debe = linea.MontoDebeFijo;
                        haber = linea.MontoHaberFijo;
                        break;

                    case "VARIABLE":
                        if (linea.NaturalezaVariable == "DEBE")
                        {
                            debe = dto.MontoVariable;
                        }
                        else // HABER
                        {
                            haber = dto.MontoVariable;
                        }
                        break;

                    case "PORCENTAJE":
                        var montoPorcentaje = dto.MontoVariable * (linea.Porcentaje!.Value / 100);
                        if (linea.NaturalezaVariable == "DEBE")
                        {
                            debe = montoPorcentaje;
                        }
                        else // HABER
                        {
                            haber = montoPorcentaje;
                        }
                        break;
                }

                movimientos.Add(new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = linea.CuentaContableId,
                    NumeroLinea = numeroLinea++,
                    Descripcion = linea.Descripcion,
                    Debe = Math.Round(debe, 2),
                    Haber = Math.Round(haber, 2)
                });
            }

            // Calculate totals
            var totalDebe = movimientos.Sum(m => m.Debe);
            var totalHaber = movimientos.Sum(m => m.Haber);

            // Validate that the entry is balanced
            if (Math.Abs(totalDebe - totalHaber) >= 0.01m)
            {
                return BadRequest(new ActionResponse<UsarPlantillaResponseDTO>
                {
                    WasSuccess = false,
                    Message = $"El asiento generado no está balanceado. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}, Diferencia: {(totalDebe - totalHaber):N2}"
                });
            }

            // Determine concept (use provided or template default)
            var concepto = string.IsNullOrWhiteSpace(dto.Concepto)
                ? (plantilla.ConceptoTemplate ?? plantilla.Nombre)
                : dto.Concepto;

            // Create AsientoContable
            var asiento = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EmpresaId = plantilla.EmpresaId,
                Numero = nuevoNumero,
                Fecha = dto.Fecha,
                PeriodoContableId = dto.PeriodoContableId,
                TipoAsiento = plantilla.TipoAsiento,
                Concepto = concepto,
                Referencia = $"PLANTILLA: {plantilla.Codigo}",
                ModuloOrigen = "MAN", // Manual origin for template-generated entries
                TotalDebe = totalDebe,
                TotalHaber = totalHaber,
                Estado = "BOR", // Always start as draft
                FechaCreacion = DateTime.UtcNow,
                CreadoPorId = userId,
                IsDeleted = false
            };

            // Associate movements with asiento
            foreach (var movimiento in movimientos)
            {
                movimiento.AsientoContableId = asiento.Id;
            }

            // Update period's last entry number
            periodo.UltimoNumeroAsiento = nuevoNumero;
            periodo.FechaModificacion = DateTime.UtcNow;
            periodo.ModificadoPorId = userId;

            // Update template usage statistics
            plantilla.UltimaUtilizacion = DateTime.UtcNow;
            plantilla.VecesUtilizada += 1;
            plantilla.FechaModificacion = DateTime.UtcNow;
            plantilla.ModificadoPorId = userId;

            // Save to database
            await _context.AsientosContables.AddAsync(asiento);
            await _context.MovimientosContables.AddRangeAsync(movimientos);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asiento contable {Numero} created from plantilla {Codigo} by user {UserId}",
                nuevoNumero, plantilla.Codigo, userId);

            return Ok(new ActionResponse<UsarPlantillaResponseDTO>
            {
                WasSuccess = true,
                Message = "Asiento contable creado exitosamente desde plantilla",
                Result = new UsarPlantillaResponseDTO
                {
                    AsientoContableId = asiento.Id,
                    NumeroAsiento = nuevoNumero,
                    Mensaje = $"Asiento #{nuevoNumero} creado exitosamente. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using plantilla asiento {Id}", id);
            return BadRequest(new ActionResponse<UsarPlantillaResponseDTO>
            {
                WasSuccess = false,
                Message = $"Error al usar la plantilla de asiento: {ex.Message}"
            });
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Validates line configurations for FIJO, VARIABLE, and PORCENTAJE types
    /// </summary>
    /// <param name="lineas">List of lines to validate</param>
    /// <returns>Error message if validation fails, null if validation succeeds</returns>
    private string? ValidateLineasConfiguration(List<PlantillaAsientoLineaDTO> lineas)
    {
        foreach (var linea in lineas)
        {
            if (linea.TipoMonto == "FIJO")
            {
                // FIJO must have either MontoDebeFijo or MontoHaberFijo (but not both)
                if (linea.MontoDebeFijo == 0 && linea.MontoHaberFijo == 0)
                {
                    return $"La línea {linea.Orden} con TipoMonto FIJO debe tener un monto en Debe o Haber";
                }
                if (linea.MontoDebeFijo > 0 && linea.MontoHaberFijo > 0)
                {
                    return $"La línea {linea.Orden} con TipoMonto FIJO no puede tener monto en Debe y Haber al mismo tiempo";
                }
            }
            else if (linea.TipoMonto == "VARIABLE")
            {
                // VARIABLE must have NaturalezaVariable (DEBE or HABER)
                if (string.IsNullOrWhiteSpace(linea.NaturalezaVariable))
                {
                    return $"La línea {linea.Orden} con TipoMonto VARIABLE debe especificar la Naturaleza (DEBE o HABER)";
                }
                if (linea.NaturalezaVariable != "DEBE" && linea.NaturalezaVariable != "HABER")
                {
                    return $"La línea {linea.Orden} tiene una Naturaleza inválida. Use 'DEBE' o 'HABER'";
                }
            }
            else if (linea.TipoMonto == "PORCENTAJE")
            {
                // PORCENTAJE must have Porcentaje and NaturalezaVariable
                if (!linea.Porcentaje.HasValue || linea.Porcentaje.Value <= 0)
                {
                    return $"La línea {linea.Orden} con TipoMonto PORCENTAJE debe especificar un porcentaje mayor a 0";
                }
                if (string.IsNullOrWhiteSpace(linea.NaturalezaVariable))
                {
                    return $"La línea {linea.Orden} con TipoMonto PORCENTAJE debe especificar la Naturaleza (DEBE o HABER)";
                }
                if (linea.NaturalezaVariable != "DEBE" && linea.NaturalezaVariable != "HABER")
                {
                    return $"La línea {linea.Orden} tiene una Naturaleza inválida. Use 'DEBE' o 'HABER'";
                }
            }
            else
            {
                return $"La línea {linea.Orden} tiene un TipoMonto inválido. Use 'FIJO', 'VARIABLE' o 'PORCENTAJE'";
            }
        }

        return null; // All validations passed
    }

    #endregion
}
