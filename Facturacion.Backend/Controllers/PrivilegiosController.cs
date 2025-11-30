using Facturacion.Backend.Data;
using Facturacion.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser,Administrador de Empresa")]
public class PrivilegiosController : ControllerBase
{
    private readonly DataContext _context;
    private readonly ILogger<PrivilegiosController> _logger;

    public PrivilegiosController(DataContext context, ILogger<PrivilegiosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los privilegios agrupados por módulo
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var modulosConPrivilegios = await _context.Modulos
                .Where(m => m.Activo)
                .OrderBy(m => m.Orden)
                .Select(m => new ModuloPrivilegiosDto
                {
                    ModuloId = m.Id,
                    NombreModulo = m.Nombre,
                    Orden = m.Orden,
                    Icono = m.Icono,
                    Privilegios = m.Privilegios!
                        .Select(p => new PrivilegioDto
                        {
                            Id = p.Id,
                            ModuloId = p.ModuloId,
                            NombreModulo = m.Nombre,
                            Accion = p.Accion,
                            Nombre = p.Nombre,
                            Descripcion = p.Descripcion
                        })
                        .OrderBy(p => p.Accion)
                        .ToList()
                })
                .ToListAsync();

            return Ok(modulosConPrivilegios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los privilegios");
            return StatusCode(500, "Error interno del servidor al obtener los privilegios.");
        }
    }

    /// <summary>
    /// Obtiene un privilegio por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        try
        {
            var privilegio = await _context.Privilegios
                .Where(p => p.Id == id)
                .Include(p => p.Modulo)
                .Select(p => new PrivilegioDto
                {
                    Id = p.Id,
                    ModuloId = p.ModuloId,
                    NombreModulo = p.Modulo!.Nombre,
                    Accion = p.Accion,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion
                })
                .FirstOrDefaultAsync();

            if (privilegio == null)
            {
                return NotFound("Privilegio no encontrado.");
            }

            return Ok(privilegio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el privilegio {PrivilegioId}", id);
            return StatusCode(500, "Error interno del servidor al obtener el privilegio.");
        }
    }
}
