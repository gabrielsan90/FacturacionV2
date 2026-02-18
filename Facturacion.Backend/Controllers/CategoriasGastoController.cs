using Facturacion.Backend.Services.Interfaces;
using Facturacion.Backend.UnitsOfWork.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CategoriasGastoController : ControllerBase
{
    private readonly IGastoUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriasGastoController> _logger;
    private readonly IExcelImportService _excelImportService;

    public CategoriasGastoController(
        IGastoUnitOfWork unitOfWork,
        ILogger<CategoriasGastoController> logger,
        IExcelImportService excelImportService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _excelImportService = excelImportService;
    }

    /// <summary>
    /// Obtiene todas las categorías de gasto (sin filtrar por empresa - legacy)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool includeInactive = false)
    {
        try
        {
            var categorias = await _unitOfWork.CategoriaGastoRepository.GetAllAsync(includeInactive);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías de gasto");
            return StatusCode(500, "Error interno al obtener categorías de gasto.");
        }
    }

    /// <summary>
    /// Obtiene las categorías de gasto de una empresa
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var categorias = await _unitOfWork.CategoriaGastoRepository.GetByEmpresaAsync(empresaId, includeInactive);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías de gasto para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener categorías de gasto.");
        }
    }

    /// <summary>
    /// Obtiene solo las categorías activas de una empresa
    /// </summary>
    [HttpGet("activas/empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetActivasByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var categorias = await _unitOfWork.CategoriaGastoRepository.GetActivasByEmpresaAsync(empresaId);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías activas para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al obtener categorías de gasto activas.");
        }
    }

    /// <summary>
    /// Obtiene solo las categorías activas (legacy)
    /// </summary>
    [HttpGet("activas")]
    public async Task<IActionResult> GetActivasAsync()
    {
        try
        {
            var categorias = await _unitOfWork.CategoriaGastoRepository.GetActivasAsync();
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías de gasto activas");
            return StatusCode(500, "Error interno al obtener categorías de gasto activas.");
        }
    }

    /// <summary>
    /// Obtiene una categoría por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        try
        {
            var categoria = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);

            if (categoria == null)
            {
                return NotFound("La categoría de gasto no existe.");
            }

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría de gasto {CategoriaGastoId}", id);
            return StatusCode(500, "Error interno al obtener categoría de gasto.");
        }
    }

    /// <summary>
    /// Crea una nueva categoría de gasto
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CategoriaGasto categoria)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que no exista otra categoría con el mismo nombre en la misma empresa
            var existente = await _unitOfWork.CategoriaGastoRepository.GetByNombreYEmpresaAsync(categoria.Nombre, categoria.EmpresaId);
            if (existente != null)
            {
                return BadRequest("Ya existe una categoría con este nombre en la empresa.");
            }

            var nuevaCategoria = await _unitOfWork.CategoriaGastoRepository.AddAsync(categoria);

            _logger.LogInformation("Categoría de gasto creada: {Id} - {Nombre} para empresa {EmpresaId}",
                nuevaCategoria.Id, categoria.Nombre, categoria.EmpresaId);

            return Ok(nuevaCategoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría de gasto");
            return StatusCode(500, "Error interno al crear la categoría.");
        }
    }

    /// <summary>
    /// Actualiza una categoría de gasto
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] CategoriaGasto categoria)
    {
        try
        {
            if (id != categoria.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID de la categoría.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);
            if (existente == null)
            {
                return NotFound("La categoría de gasto no existe.");
            }

            // Verificar que no exista otra categoría con el mismo nombre en la misma empresa
            var duplicado = await _unitOfWork.CategoriaGastoRepository.GetByNombreYEmpresaAsync(categoria.Nombre, categoria.EmpresaId);
            if (duplicado != null && duplicado.Id != id)
            {
                return BadRequest("Ya existe otra categoría con este nombre en la empresa.");
            }

            await _unitOfWork.CategoriaGastoRepository.UpdateAsync(categoria);

            _logger.LogInformation("Categoría de gasto actualizada: {Id}", id);

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría de gasto {CategoriaGastoId}", id);
            return StatusCode(500, "Error interno al actualizar la categoría.");
        }
    }

    /// <summary>
    /// Desactiva una categoría de gasto
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var categoria = await _unitOfWork.CategoriaGastoRepository.GetAsync(id);
            if (categoria == null)
            {
                return NotFound("La categoría de gasto no existe.");
            }

            if (categoria.Gastos != null && categoria.Gastos.Any(g => !g.IsDeleted))
            {
                return BadRequest("No se puede desactivar una categoría que tiene gastos asociados.");
            }

            await _unitOfWork.CategoriaGastoRepository.DeleteAsync(id);

            _logger.LogInformation("Categoría de gasto desactivada: {Id}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar categoría de gasto {CategoriaGastoId}", id);
            return StatusCode(500, "Error interno al desactivar la categoría.");
        }
    }

    /// <summary>
    /// Crea un catálogo genérico de categorías de gasto para una empresa
    /// Solo crea las que no existen aún (por nombre) en esa empresa
    /// </summary>
    [HttpPost("catalogo-generico/{empresaId:guid}")]
    public async Task<IActionResult> CrearCatalogoGenericoAsync(Guid empresaId)
    {
        try
        {
            var categoriasGenericas = new List<(string nombre, string descripcion)>
            {
                ("Alquiler", "Alquiler de oficina, bodega o local comercial"),
                ("Servicios Públicos", "Agua, electricidad, internet, teléfono"),
                ("Salarios y Planilla", "Sueldos, cargas sociales, aguinaldos, vacaciones"),
                ("Suministros de Oficina", "Papelería, tinta, materiales de oficina"),
                ("Transporte y Combustible", "Gasolina, peajes, mantenimiento vehicular"),
                ("Alimentación", "Comidas de trabajo, viáticos de alimentación"),
                ("Seguros", "Pólizas de seguros empresariales, INS, riesgos del trabajo"),
                ("Impuestos y Tasas", "Impuestos municipales, patentes, timbres fiscales"),
                ("Mantenimiento y Reparaciones", "Reparaciones de equipo, edificio, vehículos"),
                ("Publicidad y Marketing", "Anuncios, redes sociales, material promocional"),
                ("Servicios Profesionales", "Contabilidad, asesoría legal, consultoría"),
                ("Tecnología", "Software, licencias, hosting, dominios, soporte técnico"),
                ("Viáticos y Hospedaje", "Gastos de viaje, hospedaje, transporte aéreo"),
                ("Capacitación", "Cursos, seminarios, certificaciones del personal"),
                ("Equipo y Herramientas", "Compra de equipo menor, herramientas de trabajo"),
                ("Limpieza y Aseo", "Productos y servicios de limpieza"),
                ("Gastos Bancarios", "Comisiones bancarias, transferencias, chequeras"),
                ("Depreciación", "Depreciación de activos fijos"),
                ("Donaciones", "Donaciones deducibles"),
                ("Otros Gastos", "Gastos varios no clasificados")
            };

            int creadas = 0;
            int existentes = 0;

            foreach (var (nombre, descripcion) in categoriasGenericas)
            {
                var existe = await _unitOfWork.CategoriaGastoRepository.GetByNombreYEmpresaAsync(nombre, empresaId);
                if (existe != null)
                {
                    existentes++;
                    continue;
                }

                await _unitOfWork.CategoriaGastoRepository.AddAsync(new CategoriaGasto
                {
                    EmpresaId = empresaId,
                    Nombre = nombre,
                    Descripcion = descripcion,
                    Activa = true
                });
                creadas++;
            }

            _logger.LogInformation("Catálogo genérico creado para empresa {EmpresaId}: {Creadas} nuevas, {Existentes} ya existían",
                empresaId, creadas, existentes);

            return Ok(new
            {
                success = true,
                message = creadas > 0
                    ? $"Se crearon {creadas} categorías genéricas" + (existentes > 0 ? $" ({existentes} ya existían)" : "")
                    : "Todas las categorías genéricas ya existen",
                creadas,
                existentes,
                total = creadas + existentes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear catálogo genérico para empresa {EmpresaId}", empresaId);
            return StatusCode(500, "Error interno al crear el catálogo genérico.");
        }
    }

    /// <summary>
    /// Importa categorías de gasto desde un archivo Excel
    /// </summary>
    [HttpPost("importar")]
    public async Task<IActionResult> ImportarAsync(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe proporcionar un archivo Excel.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        using var stream = archivo.OpenReadStream();
        var result = await _excelImportService.ImportarCategoriasGastoAsync(stream, userId!);
        return Ok(result);
    }

    /// <summary>
    /// Descarga una plantilla de Excel para importar categorías de gasto
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        var fileBytes = _excelImportService.GenerarPlantillaCategoriasGasto();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_CategoriasGasto.xlsx");
    }
}
