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
    /// Los mapeos se crean sin cuenta contable asignada (CuentaContableId = Guid.Empty).
    /// El usuario deberá asignar las cuentas posteriormente.
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

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var fechaCreacion = DateTime.Now;
        var mapeos = new List<CuentaIntegracion>();

        // =====================================================
        // MÓDULO: VENTAS (VEN)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // VENTA_CONTADO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.VentaContado, ConceptoContable = ConceptosContables.CuentaCaja, Descripcion = "Caja por venta al contado", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.VentaContado, ConceptoContable = ConceptosContables.CuentaVentas, Descripcion = "Ingresos por ventas", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.VentaContado, ConceptoContable = ConceptosContables.CuentaIvaDebito, Descripcion = "IVA débito fiscal", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // VENTA_CREDITO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.VentaCredito, ConceptoContable = ConceptosContables.CuentaClientes, Descripcion = "Clientes por venta a crédito", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.VentaCredito, ConceptoContable = ConceptosContables.CuentaVentas, Descripcion = "Ingresos por ventas", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.VentaCredito, ConceptoContable = ConceptosContables.CuentaIvaDebito, Descripcion = "IVA débito fiscal", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // NOTA_CREDITO_VENTA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.NotaCreditoVenta, ConceptoContable = ConceptosContables.CuentaDevoluciones, Descripcion = "Devoluciones sobre ventas", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.NotaCreditoVenta, ConceptoContable = ConceptosContables.CuentaIvaDebito, Descripcion = "IVA débito fiscal (reversión)", TipoMovimiento = "D", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.NotaCreditoVenta, ConceptoContable = ConceptosContables.CuentaClientes, Descripcion = "Clientes (abono por NC)", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // NOTA_DEBITO_VENTA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.NotaDebitoVenta, ConceptoContable = ConceptosContables.CuentaClientes, Descripcion = "Clientes (cargo adicional)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Ventas, TipoOperacion = TiposOperacionContable.NotaDebitoVenta, ConceptoContable = ConceptosContables.CuentaIntereses, Descripcion = "Ingresos por intereses/cargos", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

        // =====================================================
        // MÓDULO: COMPRAS (COM)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // COMPRA_CONTADO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.CompraContado, ConceptoContable = ConceptosContables.CuentaInventario, Descripcion = "Inventario por compra", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.CompraContado, ConceptoContable = ConceptosContables.CuentaIvaCredito, Descripcion = "IVA crédito fiscal", TipoMovimiento = "D", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.CompraContado, ConceptoContable = ConceptosContables.CuentaCaja, Descripcion = "Caja (pago contado)", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // COMPRA_CREDITO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.CompraCredito, ConceptoContable = ConceptosContables.CuentaInventario, Descripcion = "Inventario por compra", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.CompraCredito, ConceptoContable = ConceptosContables.CuentaIvaCredito, Descripcion = "IVA crédito fiscal", TipoMovimiento = "D", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.CompraCredito, ConceptoContable = ConceptosContables.CuentaProveedores, Descripcion = "Proveedores (compra a crédito)", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // NOTA_CREDITO_COMPRA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.NotaCreditoCompra, ConceptoContable = ConceptosContables.CuentaProveedores, Descripcion = "Proveedores (devolución)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.NotaCreditoCompra, ConceptoContable = ConceptosContables.CuentaInventario, Descripcion = "Inventario (reversión)", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Compras, TipoOperacion = TiposOperacionContable.NotaCreditoCompra, ConceptoContable = ConceptosContables.CuentaIvaCredito, Descripcion = "IVA crédito fiscal (reversión)", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

        // =====================================================
        // MÓDULO: INVENTARIO (INV)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // ENTRADA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Inventario, TipoOperacion = TiposOperacionContable.EntradaInventario, ConceptoContable = ConceptosContables.CuentaInventario, Descripcion = "Inventario (entrada)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // SALIDA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Inventario, TipoOperacion = TiposOperacionContable.SalidaInventario, ConceptoContable = ConceptosContables.CuentaCostoVentas, Descripcion = "Costo de ventas", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Inventario, TipoOperacion = TiposOperacionContable.SalidaInventario, ConceptoContable = ConceptosContables.CuentaInventario, Descripcion = "Inventario (salida)", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // AJUSTE_POSITIVO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Inventario, TipoOperacion = TiposOperacionContable.AjustePositivo, ConceptoContable = ConceptosContables.CuentaInventario, Descripcion = "Inventario (ajuste positivo)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // AJUSTE_NEGATIVO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Inventario, TipoOperacion = TiposOperacionContable.AjusteNegativo, ConceptoContable = ConceptosContables.CuentaInventario, Descripcion = "Inventario (ajuste negativo)", TipoMovimiento = "H", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

        // =====================================================
        // MÓDULO: CUENTAS POR COBRAR (CXC)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // RECIBO_PAGO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.CuentasPorCobrar, TipoOperacion = TiposOperacionContable.ReciboPago, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (cobro a cliente)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.CuentasPorCobrar, TipoOperacion = TiposOperacionContable.ReciboPago, ConceptoContable = ConceptosContables.CuentaClientes, Descripcion = "Clientes (abono)", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // NOTA_DEBITO_INTERES
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.CuentasPorCobrar, TipoOperacion = TiposOperacionContable.NotaDebitoInteres, ConceptoContable = ConceptosContables.CuentaClientes, Descripcion = "Clientes (cargo por interés)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.CuentasPorCobrar, TipoOperacion = TiposOperacionContable.NotaDebitoInteres, ConceptoContable = ConceptosContables.CuentaIntereses, Descripcion = "Ingresos por intereses", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

        // =====================================================
        // MÓDULO: CUENTAS POR PAGAR (CXP)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // PAGO_PROVEEDOR
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.CuentasPorPagar, TipoOperacion = TiposOperacionContable.PagoProveedor, ConceptoContable = ConceptosContables.CuentaProveedores, Descripcion = "Proveedores (pago)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.CuentasPorPagar, TipoOperacion = TiposOperacionContable.PagoProveedor, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (salida de efectivo)", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

        // =====================================================
        // MÓDULO: BANCOS (BAN)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // DEPOSITO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Bancos, TipoOperacion = TiposOperacionContable.Deposito, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (depósito)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // RETIRO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Bancos, TipoOperacion = TiposOperacionContable.Retiro, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (retiro)", TipoMovimiento = "H", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // COMISION_BANCARIA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Bancos, TipoOperacion = TiposOperacionContable.ComisionBancaria, ConceptoContable = ConceptosContables.CuentaComisiones, Descripcion = "Gastos por comisiones", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Bancos, TipoOperacion = TiposOperacionContable.ComisionBancaria, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (cargo comisión)", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // INTERES_BANCARIO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Bancos, TipoOperacion = TiposOperacionContable.InteresBancario, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (interés ganado)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Bancos, TipoOperacion = TiposOperacionContable.InteresBancario, ConceptoContable = ConceptosContables.CuentaIntereses, Descripcion = "Ingresos por intereses", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

        // =====================================================
        // MÓDULO: NÓMINA (NOM)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // PAGO_PLANILLA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Nomina, TipoOperacion = TiposOperacionContable.PagoPlanilla, ConceptoContable = "CUENTA_SUELDOS", Descripcion = "Gastos de sueldos", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Nomina, TipoOperacion = TiposOperacionContable.PagoPlanilla, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (pago nómina)", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // PROVISION_PLANILLA
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Nomina, TipoOperacion = TiposOperacionContable.ProvisionPlanilla, ConceptoContable = "CUENTA_CARGAS_SOCIALES", Descripcion = "Gastos por cargas sociales", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.Nomina, TipoOperacion = TiposOperacionContable.ProvisionPlanilla, ConceptoContable = "CUENTA_PROVISION_AGUINALDO", Descripcion = "Provisión para aguinaldo", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

        // =====================================================
        // MÓDULO: ACTIVOS FIJOS (ACT)
        // =====================================================
        mapeos.AddRange(new[]
        {
            // DEPRECIACION
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.Depreciacion, ConceptoContable = ConceptosContables.CuentaDepreciacion, Descripcion = "Gasto por depreciación", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.Depreciacion, ConceptoContable = ConceptosContables.CuentaDepreciacionAcumulada, Descripcion = "Depreciación acumulada", TipoMovimiento = "H", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // BAJA_ACTIVO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.BajaActivo, ConceptoContable = ConceptosContables.CuentaDepreciacionAcumulada, Descripcion = "Depreciación acumulada (baja)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.BajaActivo, ConceptoContable = "CUENTA_PERDIDA_ACTIVO", Descripcion = "Pérdida por baja de activo", TipoMovimiento = "D", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.BajaActivo, ConceptoContable = "CUENTA_ACTIVOS_FIJOS", Descripcion = "Activos fijos (baja)", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },

            // VENTA_ACTIVO
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.VentaActivo, ConceptoContable = ConceptosContables.CuentaBancos, Descripcion = "Bancos (venta de activo)", TipoMovimiento = "D", Orden = 1, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.VentaActivo, ConceptoContable = ConceptosContables.CuentaDepreciacionAcumulada, Descripcion = "Depreciación acumulada", TipoMovimiento = "D", Orden = 2, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.VentaActivo, ConceptoContable = "CUENTA_ACTIVOS_FIJOS", Descripcion = "Activos fijos (venta)", TipoMovimiento = "H", Orden = 3, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId },
            new CuentaIntegracion { Id = Guid.NewGuid(), EmpresaId = empresaId, Modulo = ModulosContables.ActivosFijos, TipoOperacion = TiposOperacionContable.VentaActivo, ConceptoContable = "CUENTA_GANANCIA_ACTIVO", Descripcion = "Ganancia por venta de activo", TipoMovimiento = "H", Orden = 4, Activo = true, FechaCreacion = fechaCreacion, CreadoPorId = userId }
        });

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
