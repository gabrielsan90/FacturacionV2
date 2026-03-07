using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Facturacion.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser,Administrador de Empresa,Contador")]
[ApiController]
[Route("api/[controller]")]
public class MayorizacionController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IContabilidadIntegracionService _contabilidadService;
    private readonly IContabilidadUnitOfWork _contabilidadUoW;
    private readonly ILogger<MayorizacionController> _logger;

    public MayorizacionController(
        DataContext context,
        IContabilidadIntegracionService contabilidadService,
        IContabilidadUnitOfWork contabilidadUoW,
        ILogger<MayorizacionController> logger)
    {
        _context = context;
        _contabilidadService = contabilidadService;
        _contabilidadUoW = contabilidadUoW;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene documentos y gastos pendientes de mayorización (sin asiento contable).
    /// </summary>
    [HttpGet("pendientes/{empresaId:guid}")]
    public async Task<IActionResult> GetPendientesAsync(Guid empresaId)
    {
        try
        {
            // Multi-tenancy guard
            var userEmpresaId = User.FindFirstValue("EmpresaId");
            if (!User.IsInRole("SuperUser") &&
                (!Guid.TryParse(userEmpresaId, out var userEmpresa) || userEmpresa != empresaId))
                return Forbid();
            // 1. Documentos emitidos aceptados sin asiento contable
            var documentosPendientes = await _context.Documentos
                .AsNoTracking()
                .Where(d => d.EmpresaId == empresaId
                    && d.Estado == EstadoDocumento.Aceptado
                    && d.AsientoContableId == null
                    && !d.EsDocumentoRecibido
                    && d.TipoDocumento != DocumentoTipo.ReciboElectronicoPago)
                .OrderByDescending(d => d.FechaEmision)
                .Select(d => new
                {
                    d.Id,
                    Tipo = "Documento",
                    TipoDocumento = d.TipoDocumento.ToString(),
                    d.NumeroConsecutivo,
                    d.FechaEmision,
                    Receptor = d.ReceptorNombre ?? "Sin receptor",
                    Total = d.TotalVenta,
                    Moneda = d.Moneda.ToString(),
                    Modulo = "VEN"
                })
                .ToListAsync();

            // 2. Gastos aprobados sin asiento contable (LEFT JOIN via subquery)
            var gastosConAsientoIds = _context.AsientosContables
                .Where(a => a.EmpresaId == empresaId
                    && a.ModuloOrigen == ModulosContables.Compras
                    && !a.IsDeleted)
                .Select(a => a.DocumentoOrigenId);

            var gastosPendientes = await _context.Gastos
                .AsNoTracking()
                .Include(g => g.Proveedor)
                .Where(g => g.EmpresaId == empresaId
                    && g.Aprobado
                    && !g.IsDeleted
                    && !gastosConAsientoIds.Contains(g.Id))
                .OrderByDescending(g => g.FechaGasto)
                .Select(g => new
                {
                    g.Id,
                    Tipo = "Gasto",
                    TipoDocumento = "Compra",
                    NumeroConsecutivo = g.NumeroDocumento,
                    FechaEmision = g.FechaGasto,
                    Receptor = g.Proveedor != null ? g.Proveedor.Nombre : "Sin proveedor",
                    Total = g.MontoTotal,
                    Moneda = g.Moneda.ToString(),
                    Modulo = "COM"
                })
                .ToListAsync();

            return Ok(new
            {
                documentos = documentosPendientes,
                gastos = gastosPendientes,
                totalDocumentos = documentosPendientes.Count,
                totalGastos = gastosPendientes.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pendientes de mayorización para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error al obtener documentos pendientes de mayorización.");
        }
    }

    /// <summary>
    /// Genera asientos contables para los documentos/gastos seleccionados.
    /// </summary>
    [HttpPost("generar")]
    public async Task<IActionResult> GenerarAsync([FromBody] MayorizacionRequest request)
    {
        if (request?.Items == null || request.Items.Count == 0)
            return BadRequest("Debe seleccionar al menos un documento.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        int exitosos = 0, errores = 0;
        var detallesErrores = new List<string>();

        foreach (var item in request.Items)
        {
            try
            {
                if (item.Tipo == "Documento")
                {
                    var doc = await _context.Documentos
                        .Include(d => d.Detalles)
                        .Include(d => d.Empresa)
                        .FirstOrDefaultAsync(d => d.Id == item.Id);

                    if (doc == null)
                    {
                        detallesErrores.Add($"Documento {item.Id}: No encontrado");
                        errores++;
                        continue;
                    }

                    if (doc.AsientoContableId != null)
                    {
                        detallesErrores.Add($"{doc.NumeroConsecutivo}: Ya tiene asiento contable");
                        errores++;
                        continue;
                    }

                    // Pre-diagnóstico: verificar requisitos antes de llamar al servicio
                    var diagnostico = await DiagnosticarDocumentoAsync(doc);
                    if (diagnostico != null)
                    {
                        detallesErrores.Add($"{doc.NumeroConsecutivo}: {diagnostico}");
                        errores++;
                        continue;
                    }

                    AsientoContable? asiento = doc.TipoDocumento switch
                    {
                        DocumentoTipo.FacturaElectronica or DocumentoTipo.TiqueteElectronico or DocumentoTipo.FacturaElectronicaExportacion
                            => await _contabilidadService.GenerarAsientoVentaAsync(doc, userId, forzar: true),
                        DocumentoTipo.NotaCreditoElectronica
                            => await _contabilidadService.GenerarAsientoNotaCreditoVentaAsync(doc, userId, forzar: true),
                        DocumentoTipo.NotaDebitoElectronica
                            => await _contabilidadService.GenerarAsientoNotaDebitoVentaAsync(doc, userId, forzar: true),
                        _ => null
                    };

                    if (asiento != null)
                    {
                        doc.AsientoContableId = asiento.Id;
                        await _context.SaveChangesAsync();
                        exitosos++;
                    }
                    else
                    {
                        detallesErrores.Add($"{doc.NumeroConsecutivo}: Asiento desbalanceado o sin movimientos. " +
                            $"Totales: Gravado={doc.TotalGravado}, Exento={doc.TotalExento}, " +
                            $"Exonerado={doc.TotalExonerado}, Descuentos={doc.TotalDescuentos}, " +
                            $"Impuestos={doc.TotalImpuestos}, OtrosCargos={doc.TotalOtrosCargos}, " +
                            $"TotalVenta={doc.TotalVenta}. " +
                            $"Revise las cuentas de integración y que los montos del documento sean consistentes.");
                        errores++;
                    }
                }
                else if (item.Tipo == "Gasto")
                {
                    var gasto = await _context.Gastos
                        .Include(g => g.Proveedor)
                        .FirstOrDefaultAsync(g => g.Id == item.Id);

                    if (gasto == null)
                    {
                        detallesErrores.Add($"Gasto {item.Id}: No encontrado");
                        errores++;
                        continue;
                    }

                    // Check idempotency
                    var yaExiste = await _contabilidadService.ExisteAsientoParaDocumentoAsync(
                        gasto.EmpresaId, ModulosContables.Compras, gasto.Id);
                    if (yaExiste)
                    {
                        detallesErrores.Add($"{gasto.NumeroDocumento}: Ya tiene asiento contable");
                        errores++;
                        continue;
                    }

                    var asiento = await _contabilidadService.GenerarAsientoCompraAsync(gasto, userId, forzar: true);
                    if (asiento != null)
                    {
                        exitosos++;
                    }
                    else
                    {
                        detallesErrores.Add($"{gasto.NumeroDocumento}: No se pudo generar asiento (verifique cuentas de integración y período contable abierto)");
                        errores++;
                    }
                }
            }
            catch (Exception ex)
            {
                errores++;
                detallesErrores.Add($"Error procesando {item.Tipo} {item.Id}: {ex.Message}");
                _logger.LogError(ex, "Error generando asiento para {Tipo} {Id}", item.Tipo, item.Id);
            }
        }

        return Ok(new
        {
            exitosos,
            errores,
            total = request.Items.Count,
            detallesErrores
        });
    }

    /// <summary>
    /// Verifica los requisitos para generar un asiento y retorna mensaje de error específico, o null si todo está OK.
    /// </summary>
    private async Task<string?> DiagnosticarDocumentoAsync(Documento doc)
    {
        // 1. Verificar idempotencia
        string modulo = ModulosContables.Ventas;
        if (await _contabilidadService.ExisteAsientoParaDocumentoAsync(doc.EmpresaId, modulo, doc.Id))
            return "Ya existe un asiento contable para este documento (en tabla AsientosContables)";

        // 2. Determinar tipo de operación
        string tipoOperacion = doc.TipoDocumento switch
        {
            DocumentoTipo.NotaCreditoElectronica => TiposOperacionContable.NotaCreditoVenta,
            DocumentoTipo.NotaDebitoElectronica => TiposOperacionContable.NotaDebitoVenta,
            _ => doc.CondicionVenta == "01"
                ? TiposOperacionContable.VentaContado
                : TiposOperacionContable.VentaCredito
        };

        // 3. Verificar cuentas de integración
        var cuentas = await _contabilidadUoW.CuentaIntegracionRepository
            .GetByModuloYTipoOperacionAsync(doc.EmpresaId, modulo, tipoOperacion);

        if (!cuentas.Any())
            return $"No hay cuentas de integración configuradas para módulo '{modulo}' / operación '{tipoOperacion}'. Vaya a Contabilidad > Cuentas de Integración.";

        // 4. Verificar período contable abierto
        var periodo = await _contabilidadUoW.PeriodoContableRepository.GetAbiertoAsync(doc.EmpresaId);
        if (periodo == null)
            return "No hay período contable abierto. Vaya a Contabilidad > Períodos Contables y abra el período correspondiente.";

        return null; // Todo OK
    }
}

public class MayorizacionRequest
{
    public List<MayorizacionItem> Items { get; set; } = new();
}

public class MayorizacionItem
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = null!;
}
