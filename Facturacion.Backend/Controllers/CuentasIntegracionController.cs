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
public class CuentasIntegracionController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IContabilidadUnitOfWork _unitOfWork;
    private readonly ILogger<CuentasIntegracionController> _logger;

    public CuentasIntegracionController(
        DataContext context,
        IContabilidadUnitOfWork unitOfWork,
        ILogger<CuentasIntegracionController> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los mapeos de integración de una empresa, opcionalmente filtrados por módulo.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId, [FromQuery] string? modulo = null)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var query = _context.CuentasIntegracion
                .Where(c => c.EmpresaId == empresaId)
                .Include(c => c.CuentaContable)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(modulo))
            {
                query = query.Where(c => c.Modulo == modulo.ToUpper());
            }

            var cuentas = await query
                .OrderBy(c => c.Modulo)
                .ThenBy(c => c.TipoOperacion)
                .ThenBy(c => c.Orden)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} integration mappings for empresa {EmpresaId}", cuentas.Count, empresaId);
            return Ok(cuentas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving integration mappings for empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene un mapeo de integración por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var cuenta = await _context.CuentasIntegracion
                .Include(c => c.CuentaContable)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cuenta == null)
            {
                return NotFound("Mapeo de integración no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            return Ok(cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving integration mapping {Id}", id);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la lista de módulos disponibles con sus descripciones.
    /// </summary>
    [HttpGet("modulos")]
    public IActionResult GetModulos()
    {
        var modulos = new[]
        {
            new { Codigo = ModulosContables.Ventas, Descripcion = "Ventas" },
            new { Codigo = ModulosContables.Compras, Descripcion = "Compras" },
            new { Codigo = ModulosContables.Inventario, Descripcion = "Inventario" },
            new { Codigo = ModulosContables.CuentasPorCobrar, Descripcion = "Cuentas por Cobrar" },
            new { Codigo = ModulosContables.CuentasPorPagar, Descripcion = "Cuentas por Pagar" },
            new { Codigo = ModulosContables.Bancos, Descripcion = "Bancos" },
            new { Codigo = ModulosContables.Nomina, Descripcion = "Nómina" },
            new { Codigo = ModulosContables.ActivosFijos, Descripcion = "Activos Fijos" }
        };

        return Ok(modulos);
    }

    /// <summary>
    /// Obtiene los tipos de operación disponibles para un módulo específico.
    /// </summary>
    [HttpGet("tipos-operacion/{modulo}")]
    public IActionResult GetTiposOperacion(string modulo)
    {
        var moduloUpper = modulo.ToUpper();

        var tiposOperacion = moduloUpper switch
        {
            ModulosContables.Ventas => new[]
            {
                new { Codigo = TiposOperacionContable.VentaContado, Descripcion = "Venta al Contado" },
                new { Codigo = TiposOperacionContable.VentaCredito, Descripcion = "Venta a Crédito" },
                new { Codigo = TiposOperacionContable.NotaCreditoVenta, Descripcion = "Nota de Crédito Venta" },
                new { Codigo = TiposOperacionContable.NotaDebitoVenta, Descripcion = "Nota de Débito Venta" }
            },
            ModulosContables.Compras => new[]
            {
                new { Codigo = TiposOperacionContable.CompraContado, Descripcion = "Compra al Contado" },
                new { Codigo = TiposOperacionContable.CompraCredito, Descripcion = "Compra a Crédito" },
                new { Codigo = TiposOperacionContable.NotaCreditoCompra, Descripcion = "Nota de Crédito Compra" }
            },
            ModulosContables.Inventario => new[]
            {
                new { Codigo = TiposOperacionContable.EntradaInventario, Descripcion = "Entrada de Inventario" },
                new { Codigo = TiposOperacionContable.SalidaInventario, Descripcion = "Salida de Inventario" },
                new { Codigo = TiposOperacionContable.AjustePositivo, Descripcion = "Ajuste Positivo" },
                new { Codigo = TiposOperacionContable.AjusteNegativo, Descripcion = "Ajuste Negativo" }
            },
            ModulosContables.CuentasPorCobrar => new[]
            {
                new { Codigo = TiposOperacionContable.ReciboPago, Descripcion = "Recibo de Pago" },
                new { Codigo = TiposOperacionContable.NotaDebitoInteres, Descripcion = "Nota de Débito Interés" }
            },
            ModulosContables.CuentasPorPagar => new[]
            {
                new { Codigo = TiposOperacionContable.PagoProveedor, Descripcion = "Pago a Proveedor" }
            },
            ModulosContables.Bancos => new[]
            {
                new { Codigo = TiposOperacionContable.Deposito, Descripcion = "Depósito" },
                new { Codigo = TiposOperacionContable.Retiro, Descripcion = "Retiro" },
                new { Codigo = TiposOperacionContable.ComisionBancaria, Descripcion = "Comisión Bancaria" },
                new { Codigo = TiposOperacionContable.InteresBancario, Descripcion = "Interés Bancario" }
            },
            ModulosContables.Nomina => new[]
            {
                new { Codigo = TiposOperacionContable.PagoPlanilla, Descripcion = "Pago de Planilla" },
                new { Codigo = TiposOperacionContable.ProvisionPlanilla, Descripcion = "Provisión de Planilla" }
            },
            ModulosContables.ActivosFijos => new[]
            {
                new { Codigo = TiposOperacionContable.Depreciacion, Descripcion = "Depreciación" },
                new { Codigo = TiposOperacionContable.BajaActivo, Descripcion = "Baja de Activo" },
                new { Codigo = TiposOperacionContable.VentaActivo, Descripcion = "Venta de Activo" }
            },
            _ => Array.Empty<object>()
        };

        return Ok(tiposOperacion);
    }

    /// <summary>
    /// Crea un nuevo mapeo de integración contable.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CuentaIntegracion cuenta)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que no exista ya el mismo mapeo
            var existente = await _context.CuentasIntegracion
                .FirstOrDefaultAsync(c => c.EmpresaId == cuenta.EmpresaId &&
                                         c.Modulo == cuenta.Modulo &&
                                         c.TipoOperacion == cuenta.TipoOperacion &&
                                         c.ConceptoContable == cuenta.ConceptoContable);

            if (existente != null)
            {
                return BadRequest("Ya existe un mapeo para esta combinación de módulo, tipo de operación y concepto contable.");
            }

            // Si se especifica cuenta contable, verificar que exista y pertenezca a la empresa usando el repositorio
            if (cuenta.CuentaContableId != Guid.Empty)
            {
                var cuentaContable = await _unitOfWork.CuentaContableRepository.GetAsync(cuenta.CuentaContableId);
                if (cuentaContable == null)
                {
                    return BadRequest("La cuenta contable no existe o no pertenece a la empresa.");
                }

                if (cuentaContable.EmpresaId != cuenta.EmpresaId)
                {
                    return BadRequest("La cuenta contable no pertenece a la empresa.");
                }

                if (!cuentaContable.AceptaMovimientos)
                {
                    return BadRequest("La cuenta contable seleccionada no acepta movimientos.");
                }
            }

            cuenta.Id = Guid.NewGuid();
            cuenta.FechaCreacion = DateTime.Now;
            cuenta.CreadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.CuentasIntegracion.Add(cuenta);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created integration mapping {Id} for empresa {EmpresaId} by user {UserId}",
                cuenta.Id, cuenta.EmpresaId, cuenta.CreadoPorId);

            return Ok(cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating integration mapping for empresa {EmpresaId}", cuenta.EmpresaId);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un mapeo de integración existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] CuentaIntegracion cuenta)
    {
        try
        {
            if (id != cuenta.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _context.CuentasIntegracion
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existente == null)
            {
                return NotFound("Mapeo de integración no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Verificar que no exista otro mapeo igual (excluyendo el actual)
            var duplicado = await _context.CuentasIntegracion
                .FirstOrDefaultAsync(c => c.EmpresaId == cuenta.EmpresaId &&
                                         c.Modulo == cuenta.Modulo &&
                                         c.TipoOperacion == cuenta.TipoOperacion &&
                                         c.ConceptoContable == cuenta.ConceptoContable &&
                                         c.Id != id);

            if (duplicado != null)
            {
                return BadRequest("Ya existe otro mapeo para esta combinación de módulo, tipo de operación y concepto contable.");
            }

            // Si se especifica cuenta contable, verificar que exista y pertenezca a la empresa usando el repositorio
            if (cuenta.CuentaContableId != Guid.Empty)
            {
                var cuentaContable = await _unitOfWork.CuentaContableRepository.GetAsync(cuenta.CuentaContableId);
                if (cuentaContable == null)
                {
                    return BadRequest("La cuenta contable no existe o no pertenece a la empresa.");
                }

                if (cuentaContable.EmpresaId != cuenta.EmpresaId)
                {
                    return BadRequest("La cuenta contable no pertenece a la empresa.");
                }

                if (!cuentaContable.AceptaMovimientos)
                {
                    return BadRequest("La cuenta contable seleccionada no acepta movimientos.");
                }
            }

            // Preservar campos de auditoría
            cuenta.FechaCreacion = existente.FechaCreacion;
            cuenta.CreadoPorId = existente.CreadoPorId;
            cuenta.FechaModificacion = DateTime.Now;
            cuenta.ModificadoPorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Entry(existente).CurrentValues.SetValues(cuenta);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated integration mapping {Id} for empresa {EmpresaId} by user {UserId}",
                id, existente.EmpresaId, cuenta.ModificadoPorId);

            return Ok(cuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating integration mapping {Id}", id);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina un mapeo de integración.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var cuenta = await _context.CuentasIntegracion
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cuenta == null)
            {
                return NotFound("Mapeo de integración no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(cuenta.EmpresaId))
            {
                return Forbid();
            }

            _context.CuentasIntegracion.Remove(cuenta);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted integration mapping {Id} for empresa {EmpresaId}", id, cuenta.EmpresaId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting integration mapping {Id}", id);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Inicializa los mapeos de integración por defecto para una empresa.
    /// Busca las cuentas contables existentes del plan de cuentas y las asigna automáticamente.
    /// Requiere que la empresa tenga un plan de cuentas creado previamente.
    /// </summary>
    [HttpPost("seed/{empresaId:guid}")]
    public async Task<IActionResult> SeedAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            // Verificar que la empresa existe
            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            // Verificar si ya existen mapeos
            var existentes = await _context.CuentasIntegracion
                .Where(c => c.EmpresaId == empresaId)
                .CountAsync();

            if (existentes > 0)
            {
                return BadRequest($"La empresa ya tiene {existentes} mapeos configurados. Use este endpoint solo para inicializar mapeos en empresas nuevas.");
            }

            // Cargar todas las cuentas contables de la empresa (solo las que aceptan movimientos)
            var cuentas = await _context.Set<CuentaContable>()
                .Where(c => c.EmpresaId == empresaId && !c.IsDeleted && c.Activo && c.AceptaMovimientos)
                .ToListAsync();

            if (!cuentas.Any())
            {
                return BadRequest("La empresa no tiene cuentas contables creadas. Primero debe crear el Plan de Cuentas desde Contabilidad > Plan de Cuentas.");
            }

            // Mapeo de código de cuenta contable estándar CR → CuentaContable
            // Basado en el plan de cuentas estándar de Costa Rica
            var cuentaPorCodigo = cuentas.ToDictionary(c => c.Codigo, c => c.Id);

            // Helper: buscar cuenta por código exacto o por nombre parcial
            Guid? buscarCuenta(string codigo, string? nombreParcial = null)
            {
                if (cuentaPorCodigo.TryGetValue(codigo, out var id))
                    return id;
                if (nombreParcial != null)
                {
                    var cuenta = cuentas.FirstOrDefault(c =>
                        c.Nombre.Contains(nombreParcial, StringComparison.OrdinalIgnoreCase));
                    return cuenta?.Id;
                }
                return null;
            }

            // Resolver cuentas del plan estándar CR
            var caja         = buscarCuenta("1.1.01.001", "Caja General");
            var bancos       = buscarCuenta("1.1.01.003", "Bancos");
            var clientes     = buscarCuenta("1.1.02.001", "Clientes");
            var inventario   = buscarCuenta("1.1.03.001", "Inventario");
            var ivaCredito   = buscarCuenta("1.1.04.001", "IVA Crédito");
            var proveedores  = buscarCuenta("2.1.01.001", "Proveedores");
            var ivaPorPagar  = buscarCuenta("2.1.02.001", "IVA por Pagar");
            var salarios     = buscarCuenta("2.1.03.001", "Salarios por Pagar");
            var aguinaldo    = buscarCuenta("2.1.03.003", "Aguinaldo");
            var ventas       = buscarCuenta("4.1.01.001", "Ventas de Mercaderías");
            var devoluciones = buscarCuenta("4.1.02.001", "Devoluciones sobre Ventas");
            var descuentos   = buscarCuenta("4.1.02.002", "Descuentos sobre Ventas");
            var ingFinanc    = buscarCuenta("4.2.01", "Ingresos Financieros");
            var difCambPos   = buscarCuenta("4.2.02", "Diferencial Cambiario Positivo");
            var salAdm       = buscarCuenta("5.1.01", "Salarios Administrativos");
            var cargasSoc    = buscarCuenta("5.1.02", "Cargas Sociales");
            var depreciacion = buscarCuenta("5.1.05", "Depreciaciones");
            var comisiones   = buscarCuenta("5.2.02", "Comisiones");
            var comBancarias = buscarCuenta("5.3.02", "Comisiones Bancarias");
            var difCambNeg   = buscarCuenta("5.3.03", "Diferencial Cambiario Negativo");
            var costoVentas  = buscarCuenta("6.1.01", "Costo de Mercaderías");
            var depAcumEdif  = buscarCuenta("1.2.02.001", "Depreciación Edificios");
            var mobEquipo    = buscarCuenta("1.2.01.003", "Mobiliario y Equipo");

            // Cuentas por tarifa IVA (Costa Rica: 13%, 4%, 2%, 1%, exento)
            var ventas13     = buscarCuenta("4.1.01.001", "Ventas Gravadas 13%");
            var ventas4      = buscarCuenta("4.1.01.002", "Ventas Gravadas 4%");
            var ventas2      = buscarCuenta("4.1.01.003", "Ventas Gravadas 2%");
            var ventas1      = buscarCuenta("4.1.01.004", "Ventas Gravadas 1%");
            var ventasExentas = buscarCuenta("4.1.01.005", "Ventas Exentas");

            var ivaDebito13  = buscarCuenta("2.1.02.001", "IVA por Pagar 13%");
            var ivaDebito4   = buscarCuenta("2.1.02.002", "IVA por Pagar 4%");
            var ivaDebito2   = buscarCuenta("2.1.02.003", "IVA por Pagar 2%");
            var ivaDebito1   = buscarCuenta("2.1.02.004", "IVA por Pagar 1%");

            var ivaCredito13 = buscarCuenta("1.1.04.001", "IVA Crédito Fiscal 13%");
            var ivaCredito4  = buscarCuenta("1.1.04.002", "IVA Crédito Fiscal 4%");
            var ivaCredito2  = buscarCuenta("1.1.04.003", "IVA Crédito Fiscal 2%");
            var ivaCredito1  = buscarCuenta("1.1.04.004", "IVA Crédito Fiscal 1%");

            // Validar que al menos las cuentas básicas existan
            var faltantes = new List<string>();
            if (caja == null) faltantes.Add("Caja General (1.1.01.001)");
            if (bancos == null) faltantes.Add("Bancos Nacionales (1.1.01.003)");
            if (clientes == null) faltantes.Add("Clientes (1.1.02.001)");
            if (ventas == null) faltantes.Add("Ventas de Mercaderías (4.1.01.001)");
            if (proveedores == null) faltantes.Add("Proveedores (2.1.01.001)");
            if (ivaPorPagar == null) faltantes.Add("IVA por Pagar (2.1.02.001)");

            if (faltantes.Any())
            {
                return BadRequest($"Faltan cuentas contables básicas requeridas: {string.Join(", ", faltantes)}. Verifique que el Plan de Cuentas esté completo.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var fechaCreacion = DateTime.Now;
            var mapeos = new List<CuentaIntegracion>();

            // Helper para crear mapeo
            void agregar(string modulo, string tipoOp, string concepto, string desc, string tipoMov, int orden, Guid cuentaId)
            {
                mapeos.Add(new CuentaIntegracion
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    Modulo = modulo,
                    TipoOperacion = tipoOp,
                    ConceptoContable = concepto,
                    Descripcion = desc,
                    TipoMovimiento = tipoMov,
                    Orden = orden,
                    Activo = true,
                    FechaCreacion = fechaCreacion,
                    CreadoPorId = userId,
                    CuentaContableId = cuentaId
                });
            }

            // =====================================================
            // MÓDULO: VENTAS (VEN)
            // =====================================================
            // VENTA_CONTADO (genérico - fallback)
            agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaCaja, "Caja por venta al contado", "D", 1, caja!.Value);
            agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaVentas, "Ingresos por ventas (genérico)", "H", 2, ventas!.Value);
            agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaIvaDebito, "IVA débito fiscal (genérico)", "H", 3, ivaPorPagar!.Value);

            // VENTA_CONTADO por tarifa IVA
            if (ventas13.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaVentas13, "Ventas gravadas 13%", "H", 10, ventas13.Value);
            if (ventas4.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaVentas4, "Ventas gravadas 4%", "H", 11, ventas4.Value);
            if (ventas2.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaVentas2, "Ventas gravadas 2%", "H", 12, ventas2.Value);
            if (ventas1.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaVentas1, "Ventas gravadas 1%", "H", 13, ventas1.Value);
            if (ventasExentas.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaVentasExentas, "Ventas exentas", "H", 14, ventasExentas.Value);
            if (ivaDebito13.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaIvaDebito13, "IVA débito 13%", "H", 15, ivaDebito13.Value);
            if (ivaDebito4.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaIvaDebito4, "IVA débito 4%", "H", 16, ivaDebito4.Value);
            if (ivaDebito2.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaIvaDebito2, "IVA débito 2%", "H", 17, ivaDebito2.Value);
            if (ivaDebito1.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaContado, ConceptosContables.CuentaIvaDebito1, "IVA débito 1%", "H", 18, ivaDebito1.Value);

            // VENTA_CREDITO (genérico - fallback)
            agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaClientes, "Clientes por venta a crédito", "D", 1, clientes!.Value);
            agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaVentas, "Ingresos por ventas (genérico)", "H", 2, ventas!.Value);
            agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaIvaDebito, "IVA débito fiscal (genérico)", "H", 3, ivaPorPagar!.Value);

            // VENTA_CREDITO por tarifa IVA
            if (ventas13.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaVentas13, "Ventas gravadas 13%", "H", 10, ventas13.Value);
            if (ventas4.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaVentas4, "Ventas gravadas 4%", "H", 11, ventas4.Value);
            if (ventas2.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaVentas2, "Ventas gravadas 2%", "H", 12, ventas2.Value);
            if (ventas1.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaVentas1, "Ventas gravadas 1%", "H", 13, ventas1.Value);
            if (ventasExentas.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaVentasExentas, "Ventas exentas", "H", 14, ventasExentas.Value);
            if (ivaDebito13.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaIvaDebito13, "IVA débito 13%", "H", 15, ivaDebito13.Value);
            if (ivaDebito4.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaIvaDebito4, "IVA débito 4%", "H", 16, ivaDebito4.Value);
            if (ivaDebito2.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaIvaDebito2, "IVA débito 2%", "H", 17, ivaDebito2.Value);
            if (ivaDebito1.HasValue) agregar(ModulosContables.Ventas, TiposOperacionContable.VentaCredito, ConceptosContables.CuentaIvaDebito1, "IVA débito 1%", "H", 18, ivaDebito1.Value);

            // NOTA_CREDITO_VENTA
            agregar(ModulosContables.Ventas, TiposOperacionContable.NotaCreditoVenta, ConceptosContables.CuentaDevoluciones, "Devoluciones sobre ventas", "D", 1, devoluciones ?? ventas!.Value);
            agregar(ModulosContables.Ventas, TiposOperacionContable.NotaCreditoVenta, ConceptosContables.CuentaIvaDebito, "IVA débito fiscal (reversión)", "D", 2, ivaPorPagar!.Value);
            agregar(ModulosContables.Ventas, TiposOperacionContable.NotaCreditoVenta, ConceptosContables.CuentaClientes, "Clientes (abono por NC)", "H", 3, clientes!.Value);

            // NOTA_DEBITO_VENTA
            agregar(ModulosContables.Ventas, TiposOperacionContable.NotaDebitoVenta, ConceptosContables.CuentaClientes, "Clientes (cargo adicional)", "D", 1, clientes!.Value);
            agregar(ModulosContables.Ventas, TiposOperacionContable.NotaDebitoVenta, ConceptosContables.CuentaIntereses, "Ingresos por intereses/cargos", "H", 2, ingFinanc ?? ventas!.Value);

            // =====================================================
            // MÓDULO: COMPRAS (COM)
            // =====================================================
            // COMPRA_CONTADO (genérico - fallback)
            agregar(ModulosContables.Compras, TiposOperacionContable.CompraContado, ConceptosContables.CuentaInventario, "Inventario por compra", "D", 1, inventario ?? caja!.Value);
            agregar(ModulosContables.Compras, TiposOperacionContable.CompraContado, ConceptosContables.CuentaIvaCredito, "IVA crédito fiscal (genérico)", "D", 2, ivaCredito ?? caja!.Value);
            agregar(ModulosContables.Compras, TiposOperacionContable.CompraContado, ConceptosContables.CuentaCaja, "Caja (pago contado)", "H", 3, caja!.Value);

            // COMPRA_CONTADO por tarifa IVA
            if (ivaCredito13.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraContado, ConceptosContables.CuentaIvaCredito13, "IVA crédito fiscal 13%", "D", 10, ivaCredito13.Value);
            if (ivaCredito4.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraContado, ConceptosContables.CuentaIvaCredito4, "IVA crédito fiscal 4%", "D", 11, ivaCredito4.Value);
            if (ivaCredito2.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraContado, ConceptosContables.CuentaIvaCredito2, "IVA crédito fiscal 2%", "D", 12, ivaCredito2.Value);
            if (ivaCredito1.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraContado, ConceptosContables.CuentaIvaCredito1, "IVA crédito fiscal 1%", "D", 13, ivaCredito1.Value);

            // COMPRA_CREDITO (genérico - fallback)
            agregar(ModulosContables.Compras, TiposOperacionContable.CompraCredito, ConceptosContables.CuentaInventario, "Inventario por compra", "D", 1, inventario ?? caja!.Value);
            agregar(ModulosContables.Compras, TiposOperacionContable.CompraCredito, ConceptosContables.CuentaIvaCredito, "IVA crédito fiscal (genérico)", "D", 2, ivaCredito ?? caja!.Value);
            agregar(ModulosContables.Compras, TiposOperacionContable.CompraCredito, ConceptosContables.CuentaProveedores, "Proveedores (compra a crédito)", "H", 3, proveedores!.Value);

            // COMPRA_CREDITO por tarifa IVA
            if (ivaCredito13.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraCredito, ConceptosContables.CuentaIvaCredito13, "IVA crédito fiscal 13%", "D", 10, ivaCredito13.Value);
            if (ivaCredito4.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraCredito, ConceptosContables.CuentaIvaCredito4, "IVA crédito fiscal 4%", "D", 11, ivaCredito4.Value);
            if (ivaCredito2.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraCredito, ConceptosContables.CuentaIvaCredito2, "IVA crédito fiscal 2%", "D", 12, ivaCredito2.Value);
            if (ivaCredito1.HasValue) agregar(ModulosContables.Compras, TiposOperacionContable.CompraCredito, ConceptosContables.CuentaIvaCredito1, "IVA crédito fiscal 1%", "D", 13, ivaCredito1.Value);

            // NOTA_CREDITO_COMPRA
            agregar(ModulosContables.Compras, TiposOperacionContable.NotaCreditoCompra, ConceptosContables.CuentaProveedores, "Proveedores (devolución)", "D", 1, proveedores!.Value);
            agregar(ModulosContables.Compras, TiposOperacionContable.NotaCreditoCompra, ConceptosContables.CuentaInventario, "Inventario (reversión)", "H", 2, inventario ?? caja!.Value);
            agregar(ModulosContables.Compras, TiposOperacionContable.NotaCreditoCompra, ConceptosContables.CuentaIvaCredito, "IVA crédito fiscal (reversión)", "H", 3, ivaCredito ?? caja!.Value);

            // =====================================================
            // MÓDULO: INVENTARIO (INV)
            // =====================================================
            if (inventario.HasValue)
            {
                agregar(ModulosContables.Inventario, TiposOperacionContable.EntradaInventario, ConceptosContables.CuentaInventario, "Inventario (entrada)", "D", 1, inventario.Value);
                agregar(ModulosContables.Inventario, TiposOperacionContable.SalidaInventario, ConceptosContables.CuentaCostoVentas, "Costo de ventas", "D", 1, costoVentas ?? inventario.Value);
                agregar(ModulosContables.Inventario, TiposOperacionContable.SalidaInventario, ConceptosContables.CuentaInventario, "Inventario (salida)", "H", 2, inventario.Value);
                agregar(ModulosContables.Inventario, TiposOperacionContable.AjustePositivo, ConceptosContables.CuentaInventario, "Inventario (ajuste positivo)", "D", 1, inventario.Value);
                agregar(ModulosContables.Inventario, TiposOperacionContable.AjusteNegativo, ConceptosContables.CuentaInventario, "Inventario (ajuste negativo)", "H", 1, inventario.Value);
            }

            // =====================================================
            // MÓDULO: CUENTAS POR COBRAR (CXC)
            // =====================================================
            agregar(ModulosContables.CuentasPorCobrar, TiposOperacionContable.ReciboPago, ConceptosContables.CuentaBancos, "Bancos (cobro a cliente)", "D", 1, bancos!.Value);
            agregar(ModulosContables.CuentasPorCobrar, TiposOperacionContable.ReciboPago, ConceptosContables.CuentaClientes, "Clientes (abono)", "H", 2, clientes!.Value);
            agregar(ModulosContables.CuentasPorCobrar, TiposOperacionContable.NotaDebitoInteres, ConceptosContables.CuentaClientes, "Clientes (cargo por interés)", "D", 1, clientes!.Value);
            agregar(ModulosContables.CuentasPorCobrar, TiposOperacionContable.NotaDebitoInteres, ConceptosContables.CuentaIntereses, "Ingresos por intereses", "H", 2, ingFinanc ?? ventas!.Value);

            // =====================================================
            // MÓDULO: CUENTAS POR PAGAR (CXP)
            // =====================================================
            agregar(ModulosContables.CuentasPorPagar, TiposOperacionContable.PagoProveedor, ConceptosContables.CuentaProveedores, "Proveedores (pago)", "D", 1, proveedores!.Value);
            agregar(ModulosContables.CuentasPorPagar, TiposOperacionContable.PagoProveedor, ConceptosContables.CuentaBancos, "Bancos (salida de efectivo)", "H", 2, bancos!.Value);

            // =====================================================
            // MÓDULO: BANCOS (BAN)
            // =====================================================
            agregar(ModulosContables.Bancos, TiposOperacionContable.Deposito, ConceptosContables.CuentaBancos, "Bancos (depósito)", "D", 1, bancos!.Value);
            agregar(ModulosContables.Bancos, TiposOperacionContable.Retiro, ConceptosContables.CuentaBancos, "Bancos (retiro)", "H", 1, bancos!.Value);
            agregar(ModulosContables.Bancos, TiposOperacionContable.ComisionBancaria, ConceptosContables.CuentaComisiones, "Gastos por comisiones", "D", 1, comBancarias ?? bancos!.Value);
            agregar(ModulosContables.Bancos, TiposOperacionContable.ComisionBancaria, ConceptosContables.CuentaBancos, "Bancos (cargo comisión)", "H", 2, bancos!.Value);
            agregar(ModulosContables.Bancos, TiposOperacionContable.InteresBancario, ConceptosContables.CuentaBancos, "Bancos (interés ganado)", "D", 1, bancos!.Value);
            agregar(ModulosContables.Bancos, TiposOperacionContable.InteresBancario, ConceptosContables.CuentaIntereses, "Ingresos por intereses", "H", 2, ingFinanc ?? bancos!.Value);

            // =====================================================
            // MÓDULO: NÓMINA (NOM)
            // =====================================================
            if (salAdm.HasValue)
            {
                agregar(ModulosContables.Nomina, TiposOperacionContable.PagoPlanilla, "CUENTA_SUELDOS", "Gastos de sueldos", "D", 1, salAdm.Value);
                agregar(ModulosContables.Nomina, TiposOperacionContable.PagoPlanilla, ConceptosContables.CuentaBancos, "Bancos (pago nómina)", "H", 2, bancos!.Value);
            }
            if (cargasSoc.HasValue)
            {
                agregar(ModulosContables.Nomina, TiposOperacionContable.ProvisionPlanilla, "CUENTA_CARGAS_SOCIALES", "Gastos por cargas sociales", "D", 1, cargasSoc.Value);
                agregar(ModulosContables.Nomina, TiposOperacionContable.ProvisionPlanilla, "CUENTA_PROVISION_AGUINALDO", "Provisión para aguinaldo", "H", 2, aguinaldo ?? salarios ?? bancos!.Value);
            }

            // =====================================================
            // MÓDULO: ACTIVOS FIJOS (ACT)
            // =====================================================
            if (depreciacion.HasValue && depAcumEdif.HasValue)
            {
                agregar(ModulosContables.ActivosFijos, TiposOperacionContable.Depreciacion, ConceptosContables.CuentaDepreciacion, "Gasto por depreciación", "D", 1, depreciacion.Value);
                agregar(ModulosContables.ActivosFijos, TiposOperacionContable.Depreciacion, ConceptosContables.CuentaDepreciacionAcumulada, "Depreciación acumulada", "H", 2, depAcumEdif.Value);
            }
            if (depAcumEdif.HasValue && mobEquipo.HasValue)
            {
                agregar(ModulosContables.ActivosFijos, TiposOperacionContable.BajaActivo, ConceptosContables.CuentaDepreciacionAcumulada, "Depreciación acumulada (baja)", "D", 1, depAcumEdif.Value);
                agregar(ModulosContables.ActivosFijos, TiposOperacionContable.BajaActivo, "CUENTA_ACTIVOS_FIJOS", "Activos fijos (baja)", "H", 2, mobEquipo.Value);

                agregar(ModulosContables.ActivosFijos, TiposOperacionContable.VentaActivo, ConceptosContables.CuentaBancos, "Bancos (venta de activo)", "D", 1, bancos!.Value);
                agregar(ModulosContables.ActivosFijos, TiposOperacionContable.VentaActivo, ConceptosContables.CuentaDepreciacionAcumulada, "Depreciación acumulada", "D", 2, depAcumEdif.Value);
                agregar(ModulosContables.ActivosFijos, TiposOperacionContable.VentaActivo, "CUENTA_ACTIVOS_FIJOS", "Activos fijos (venta)", "H", 3, mobEquipo.Value);
            }

            // Insertar todos los mapeos
            _context.CuentasIntegracion.AddRange(mapeos);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} integration mappings for empresa {EmpresaId}", mapeos.Count, empresaId);

            return Ok(new
            {
                Message = $"Se crearon {mapeos.Count} mapeos de integración predeterminados para la empresa.",
                Count = mapeos.Count,
                Mapeos = mapeos.GroupBy(m => m.Modulo)
                               .Select(g => new { Modulo = g.Key, Cantidad = g.Count() })
                               .ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding integration mappings for empresa {EmpresaId}", empresaId);
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si el usuario actual tiene acceso a la empresa especificada.
    /// Los SuperUser tienen acceso a todas las empresas.
    /// </summary>
    private async Task<bool> TieneAccesoEmpresaAsync(Guid empresaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        // SuperUser tiene acceso a todas las empresas
        if (User.IsInRole("SuperUser"))
        {
            return true;
        }

        // Verificar si el usuario tiene acceso a la empresa
        return await _context.UsuariosEmpresas
            .AnyAsync(ue => ue.UserId == userId && ue.EmpresaId == empresaId);
    }
}
