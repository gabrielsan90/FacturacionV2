using Facturacion.Backend.Data;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controlador para la gestión del catálogo de bancos.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class BancosController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<BancosController> _logger;

    public BancosController(DataContext context, ILogger<BancosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los bancos activos de una empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var bancos = await _context.Bancos
                .Where(b => !b.IsDeleted && b.Activo && b.EmpresaId == empresaId)
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            return Ok(bancos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo bancos de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los bancos.");
        }
    }

    /// <summary>
    /// Obtiene todos los bancos (incluyendo inactivos) de una empresa para gestión.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/todos")]
    public async Task<IActionResult> GetTodosByEmpresaAsync(Guid empresaId)
    {
        try
        {
            var bancos = await _context.Bancos
                .Where(b => !b.IsDeleted && b.EmpresaId == empresaId)
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            return Ok(bancos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todos los bancos de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los bancos.");
        }
    }

    /// <summary>
    /// Obtiene un banco por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var banco = await _context.Bancos
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (banco == null)
            {
                return NotFound("Banco no encontrado.");
            }

            return Ok(banco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo banco {Id}", id);
            return BadRequest("Error al obtener el banco.");
        }
    }

    /// <summary>
    /// Crea un nuevo banco.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Banco banco)
    {
        try
        {
            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(banco.Codigo))
            {
                return BadRequest("El código es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(banco.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }

            // Asignar empresa del admin si no viene
            if (banco.EmpresaId == Guid.Empty)
            {
                var empresaClaim = User.FindFirst("EmpresaId")?.Value;
                if (!string.IsNullOrEmpty(empresaClaim) && Guid.TryParse(empresaClaim, out var parsedEmpresaId))
                {
                    banco.EmpresaId = parsedEmpresaId;
                }
                else
                {
                    return BadRequest("No se pudo determinar la empresa.");
                }
            }

            // Verificar código único dentro de la empresa
            var existenteCodigo = await _context.Bancos
                .FirstOrDefaultAsync(b => b.Codigo == banco.Codigo && b.EmpresaId == banco.EmpresaId && !b.IsDeleted);

            if (existenteCodigo != null)
            {
                return BadRequest("Ya existe un banco con este código en esta empresa.");
            }

            banco.Id = Guid.NewGuid();
            banco.FechaCreacion = DateTime.UtcNow;
            banco.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Bancos.Add(banco);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Banco creado: {Codigo} - {Nombre}", banco.Codigo, banco.Nombre);

            return Ok(banco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando banco {Codigo}", banco.Codigo);
            return BadRequest("Error al crear el banco.");
        }
    }

    /// <summary>
    /// Actualiza un banco existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Banco banco)
    {
        try
        {
            if (id != banco.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(banco.Codigo))
            {
                return BadRequest("El código es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(banco.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }

            var existente = await _context.Bancos
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (existente == null)
            {
                return NotFound("Banco no encontrado.");
            }

            // Verificar código único dentro de la empresa (excluyendo el actual)
            var duplicadoCodigo = await _context.Bancos
                .FirstOrDefaultAsync(b => b.Codigo == banco.Codigo && b.EmpresaId == existente.EmpresaId && b.Id != id && !b.IsDeleted);

            if (duplicadoCodigo != null)
            {
                return BadRequest("Ya existe otro banco con este código.");
            }

            // Actualizar propiedades
            existente.Codigo = banco.Codigo;
            existente.Nombre = banco.Nombre;
            existente.NombreCorto = banco.NombreCorto;
            existente.CodigoSINPE = banco.CodigoSINPE;
            existente.CodigoSWIFT = banco.CodigoSWIFT;
            existente.EsNacional = banco.EsNacional;
            existente.EsEstatal = banco.EsEstatal;
            existente.Pais = banco.Pais;
            existente.SitioWeb = banco.SitioWeb;
            existente.Telefono = banco.Telefono;
            existente.Activo = banco.Activo;
            existente.FechaModificacion = DateTime.UtcNow;
            existente.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Banco actualizado: {Id}", id);

            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando banco {Id}", id);
            return BadRequest("Error al actualizar el banco.");
        }
    }

    /// <summary>
    /// Elimina un banco (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var banco = await _context.Bancos
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (banco == null)
            {
                return NotFound("Banco no encontrado.");
            }

            // Verificar si tiene cuentas bancarias asociadas
            var tieneCuentas = await _context.CuentasBancarias
                .AnyAsync(c => c.BancoId == id && !c.IsDeleted);

            if (tieneCuentas)
            {
                return BadRequest("No se puede eliminar un banco que tiene cuentas bancarias asociadas.");
            }

            banco.IsDeleted = true;
            banco.FechaEliminacion = DateTime.UtcNow;
            banco.UsuarioEliminacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Banco eliminado: {Id}", id);

            return Ok(new { Message = "Banco eliminado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando banco {Id}", id);
            return BadRequest("Error al eliminar el banco.");
        }
    }

    /// <summary>
    /// Genera un catálogo predefinido de bancos de Costa Rica para una empresa.
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/generar-catalogo")]
    public async Task<IActionResult> GenerarCatalogoAsync(Guid empresaId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar si ya existen bancos para esta empresa
            var existentes = await _context.Bancos
                .Where(b => !b.IsDeleted && b.EmpresaId == empresaId)
                .CountAsync();

            if (existentes > 0)
            {
                return BadRequest("Ya existen bancos en el catálogo de esta empresa. El catálogo predefinido solo se puede generar cuando está vacío.");
            }

            // Catálogo de bancos de Costa Rica
            var bancosPredefinidos = new List<(string Codigo, string Nombre, string NombreCorto, string CodigoSINPE, bool EsEstatal)>
            {
                // Bancos Estatales
                ("BCR", "Banco de Costa Rica", "BCR", "102", true),
                ("BNCR", "Banco Nacional de Costa Rica", "Banco Nacional", "101", true),
                ("BPDC", "Banco Popular y de Desarrollo Comunal", "Banco Popular", "117", true),
                ("BCCR", "Banco Central de Costa Rica", "BCCR", "100", true),

                // Bancos Privados Nacionales
                ("BAC", "BAC San José", "BAC", "107", false),
                ("DAVIVIENDA", "Banco Davivienda Costa Rica", "Davivienda", "114", false),
                ("LAFISE", "Banco Lafise", "Lafise", "111", false),
                ("PROMERICA", "Banco Promérica de Costa Rica", "Promérica", "121", false),
                ("IMPROSA", "Banco Improsa", "Improsa", "115", false),
                ("BCT", "Banco BCT", "BCT", "106", false),
                ("GENERAL", "Banco General Costa Rica", "Banco General", "135", false),
                ("CATHAY", "Banco Cathay de Costa Rica", "Cathay", "116", false),

                // Bancos Internacionales
                ("SCOTIA", "Scotiabank de Costa Rica", "Scotiabank", "105", false),
                ("CITI", "Citibank de Costa Rica", "Citibank", "108", false),

                // Otros
                ("COOCIQUE", "Coocique R.L.", "Coocique", "137", false),
                ("MUTUAL", "Mutual Alajuela", "Mutual", "803", false),
                ("COOPENAE", "Coopenae R.L.", "Coopenae", "802", false),
                ("CREDECOOP", "Credecoop R.L.", "Credecoop", "811", false)
            };

            var bancosCreados = new List<Banco>();

            foreach (var (codigo, nombre, nombreCorto, codigoSINPE, esEstatal) in bancosPredefinidos)
            {
                var banco = new Banco
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    Codigo = codigo,
                    Nombre = nombre,
                    NombreCorto = nombreCorto,
                    CodigoSINPE = codigoSINPE,
                    EsNacional = true,
                    EsEstatal = esEstatal,
                    Pais = "Costa Rica",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacionId = userId
                };
                bancosCreados.Add(banco);
            }

            _context.Bancos.AddRange(bancosCreados);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Catálogo de {Count} bancos generado", bancosCreados.Count);

            return Ok(new
            {
                success = true,
                message = $"Se generaron {bancosCreados.Count} bancos exitosamente.",
                count = bancosCreados.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando catálogo de bancos");
            return BadRequest("Error al generar el catálogo de bancos.");
        }
    }
}
