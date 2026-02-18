using Facturacion.Backend.Data;
using Facturacion.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Controllers;

/// <summary>
/// Controller para gestionar la recepción de documentos electrónicos de proveedores
/// Permite recibir, validar y consultar documentos recibidos (compras)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DocumentosRecibidosController : ControllerBase
{
    private readonly IDocumentoRecepcionService _recepcionService;
    private readonly IEmailReaderService _emailReaderService;
    private readonly ILogger<DocumentosRecibidosController> _logger;
    private readonly DataContext _context;

    public DocumentosRecibidosController(
        IDocumentoRecepcionService recepcionService,
        IEmailReaderService emailReaderService,
        ILogger<DocumentosRecibidosController> logger,
        DataContext context)
    {
        _recepcionService = recepcionService;
        _emailReaderService = emailReaderService;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Recibe un documento electrónico XML de un proveedor
    /// POST /api/documentosrecibidos/recibir
    /// </summary>
    [HttpPost("recibir")]
    public async Task<IActionResult> RecibirDocumento([FromBody] RecibirDocumentoRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.XmlContent))
            {
                return BadRequest(new { mensaje = "El contenido XML es requerido" });
            }

            var resultado = await _recepcionService.RecibirDocumentoAsync(
                request.XmlContent,
                request.EmpresaId);

            if (resultado.Exitoso)
            {
                _logger.LogInformation(
                    "Documento recibido exitosamente. Clave: {Clave}, Emisor: {Emisor}",
                    resultado.Clave,
                    resultado.EmisorNombre);

                return Ok(resultado);
            }
            else
            {
                _logger.LogWarning(
                    "Error al recibir documento: {Mensaje}. Errores: {Errores}",
                    resultado.Mensaje,
                    string.Join(", ", resultado.Errores));

                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recibir documento");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todos los documentos recibidos de una empresa
    /// GET /api/documentosrecibidos/empresa/{empresaId}
    /// </summary>
    [HttpGet("empresa/{empresaId}")]
    public async Task<IActionResult> GetDocumentosRecibidos(Guid empresaId)
    {
        try
        {
            var documentos = await _recepcionService.ObtenerDocumentosRecibidosAsync(empresaId);

            var documentosDto = documentos.Select(d => new
            {
                d.Id,
                d.Clave,
                d.NumeroConsecutivo,
                d.TipoDocumento,
                TipoDocumentoNombre = d.TipoDocumento.ToString(),
                d.FechaEmision,
                EmisorNombre = d.Proveedor?.Nombre,
                EmisorNumeroIdentificacion = d.Proveedor?.NumeroIdentificacion,
                d.TotalVenta,
                d.Moneda,
                d.Estado,
                d.EsDocumentoRecibido,
                d.FechaCreacion
            }).ToList();

            _logger.LogInformation(
                "Obtenidos {Count} documentos recibidos para empresa {EmpresaId}",
                documentosDto.Count,
                empresaId);

            return Ok(documentosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos recibidos");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un documento recibido por su ID
    /// GET /api/documentosrecibidos/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocumentoRecibido(Guid id)
    {
        try
        {
            var documento = await _context.Documentos
                .Include(d => d.Proveedor)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.EsDocumentoRecibido && !d.IsDeleted);

            if (documento == null)
            {
                return NotFound(new { mensaje = "Documento no encontrado" });
            }

            return Ok(new
            {
                documento.Id,
                documento.Clave,
                documento.NumeroConsecutivo,
                documento.TipoDocumento,
                TipoDocumentoNombre = documento.TipoDocumento.ToString(),
                documento.FechaEmision,
                Emisor = new
                {
                    Nombre = documento.Proveedor?.Nombre,
                    NumeroIdentificacion = documento.Proveedor?.NumeroIdentificacion,
                    NombreComercial = documento.Proveedor?.NombreComercial
                },
                Receptor = new
                {
                    documento.ReceptorNombre,
                    documento.ReceptorNumeroIdentificacion,
                    documento.ReceptorActividadEconomica
                },
                Financiero = new
                {
                    documento.Moneda,
                    documento.TipoCambio,
                    documento.TotalGravado,
                    documento.TotalExento,
                    documento.TotalExonerado,
                    documento.Subtotal,
                    documento.TotalDescuentos,
                    documento.TotalImpuestos,
                    documento.TotalVenta
                },
                documento.CondicionVenta,
                documento.PlazoCreditoDias,
                documento.MedioPago,
                documento.Observaciones,
                documento.Estado,
                documento.EsDocumentoRecibido,
                documento.XmlGenerado,
                documento.FechaCreacion
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documento recibido");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Verifica y valida un XML antes de recibirlo (pre-validación)
    /// POST /api/documentosrecibidos/verificar
    /// </summary>
    [HttpPost("verificar")]
    public async Task<IActionResult> VerificarXml([FromBody] VerificarXmlRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.XmlContent))
            {
                return BadRequest(new { mensaje = "El contenido XML es requerido" });
            }

            var resultado = await _recepcionService.ValidarXmlAsync(request.XmlContent);

            if (resultado.Exitoso)
            {
                _logger.LogInformation(
                    "XML validado exitosamente. Clave: {Clave}, Emisor: {Emisor}",
                    resultado.Clave,
                    resultado.EmisorNombre);

                return Ok(resultado);
            }
            else
            {
                _logger.LogWarning(
                    "XML inválido: {Mensaje}. Errores: {Errores}",
                    resultado.Mensaje,
                    string.Join(", ", resultado.Errores));

                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar XML");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Busca un documento por su clave
    /// GET /api/documentosrecibidos/buscar/{clave}
    /// </summary>
    [HttpGet("buscar/{clave}")]
    public async Task<IActionResult> BuscarPorClave(string clave)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(clave) || clave.Length != 50)
            {
                return BadRequest(new { mensaje = "La clave debe tener exactamente 50 dígitos" });
            }

            var documento = await _recepcionService.BuscarDocumentoPorClaveAsync(clave);

            if (documento == null)
            {
                return NotFound(new { mensaje = "Documento no encontrado con la clave especificada" });
            }

            return Ok(new
            {
                documento.Id,
                documento.Clave,
                documento.NumeroConsecutivo,
                documento.TipoDocumento,
                documento.FechaEmision,
                EmisorNombre = documento.Proveedor?.Nombre,
                documento.TotalVenta,
                documento.Estado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar documento por clave");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Lee correos no leídos vía IMAP y procesa XMLs adjuntos
    /// POST /api/documentosrecibidos/leer-correos/{empresaId}
    /// </summary>
    [HttpPost("leer-correos/{empresaId}")]
    public async Task<IActionResult> LeerCorreos(Guid empresaId)
    {
        try
        {
            _logger.LogInformation("Iniciando lectura de correos IMAP para empresa {EmpresaId}", empresaId);

            var resultado = await _emailReaderService.LeerCorreosAsync(empresaId);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            else
            {
                return BadRequest(resultado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer correos para empresa {EmpresaId}", empresaId);
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }
}

/// <summary>
/// Request para recibir un documento
/// </summary>
public class RecibirDocumentoRequest
{
    public string XmlContent { get; set; } = null!;
    public Guid EmpresaId { get; set; }
}

/// <summary>
/// Request para verificar un XML
/// </summary>
public class VerificarXmlRequest
{
    public string XmlContent { get; set; } = null!;
}
