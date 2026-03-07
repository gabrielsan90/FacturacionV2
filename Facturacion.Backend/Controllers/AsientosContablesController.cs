using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
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
public class AsientosContablesController : ControllerBase
{
    private readonly IContabilidadUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<AsientosContablesController> _logger;

    public AsientosContablesController(
        IContabilidadUnitOfWork unitOfWork,
        DataContext context,
        ILogger<AsientosContablesController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    #region DTOs

    /// <summary>
    /// DTO for creating a new AsientoContable with movements
    /// </summary>
    public class AsientoCreateDTO
    {
        public Guid EmpresaId { get; set; }
        public DateTime Fecha { get; set; }
        public Guid PeriodoContableId { get; set; }
        public string TipoAsiento { get; set; } = "DIA";
        public string Concepto { get; set; } = null!;
        public string? Referencia { get; set; }
        public string? ModuloOrigen { get; set; }
        public Guid? DocumentoOrigenId { get; set; }
        public string? Observaciones { get; set; }
        public List<MovimientoCreateDTO> Movimientos { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a movement within an AsientoContable
    /// </summary>
    public class MovimientoCreateDTO
    {
        public Guid CuentaContableId { get; set; }
        public int NumeroLinea { get; set; }
        public string? Descripcion { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public string? CentroCosto { get; set; }
        public string? Tercero { get; set; }
        public Guid? ClienteId { get; set; }
        public Guid? ProveedorId { get; set; }
        public string? DocumentoReferencia { get; set; }
    }

    /// <summary>
    /// DTO for list display with summary information
    /// </summary>
    internal class AsientoListDTO
    {
        public Guid Id { get; set; }
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string PeriodoNombre { get; set; } = "";
        public string TipoAsiento { get; set; } = "";
        public string TipoAsientoDescripcion { get; set; } = "";
        public string Concepto { get; set; } = "";
        public string Referencia { get; set; } = "";
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public string Estado { get; set; } = "";
        public string EstadoDescripcion { get; set; } = "";
        public bool EstaBalanceado { get; set; }
        public string ModuloOrigen { get; set; } = "";
        public string ModuloOrigenDescripcion { get; set; } = "";
        public DateTime FechaCreacion { get; set; }
        public string CreadoPorNombre { get; set; } = "";
    }

    /// <summary>
    /// DTO for detailed view with movements
    /// </summary>
    internal class AsientoDetailDTO
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public Guid PeriodoContableId { get; set; }
        public string PeriodoNombre { get; set; } = null!;
        public string TipoAsiento { get; set; } = null!;
        public string TipoAsientoDescripcion { get; set; } = null!;
        public string Concepto { get; set; } = null!;
        public string? Referencia { get; set; }
        public string? ModuloOrigen { get; set; }
        public string ModuloOrigenDescripcion { get; set; } = null!;
        public Guid? DocumentoOrigenId { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public decimal Diferencia { get; set; }
        public bool EstaBalanceado { get; set; }
        public string Estado { get; set; } = null!;
        public string EstadoDescripcion { get; set; } = null!;
        public string? Observaciones { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? AprobadoPorNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? CreadoPorNombre { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? ModificadoPorNombre { get; set; }
        public List<MovimientoDetailDTO> Movimientos { get; set; } = new();
    }

    /// <summary>
    /// DTO for movement detail display
    /// </summary>
    internal class MovimientoDetailDTO
    {
        public Guid Id { get; set; }
        public int NumeroLinea { get; set; }
        public Guid CuentaContableId { get; set; }
        public string CuentaCodigo { get; set; } = null!;
        public string CuentaNombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public decimal Saldo { get; set; }
        public string? CentroCosto { get; set; }
        public string? Tercero { get; set; }
        public Guid? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public Guid? ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
        public string? DocumentoReferencia { get; set; }
    }

    #endregion

    #region GET Endpoints

    /// <summary>
    /// GET /api/asientoscontables?empresaId={guid}&periodoId={guid?}&estado={string?}
    /// Get accounting entries filtered by empresa, period, and status
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        [FromQuery] Guid empresaId,
        [FromQuery] Guid? periodoId = null,
        [FromQuery] string? estado = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            IEnumerable<AsientoContable> asientos;

            // Use repository method based on filters
            if (periodoId.HasValue)
            {
                asientos = await _unitOfWork.AsientoContableRepository.GetByPeriodoAsync(periodoId.Value);
            }
            else
            {
                asientos = await _unitOfWork.AsientoContableRepository.GetByEmpresaAsync(empresaId);
            }

            // Filter by status if provided (apply additional filter in memory)
            if (!string.IsNullOrWhiteSpace(estado))
            {
                asientos = asientos.Where(a => a.Estado == estado.ToUpper()).ToList();
            }

            // Map to list DTOs
            var asientosList = asientos.Select(a => new AsientoListDTO
            {
                Id = a.Id,
                Numero = a.Numero,
                Fecha = a.Fecha,
                PeriodoNombre = a.PeriodoContable?.PeriodoNombre ?? "",
                TipoAsiento = a.TipoAsiento,
                TipoAsientoDescripcion = a.TipoAsientoDescripcion,
                Concepto = a.Concepto,
                Referencia = a.Referencia ?? "",
                TotalDebe = a.TotalDebe,
                TotalHaber = a.TotalHaber,
                Estado = a.Estado,
                EstadoDescripcion = a.EstadoDescripcion,
                EstaBalanceado = a.EstaBalanceado,
                ModuloOrigen = a.ModuloOrigen ?? "",
                ModuloOrigenDescripcion = a.ModuloOrigenDescripcion,
                FechaCreacion = a.FechaCreacion,
                CreadoPorNombre = a.CreadoPor?.Email ?? ""
            }).ToList();

            return Ok(new ActionResponse<IEnumerable<AsientoListDTO>>
            {
                WasSuccess = true,
                Result = asientosList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asientos contables for empresa {EmpresaId}", empresaId);
            return BadRequest(new ActionResponse<IEnumerable<AsientoListDTO>>
            {
                WasSuccess = false,
                Message = $"Error al obtener los asientos contables: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// GET /api/asientoscontables/{id}
    /// Get single accounting entry with all movements
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        try
        {
            var asiento = await _unitOfWork.AsientoContableRepository.GetWithMovimientosAsync(id);

            if (asiento == null)
            {
                return NotFound(new ActionResponse<AsientoDetailDTO>
                {
                    WasSuccess = false,
                    Message = "Asiento contable no encontrado"
                });
            }

            if (!await TieneAccesoEmpresaAsync(asiento.EmpresaId))
            {
                return Forbid();
            }

            // Map to detail DTO
            var asientoDetail = new AsientoDetailDTO
            {
                Id = asiento.Id,
                EmpresaId = asiento.EmpresaId,
                Numero = asiento.Numero,
                Fecha = asiento.Fecha,
                PeriodoContableId = asiento.PeriodoContableId,
                PeriodoNombre = asiento.PeriodoContable?.PeriodoNombre ?? "",
                TipoAsiento = asiento.TipoAsiento,
                TipoAsientoDescripcion = asiento.TipoAsientoDescripcion,
                Concepto = asiento.Concepto,
                Referencia = asiento.Referencia,
                ModuloOrigen = asiento.ModuloOrigen,
                ModuloOrigenDescripcion = asiento.ModuloOrigenDescripcion,
                DocumentoOrigenId = asiento.DocumentoOrigenId,
                TotalDebe = asiento.TotalDebe,
                TotalHaber = asiento.TotalHaber,
                Diferencia = asiento.Diferencia,
                EstaBalanceado = asiento.EstaBalanceado,
                Estado = asiento.Estado,
                EstadoDescripcion = asiento.EstadoDescripcion,
                Observaciones = asiento.Observaciones,
                FechaAprobacion = asiento.FechaAprobacion,
                AprobadoPorNombre = asiento.AprobadoPor?.Email,
                FechaCreacion = asiento.FechaCreacion,
                CreadoPorNombre = asiento.CreadoPor?.Email,
                FechaModificacion = asiento.FechaModificacion,
                ModificadoPorNombre = asiento.ModificadoPor?.Email,
                Movimientos = asiento.Movimientos?.OrderBy(m => m.NumeroLinea).Select(m => new MovimientoDetailDTO
                {
                    Id = m.Id,
                    NumeroLinea = m.NumeroLinea,
                    CuentaContableId = m.CuentaContableId,
                    CuentaCodigo = m.CuentaContable?.Codigo ?? "",
                    CuentaNombre = m.CuentaContable?.Nombre ?? "",
                    Descripcion = m.Descripcion,
                    Debe = m.Debe,
                    Haber = m.Haber,
                    Saldo = m.Saldo,
                    CentroCosto = m.CentroCosto,
                    Tercero = m.Tercero,
                    ClienteId = m.ClienteId,
                    ClienteNombre = m.Cliente?.Nombre,
                    ProveedorId = m.ProveedorId,
                    ProveedorNombre = m.Proveedor?.Nombre,
                    DocumentoReferencia = m.DocumentoReferencia
                }).ToList() ?? new List<MovimientoDetailDTO>()
            };

            return Ok(new ActionResponse<AsientoDetailDTO>
            {
                WasSuccess = true,
                Result = asientoDetail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asiento contable {Id}", id);
            return BadRequest(new ActionResponse<AsientoDetailDTO>
            {
                WasSuccess = false,
                Message = $"Error al obtener el asiento contable: {ex.Message}"
            });
        }
    }

    #endregion

    #region POST/PUT/DELETE Endpoints

    /// <summary>
    /// POST /api/asientoscontables
    /// Create a new accounting entry with movements
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] AsientoCreateDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(dto.EmpresaId))
            {
                return Forbid();
            }

            // Validate that the entry is balanced
            var totalDebe = dto.Movimientos.Sum(m => m.Debe);
            var totalHaber = dto.Movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) >= 0.01m)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"El asiento no está balanceado. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}, Diferencia: {(totalDebe - totalHaber):N2}"
                });
            }

            // Verify period exists and is open
            var periodo = await _unitOfWork.PeriodoContableRepository.GetAsync(dto.PeriodoContableId);

            if (periodo == null)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "El período contable no existe"
                });
            }

            if (periodo.Estado != "ABT")
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "El período contable está cerrado. No se pueden crear asientos."
                });
            }

            // Verify date is within period range
            if (dto.Fecha < periodo.FechaInicio || dto.Fecha > periodo.FechaFin)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"La fecha del asiento debe estar dentro del período {periodo.PeriodoNombre} ({periodo.FechaInicio:dd/MM/yyyy} - {periodo.FechaFin:dd/MM/yyyy})"
                });
            }

            // Validate all accounts exist and accept movements (using DataContext for complex query)
            var cuentaIds = dto.Movimientos.Select(m => m.CuentaContableId).Distinct().ToList();
            var cuentas = await _context.CuentasContables
                .Where(c => cuentaIds.Contains(c.Id) && !c.IsDeleted)
                .ToListAsync();

            if (cuentas.Count != cuentaIds.Count)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "Una o más cuentas contables no existen"
                });
            }

            var cuentasNoAceptan = cuentas.Where(c => !c.AceptaMovimientos).ToList();
            if (cuentasNoAceptan.Any())
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"Las siguientes cuentas no aceptan movimientos: {string.Join(", ", cuentasNoAceptan.Select(c => c.Codigo))}"
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Generate next entry number
            var nuevoNumero = periodo.UltimoNumeroAsiento + 1;

            // Create AsientoContable
            var asiento = new AsientoContable
            {
                Id = Guid.NewGuid(),
                EmpresaId = dto.EmpresaId,
                Numero = nuevoNumero,
                Fecha = dto.Fecha,
                PeriodoContableId = dto.PeriodoContableId,
                TipoAsiento = dto.TipoAsiento.ToUpper(),
                Concepto = dto.Concepto,
                Referencia = dto.Referencia,
                ModuloOrigen = dto.ModuloOrigen?.ToUpper(),
                DocumentoOrigenId = dto.DocumentoOrigenId,
                TotalDebe = totalDebe,
                TotalHaber = totalHaber,
                Estado = "BOR", // Always start as draft
                Observaciones = dto.Observaciones,
                CreadoPorId = userId,
                IsDeleted = false,
                Movimientos = dto.Movimientos.Select(m => new MovimientoContable
                {
                    Id = Guid.NewGuid(),
                    CuentaContableId = m.CuentaContableId,
                    NumeroLinea = m.NumeroLinea,
                    Descripcion = m.Descripcion,
                    Debe = m.Debe,
                    Haber = m.Haber,
                    CentroCosto = m.CentroCosto,
                    Tercero = m.Tercero,
                    ClienteId = m.ClienteId,
                    ProveedorId = m.ProveedorId,
                    DocumentoReferencia = m.DocumentoReferencia
                }).ToList()
            };

            // Update period's last entry number
            periodo.UltimoNumeroAsiento = nuevoNumero;
            periodo.ModificadoPorId = userId;

            // Save using repository and unit of work
            await _unitOfWork.AsientoContableRepository.AddAsync(asiento);
            await _unitOfWork.PeriodoContableRepository.UpdateAsync(periodo);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Asiento contable {Numero} created by user {UserId}", nuevoNumero, userId);

            return Ok(new ActionResponse<AsientoContable>
            {
                WasSuccess = true,
                Message = "Asiento contable creado exitosamente",
                Result = asiento
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asiento contable");
            return BadRequest(new ActionResponse<AsientoContable>
            {
                WasSuccess = false,
                Message = $"Error al crear el asiento contable: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// PUT /api/asientoscontables/{id}
    /// Update an existing accounting entry (only BOR status)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] AsientoCreateDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Get existing asiento with movements
            var asiento = await _unitOfWork.AsientoContableRepository.GetWithMovimientosAsync(id);

            if (asiento == null)
            {
                return NotFound(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "Asiento contable no encontrado"
                });
            }

            if (!await TieneAccesoEmpresaAsync(asiento.EmpresaId))
            {
                return Forbid();
            }

            // Only BOR (draft) entries can be edited
            if (asiento.Estado != "BOR")
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"No se puede editar un asiento en estado {asiento.EstadoDescripcion}. Solo se pueden editar asientos en Borrador."
                });
            }

            // Validate that the entry is balanced
            var totalDebe = dto.Movimientos.Sum(m => m.Debe);
            var totalHaber = dto.Movimientos.Sum(m => m.Haber);

            if (Math.Abs(totalDebe - totalHaber) >= 0.01m)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"El asiento no está balanceado. Debe: {totalDebe:N2}, Haber: {totalHaber:N2}, Diferencia: {(totalDebe - totalHaber):N2}"
                });
            }

            // Verify period is still open
            var periodo = await _unitOfWork.PeriodoContableRepository.GetAsync(dto.PeriodoContableId);

            if (periodo == null || periodo.Estado != "ABT")
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "El período contable está cerrado. No se pueden modificar asientos."
                });
            }

            // Validate date is within period
            if (dto.Fecha < periodo.FechaInicio || dto.Fecha > periodo.FechaFin)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"La fecha del asiento debe estar dentro del período {periodo.PeriodoNombre}"
                });
            }

            // Validate all accounts (using DataContext for complex query)
            var cuentaIds = dto.Movimientos.Select(m => m.CuentaContableId).Distinct().ToList();
            var cuentas = await _context.CuentasContables
                .Where(c => cuentaIds.Contains(c.Id) && !c.IsDeleted)
                .ToListAsync();

            if (cuentas.Count != cuentaIds.Count)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "Una o más cuentas contables no existen"
                });
            }

            var cuentasNoAceptan = cuentas.Where(c => !c.AceptaMovimientos).ToList();
            if (cuentasNoAceptan.Any())
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"Las siguientes cuentas no aceptan movimientos: {string.Join(", ", cuentasNoAceptan.Select(c => c.Codigo))}"
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update asiento properties
            asiento.Fecha = dto.Fecha;
            asiento.PeriodoContableId = dto.PeriodoContableId;
            asiento.TipoAsiento = dto.TipoAsiento.ToUpper();
            asiento.Concepto = dto.Concepto;
            asiento.Referencia = dto.Referencia;
            asiento.ModuloOrigen = dto.ModuloOrigen?.ToUpper();
            asiento.DocumentoOrigenId = dto.DocumentoOrigenId;
            asiento.TotalDebe = totalDebe;
            asiento.TotalHaber = totalHaber;
            asiento.Observaciones = dto.Observaciones;
            asiento.ModificadoPorId = userId;

            // Update movements collection
            asiento.Movimientos = dto.Movimientos.Select(m => new MovimientoContable
            {
                Id = Guid.NewGuid(),
                AsientoContableId = asiento.Id,
                CuentaContableId = m.CuentaContableId,
                NumeroLinea = m.NumeroLinea,
                Descripcion = m.Descripcion,
                Debe = m.Debe,
                Haber = m.Haber,
                CentroCosto = m.CentroCosto,
                Tercero = m.Tercero,
                ClienteId = m.ClienteId,
                ProveedorId = m.ProveedorId,
                DocumentoReferencia = m.DocumentoReferencia
            }).ToList();

            // Update using repository
            await _unitOfWork.AsientoContableRepository.UpdateAsync(asiento);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Asiento contable {Numero} updated by user {UserId}", asiento.Numero, userId);

            return Ok(new ActionResponse<AsientoContable>
            {
                WasSuccess = true,
                Message = "Asiento contable actualizado exitosamente",
                Result = asiento
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asiento contable {Id}", id);
            return BadRequest(new ActionResponse<AsientoContable>
            {
                WasSuccess = false,
                Message = $"Error al actualizar el asiento contable: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// DELETE /api/asientoscontables/{id}
    /// Delete an accounting entry (only BOR status, soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var asiento = await _unitOfWork.AsientoContableRepository.GetAsync(id);

            if (asiento == null)
            {
                return NotFound(new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Asiento contable no encontrado"
                });
            }

            if (!await TieneAccesoEmpresaAsync(asiento.EmpresaId))
            {
                return Forbid();
            }

            // Only BOR (draft) entries can be deleted
            if (asiento.Estado != "BOR")
            {
                return BadRequest(new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = $"No se puede eliminar un asiento en estado {asiento.EstadoDescripcion}. Solo se pueden eliminar asientos en Borrador."
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Delete using repository
            await _unitOfWork.AsientoContableRepository.DeleteAsync(id, userId!);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Asiento contable {Numero} deleted by user {UserId}", asiento.Numero, userId);

            return Ok(new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = "Asiento contable eliminado exitosamente",
                Result = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asiento contable {Id}", id);
            return BadRequest(new ActionResponse<bool>
            {
                WasSuccess = false,
                Message = $"Error al eliminar el asiento contable: {ex.Message}"
            });
        }
    }

    #endregion

    #region State Transition Endpoints

    /// <summary>
    /// POST /api/asientoscontables/{id}/aprobar
    /// Approve an accounting entry (BOR -> APR) and update account balances
    /// </summary>
    [HttpPost("{id}/aprobar")]
    public async Task<IActionResult> AprobarAsync(Guid id)
    {
        try
        {
            var asiento = await _unitOfWork.AsientoContableRepository.GetWithMovimientosAsync(id);

            if (asiento == null)
            {
                return NotFound(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "Asiento contable no encontrado"
                });
            }

            if (!await TieneAccesoEmpresaAsync(asiento.EmpresaId))
            {
                return Forbid();
            }

            // Verify entry is in BOR status
            if (asiento.Estado != "BOR")
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"No se puede aprobar un asiento en estado {asiento.EstadoDescripcion}. Solo se pueden aprobar asientos en Borrador."
                });
            }

            // Verify entry is balanced
            if (!asiento.EstaBalanceado)
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"El asiento no está balanceado. Diferencia: {asiento.Diferencia:N2}"
                });
            }

            // Verify period is still open
            var periodo = await _unitOfWork.PeriodoContableRepository.GetAsync(asiento.PeriodoContableId);

            if (periodo == null || periodo.Estado != "ABT")
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "El período contable está cerrado. No se pueden aprobar asientos."
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Update account balances (complex business logic requiring DataContext)
            foreach (var movimiento in asiento.Movimientos!)
            {
                var cuenta = await _context.CuentasContables
                    .FirstOrDefaultAsync(c => c.Id == movimiento.CuentaContableId);

                if (cuenta != null)
                {
                    // Update balance based on account nature
                    if (cuenta.Naturaleza == "D") // Debit nature
                    {
                        cuenta.SaldoActual += movimiento.Debe - movimiento.Haber;
                    }
                    else // Credit nature
                    {
                        cuenta.SaldoActual += movimiento.Haber - movimiento.Debe;
                    }
                }
            }

            // Update period totals
            periodo.TotalDebe += asiento.TotalDebe;
            periodo.TotalHaber += asiento.TotalHaber;
            periodo.CantidadAsientos += 1;
            periodo.ModificadoPorId = userId;

            // Approve using repository
            await _unitOfWork.AsientoContableRepository.AprobarAsync(id, userId!);
            await _unitOfWork.PeriodoContableRepository.UpdateAsync(periodo);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Asiento contable {Numero} approved by user {UserId}", asiento.Numero, userId);

            return Ok(new ActionResponse<AsientoContable>
            {
                WasSuccess = true,
                Message = "Asiento contable aprobado exitosamente",
                Result = asiento
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving asiento contable {Id}", id);
            return BadRequest(new ActionResponse<AsientoContable>
            {
                WasSuccess = false,
                Message = $"Error al aprobar el asiento contable: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// POST /api/asientoscontables/{id}/anular
    /// Void an accounting entry (APR -> ANU) and reverse account balances
    /// </summary>
    [HttpPost("{id}/anular")]
    public async Task<IActionResult> AnularAsync(Guid id)
    {
        try
        {
            var asiento = await _unitOfWork.AsientoContableRepository.GetWithMovimientosAsync(id);

            if (asiento == null)
            {
                return NotFound(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "Asiento contable no encontrado"
                });
            }

            if (!await TieneAccesoEmpresaAsync(asiento.EmpresaId))
            {
                return Forbid();
            }

            // Verify entry is in APR status
            if (asiento.Estado != "APR")
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = $"No se puede anular un asiento en estado {asiento.EstadoDescripcion}. Solo se pueden anular asientos Aprobados."
                });
            }

            // Verify period is still open (some systems allow voiding in closed periods)
            var periodo = await _unitOfWork.PeriodoContableRepository.GetAsync(asiento.PeriodoContableId);

            if (periodo == null || periodo.Estado != "ABT")
            {
                return BadRequest(new ActionResponse<AsientoContable>
                {
                    WasSuccess = false,
                    Message = "El período contable está cerrado. No se pueden anular asientos."
                });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Reverse account balances (complex business logic requiring DataContext)
            foreach (var movimiento in asiento.Movimientos!)
            {
                var cuenta = await _context.CuentasContables
                    .FirstOrDefaultAsync(c => c.Id == movimiento.CuentaContableId);

                if (cuenta != null)
                {
                    // Reverse the balance update
                    if (cuenta.Naturaleza == "D") // Debit nature
                    {
                        cuenta.SaldoActual -= movimiento.Debe - movimiento.Haber;
                    }
                    else // Credit nature
                    {
                        cuenta.SaldoActual -= movimiento.Haber - movimiento.Debe;
                    }
                }
            }

            // Update period totals (subtract)
            periodo.TotalDebe -= asiento.TotalDebe;
            periodo.TotalHaber -= asiento.TotalHaber;
            periodo.CantidadAsientos -= 1;
            periodo.ModificadoPorId = userId;

            // Void using repository
            await _unitOfWork.AsientoContableRepository.AnularAsync(id, userId!);
            await _unitOfWork.PeriodoContableRepository.UpdateAsync(periodo);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Asiento contable {Numero} voided by user {UserId}", asiento.Numero, userId);

            return Ok(new ActionResponse<AsientoContable>
            {
                WasSuccess = true,
                Message = "Asiento contable anulado exitosamente",
                Result = asiento
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding asiento contable {Id}", id);
            return BadRequest(new ActionResponse<AsientoContable>
            {
                WasSuccess = false,
                Message = $"Error al anular el asiento contable: {ex.Message}"
            });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Verifica si el usuario tiene acceso a una empresa específica
    /// SuperUser tiene acceso a todas las empresas
    /// Otros usuarios solo tienen acceso a sus empresas asignadas
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // SuperUser tiene acceso a todas las empresas
        if (userRoles.Contains("SuperUser"))
        {
            return true;
        }

        // Otros usuarios solo tienen acceso a sus empresas asignadas
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }

    #endregion
}
