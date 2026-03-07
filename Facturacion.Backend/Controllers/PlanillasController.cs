using ClosedXML.Excel;
using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
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
public class PlanillasController : ControllerBase
{
    private readonly IPlanillaUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly IContabilidadIntegracionService _contabilidadIntegracion;
    private readonly ILogger<PlanillasController> _logger;

    public PlanillasController(
        IPlanillaUnitOfWork unitOfWork,
        DataContext context,
        IContabilidadIntegracionService contabilidadIntegracion,
        ILogger<PlanillasController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _contabilidadIntegracion = contabilidadIntegracion;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las planillas de una empresa con filtros opcionales.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(
        Guid empresaId,
        [FromQuery] int? anio = null,
        [FromQuery] int? mes = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? tipoPlanilla = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var planillas = await _unitOfWork.PlanillaRepository.GetByEmpresaAsync(empresaId);

            // Aplicar filtros
            if (anio.HasValue)
            {
                planillas = planillas.Where(p => p.Anio == anio.Value);
            }

            if (mes.HasValue)
            {
                planillas = planillas.Where(p => p.Mes == mes.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                planillas = planillas.Where(p => p.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(tipoPlanilla))
            {
                planillas = planillas.Where(p => p.TipoPlanilla == tipoPlanilla);
            }

            var resultado = planillas
                .OrderByDescending(p => p.Anio)
                .ThenByDescending(p => p.Mes)
                .ThenByDescending(p => p.Periodo)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Anio,
                    p.Mes,
                    p.Periodo,
                    p.TipoPlanilla,
                    p.TipoPlanillaDescripcion,
                    p.Descripcion,
                    p.FechaInicio,
                    p.FechaFin,
                    p.FechaPago,
                    p.TotalSalarioBruto,
                    p.TotalDeducciones,
                    p.TotalSalarioNeto,
                    p.TotalCargasSociales,
                    p.TotalCCSSPatronal,
                    p.TotalINS,
                    p.Estado,
                    p.EstadoDescripcion,
                    p.FechaAprobacion,
                    AprobadoPor = p.AprobadoPor?.FullName,
                    p.PeriodoDescripcion,
                    p.CostoTotal,
                    p.FechaCreacion,
                    CantidadEmpleados = p.Detalles != null ? p.Detalles.Count : 0
                })
                .ToList();

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo planillas de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener las planillas.");
        }
    }

    /// <summary>
    /// Obtiene una planilla específica con sus detalles y deducciones.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var planilla = await _context.Planillas
            .Include(p => p.AprobadoPor)
            .Include(p => p.Detalles!)
                .ThenInclude(d => d.Empleado)
                    .ThenInclude(e => e!.Departamento)
            .Include(p => p.Detalles!)
                .ThenInclude(d => d.Empleado)
                    .ThenInclude(e => e!.Puesto)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (planilla == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(planilla.EmpresaId))
        {
            return Forbid();
        }

