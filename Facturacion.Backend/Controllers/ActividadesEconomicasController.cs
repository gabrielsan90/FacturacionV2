using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para consultar y gestionar actividades económicas (CIIU)
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/ae
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ActividadesEconomicasController : ControllerBase
{
    private readonly IActividadEconomicaService _actividadService;
    private readonly DataContext _context;
    private readonly ILogger<ActividadesEconomicasController> _logger;

    public ActividadesEconomicasController(
        IActividadEconomicaService actividadService,
        DataContext context,
        ILogger<ActividadesEconomicasController> logger)
    {
        _actividadService = actividadService;
        _context = context;
        _logger = logger;

        // Configure EPPlus license
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Obtiene información detallada de una actividad económica específica
    /// GET: api/ActividadesEconomicas/620101
    /// </summary>
    /// <param name="codigo">Código de actividad económica</param>
    /// <returns>Información de la actividad económica o NotFound si no existe</returns>
    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código de actividad económica es obligatorio");
            }

            var actividad = await _actividadService.ObtenerPorCodigoAsync(codigo);

            if (actividad == null)
            {
                return NotFound($"No se encontró la actividad económica: {codigo}");
            }

            return Ok(actividad);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar actividad económica: {Codigo}", codigo);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar actividad económica: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Valida que un código de actividad económica existe y está activo
    /// GET: api/ActividadesEconomicas/validar/620101
    /// </summary>
    /// <param name="codigo">Código de actividad económica a validar</param>
    /// <returns>True si el código es válido y está activo, False en caso contrario</returns>
    [HttpGet("validar/{codigo}")]
    public async Task<IActionResult> ValidarAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código de actividad económica es obligatorio");
            }

            var esValido = await _actividadService.ValidarCodigoAsync(codigo);

            return Ok(new { codigo, esValido });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar actividad económica: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Busca actividades económicas por descripción
    /// GET: api/ActividadesEconomicas/buscar/descripcion?q=programacion&top=10
    /// </summary>
    /// <param name="q">Texto a buscar en la descripción</param>
    /// <param name="top">Número máximo de resultados (default: 10)</param>
    /// <returns>Lista de actividades económicas que coinciden con la búsqueda</returns>
    [HttpGet("buscar/descripcion")]
    public async Task<IActionResult> BuscarPorDescripcionAsync([FromQuery] string q, [FromQuery] int top = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("El parámetro de búsqueda 'q' es obligatorio");
            }

            if (top <= 0 || top > 100)
            {
                return BadRequest("El parámetro 'top' debe estar entre 1 y 100");
            }

            var resultado = await _actividadService.BuscarPorDescripcionAsync(q, top);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar actividades económicas");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Realiza una búsqueda general de actividades económicas
    /// POST: api/ActividadesEconomicas/buscar
    /// </summary>
    /// <param name="parametros">Parámetros de búsqueda</param>
    /// <returns>Lista de actividades económicas que coinciden con la búsqueda</returns>
    [HttpPost("buscar")]
    public async Task<IActionResult> BuscarAsync([FromBody] ActividadEconomicaBusquedaDTO parametros)
    {
        try
        {
            if (parametros == null)
            {
                return BadRequest("Los parámetros de búsqueda son obligatorios");
            }

            if (parametros.Top <= 0 || parametros.Top > 100)
            {
                parametros.Top = 10;
            }

            var resultado = await _actividadService.BuscarAsync(parametros);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar actividades económicas");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todas las actividades económicas activas
    /// GET: api/ActividadesEconomicas/todas
    /// </summary>
    /// <returns>Lista de todas las actividades económicas activas</returns>
    [HttpGet("todas")]
    public async Task<IActionResult> ObtenerTodasActivasAsync()
    {
        try
        {
            var actividades = await _actividadService.ObtenerTodasActivasAsync();

            return Ok(actividades);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al obtener actividades económicas");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Limpia la caché de actividades económicas
    /// POST: api/ActividadesEconomicas/limpiar-cache
    /// </summary>
    /// <returns>Confirmación de limpieza de caché</returns>
    [HttpPost("limpiar-cache")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public IActionResult LimpiarCacheAsync()
    {
        try
        {
            _actividadService.LimpiarCache();
            _logger.LogInformation("Caché de actividades económicas limpiada");
            return Ok(new { message = "Caché limpiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar caché de actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ========================================
    // ENDPOINTS CRUD PARA GESTIÓN LOCAL
    // ========================================

    /// <summary>
    /// Busca actividades económicas en el catálogo local por código o descripción
    /// GET: api/ActividadesEconomicas/buscar-local?q=620101&top=20
    /// </summary>
    [HttpGet("buscar-local")]
    public async Task<IActionResult> BuscarLocalAsync([FromQuery] string q, [FromQuery] int top = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest(new { message = "El parámetro de búsqueda debe tener al menos 2 caracteres" });
            }

            if (top <= 0 || top > 100)
            {
                top = 20;
            }

            var queryLower = q.ToLower();

            var actividades = await _context.ActividadesEconomicas
                .Where(a => a.Activa &&
                    (a.CodigoCIIU4.ToLower().Contains(queryLower) ||
                     a.Descripcion.ToLower().Contains(queryLower)))
                .OrderBy(a => a.CodigoCIIU4)
                .Take(top)
                .Select(a => new { codigo = a.CodigoCIIU4, descripcion = a.Descripcion })
                .ToListAsync();

            return Ok(actividades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar actividades económicas locales: {Query}", q);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene todas las actividades económicas del catálogo local (paginado)
    /// GET: api/ActividadesEconomicas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] bool? activa = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        try
        {
            var query = _context.ActividadesEconomicas.AsQueryable();

            if (activa.HasValue)
            {
                query = query.Where(a => a.Activa == activa.Value);
            }

            var total = await query.CountAsync();
            var actividades = await query
                .OrderBy(a => a.CodigoCIIU4)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = actividades,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividades económicas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene una actividad económica por ID
    /// GET: api/ActividadesEconomicas/id/1
    /// </summary>
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        try
        {
            var actividad = await _context.ActividadesEconomicas.FindAsync(id);

            if (actividad == null)
            {
                return NotFound(new { message = $"No se encontró la actividad económica con ID: {id}" });
            }

            return Ok(actividad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividad económica {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea una nueva actividad económica
    /// POST: api/ActividadesEconomicas
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperUser,Administrador")]
    public async Task<IActionResult> CreateAsync([FromBody] ActividadEconomica actividad)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que no exista el código
            var existe = await _context.ActividadesEconomicas
                .AnyAsync(a => a.CodigoCIIU4 == actividad.CodigoCIIU4);

            if (existe)
            {
                return BadRequest(new { message = $"Ya existe una actividad económica con el código: {actividad.CodigoCIIU4}" });
            }

            _context.ActividadesEconomicas.Add(actividad);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actividad económica creada: {Codigo} - {Descripcion}", actividad.CodigoCIIU4, actividad.Descripcion);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = actividad.Id }, actividad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear actividad económica");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza una actividad económica existente
    /// PUT: api/ActividadesEconomicas/1
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] ActividadEconomica actividad)
    {
        try
        {
            if (id != actividad.Id)
            {
                return BadRequest(new { message = "El ID no coincide con el ID de la actividad económica" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _context.ActividadesEconomicas.FindAsync(id);
            if (existente == null)
            {
                return NotFound(new { message = $"No se encontró la actividad económica con ID: {id}" });
            }

            // Validar que no exista otro registro con el mismo código
            var duplicado = await _context.ActividadesEconomicas
                .AnyAsync(a => a.CodigoCIIU4 == actividad.CodigoCIIU4 && a.Id != id);

            if (duplicado)
            {
                return BadRequest(new { message = $"Ya existe otra actividad económica con el código: {actividad.CodigoCIIU4}" });
            }

            existente.CodigoCIIU4 = actividad.CodigoCIIU4;
            existente.Descripcion = actividad.Descripcion;
            existente.Activa = actividad.Activa;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Actividad económica actualizada: {Id} - {Codigo}", id, actividad.CodigoCIIU4);

            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar actividad económica {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina una actividad económica
    /// DELETE: api/ActividadesEconomicas/1
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var actividad = await _context.ActividadesEconomicas.FindAsync(id);

            if (actividad == null)
            {
                return NotFound(new { message = $"No se encontró la actividad económica con ID: {id}" });
            }

            // Verificar si está en uso
            var enUso = await _context.EmpresasActividadesEconomicas
                .AnyAsync(ea => ea.ActividadEconomicaId == id);

            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar la actividad económica porque está en uso por una o más empresas" });
            }

            _context.ActividadesEconomicas.Remove(actividad);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actividad económica eliminada: {Id} - {Codigo}", id, actividad.CodigoCIIU4);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar actividad económica {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Importa actividades económicas desde un archivo Excel
    /// POST: api/ActividadesEconomicas/importar-excel
    /// </summary>
    [HttpPost("importar-excel")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public async Task<IActionResult> ImportarExcelAsync(IFormFile archivo)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { message = "Debe proporcionar un archivo Excel válido" });
            }

            // Validar extensión
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest(new { message = "El archivo debe ser un Excel (.xlsx o .xls)" });
            }

            var resultados = new
            {
                procesados = 0,
                creados = 0,
                actualizados = 0,
                duplicadosEnExcel = 0,
                errores = new List<string>()
            };

            using var stream = new MemoryStream();
            await archivo.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                return BadRequest(new { message = "El archivo Excel no contiene hojas de trabajo" });
            }

            // Validar encabezados (asumiendo que están en la fila 1)
            var headerCodigoCIIU4 = worksheet.Cells[1, 1].Value?.ToString();
            var headerDescripcion = worksheet.Cells[1, 2].Value?.ToString();

            if (string.IsNullOrWhiteSpace(headerCodigoCIIU4) || string.IsNullOrWhiteSpace(headerDescripcion))
            {
                return BadRequest(new
                {
                    message = "El archivo debe tener encabezados 'CodigoCIIU4' y 'Descripcion' en las primeras dos columnas"
                });
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;

            // Detectar duplicados dentro del Excel
            var codigosEnExcel = new Dictionary<string, int>(); // codigo -> primera fila donde aparece
            var duplicadosEnExcel = new List<string>();

            // Primera pasada: detectar duplicados
            for (int row = 2; row <= rowCount; row++)
            {
                var codigo = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    if (codigosEnExcel.ContainsKey(codigo))
                    {
                        duplicadosEnExcel.Add($"Fila {row}: Código '{codigo}' duplicado (ya existe en fila {codigosEnExcel[codigo]})");
                    }
                    else
                    {
                        codigosEnExcel[codigo] = row;
                    }
                }
            }

            // Agregar advertencias de duplicados
            foreach (var dup in duplicadosEnExcel)
            {
                ((List<string>)resultados.errores).Add(dup);
            }
            resultados = resultados with { duplicadosEnExcel = duplicadosEnExcel.Count };

            // Segunda pasada: procesar solo la primera ocurrencia de cada código
            var codigosProcesados = new HashSet<string>();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var codigo = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var descripcion = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descripcion))
                    {
                        ((List<string>)resultados.errores).Add($"Fila {row}: Código o descripción vacíos");
                        continue;
                    }

                    // Saltar si ya se procesó este código (duplicado dentro del Excel)
                    if (codigosProcesados.Contains(codigo))
                    {
                        continue; // Ya fue reportado como duplicado en la primera pasada
                    }
                    codigosProcesados.Add(codigo);

                    // Validar longitud
                    if (codigo.Length > 6)
                    {
                        ((List<string>)resultados.errores).Add($"Fila {row}: Código {codigo} excede 6 caracteres");
                        continue;
                    }

                    if (descripcion.Length > 500)
                    {
                        descripcion = descripcion.Substring(0, 500);
                    }

                    // Buscar si existe
                    var existente = await _context.ActividadesEconomicas
                        .FirstOrDefaultAsync(a => a.CodigoCIIU4 == codigo);

                    if (existente != null)
                    {
                        // Actualizar
                        existente.Descripcion = descripcion;
                        existente.Activa = true;
                        resultados = resultados with { actualizados = resultados.actualizados + 1 };
                    }
                    else
                    {
                        // Crear
                        var nueva = new ActividadEconomica
                        {
                            CodigoCIIU4 = codigo,
                            Descripcion = descripcion,
                            Activa = true
                        };
                        _context.ActividadesEconomicas.Add(nueva);
                        resultados = resultados with { creados = resultados.creados + 1 };
                    }

                    resultados = resultados with { procesados = resultados.procesados + 1 };
                }
                catch (Exception ex)
                {
                    ((List<string>)resultados.errores).Add($"Fila {row}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Importación Excel completada: {Procesados} procesados, {Creados} creados, {Actualizados} actualizados, {Duplicados} duplicados en Excel, {Errores} errores",
                resultados.procesados, resultados.creados, resultados.actualizados, resultados.duplicadosEnExcel, resultados.errores.Count);

            return Ok(new
            {
                success = true,
                message = "Importación completada",
                resultados = new
                {
                    resultados.procesados,
                    resultados.creados,
                    resultados.actualizados,
                    resultados.duplicadosEnExcel,
                    errores = resultados.errores.Count,
                    detalleErrores = resultados.errores
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar archivo Excel");
            return StatusCode(500, new { message = $"Error al procesar el archivo: {ex.Message}" });
        }
    }

    /// <summary>
    /// Exporta un template de Excel para importación
    /// GET: api/ActividadesEconomicas/exportar-template
    /// </summary>
    [HttpGet("exportar-template")]
    public IActionResult ExportarTemplate()
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Actividades Económicas");

            // Encabezados
            worksheet.Cells[1, 1].Value = "CodigoCIIU4";
            worksheet.Cells[1, 2].Value = "Descripcion";

            // Dar formato a los encabezados
            using (var range = worksheet.Cells[1, 1, 1, 2])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Agregar algunos ejemplos
            worksheet.Cells[2, 1].Value = "620101";
            worksheet.Cells[2, 2].Value = "Desarrollo de sistemas informáticos y suministro de software";

            worksheet.Cells[3, 1].Value = "620102";
            worksheet.Cells[3, 2].Value = "Consultores en informática y gestión de instalaciones informáticas";

            // Ajustar columnas
            worksheet.Column(1).Width = 15;
            worksheet.Column(2).Width = 80;

            var fileBytes = package.GetAsByteArray();

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Template_Actividades_Economicas.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar template");
            return StatusCode(500, new { message = "Error al generar el template" });
        }
    }
}
