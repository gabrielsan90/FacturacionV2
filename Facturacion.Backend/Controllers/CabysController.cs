using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities.Catalogos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para consultar códigos CABYS (Códigos de Actividades, Bienes y Servicios)
/// API oficial de Hacienda: https://api.hacienda.go.cr/fe/cabys
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class CabysController : ControllerBase
{
    private readonly ICabysService _cabysService;
    private readonly DataContext _context;
    private readonly ILogger<CabysController> _logger;

    public CabysController(
        ICabysService cabysService,
        DataContext context,
        ILogger<CabysController> logger)
    {
        _cabysService = cabysService;
        _context = context;
        _logger = logger;

        // Configure EPPlus license
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Obtiene información detallada de un código CABYS específico
    /// GET: api/Cabys/12345678901234
    /// </summary>
    /// <param name="codigo">Código CABYS (13 dígitos)</param>
    /// <returns>Información del código CABYS o NotFound si no existe</returns>
    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código CABYS es obligatorio");
            }

            var cabys = await _cabysService.ObtenerPorCodigoAsync(codigo);

            if (cabys == null)
            {
                return NotFound($"No se encontró el código CABYS: {codigo}");
            }

            return Ok(cabys);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar código CABYS: {Codigo}", codigo);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar código CABYS: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Valida que un código CABYS existe y es válido
    /// GET: api/Cabys/validar/12345678901234
    /// </summary>
    /// <param name="codigo">Código CABYS a validar (13 dígitos)</param>
    /// <returns>True si el código es válido, False en caso contrario</returns>
    [HttpGet("validar/{codigo}")]
    public async Task<IActionResult> ValidarAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código CABYS es obligatorio");
            }

            var esValido = await _cabysService.ValidarCodigoAsync(codigo);

            return Ok(new { codigo, esValido });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar código CABYS: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Busca códigos CABYS por descripción
    /// GET: api/Cabys/buscar/descripcion?q=cafe&top=10
    /// </summary>
    /// <param name="q">Texto a buscar en la descripción</param>
    /// <param name="top">Número máximo de resultados (default: 10)</param>
    /// <returns>Lista de códigos CABYS que coinciden con la búsqueda</returns>
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

            var resultado = await _cabysService.BuscarPorDescripcionAsync(q, top);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar códigos CABYS");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar códigos CABYS");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Realiza una búsqueda general de códigos CABYS
    /// POST: api/Cabys/buscar
    /// </summary>
    /// <param name="parametros">Parámetros de búsqueda</param>
    /// <returns>Lista de códigos CABYS que coinciden con la búsqueda</returns>
    [HttpPost("buscar")]
    public async Task<IActionResult> BuscarAsync([FromBody] CabysBusquedaDTO parametros)
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

            var resultado = await _cabysService.BuscarAsync(parametros);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al buscar códigos CABYS");
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar códigos CABYS");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene el porcentaje de impuesto para un código CABYS
    /// GET: api/Cabys/12345678901234/impuesto
    /// </summary>
    /// <param name="codigo">Código CABYS (13 dígitos)</param>
    /// <returns>Porcentaje de impuesto o NotFound si el código no existe</returns>
    [HttpGet("{codigo}/impuesto")]
    public async Task<IActionResult> ObtenerImpuestoAsync(string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest("El código CABYS es obligatorio");
            }

            var impuesto = await _cabysService.ObtenerPorcentajeImpuestoAsync(codigo);

            if (!impuesto.HasValue)
            {
                return NotFound($"No se encontró el código CABYS: {codigo}");
            }

            return Ok(new { codigo, impuesto = impuesto.Value });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al consultar impuesto de código CABYS: {Codigo}", codigo);
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar impuesto de código CABYS: {Codigo}", codigo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Limpia la caché de códigos CABYS
    /// POST: api/Cabys/limpiar-cache
    /// </summary>
    /// <returns>Confirmación de limpieza de caché</returns>
    [HttpPost("limpiar-cache")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public IActionResult LimpiarCacheAsync()
    {
        try
        {
            _cabysService.LimpiarCache();
            _logger.LogInformation("Caché de códigos CABYS limpiada");
            return Ok(new { message = "Caché limpiada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar caché de códigos CABYS");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ========================================
    // ENDPOINTS CRUD PARA GESTIÓN LOCAL
    // ========================================

    /// <summary>
    /// Obtiene todos los códigos CABYS del catálogo local (paginado)
    /// GET: api/Cabys/local
    /// </summary>
    [HttpGet("local")]
    public async Task<IActionResult> GetAllLocalAsync([FromQuery] bool? activo = null, [FromQuery] string? categoria = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        try
        {
            var query = _context.CatalogosCAByS.AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(c => c.Activo == activo.Value);
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(c => c.Categoria == categoria);
            }

            var total = await query.CountAsync();
            var cabys = await query
                .OrderBy(c => c.Codigo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = cabys,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener códigos CABYS locales");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Busca códigos CABYS en el catálogo local por código o descripción
    /// GET: api/Cabys/local/buscar?q=1234567890123&top=20
    /// </summary>
    [HttpGet("local/buscar")]
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

            var cabys = await _context.CatalogosCAByS
                .Where(c => c.Activo &&
                    (c.Codigo.Contains(q) ||
                     c.Descripcion.ToLower().Contains(queryLower)))
                .OrderBy(c => c.Codigo)
                .Take(top)
                .Select(c => new
                {
                    id = c.Id,
                    codigo = c.Codigo,
                    descripcion = c.Descripcion,
                    impuesto = c.TarifaImpuesto
                })
                .ToListAsync();

            return Ok(cabys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar códigos CABYS locales: {Query}", q);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un código CABYS por ID del catálogo local
    /// GET: api/Cabys/local/1
    /// </summary>
    [HttpGet("local/{id:int}")]
    public async Task<IActionResult> GetByIdLocalAsync(int id)
    {
        try
        {
            var cabys = await _context.CatalogosCAByS.FindAsync(id);

            if (cabys == null)
            {
                return NotFound(new { message = $"No se encontró el código CABYS con ID: {id}" });
            }

            return Ok(cabys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener código CABYS {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo código CABYS en el catálogo local
    /// POST: api/Cabys/local
    /// </summary>
    [HttpPost("local")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public async Task<IActionResult> CreateLocalAsync([FromBody] CAByS cabys)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que no exista el código
            var existe = await _context.CatalogosCAByS
                .AnyAsync(c => c.Codigo == cabys.Codigo);

            if (existe)
            {
                return BadRequest(new { message = $"Ya existe un código CABYS: {cabys.Codigo}" });
            }

            _context.CatalogosCAByS.Add(cabys);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Código CABYS creado: {Codigo} - {Descripcion}", cabys.Codigo, cabys.Descripcion);

            return CreatedAtAction(nameof(GetByIdLocalAsync), new { id = cabys.Id }, cabys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear código CABYS");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un código CABYS existente en el catálogo local
    /// PUT: api/Cabys/local/1
    /// </summary>
    [HttpPut("local/{id:int}")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public async Task<IActionResult> UpdateLocalAsync(int id, [FromBody] CAByS cabys)
    {
        try
        {
            if (id != cabys.Id)
            {
                return BadRequest(new { message = "El ID no coincide con el ID del código CABYS" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existente = await _context.CatalogosCAByS.FindAsync(id);
            if (existente == null)
            {
                return NotFound(new { message = $"No se encontró el código CABYS con ID: {id}" });
            }

            // Validar que no exista otro registro con el mismo código
            var duplicado = await _context.CatalogosCAByS
                .AnyAsync(c => c.Codigo == cabys.Codigo && c.Id != id);

            if (duplicado)
            {
                return BadRequest(new { message = $"Ya existe otro código CABYS: {cabys.Codigo}" });
            }

            existente.Codigo = cabys.Codigo;
            existente.Descripcion = cabys.Descripcion;
            existente.Categoria = cabys.Categoria;
            existente.CodigoImpuesto = cabys.CodigoImpuesto;
            existente.TarifaImpuesto = cabys.TarifaImpuesto;
            existente.Activo = cabys.Activo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Código CABYS actualizado: {Id} - {Codigo}", id, cabys.Codigo);

            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar código CABYS {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un código CABYS del catálogo local
    /// DELETE: api/Cabys/local/1
    /// </summary>
    [HttpDelete("local/{id:int}")]
    [Authorize(Roles = "SuperUser,Administrador")]
    public async Task<IActionResult> DeleteLocalAsync(int id)
    {
        try
        {
            var cabys = await _context.CatalogosCAByS.FindAsync(id);

            if (cabys == null)
            {
                return NotFound(new { message = $"No se encontró el código CABYS con ID: {id}" });
            }

            // TODO: Agregar validación de uso cuando Producto tenga relación con CABYS
            // var enUso = await _context.Productos.AnyAsync(p => p.CodigoCabysId == cabys.Id);

            _context.CatalogosCAByS.Remove(cabys);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Código CABYS eliminado: {Id} - {Codigo}", id, cabys.Codigo);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar código CABYS {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Importa códigos CABYS desde un archivo Excel
    /// POST: api/Cabys/local/importar-excel
    /// </summary>
    [HttpPost("local/importar-excel")]
    [Authorize(Roles = "SuperUser,Administrador")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
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

            var procesados = 0;
            var creados = 0;
            var actualizados = 0;
            var duplicadosEnExcel = 0;
            var errores = new List<string>();

            using var stream = new MemoryStream();
            await archivo.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                return BadRequest(new { message = "El archivo Excel no contiene hojas de trabajo" });
            }

            // Validar encabezados (asumiendo que están en la fila 1)
            var headerCodigo = worksheet.Cells[1, 1].Value?.ToString();
            var headerDescripcion = worksheet.Cells[1, 2].Value?.ToString();
            var headerImpuesto = worksheet.Cells[1, 3].Value?.ToString();
            var headerCategoria = worksheet.Cells[1, 4].Value?.ToString();

            if (string.IsNullOrWhiteSpace(headerCodigo) || string.IsNullOrWhiteSpace(headerDescripcion))
            {
                return BadRequest(new
                {
                    message = "El archivo debe tener encabezados 'Codigo', 'Descripcion', 'Impuesto' y 'Categoria' en las primeras cuatro columnas"
                });
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;

            // OPTIMIZACIÓN: Cargar todos los códigos existentes en memoria
            _logger.LogInformation("Cargando códigos CABYS existentes en memoria...");
            var codigosExistentes = await _context.CatalogosCAByS
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Codigo, c => c.Id);
            _logger.LogInformation("Se cargaron {Count} códigos existentes", codigosExistentes.Count);

            // OPTIMIZACIÓN: Cargar todos los impuestos en memoria para asociar
            var impuestos = await _context.Impuestos
                .Where(i => i.Activo)
                .AsNoTracking()
                .ToListAsync();
            _logger.LogInformation("Se cargaron {Count} impuestos", impuestos.Count);

            // Detectar duplicados dentro del Excel
            var codigosEnExcel = new Dictionary<string, int>(); // codigo -> primera fila donde aparece
            var duplicadosEnExcelList = new List<string>();

            // Primera pasada: detectar duplicados
            for (int row = 2; row <= rowCount; row++)
            {
                var codigo = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    if (codigosEnExcel.ContainsKey(codigo))
                    {
                        duplicadosEnExcelList.Add($"Fila {row}: Código '{codigo}' duplicado (ya existe en fila {codigosEnExcel[codigo]})");
                    }
                    else
                    {
                        codigosEnExcel[codigo] = row;
                    }
                }
            }

            // Agregar advertencias de duplicados (máximo 100 para no saturar respuesta)
            var duplicadosMostrados = duplicadosEnExcelList.Take(100).ToList();
            errores.AddRange(duplicadosMostrados);
            if (duplicadosEnExcelList.Count > 100)
            {
                errores.Add($"... y {duplicadosEnExcelList.Count - 100} duplicados más");
            }
            duplicadosEnExcel = duplicadosEnExcelList.Count;

            // Segunda pasada: procesar solo la primera ocurrencia de cada código
            var codigosProcesados = new HashSet<string>();
            var nuevosRegistros = new List<CAByS>();
            var registrosActualizar = new List<(int Id, string Descripcion, string? CodigoImpuesto, decimal? TarifaImpuesto, string? Categoria)>();
            const int batchSize = 500;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var codigo = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var descripcion = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var impuestoStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var categoria = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descripcion))
                    {
                        if (errores.Count < 200) // Limitar errores en respuesta
                            errores.Add($"Fila {row}: Código o descripción vacíos");
                        continue;
                    }

                    // Saltar si ya se procesó este código (duplicado dentro del Excel)
                    if (codigosProcesados.Contains(codigo))
                    {
                        continue; // Ya fue reportado como duplicado en la primera pasada
                    }
                    codigosProcesados.Add(codigo);

                    // Validar longitud del código (13 dígitos)
                    if (codigo.Length != 13)
                    {
                        if (errores.Count < 200)
                            errores.Add($"Fila {row}: Código {codigo} debe tener exactamente 13 dígitos");
                        continue;
                    }

                    if (descripcion.Length > 500)
                    {
                        descripcion = descripcion.Substring(0, 500);
                    }

                    // Determinar impuesto basado en el valor de la columna
                    string? codigoImpuesto = null;
                    decimal? tarifaImpuesto = null;

                    if (!string.IsNullOrWhiteSpace(impuestoStr))
                    {
                        // Si dice "Excento" o "Exento", no asociar impuesto
                        var impuestoLower = impuestoStr.ToLowerInvariant().Trim();
                        if (impuestoLower == "excento" || impuestoLower == "exento" || impuestoLower == "exonerado")
                        {
                            // Sin impuesto - dejar null
                            codigoImpuesto = null;
                            tarifaImpuesto = 0;
                        }
                        else if (decimal.TryParse(impuestoStr.Replace("%", ""), out var impuestoValue))
                        {
                            // Buscar el impuesto correspondiente por porcentaje
                            var impuestoEncontrado = impuestos.FirstOrDefault(i => i.Porcentaje == impuestoValue);
                            if (impuestoEncontrado != null)
                            {
                                codigoImpuesto = impuestoEncontrado.Codigo;
                                tarifaImpuesto = impuestoEncontrado.Porcentaje;
                            }
                            else
                            {
                                // Si no encuentra el impuesto, solo guardar la tarifa
                                tarifaImpuesto = impuestoValue;
                            }
                        }
                    }

                    // Verificar si existe usando el diccionario en memoria (OPTIMIZACIÓN)
                    if (codigosExistentes.TryGetValue(codigo, out var existenteId))
                    {
                        // Marcar para actualización
                        registrosActualizar.Add((existenteId, descripcion, codigoImpuesto, tarifaImpuesto, categoria));
                        actualizados++;
                    }
                    else
                    {
                        // Crear nuevo
                        var nuevo = new CAByS
                        {
                            Codigo = codigo,
                            Descripcion = descripcion,
                            CodigoImpuesto = codigoImpuesto,
                            TarifaImpuesto = tarifaImpuesto,
                            Categoria = categoria,
                            Activo = true
                        };
                        nuevosRegistros.Add(nuevo);
                        codigosExistentes[codigo] = 0; // Marcar como procesado
                        creados++;
                    }

                    procesados++;

                    // Guardar en lotes para evitar timeout
                    if ((procesados % batchSize) == 0)
                    {
                        // Insertar nuevos registros en lote
                        if (nuevosRegistros.Count > 0)
                        {
                            _context.CatalogosCAByS.AddRange(nuevosRegistros);
                            await _context.SaveChangesAsync();
                            nuevosRegistros.Clear();
                        }

                        // Actualizar registros existentes en lote
                        if (registrosActualizar.Count > 0)
                        {
                            foreach (var (id, desc, codImp, tarImp, cat) in registrosActualizar)
                            {
                                await _context.CatalogosCAByS
                                    .Where(c => c.Id == id)
                                    .ExecuteUpdateAsync(setters => setters
                                        .SetProperty(c => c.Descripcion, desc)
                                        .SetProperty(c => c.CodigoImpuesto, codImp)
                                        .SetProperty(c => c.TarifaImpuesto, tarImp)
                                        .SetProperty(c => c.Categoria, cat)
                                        .SetProperty(c => c.Activo, true));
                            }
                            registrosActualizar.Clear();
                        }

                        _logger.LogInformation("Procesados {Count} registros de {Total}...", procesados, rowCount - 1);
                    }
                }
                catch (Exception ex)
                {
                    if (errores.Count < 200)
                        errores.Add($"Fila {row}: {ex.Message}");
                }
            }

            // Guardar registros restantes
            if (nuevosRegistros.Count > 0)
            {
                _context.CatalogosCAByS.AddRange(nuevosRegistros);
                await _context.SaveChangesAsync();
            }

            if (registrosActualizar.Count > 0)
            {
                foreach (var (id, desc, codImp, tarImp, cat) in registrosActualizar)
                {
                    await _context.CatalogosCAByS
                        .Where(c => c.Id == id)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(c => c.Descripcion, desc)
                            .SetProperty(c => c.CodigoImpuesto, codImp)
                            .SetProperty(c => c.TarifaImpuesto, tarImp)
                            .SetProperty(c => c.Categoria, cat)
                            .SetProperty(c => c.Activo, true));
                }
            }

            _logger.LogInformation(
                "Importación Excel CABYS completada: {Procesados} procesados, {Creados} creados, {Actualizados} actualizados, {Duplicados} duplicados en Excel, {Errores} errores",
                procesados, creados, actualizados, duplicadosEnExcel, errores.Count);

            return Ok(new
            {
                success = true,
                message = "Importación completada",
                resultados = new
                {
                    procesados,
                    creados,
                    actualizados,
                    duplicadosEnExcel,
                    errores = errores.Count,
                    detalleErrores = errores.Take(100).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar archivo Excel CABYS");
            return StatusCode(500, new { message = $"Error al procesar el archivo: {ex.Message}" });
        }
    }

    /// <summary>
    /// Exporta un template de Excel para importación de códigos CABYS
    /// GET: api/Cabys/local/exportar-template
    /// </summary>
    [HttpGet("local/exportar-template")]
    public IActionResult ExportarTemplate()
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Códigos CABYS");

            // Encabezados
            worksheet.Cells[1, 1].Value = "Codigo";
            worksheet.Cells[1, 2].Value = "Descripcion";
            worksheet.Cells[1, 3].Value = "Impuesto";
            worksheet.Cells[1, 4].Value = "Categoria";

            // Dar formato a los encabezados
            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Agregar algunos ejemplos
            worksheet.Cells[2, 1].Value = "8523490100000";
            worksheet.Cells[2, 2].Value = "Software de contabilidad y facturación";
            worksheet.Cells[2, 3].Value = 13.00;
            worksheet.Cells[2, 4].Value = "Servicio";

            worksheet.Cells[3, 1].Value = "8523490200000";
            worksheet.Cells[3, 2].Value = "Servicios de consultoría informática";
            worksheet.Cells[3, 3].Value = 13.00;
            worksheet.Cells[3, 4].Value = "Servicio";

            // Ajustar columnas
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 80;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 20;

            var fileBytes = package.GetAsByteArray();

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Template_Codigos_CABYS.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar template CABYS");
            return StatusCode(500, new { message = "Error al generar el template" });
        }
    }
}