        return Ok(planilla);
    }

    /// <summary>
    /// Genera una nueva planilla para un período específico.
    /// Incluye cálculo automático de salarios, deducciones y cargas sociales.
    /// </summary>
    [HttpPost("generar")]
    public async Task<IActionResult> GenerarPlanillaAsync([FromBody] GenerarPlanillaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await TieneAccesoEmpresaAsync(request.EmpresaId))
        {
            return Forbid();
        }

        // Verificar que no exista una planilla para el mismo período
        var existente = await _context.Planillas
            .FirstOrDefaultAsync(p => p.EmpresaId == request.EmpresaId &&
                                     p.Anio == request.Anio &&
                                     p.Mes == request.Mes &&
                                     p.Periodo == request.Periodo &&
                                     p.TipoPlanilla == request.TipoPlanilla &&
                                     !p.IsDeleted);

        if (existente != null)
        {
            return BadRequest("Ya existe una planilla para este período. Elimine la planilla existente antes de generar una nueva.");
        }

        // Obtener empleados activos de la empresa
        var empleados = await _context.Empleados
            .Where(e => e.EmpresaId == request.EmpresaId &&
                       !e.IsDeleted &&
                       e.Estado == "ACT" &&
                       e.FechaIngreso <= request.FechaFin)
            .ToListAsync();

        if (!empleados.Any())
        {
            return BadRequest("No hay empleados activos para generar la planilla.");
        }

        // Crear la planilla
        var planilla = new Planilla
        {
            Id = Guid.NewGuid(),
            EmpresaId = request.EmpresaId,
            Codigo = await GenerarCodigoPlanillaAsync(request.EmpresaId, request.Anio, request.Mes, request.Periodo),
            Anio = request.Anio,
            Mes = request.Mes,
            Periodo = request.Periodo,
            TipoPlanilla = request.TipoPlanilla,
            Descripcion = request.Descripcion,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Estado = "BOR", // Borrador
            FechaCreacion = DateTime.Now,
            UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Detalles = new List<DetallePlanilla>()
        };

        // Generar detalles de planilla para cada empleado
        foreach (var empleado in empleados)
        {
            var detalle = CalcularDetallePlanilla(empleado, request);
            planilla.Detalles.Add(detalle);
        }

        // Calcular totales de la planilla
        planilla.TotalSalarioBruto = planilla.Detalles.Sum(d => d.SalarioBruto);
        planilla.TotalDeducciones = planilla.Detalles.Sum(d => d.TotalDeducciones);
        planilla.TotalSalarioNeto = planilla.Detalles.Sum(d => d.SalarioNeto);
        planilla.TotalCargasSociales = planilla.Detalles.Sum(d => d.TotalCargasSociales);
        planilla.TotalCCSSPatronal = planilla.Detalles.Sum(d => d.CargaCCSSPatronal);
        planilla.TotalINS = planilla.Detalles.Sum(d => d.CargaINS);

        _context.Planillas.Add(planilla);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            planilla.Id,
            planilla.Codigo,
            planilla.Estado,
            EmpleadosProcesados = empleados.Count,
            planilla.TotalSalarioBruto,
            planilla.TotalDeducciones,
            planilla.TotalSalarioNeto,
            planilla.TotalCargasSociales,
            Mensaje = $"Planilla generada exitosamente con {empleados.Count} empleados."
        });
    }

    /// <summary>
    /// Aprueba una planilla en estado Borrador o Calculada.
    /// </summary>
    [HttpPost("{id:guid}/aprobar")]
    public async Task<IActionResult> AprobarPlanillaAsync(Guid id)
    {
        var planilla = await _context.Planillas
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (planilla == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(planilla.EmpresaId))
        {
            return Forbid();
        }

        if (planilla.Estado != "BOR" && planilla.Estado != "CAL")
        {
            return BadRequest($"Solo se pueden aprobar planillas en estado Borrador o Calculada. Estado actual: {planilla.EstadoDescripcion}");
        }

        planilla.Estado = "APR";
        planilla.FechaAprobacion = DateTime.Now;
        planilla.AprobadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        planilla.FechaModificacion = DateTime.Now;
        planilla.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Planillas.Update(planilla);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            planilla.Id,
            planilla.Estado,
            planilla.EstadoDescripcion,
            planilla.FechaAprobacion,
            Mensaje = "Planilla aprobada exitosamente."
        });
    }

    /// <summary>
    /// Marca una planilla como pagada.
    /// </summary>
    [HttpPost("{id:guid}/pagar")]
    public async Task<IActionResult> PagarPlanillaAsync(Guid id, [FromBody] PagarPlanillaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var planilla = await _context.Planillas
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (planilla == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(planilla.EmpresaId))
        {
            return Forbid();
        }

        if (planilla.Estado != "APR")
        {
            return BadRequest($"Solo se pueden marcar como pagadas las planillas aprobadas. Estado actual: {planilla.EstadoDescripcion}");
        }

        // Marcar planilla como pagada
        planilla.Estado = "PAG";
        planilla.FechaPago = request.FechaPago;
        planilla.FechaModificacion = DateTime.Now;
        planilla.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Marcar todos los detalles como pagados
        if (planilla.Detalles != null)
        {
            foreach (var detalle in planilla.Detalles)
            {
                detalle.EstadoPago = "PAG";
                detalle.FechaPago = request.FechaPago;
            }
        }

        _context.Planillas.Update(planilla);
        await _context.SaveChangesAsync();

        // Generar asiento contable para el pago de planilla (si está habilitado)
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _contabilidadIntegracion.GenerarAsientoPlanillaAsync(planilla, userId!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generando asiento contable para planilla {PlanillaId} - operación continúa", planilla.Id);
        }

        return Ok(new
        {
            planilla.Id,
            planilla.Estado,
            planilla.EstadoDescripcion,
            planilla.FechaPago,
            EmpleadosPagados = planilla.Detalles?.Count ?? 0,
            planilla.TotalSalarioNeto,
            Mensaje = "Planilla marcada como pagada exitosamente."
        });
    }

    /// <summary>
    /// Actualiza una planilla existente (solo en estado Borrador).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Planilla planilla)
    {
        if (id != planilla.Id)
        {
            return BadRequest("El ID no coincide.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existente = await _context.Planillas
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (existente == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
        {
            return Forbid();
        }

        if (existente.Estado != "BOR")
        {
            return BadRequest("Solo se pueden modificar planillas en estado Borrador.");
        }

        // Preservar campos de auditoría
        planilla.FechaCreacion = existente.FechaCreacion;
        planilla.UsuarioCreacionId = existente.UsuarioCreacionId;
        planilla.FechaModificacion = DateTime.Now;
        planilla.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Entry(existente).CurrentValues.SetValues(planilla);
        await _context.SaveChangesAsync();

        return Ok(planilla);
    }

    /// <summary>
    /// Elimina una planilla (soft delete). Solo se pueden eliminar planillas en estado Borrador.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var planilla = await _context.Planillas
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (planilla == null)
        {
            return NotFound();
        }

        if (!await TieneAccesoEmpresaAsync(planilla.EmpresaId))
        {
            return Forbid();
        }

        if (planilla.Estado != "BOR")
        {
            return BadRequest("Solo se pueden eliminar planillas en estado Borrador. Use el estado Anulada para planillas aprobadas.");
        }

        // Soft delete
        planilla.IsDeleted = true;
        planilla.FechaEliminacion = DateTime.Now;
        planilla.UsuarioEliminacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Eliminar detalles también
        if (planilla.Detalles != null)
        {
            _context.DetallesPlanilla.RemoveRange(planilla.Detalles);
        }

        _context.Planillas.Update(planilla);
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Exporta planilla en formato CCSS (SICERE) a Excel.
    /// Solo incluye empleados con AportaCCSS = true.
    /// </summary>
    [HttpGet("{id:guid}/exportar-ccss")]
    public async Task<IActionResult> ExportarCCSSAsync(Guid id)
    {
        try
        {
            var planilla = await _context.Planillas
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Empleado)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (planilla == null)
            {
                return NotFound();
            }

            if (!await TieneAccesoEmpresaAsync(planilla.EmpresaId))
            {
                return Forbid();
            }

            var detalles = (planilla.Detalles ?? new List<DetallePlanilla>())
                .Where(d => d.Empleado != null && d.Empleado.AportaCCSS)
                .OrderBy(d => d.Empleado!.PrimerApellido)
                .ThenBy(d => d.Empleado!.Nombre)
                .ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("CCSS_SICERE");

            // Header
            ws.Cell("A1").Value = "Planilla CCSS - Formato SICERE";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 14;
            ws.Range("A1:I1").Merge();

            ws.Cell("A2").Value = $"Período: {planilla.Anio}-{planilla.Mes:D2} P{planilla.Periodo} | {planilla.TipoPlanillaDescripcion}";
            ws.Range("A2:I2").Merge();

            // Column headers
            int row = 4;
            var headers = new[]
            {
                "Tipo ID", "Identificación", "Nombre Completo",
                "Días Laborados", "Salario Bruto",
                "CCSS Obrero (9.67%)", "CCSS Patronal (14.50%)",
                "Otras Instituciones (7.25%)", "Banco Popular (1%)"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.Bold = true;
                ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 102, 51);
                ws.Cell(row, i + 1).Style.Font.FontColor = XLColor.White;
            }

            // Data
            row++;
            decimal sumBruto = 0, sumCCSSObrero = 0, sumCCSSPatronal = 0, sumOtrasInst = 0, sumBP = 0;

            foreach (var det in detalles)
            {
                var emp = det.Empleado!;
                ws.Cell(row, 1).Value = emp.TipoIdentificacion;
                ws.Cell(row, 2).Value = emp.Identificacion;
                ws.Cell(row, 3).Value = emp.NombreCompleto;
                ws.Cell(row, 4).Value = det.DiasLaborados;
                ws.Cell(row, 4).Style.NumberFormat.Format = "0.00";
                ws.Cell(row, 5).Value = det.SalarioBruto;
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 6).Value = det.DeduccionCCSS;
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 7).Value = det.CargaCCSSPatronal;
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 8).Value = det.CargaOtrasInstituciones;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 9).Value = det.DeduccionBancoPopular;
                ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";

                sumBruto += det.SalarioBruto;
                sumCCSSObrero += det.DeduccionCCSS;
                sumCCSSPatronal += det.CargaCCSSPatronal;
                sumOtrasInst += det.CargaOtrasInstituciones;
                sumBP += det.DeduccionBancoPopular;
                row++;
            }

            // Totals
            ws.Cell(row, 3).Value = "TOTALES";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 5).Value = sumBruto;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 6).Value = sumCCSSObrero;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 7).Value = sumCCSSPatronal;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 8).Value = sumOtrasInst;
            ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 8).Style.Font.Bold = true;
            ws.Cell(row, 9).Value = sumBP;
            ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 9).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);

            var fileName = $"CCSS_Planilla_{planilla.Anio}{planilla.Mes:D2}_P{planilla.Periodo}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting CCSS for planilla {PlanillaId}", id);
            return StatusCode(500, "Error al exportar la planilla CCSS");
        }
    }

    #region Métodos privados de cálculo

    /// <summary>
    /// Genera un código único para la planilla.
    /// </summary>
    private async Task<string> GenerarCodigoPlanillaAsync(Guid empresaId, int anio, int mes, int periodo)
    {
        var consecutivo = await _context.Planillas
            .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
            .CountAsync() + 1;

        return $"PLN-{anio}{mes:D2}{periodo}-{consecutivo:D4}";
    }

    /// <summary>
    /// Calcula el detalle de planilla para un empleado según legislación de Costa Rica.
    /// </summary>
    private DetallePlanilla CalcularDetallePlanilla(Empleado empleado, GenerarPlanillaRequest request)
    {
        var detalle = new DetallePlanilla
        {
            Id = Guid.NewGuid(),
            EmpleadoId = empleado.Id,
            SalarioBase = empleado.SalarioBase
        };

        // Calcular días laborados (por defecto 15 para quincenal, ajustar según tipo)
        detalle.DiasLaborados = request.Periodo == 1 || request.Periodo == 2 ? 15 : 30;

        // Calcular salario bruto
        detalle.SalarioBruto = detalle.SalarioBase;

        // Sumar horas extra si se especifican
        detalle.HorasExtra = 0;
        detalle.MontoHorasExtra = 0;

        // Sumar comisiones y bonificaciones
        detalle.Comisiones = 0;
        detalle.Bonificaciones = 0;
        detalle.OtrosIngresos = 0;

        // Total ingresos
        detalle.TotalIngresos = detalle.SalarioBruto + detalle.MontoHorasExtra +
                                detalle.Comisiones + detalle.Bonificaciones + detalle.OtrosIngresos;

        // ===== DEDUCCIONES TRABAJADOR (Costa Rica) =====
        // CCSS Trabajador: 9.67% (SEM 5.50% + IVM 4.17%)
        if (empleado.AportaCCSS)
        {
            detalle.DeduccionCCSS = Math.Round(detalle.TotalIngresos * 0.0967m, 2);
        }

        // Banco Popular: 1%
        detalle.DeduccionBancoPopular = Math.Round(detalle.TotalIngresos * 0.01m, 2);

        // Impuesto sobre la Renta (simplificado - debería tener tabla progresiva)
        // Exoneración: Primer ₡941,000 mensual (aprox ₡470,500 quincenal en 2024)
        decimal baseImponibleRenta = detalle.TotalIngresos - 470500;
        if (baseImponibleRenta > 0)
        {
            // Aplicar tabla progresiva simplificada (10% para este ejemplo)
            detalle.DeduccionRenta = Math.Round(baseImponibleRenta * 0.10m, 2);
        }

        // Otras deducciones
        detalle.DeduccionPensionAlimenticia = 0;
        detalle.DeduccionEmbargo = 0;
        detalle.DeduccionPrestamo = 0;
        detalle.OtrasDeducciones = 0;

        // Total deducciones
        detalle.TotalDeducciones = detalle.DeduccionCCSS + detalle.DeduccionBancoPopular +
                                   detalle.DeduccionRenta + detalle.DeduccionPensionAlimenticia +
                                   detalle.DeduccionEmbargo + detalle.DeduccionPrestamo +
                                   detalle.OtrasDeducciones;

        // Salario Neto
        detalle.SalarioNeto = detalle.TotalIngresos - detalle.TotalDeducciones;

        // ===== CARGAS SOCIALES PATRONALES (Costa Rica) =====
        if (empleado.AportaCCSS)
        {
            // CCSS Patronal: 14.50% (SEM 9.25% + IVM 5.25%)
            detalle.CargaCCSSPatronal = Math.Round(detalle.TotalIngresos * 0.1450m, 2);

            // Otras instituciones: IMAS 0.50%, INA 1.50%, Asignaciones Familiares 5.00%, BP 0.25%
            // Total: 7.25%
            detalle.CargaOtrasInstituciones = Math.Round(detalle.TotalIngresos * 0.0725m, 2);
        }

        // INS (Riesgo de Trabajo): promedio 1.21% según actividad económica
        detalle.CargaINS = Math.Round(detalle.TotalIngresos * 0.0121m, 2);

        // Provisión Aguinaldo: 8.33% (1/12)
        detalle.CargaAguinaldo = Math.Round(detalle.TotalIngresos * 0.0833m, 2);

        // Provisión Vacaciones: 4.16% (1/24)
        detalle.CargaVacaciones = Math.Round(detalle.TotalIngresos * 0.0416m, 2);

        // Provisión Cesantía: 5.33% (1/12 del salario + cargas sociales aprox)
        detalle.CargaCesantia = Math.Round(detalle.TotalIngresos * 0.0533m, 2);

        // Total cargas sociales
        detalle.TotalCargasSociales = detalle.CargaCCSSPatronal + detalle.CargaOtrasInstituciones +
                                      detalle.CargaINS + detalle.CargaAguinaldo +
                                      detalle.CargaVacaciones + detalle.CargaCesantia;

        // Estado del pago
        detalle.EstadoPago = "PEN"; // Pendiente

        return detalle;
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a la empresa especificada.
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

    #endregion
}

#region DTOs de Request

public class GenerarPlanillaRequest
{
    public Guid EmpresaId { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public int Periodo { get; set; } = 1;
    public string TipoPlanilla { get; set; } = "ORD";
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}

public class PagarPlanillaRequest
{
    public DateTime FechaPago { get; set; }
}

#endregion
