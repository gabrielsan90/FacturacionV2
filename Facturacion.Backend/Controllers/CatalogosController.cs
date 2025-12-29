using Facturacion.Backend.Data;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities.Catalogos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para catálogos oficiales de Hacienda v4.4
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    private readonly ILogger<CatalogosController> _logger;

    public CatalogosController(
        ICatalogoUnitOfWork unitOfWork,
        DataContext context,
        ILogger<CatalogosController> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las provincias activas de Costa Rica
    /// </summary>
    /// <returns>Lista de provincias con formato { codigo, nombre }</returns>
    [HttpGet("provincias")]
    public async Task<IActionResult> GetProvinciasAsync()
    {
        try
        {
            var provincias = await _unitOfWork.Provincias.GetAllAsync();

            // Mapear a formato esperado por frontend: { codigo, nombre }
            var resultado = provincias.Select(p => new
            {
                codigo = int.Parse(p.Codigo),
                nombre = p.Descripcion
            }).ToList();

            return Ok(new { success = true, data = resultado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener provincias");
            return StatusCode(500, new { success = false, message = "Error al obtener provincias" });
        }
    }

    /// <summary>
    /// Obtiene todos los cantones de una provincia específica
    /// </summary>
    /// <param name="provinciaId">ID de la provincia (1-7)</param>
    /// <returns>Lista de cantones con formato { codigo, nombre }</returns>
    [HttpGet("cantones/{provinciaId:int}")]
    public async Task<IActionResult> GetCantonesByProvinciaAsync(int provinciaId)
    {
        try
        {
            if (provinciaId < 1 || provinciaId > 7)
            {
                return BadRequest(new { success = false, message = "ID de provincia inválido. Debe ser entre 1 y 7." });
            }

            var cantones = await _unitOfWork.Cantones.GetByProvinciaAsync(provinciaId);

            // Mapear a formato esperado por frontend: { codigo, nombre }
            var resultado = cantones.Select(c => new
            {
                codigo = int.Parse(c.Codigo),
                nombre = c.Descripcion
            }).ToList();

            return Ok(new { success = true, data = resultado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cantones para provincia {ProvinciaId}", provinciaId);
            return StatusCode(500, new { success = false, message = "Error al obtener cantones" });
        }
    }

    /// <summary>
    /// Obtiene todos los distritos de un cantón específico
    /// </summary>
    /// <param name="provinciaId">ID de la provincia (1-7)</param>
    /// <param name="cantonId">Código del cantón (formato: 0101, 0102, etc.)</param>
    /// <returns>Lista de distritos con formato { codigo, nombre }</returns>
    [HttpGet("distritos/{provinciaId:int}/{cantonId:int}")]
    public async Task<IActionResult> GetDistritosByCantonAsync(int provinciaId, int cantonId)
    {
        try
        {
            if (provinciaId < 1 || provinciaId > 7)
            {
                return BadRequest(new { success = false, message = "ID de provincia inválido. Debe ser entre 1 y 7." });
            }

            // Construir código de cantón en formato "PCC" (ej: 101 = San José, Carmen)
            string codigoCanton = $"{provinciaId}{cantonId:D2}";

            // Buscar el cantón por código
            var canton = await _unitOfWork.Cantones.GetByCodigoAsync(codigoCanton);
            if (canton == null)
            {
                return NotFound(new { success = false, message = "Cantón no encontrado" });
            }

            var distritos = await _unitOfWork.Distritos.GetByCantonAsync(canton.Id);

            // Mapear a formato esperado por frontend: { codigo, nombre }
            var resultado = distritos.Select(d => new
            {
                codigo = int.Parse(d.Codigo),
                nombre = d.Descripcion
            }).ToList();

            return Ok(new { success = true, data = resultado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener distritos para cantón {ProvinciaId}-{CantonId}", provinciaId, cantonId);
            return StatusCode(500, new { success = false, message = "Error al obtener distritos" });
        }
    }

    /// <summary>
    /// Obtiene una provincia por su ID
    /// </summary>
    [HttpGet("provincias/{id:int}")]
    public async Task<IActionResult> GetProvinciaAsync(int id)
    {
        try
        {
            var provincia = await _unitOfWork.Provincias.GetAsync(id);
            if (provincia == null)
            {
                return NotFound(new { success = false, message = "Provincia no encontrada" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = provincia.Id,
                    codigo = int.Parse(provincia.Codigo),
                    nombre = provincia.Descripcion,
                    activo = provincia.Activo
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener provincia {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener provincia" });
        }
    }

    // ==========================================
    // CATÁLOGOS HACIENDA v4.4
    // ==========================================

    /// <summary>
    /// Busca códigos CAByS por texto (código o descripción)
    /// </summary>
    [HttpGet("cabys")]
    public async Task<IActionResult> BuscarCAByS([FromQuery] string busqueda, [FromQuery] int limite = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(busqueda) || busqueda.Length < 3)
            {
                return Ok(new { success = true, data = new List<object>() });
            }

            var cabys = await _context.Set<CAByS>()
                .Where(c => c.Activo &&
                           (c.Codigo.Contains(busqueda) ||
                            c.Descripcion.Contains(busqueda)))
                .Take(limite)
                .Select(c => new
                {
                    c.Codigo,
                    c.Descripcion,
                    c.Categoria,
                    c.CodigoImpuesto,
                    c.TarifaImpuesto
                })
                .ToListAsync();

            return Ok(new { success = true, data = cabys });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar CAByS: {Busqueda}", busqueda);
            return StatusCode(500, new { success = false, message = "Error al buscar códigos CAByS" });
        }
    }

    /// <summary>
    /// Obtiene un código CAByS específico
    /// </summary>
    [HttpGet("cabys/{codigo}")]
    public async Task<IActionResult> GetCAByS(string codigo)
    {
        try
        {
            var cabys = await _context.Set<CAByS>()
                .FirstOrDefaultAsync(c => c.Codigo == codigo && c.Activo);

            if (cabys == null)
                return NotFound(new { success = false, message = "Código CAByS no encontrado" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    cabys.Codigo,
                    cabys.Descripcion,
                    cabys.Categoria,
                    cabys.CodigoImpuesto,
                    cabys.TarifaImpuesto
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener CAByS: {Codigo}", codigo);
            return StatusCode(500, new { success = false, message = "Error al obtener código CAByS" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de descuento de Hacienda v4.4
    /// </summary>
    [HttpGet("tipos-descuento")]
    public async Task<IActionResult> GetTiposDescuento()
    {
        try
        {
            var tipos = await _context.Set<TipoDescuentoHacienda>()
                .Where(t => t.Activo)
                .OrderBy(t => t.Codigo)
                .Select(t => new
                {
                    t.Codigo,
                    t.Descripcion
                })
                .ToListAsync();

            return Ok(new { success = true, data = tipos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de descuento");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de descuento" });
        }
    }

    /// <summary>
    /// Obtiene todos los códigos de referencia de Hacienda v4.4
    /// </summary>
    [HttpGet("codigos-referencia")]
    public async Task<IActionResult> GetCodigosReferencia()
    {
        try
        {
            var codigos = await _context.Set<CodigoReferencia>()
                .Where(c => c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new
                {
                    c.Codigo,
                    c.Descripcion
                })
                .ToListAsync();

            return Ok(new { success = true, data = codigos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener códigos de referencia");
            return StatusCode(500, new { success = false, message = "Error al obtener códigos de referencia" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de documento de referencia
    /// </summary>
    [HttpGet("tipos-documento-referencia")]
    public async Task<IActionResult> GetTiposDocumentoReferencia()
    {
        try
        {
            var tipos = await _context.Set<TipoDocumentoReferencia>()
                .Where(t => t.Activo)
                .OrderBy(t => t.Codigo)
                .Select(t => new
                {
                    t.Codigo,
                    t.Descripcion
                })
                .ToListAsync();

            return Ok(new { success = true, data = tipos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de documento de referencia");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de documento de referencia" });
        }
    }

    /// <summary>
    /// Obtiene todos los medios de pago (incluye SINPE Móvil v4.4)
    /// </summary>
    [HttpGet("medios-pago")]
    public async Task<IActionResult> GetMediosPago()
    {
        try
        {
            var medios = await _context.Set<MedioPago>()
                .Where(m => m.Activo)
                .OrderBy(m => m.Codigo)
                .Select(m => new
                {
                    m.Codigo,
                    nombre = m.Descripcion
                })
                .ToListAsync();

            return Ok(new { success = true, data = medios });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener medios de pago");
            return StatusCode(500, new { success = false, message = "Error al obtener medios de pago" });
        }
    }

    /// <summary>
    /// Obtiene todas las formas de pago (alias de medios-pago para frontend)
    /// </summary>
    [HttpGet("formas-pago")]
    public async Task<IActionResult> GetFormasPago()
    {
        try
        {
            var formas = await _context.Set<MedioPago>()
                .Where(m => m.Activo)
                .OrderBy(m => m.Codigo)
                .Select(m => new
                {
                    m.Codigo,
                    nombre = m.Descripcion
                })
                .ToListAsync();

            return Ok(formas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener formas de pago");
            return StatusCode(500, new { success = false, message = "Error al obtener formas de pago" });
        }
    }

    /// <summary>
    /// Obtiene todas las condiciones de venta (incluye nuevos códigos v4.4)
    /// </summary>
    [HttpGet("condiciones-venta")]
    public async Task<IActionResult> GetCondicionesVenta()
    {
        try
        {
            var condiciones = await _context.Set<CondicionVenta>()
                .Where(c => c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new
                {
                    c.Codigo,
                    nombre = c.Descripcion
                })
                .ToListAsync();

            return Ok(condiciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener condiciones de venta");
            return StatusCode(500, new { success = false, message = "Error al obtener condiciones de venta" });
        }
    }

    /// <summary>
    /// Obtiene todas las unidades de medida
    /// </summary>
    [HttpGet("unidades-medida")]
    public async Task<IActionResult> GetUnidadesMedida()
    {
        try
        {
            var unidades = await _context.Set<UnidadMedida>()
                .Where(u => u.Activo)
                .OrderBy(u => u.Codigo)
                .Select(u => new
                {
                    u.Id,
                    u.Codigo,
                    nombre = u.Descripcion
                })
                .ToListAsync();

            return Ok(unidades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener unidades de medida");
            return StatusCode(500, new { success = false, message = "Error al obtener unidades de medida" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de documento (Hacienda v4.4)
    /// </summary>
    [HttpGet("tipos-documentos")]
    public async Task<IActionResult> GetTiposDocumentos()
    {
        try
        {
            var tipos = await _context.TiposDocumento
                .Where(t => t.Activo)
                .OrderBy(t => t.Codigo)
                .Select(t => new
                {
                    t.Codigo,
                    nombre = t.Descripcion
                })
                .ToListAsync();

            return Ok(tipos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de documentos");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de documentos" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de referencia (Hacienda v4.4)
    /// </summary>
    [HttpGet("tipos-referencias")]
    public async Task<IActionResult> GetTiposReferencias()
    {
        try
        {
            var tipos = await _context.Set<CodigoReferencia>()
                .Where(c => c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new
                {
                    c.Codigo,
                    nombre = c.Descripcion
                })
                .ToListAsync();

            return Ok(tipos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de referencias");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de referencias" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de otros cargos (Hacienda v4.4)
    /// </summary>
    [HttpGet("tipos-otros-cargos")]
    public IActionResult GetTiposOtrosCargos()
    {
        try
        {
            // Lista de tipos de otros cargos según Hacienda v4.4
            var tipos = new[]
            {
                new { codigo = "01", nombre = "Contribución parafiscal" },
                new { codigo = "02", nombre = "Timbre de la Cruz Roja" },
                new { codigo = "03", nombre = "Timbre de Benemérito Cuerpo de Bomberos de Costa Rica" },
                new { codigo = "04", nombre = "Cobro de un tercero" },
                new { codigo = "05", nombre = "Costos de exportación" },
                new { codigo = "06", nombre = "Impuesto de servicio 10%" },
                new { codigo = "07", nombre = "Timbre de Colegios Profesionales" },
                new { codigo = "99", nombre = "Otros Cargos" }
            };

            return Ok(tipos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de otros cargos");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de otros cargos" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de códigos de producto (Hacienda v4.4)
    /// </summary>
    [HttpGet("tipos-codigos-producto")]
    public async Task<IActionResult> GetTiposCodigosProducto()
    {
        try
        {
            var tipos = await _context.TiposCodigo
                .Where(t => t.Activo)
                .OrderBy(t => t.Codigo)
                .Select(t => new
                {
                    t.Codigo,
                    nombre = t.Descripcion
                })
                .ToListAsync();

            return Ok(tipos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de códigos de producto");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de códigos de producto" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de identificación (Hacienda v4.4)
    /// </summary>
    [HttpGet("tipos-identificacion")]
    public IActionResult GetTiposIdentificacion()
    {
        try
        {
            // Lista de tipos de identificación según Hacienda v4.4
            var tipos = new[]
            {
                new { codigo = "01", nombre = "Cédula Física" },
                new { codigo = "02", nombre = "Cédula Jurídica" },
                new { codigo = "03", nombre = "DIMEX" },
                new { codigo = "04", nombre = "NITE" },
                new { codigo = "05", nombre = "Pasaporte" },
                new { codigo = "06", nombre = "Extranjero no domiciliado" },
                new { codigo = "07", nombre = "No contribuyente" }
            };

            return Ok(tipos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de identificación");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de identificación" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de exoneración (Hacienda v4.4)
    /// </summary>
    [HttpGet("tipos-exoneracion")]
    public async Task<IActionResult> GetTiposExoneracion()
    {
        try
        {
            var tipos = await _context.CodigosExoneracion
                .Where(c => c.Activo)
                .OrderBy(c => c.Codigo)
                .Select(c => new
                {
                    c.Codigo,
                    nombre = c.Descripcion
                })
                .ToListAsync();

            return Ok(tipos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de exoneración");
            return StatusCode(500, new { success = false, message = "Error al obtener tipos de exoneración" });
        }
    }

    /// <summary>
    /// Obtiene todos los tipos de impuesto
    /// </summary>
    [HttpGet("impuestos")]
    public async Task<IActionResult> GetImpuestos()
    {
        try
        {
            var impuestos = await _context.Set<Impuesto>()
                .Where(i => i.Activo)
                .OrderBy(i => i.Codigo)
                .Select(i => new
                {
                    i.Id,
                    i.Codigo,
                    descripcion = i.Descripcion,
                    tarifa = i.Porcentaje
                })
                .ToListAsync();

            return Ok(impuestos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener impuestos");
            return StatusCode(500, new { success = false, message = "Error al obtener impuestos" });
        }
    }

    /// <summary>
    /// Obtiene todas las formas farmacéuticas (Hacienda v4.4 - Medicamentos)
    /// </summary>
    [HttpGet("formas-farmaceuticas")]
    public async Task<IActionResult> GetFormasFarmaceuticas()
    {
        try
        {
            var formas = await _context.FormasFarmaceuticas
                .Where(f => f.Activo)
                .OrderBy(f => f.Codigo)
                .Select(f => new
                {
                    f.Codigo,
                    nombre = f.Descripcion
                })
                .ToListAsync();

            return Ok(formas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener formas farmacéuticas");
            return StatusCode(500, new { success = false, message = "Error al obtener formas farmacéuticas" });
        }
    }

    /// <summary>
    /// Obtiene todas las monedas soportadas (ISO 4217 - Hacienda v4.4)
    /// </summary>
    [HttpGet("monedas")]
    public IActionResult GetMonedas()
    {
        try
        {
            // Lista de monedas principales según Hacienda Costa Rica
            var monedas = new[]
            {
                new { codigo = "CRC", nombre = "Colón Costarricense" },
                new { codigo = "USD", nombre = "Dólar Estadounidense" },
                new { codigo = "EUR", nombre = "Euro" },
                new { codigo = "CAD", nombre = "Dólar Canadiense" },
                new { codigo = "GBP", nombre = "Libra Esterlina" },
                new { codigo = "JPY", nombre = "Yen Japonés" },
                new { codigo = "CHF", nombre = "Franco Suizo" },
                new { codigo = "MXN", nombre = "Peso Mexicano" },
                new { codigo = "CNY", nombre = "Yuan Chino" },
                new { codigo = "BRL", nombre = "Real Brasileño" }
            };

            return Ok(monedas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener monedas");
            return StatusCode(500, new { success = false, message = "Error al obtener monedas" });
        }
    }

    /// <summary>
    /// Obtiene todas las tarifas de IVA (Hacienda v4.4)
    /// </summary>
    [HttpGet("codigos-tarifas-iva")]
    public async Task<IActionResult> GetCodigosTarifasIVA()
    {
        try
        {
            var tarifas = await _context.Set<TarifaIVA>()
                .Where(t => t.Activo)
                .OrderBy(t => t.Codigo)
                .Select(t => new
                {
                    t.Id,
                    t.Codigo,
                    t.Descripcion,
                    t.Porcentaje
                })
                .ToListAsync();

            return Ok(tarifas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener códigos de tarifas IVA");
            return StatusCode(500, new { success = false, message = "Error al obtener códigos de tarifas IVA" });
        }
    }

    /// <summary>
    /// Obtiene las actividades económicas CIIU4 (obligatorio desde 31/08/2025)
    /// </summary>
    [HttpGet("actividades-economicas")]
    public async Task<IActionResult> BuscarActividadesEconomicas([FromQuery] string? busqueda = null, [FromQuery] int limite = 50)
    {
        try
        {
            var query = _context.ActividadesEconomicas
                .Where(a => a.Activa);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(a => a.CodigoCIIU4.Contains(busqueda) ||
                                        a.Descripcion.Contains(busqueda));
            }

            var actividades = await query
                .Take(limite)
                .OrderBy(a => a.CodigoCIIU4)
                .Select(a => new
                {
                    codigo = a.CodigoCIIU4,
                    descripcion = a.Descripcion
                })
                .ToListAsync();

            return Ok(new { success = true, data = actividades });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar actividades económicas");
            return StatusCode(500, new { success = false, message = "Error al buscar actividades económicas" });
        }
    }
}
