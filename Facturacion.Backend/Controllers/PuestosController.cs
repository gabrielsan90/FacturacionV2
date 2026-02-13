using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
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
public class PuestosController : ControllerBase
{
    private readonly IPuestoRepository _puestoRepository;
    private readonly DataContext _context;
    private readonly ILogger<PuestosController> _logger;

    public PuestosController(
        IPuestoRepository puestoRepository,
        DataContext context,
        ILogger<PuestosController> logger)
    {
        _puestoRepository = puestoRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los puestos de una empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}")]
    public async Task<IActionResult> GetByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var puestos = await _puestoRepository.GetByEmpresaAsync(empresaId);
            return Ok(puestos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo puestos de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los puestos.");
        }
    }

    /// <summary>
    /// Obtiene solo los puestos activos de una empresa.
    /// </summary>
    [HttpGet("empresa/{empresaId:guid}/activos")]
    public async Task<IActionResult> GetActivosByEmpresaAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var puestos = await _puestoRepository.GetActivosAsync(empresaId);
            return Ok(puestos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo puestos activos de empresa {EmpresaId}", empresaId);
            return BadRequest("Error al obtener los puestos activos.");
        }
    }

    /// <summary>
    /// Obtiene un puesto específico.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var puesto = await _puestoRepository.GetAsync(id);

            if (puesto == null)
            {
                return NotFound("Puesto no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(puesto.EmpresaId))
            {
                return Forbid();
            }

            return Ok(puesto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo puesto {Id}", id);
            return BadRequest("Error al obtener el puesto.");
        }
    }

    /// <summary>
    /// Crea un nuevo puesto.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Puesto puesto)
    {
        try
        {
            // Validar solo campos requeridos del negocio
            if (puesto.EmpresaId == Guid.Empty)
            {
                return BadRequest("La empresa es obligatoria.");
            }
            if (string.IsNullOrWhiteSpace(puesto.Codigo))
            {
                return BadRequest("El código es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(puesto.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }

            if (!await TieneAccesoEmpresaAsync(puesto.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único por empresa
            var existenteCodigo = await _puestoRepository.GetByCodigoAsync(puesto.EmpresaId, puesto.Codigo);
            if (existenteCodigo != null)
            {
                return BadRequest("Ya existe un puesto con este código en la empresa.");
            }

            // Validar departamento si se proporciona
            if (puesto.DepartamentoId.HasValue)
            {
                var departamento = await _context.Departamentos
                    .FirstOrDefaultAsync(d => d.Id == puesto.DepartamentoId.Value &&
                                             d.EmpresaId == puesto.EmpresaId &&
                                             !d.IsDeleted);
                if (departamento == null)
                {
                    return BadRequest("El departamento especificado no existe o no pertenece a la empresa.");
                }
            }

            puesto.Id = Guid.NewGuid();
            puesto.UsuarioCreacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var nuevoPuesto = await _puestoRepository.AddAsync(puesto);
            return Ok(nuevoPuesto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando puesto {Codigo}", puesto.Codigo);
            return BadRequest("Error al crear el puesto.");
        }
    }

    /// <summary>
    /// Actualiza un puesto existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] Puesto puesto)
    {
        try
        {
            if (id != puesto.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            // Validar solo campos requeridos del negocio
            if (string.IsNullOrWhiteSpace(puesto.Codigo))
            {
                return BadRequest("El código es obligatorio.");
            }
            if (string.IsNullOrWhiteSpace(puesto.Nombre))
            {
                return BadRequest("El nombre es obligatorio.");
            }

            var existente = await _puestoRepository.GetAsync(id);
            if (existente == null)
            {
                return NotFound("Puesto no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(existente.EmpresaId))
            {
                return Forbid();
            }

            // Verificar código único (excluyendo el puesto actual)
            var duplicadoCodigo = await _puestoRepository.GetByCodigoAsync(puesto.EmpresaId, puesto.Codigo);
            if (duplicadoCodigo != null && duplicadoCodigo.Id != id)
            {
                return BadRequest("Ya existe otro puesto con este código en la empresa.");
            }

            // Validar departamento si se proporciona
            if (puesto.DepartamentoId.HasValue)
            {
                var departamento = await _context.Departamentos
                    .FirstOrDefaultAsync(d => d.Id == puesto.DepartamentoId.Value &&
                                             d.EmpresaId == puesto.EmpresaId &&
                                             !d.IsDeleted);
                if (departamento == null)
                {
                    return BadRequest("El departamento especificado no existe o no pertenece a la empresa.");
                }
            }

            // Preservar campos de auditoría
            puesto.FechaCreacion = existente.FechaCreacion;
            puesto.UsuarioCreacionId = existente.UsuarioCreacionId;
            puesto.UsuarioModificacionId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _puestoRepository.UpdateAsync(puesto);
            return Ok(puesto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando puesto {Id}", id);
            return BadRequest("Error al actualizar el puesto.");
        }
    }

    /// <summary>
    /// Elimina un puesto (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var puesto = await _context.Puestos
                .Include(p => p.Empleados)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (puesto == null)
            {
                return NotFound("Puesto no encontrado.");
            }

            if (!await TieneAccesoEmpresaAsync(puesto.EmpresaId))
            {
                return Forbid();
            }

            // No permitir eliminar si tiene empleados asignados
            if (puesto.Empleados?.Any(e => !e.IsDeleted) == true)
            {
                return BadRequest("No se puede eliminar un puesto que tiene empleados asignados. Reasigne los empleados primero.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _puestoRepository.DeleteAsync(id, userId!);

            return Ok(new { Message = "Puesto eliminado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando puesto {Id}", id);
            return BadRequest("Error al eliminar el puesto.");
        }
    }

    /// <summary>
    /// Genera un catálogo predefinido de puestos para la empresa.
    /// </summary>
    [HttpPost("empresa/{empresaId:guid}/generar-catalogo")]
    public async Task<IActionResult> GenerarCatalogoAsync(Guid empresaId)
    {
        try
        {
            if (!await TieneAccesoEmpresaAsync(empresaId))
            {
                return Forbid();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar si ya existen puestos
            var existentes = await _context.Puestos
                .Where(p => p.EmpresaId == empresaId && !p.IsDeleted)
                .CountAsync();

            if (existentes > 0)
            {
                return BadRequest("Ya existen puestos en esta empresa. El catálogo predefinido solo se puede generar cuando no hay puestos.");
            }

            // Catálogo predefinido de puestos con niveles jerárquicos y salarios base
            var puestosPredefinidos = new List<(string Codigo, string Nombre, string Descripcion, int Nivel, decimal SalMin, decimal SalMax, bool Confianza, bool PersonalCargo)>
            {
                // Nivel 1 - Ejecutivo
                ("GG001", "Gerente General", "Dirección general de la empresa", 1, 2500000, 5000000, true, true),

                // Nivel 2 - Gerencial
                ("GER001", "Gerente Administrativo", "Gestión de operaciones administrativas", 2, 1800000, 3500000, true, true),
                ("GER002", "Gerente Financiero", "Dirección financiera y contable", 2, 1800000, 3500000, true, true),
                ("GER003", "Gerente de Ventas", "Dirección comercial y de ventas", 2, 1800000, 3500000, true, true),
                ("GER004", "Gerente de Operaciones", "Dirección de operaciones y producción", 2, 1800000, 3500000, true, true),
                ("GER005", "Gerente de Recursos Humanos", "Dirección de gestión humana", 2, 1800000, 3500000, true, true),
                ("GER006", "Gerente de TI", "Dirección de tecnología e información", 2, 1800000, 3500000, true, true),

                // Nivel 3 - Supervisión
                ("SUP001", "Supervisor de Ventas", "Supervisión del equipo de ventas", 3, 900000, 1500000, false, true),
                ("SUP002", "Supervisor de Producción", "Supervisión de líneas de producción", 3, 900000, 1500000, false, true),
                ("SUP003", "Supervisor de Bodega", "Supervisión de almacén e inventarios", 3, 800000, 1400000, false, true),
                ("SUP004", "Jefe de Contabilidad", "Supervisión del área contable", 3, 1000000, 1800000, false, true),
                ("SUP005", "Coordinador de RRHH", "Coordinación de procesos de recursos humanos", 3, 900000, 1500000, false, true),

                // Nivel 4 - Operativo
                ("OPE001", "Contador", "Registro y análisis contable", 4, 700000, 1200000, false, false),
                ("OPE002", "Asistente Contable", "Apoyo en labores contables", 4, 500000, 800000, false, false),
                ("OPE003", "Vendedor", "Atención y ventas a clientes", 4, 500000, 900000, false, false),
                ("OPE004", "Ejecutivo de Ventas", "Gestión de cartera de clientes", 4, 600000, 1100000, false, false),
                ("OPE005", "Recepcionista", "Atención telefónica y recepción", 4, 450000, 650000, false, false),
                ("OPE006", "Asistente Administrativo", "Apoyo administrativo general", 4, 500000, 800000, false, false),
                ("OPE007", "Secretaria Ejecutiva", "Asistencia a gerencia", 4, 550000, 900000, false, false),
                ("OPE008", "Bodeguero", "Manejo de inventarios y despachos", 4, 450000, 700000, false, false),
                ("OPE009", "Operario de Producción", "Labores operativas de producción", 4, 430000, 650000, false, false),
                ("OPE010", "Técnico de Mantenimiento", "Mantenimiento de equipos", 4, 500000, 850000, false, false),
                ("OPE011", "Mensajero", "Mensajería y diligencias", 4, 420000, 550000, false, false),
                ("OPE012", "Conserje", "Limpieza y mantenimiento de instalaciones", 4, 400000, 500000, false, false),
                ("OPE013", "Vigilante", "Seguridad de instalaciones", 4, 430000, 600000, false, false),
                ("OPE014", "Desarrollador de Software", "Desarrollo y mantenimiento de sistemas", 4, 800000, 1800000, false, false),
                ("OPE015", "Soporte Técnico", "Asistencia técnica a usuarios", 4, 550000, 900000, false, false),
                ("OPE016", "Diseñador Gráfico", "Diseño de materiales visuales", 4, 600000, 1000000, false, false),
                ("OPE017", "Community Manager", "Gestión de redes sociales", 4, 550000, 950000, false, false),
                ("OPE018", "Analista de Compras", "Gestión de adquisiciones", 4, 600000, 1000000, false, false),
                ("OPE019", "Cobrador", "Gestión de cobros y recuperación", 4, 500000, 800000, false, false),
                ("OPE020", "Cajero", "Manejo de caja y transacciones", 4, 450000, 700000, false, false)
            };

            var puestosCreados = new List<Puesto>();

            foreach (var (codigo, nombre, descripcion, nivel, salMin, salMax, confianza, personalCargo) in puestosPredefinidos)
            {
                var puesto = new Puesto
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaId,
                    Codigo = codigo,
                    Nombre = nombre,
                    Descripcion = descripcion,
                    NivelJerarquico = nivel,
                    SalarioMinimo = salMin,
                    SalarioMaximo = salMax,
                    EsDeConfianza = confianza,
                    TienePersonalACargo = personalCargo,
                    Activo = true,
                    UsuarioCreacionId = userId
                };
                puestosCreados.Add(puesto);
            }

            // Guardar todos los puestos
            foreach (var puesto in puestosCreados)
            {
                await _puestoRepository.AddAsync(puesto);
            }

            _logger.LogInformation("Catálogo de {Count} puestos generado para empresa {EmpresaId}",
                puestosCreados.Count, empresaId);

            return Ok(new
            {
                success = true,
                message = $"Se generaron {puestosCreados.Count} puestos exitosamente.",
                count = puestosCreados.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando catálogo de puestos para empresa {EmpresaId}", empresaId);
            return BadRequest("Error al generar el catálogo de puestos.");
        }
    }

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
}
