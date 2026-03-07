using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ConfiguracionContableController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IContabilidadUnitOfWork _unitOfWork;
    private readonly ILogger<ConfiguracionContableController> _logger;

    public ConfiguracionContableController(
        DataContext context,
        IContabilidadUnitOfWork unitOfWork,
        ILogger<ConfiguracionContableController> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la configuración contable de una empresa. Si no existe, crea una con valores por defecto.
    /// GET /api/configuracioncontable/empresa/{empresaId}
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var configuracion = await _context.ConfiguracionesContables
                .Where(c => c.EmpresaId == empresaId)
                .Include(c => c.Empresa)
                .Include(c => c.CuentaVentasGravadas)
                .Include(c => c.CuentaVentasExentas)
                .Include(c => c.CuentaIvaDebito)
                .Include(c => c.CuentaIvaCredito)
                .Include(c => c.CuentaClientes)
                .Include(c => c.CuentaProveedores)
                .Include(c => c.CuentaInventario)
                .Include(c => c.CuentaCostoVentas)
                .Include(c => c.CuentaCajaGeneral)
                .Include(c => c.CuentaBancosColones)
                .Include(c => c.CuentaBancosDolares)
                .Include(c => c.CuentaDifCambiariaGanancia)
                .Include(c => c.CuentaDifCambiariaPerdida)
                .Include(c => c.CuentaUtilidadEjercicio)
                .Include(c => c.CuentaPerdidaEjercicio)
                .Include(c => c.CuentaSalariosPorPagar)
                .Include(c => c.CuentaAguinaldoPorPagar)
                .Include(c => c.CuentaVacacionesPorPagar)
                .Include(c => c.CuentaCesantiaPorPagar)
                .Include(c => c.CuentaCCSSPatronal)
                .Include(c => c.CuentaCCSSObrero)
                .Include(c => c.CuentaINSPatronal)
                .Include(c => c.CuentaRetencionISR)
                .Include(c => c.CuentaGastoSalarios)
                .Include(c => c.CuentaGastoCargasSociales)
                .Include(c => c.CuentaGastoAguinaldo)
                .Include(c => c.CuentaGastoVacaciones)
                .Include(c => c.CuentaGastoCesantia)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync();

            // Si no existe configuración, crear una con valores por defecto
            if (configuracion == null)
            {
                _logger.LogInformation("No existe configuración contable para empresa {EmpresaId}. Creando configuración por defecto.", empresaId);

                configuracion = CrearConfiguracionPorDefecto(empresaId);

                _context.ConfiguracionesContables.Add(configuracion);
                await _context.SaveChangesAsync();

                // Recargar con navegación
                configuracion = await _context.ConfiguracionesContables
                    .Where(c => c.Id == configuracion.Id)
                    .Include(c => c.Empresa)
                    .Include(c => c.CreadoPor)
                    .FirstOrDefaultAsync();
            }

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración contable para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la configuración contable por ID
    /// GET /api/configuracioncontable/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var configuracion = await _context.ConfiguracionesContables
                .Include(c => c.Empresa)
                .Include(c => c.CuentaVentasGravadas)
                .Include(c => c.CuentaVentasExentas)
                .Include(c => c.CuentaIvaDebito)
                .Include(c => c.CuentaIvaCredito)
                .Include(c => c.CuentaClientes)
                .Include(c => c.CuentaProveedores)
                .Include(c => c.CuentaInventario)
                .Include(c => c.CuentaCostoVentas)
                .Include(c => c.CuentaCajaGeneral)
                .Include(c => c.CuentaBancosColones)
                .Include(c => c.CuentaBancosDolares)
                .Include(c => c.CuentaDifCambiariaGanancia)
                .Include(c => c.CuentaDifCambiariaPerdida)
                .Include(c => c.CuentaUtilidadEjercicio)
                .Include(c => c.CuentaPerdidaEjercicio)
                .Include(c => c.CuentaSalariosPorPagar)
                .Include(c => c.CuentaAguinaldoPorPagar)
                .Include(c => c.CuentaVacacionesPorPagar)
                .Include(c => c.CuentaCesantiaPorPagar)
                .Include(c => c.CuentaCCSSPatronal)
                .Include(c => c.CuentaCCSSObrero)
                .Include(c => c.CuentaINSPatronal)
                .Include(c => c.CuentaRetencionISR)
                .Include(c => c.CuentaGastoSalarios)
                .Include(c => c.CuentaGastoCargasSociales)
                .Include(c => c.CuentaGastoAguinaldo)
                .Include(c => c.CuentaGastoVacaciones)
                .Include(c => c.CuentaGastoCesantia)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (configuracion == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(configuracion.EmpresaId))
            {
                return Forbid();
            }

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración contable {Id}", id);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea o actualiza la configuración contable.
    /// POST /api/configuracioncontable
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ConfiguracionContable configuracion)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(configuracion.EmpresaId))
            {
                return Forbid();
            }

            // Check if config already exists for this empresa
            var existente = await _context.ConfiguracionesContables
                .FirstOrDefaultAsync(c => c.EmpresaId == configuracion.EmpresaId);

            if (existente != null)
            {
                // Update existing - delegate to PUT logic
                configuracion.Id = existente.Id;
                configuracion.FechaCreacion = existente.FechaCreacion;
                configuracion.CreadoPorId = existente.CreadoPorId;
                configuracion.FechaModificacion = DateTime.Now;
                configuracion.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _context.Entry(existente).CurrentValues.SetValues(configuracion);
            }
            else
            {
                // Create new
                configuracion.Id = Guid.NewGuid();
                configuracion.FechaCreacion = DateTime.Now;
                configuracion.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.ConfiguracionesContables.Add(configuracion);
            }

            await _context.SaveChangesAsync();

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear/actualizar configuración contable");
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Inicializa la configuración contable para una empresa con valores por defecto de Costa Rica.
    /// POST /api/configuracioncontable/inicializar/{empresaId}
    /// </summary>
    [HttpPost("inicializar/{empresaId:guid}")]
    public async Task<IActionResult> InicializarAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            // Verificar si ya existe configuración
            var existente = await _context.ConfiguracionesContables
                .FirstOrDefaultAsync(c => c.EmpresaId == empresaId);

            if (existente != null)
            {
                return BadRequest("Ya existe una configuración contable para esta empresa.");
            }

            // Crear configuración con valores por defecto de Costa Rica
            var configuracion = CrearConfiguracionPorDefecto(empresaId);

            _context.ConfiguracionesContables.Add(configuracion);
            await _context.SaveChangesAsync();

            // Recargar con navegación
            configuracion = await _context.ConfiguracionesContables
                .Where(c => c.Id == configuracion.Id)
                .Include(c => c.Empresa)
                .Include(c => c.CreadoPor)
                .FirstOrDefaultAsync();

            _logger.LogInformation("Configuración contable inicializada para empresa {EmpresaId} por usuario {UserId}",
                empresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar configuración contable para empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza la configuración contable.
    /// PUT /api/configuracioncontable/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] ConfiguracionContable configuracion)
    {
        try
        {
            if (id != configuracion.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _context.ConfiguracionesContables
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existente == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Validar cuentas contables si están asignadas
            var cuentasIds = new List<Guid?>();
            cuentasIds.Add(configuracion.CuentaVentasGravadasId);
            cuentasIds.Add(configuracion.CuentaVentasExentasId);
            cuentasIds.Add(configuracion.CuentaIvaDebitoId);
            cuentasIds.Add(configuracion.CuentaIvaCreditoId);
            cuentasIds.Add(configuracion.CuentaClientesId);
            cuentasIds.Add(configuracion.CuentaProveedoresId);
            cuentasIds.Add(configuracion.CuentaInventarioId);
            cuentasIds.Add(configuracion.CuentaCostoVentasId);
            cuentasIds.Add(configuracion.CuentaCajaGeneralId);
            cuentasIds.Add(configuracion.CuentaBancosColonesId);
            cuentasIds.Add(configuracion.CuentaBancosDolaresId);
            cuentasIds.Add(configuracion.CuentaDifCambiariaGananciaId);
            cuentasIds.Add(configuracion.CuentaDifCambiariaPerdidaId);
            cuentasIds.Add(configuracion.CuentaUtilidadEjercicioId);
            cuentasIds.Add(configuracion.CuentaPerdidaEjercicioId);
            cuentasIds.Add(configuracion.CuentaSalariosPorPagarId);
            cuentasIds.Add(configuracion.CuentaAguinaldoPorPagarId);
            cuentasIds.Add(configuracion.CuentaVacacionesPorPagarId);
            cuentasIds.Add(configuracion.CuentaCesantiaPorPagarId);
            cuentasIds.Add(configuracion.CuentaCCSSPatronalId);
            cuentasIds.Add(configuracion.CuentaCCSSObreroId);
            cuentasIds.Add(configuracion.CuentaINSPatronalId);
            cuentasIds.Add(configuracion.CuentaRetencionISRId);
            cuentasIds.Add(configuracion.CuentaGastoSalariosId);
            cuentasIds.Add(configuracion.CuentaGastoCargasSocialesId);
            cuentasIds.Add(configuracion.CuentaGastoAguinaldoId);
            cuentasIds.Add(configuracion.CuentaGastoVacacionesId);
            cuentasIds.Add(configuracion.CuentaGastoCesantiaId);

            var cuentasIdsNoNulas = cuentasIds.Where(id => id.HasValue).Select(id => id!.Value).ToList();

            if (cuentasIdsNoNulas.Any())
            {
                // Validate each account using repository
                var cuentasInvalidas = new List<Guid>();
                foreach (var cuentaId in cuentasIdsNoNulas)
                {
                    var cuenta = await _unitOfWork.CuentaContableRepository.GetAsync(cuentaId);
                    if (cuenta == null || cuenta.EmpresaId != existente.EmpresaId)
                    {
                        cuentasInvalidas.Add(cuentaId);
                    }
                }

                if (cuentasInvalidas.Any())
                {
                    return BadRequest($"Las siguientes cuentas no existen o no pertenecen a la empresa: {string.Join(", ", cuentasInvalidas)}");
                }
            }

            // Preservar campos de auditoría
            configuracion.EmpresaId = existente.EmpresaId;
            configuracion.FechaCreacion = existente.FechaCreacion;
            configuracion.CreadoPorId = existente.CreadoPorId;
            configuracion.FechaModificacion = DateTime.Now;
            configuracion.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Entry(existente).CurrentValues.SetValues(configuracion);
            await _context.SaveChangesAsync();

            // Recargar con navegación
            configuracion = await _context.ConfiguracionesContables
                .Where(c => c.Id == id)
                .Include(c => c.Empresa)
                .Include(c => c.CuentaVentasGravadas)
                .Include(c => c.CuentaVentasExentas)
                .Include(c => c.CuentaIvaDebito)
                .Include(c => c.CuentaIvaCredito)
                .Include(c => c.CuentaClientes)
                .Include(c => c.CuentaProveedores)
                .Include(c => c.CuentaInventario)
                .Include(c => c.CuentaCostoVentas)
                .Include(c => c.CuentaCajaGeneral)
                .Include(c => c.CuentaBancosColones)
                .Include(c => c.CuentaBancosDolares)
                .Include(c => c.CuentaDifCambiariaGanancia)
                .Include(c => c.CuentaDifCambiariaPerdida)
                .Include(c => c.CuentaUtilidadEjercicio)
                .Include(c => c.CuentaPerdidaEjercicio)
                .Include(c => c.CuentaSalariosPorPagar)
                .Include(c => c.CuentaAguinaldoPorPagar)
                .Include(c => c.CuentaVacacionesPorPagar)
                .Include(c => c.CuentaCesantiaPorPagar)
                .Include(c => c.CuentaCCSSPatronal)
                .Include(c => c.CuentaCCSSObrero)
                .Include(c => c.CuentaINSPatronal)
                .Include(c => c.CuentaRetencionISR)
                .Include(c => c.CuentaGastoSalarios)
                .Include(c => c.CuentaGastoCargasSociales)
                .Include(c => c.CuentaGastoAguinaldo)
                .Include(c => c.CuentaGastoVacaciones)
                .Include(c => c.CuentaGastoCesantia)
                .Include(c => c.ModificadoPor)
                .FirstOrDefaultAsync();

            _logger.LogInformation("Configuración contable actualizada para empresa {EmpresaId} por usuario {UserId}",
                existente.EmpresaId, User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuración contable {Id}", id);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una configuración contable con valores por defecto para Costa Rica.
    /// </summary>
    private ConfiguracionContable CrearConfiguracionPorDefecto(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return new ConfiguracionContable
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,

            // Período Fiscal - Costa Rica (Ley 9635/2020)
            MesInicioPeriodoFiscal = 1,  // Enero
            DiaInicioPeriodoFiscal = 1,
            NumeroPeriodosPorAnio = 12,  // Mensual

            // Moneda - Costa Rica
            MonedaBase = "CRC",
            DecimalesMoneda = 2,
            DecimalesTipoCambio = 4,

            // Numeración de Asientos
            TipoNumeracion = "PER",  // Por período
            UsarPrefijoNumero = false,
            FormatoPrefijo = null,
            LongitudNumero = 6,

            // Control de Períodos
            PeriodosAbiertosSimultaneos = 2,
            PermitirMovimientosFuturos = false,
            BloquearPeriodosCerrados = true,

            // Generación Automática de Asientos
            GenerarAsientosAutomaticos = true,
            FrecuenciaGeneracion = "INM",  // Inmediata
            AprobarAsientosAutomaticos = false,

            // Cierre Contable
            RequiereAprobacionCierre = true,
            ValidarBalanceAntesCierre = true,
            GenerarAsientoCierreAutomatico = true,

            // Diferencias y Ajustes
            ToleranciaDiferencias = 0.01m,
            RegistrarDiferenciasCambiarias = true,

            // Workflow y Aprobaciones
            RequiereAprobacionAsientos = true,
            MontoLimiteSinAprobacion = 100000m,
            NivelesAprobacion = 1,

            // Cuentas Principales - No asignadas por defecto
            CuentaVentasGravadasId = null,
            CuentaVentasExentasId = null,
            CuentaIvaDebitoId = null,
            CuentaIvaCreditoId = null,
            CuentaClientesId = null,
            CuentaProveedoresId = null,
            CuentaInventarioId = null,
            CuentaCostoVentasId = null,
            CuentaCajaGeneralId = null,
            CuentaBancosColonesId = null,
            CuentaBancosDolaresId = null,
            CuentaDifCambiariaGananciaId = null,
            CuentaDifCambiariaPerdidaId = null,
            CuentaUtilidadEjercicioId = null,
            CuentaPerdidaEjercicioId = null,

            // Cuentas de Planilla - No asignadas por defecto
            CuentaSalariosPorPagarId = null,
            CuentaAguinaldoPorPagarId = null,
            CuentaVacacionesPorPagarId = null,
            CuentaCesantiaPorPagarId = null,
            CuentaCCSSPatronalId = null,
            CuentaCCSSObreroId = null,
            CuentaINSPatronalId = null,
            CuentaRetencionISRId = null,
            CuentaGastoSalariosId = null,
            CuentaGastoCargasSocialesId = null,
            CuentaGastoAguinaldoId = null,
            CuentaGastoVacacionesId = null,
            CuentaGastoCesantiaId = null,

            // Auditoría
            FechaCreacion = DateTime.Now,
            CreadoPorId = userId
        };
    }

    /// <summary>
    /// Verifica si el usuario tiene acceso a la empresa.
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // Verificar si es SuperUser
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
