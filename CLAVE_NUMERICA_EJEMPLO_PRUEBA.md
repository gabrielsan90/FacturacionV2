# Ejemplos de Prueba: Generación de Clave Numérica

Este documento contiene ejemplos de código para probar la generación de claves numéricas de Hacienda en diferentes escenarios.

## Ejemplo 1: Prueba Básica con Documento de Factura

```csharp
using Facturacion.Backend.Services.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.Extensions.DependencyInjection;

// En un controlador o servicio con DI
public class PruebaClaveController : Controller
{
    private readonly IClaveGeneradorService _claveGenerador;
    private readonly DataContext _context;

    public PruebaClaveController(
        IClaveGeneradorService claveGenerador,
        DataContext context)
    {
        _claveGenerador = claveGenerador;
        _context = context;
    }

    [HttpGet("api/prueba/generar-clave")]
    public async Task<IActionResult> GenerarClaveEjemplo()
    {
        // Crear documento de ejemplo
        var documento = new Documento
        {
            Id = Guid.NewGuid(),
            FechaEmision = new DateTime(2025, 11, 29),
            NumeroConsecutivo = "001-00001-01-0000000001",
            EmpresaId = Guid.Parse("..."), // ID de empresa existente
            SucursalId = Guid.Parse("..."), // ID de sucursal existente
            TerminalId = Guid.Parse("...")  // ID de terminal existente
        };

        // Cargar relaciones necesarias
        documento.Empresa = await _context.Empresas
            .FirstOrDefaultAsync(e => e.Id == documento.EmpresaId);

        documento.Sucursal = await _context.Sucursales
            .FirstOrDefaultAsync(s => s.Id == documento.SucursalId);

        documento.Terminal = await _context.Terminales
            .FirstOrDefaultAsync(t => t.Id == documento.TerminalId);

        // Generar clave
        var clave = await _claveGenerador.GenerarClaveAsync(documento, situacion: 1);

        // Validar clave
        var esValida = _claveGenerador.ValidarClave(clave);

        return Ok(new
        {
            Clave = clave,
            Longitud = clave.Length,
            EsValida = esValida,
            Componentes = new
            {
                Pais = clave.Substring(0, 3),
                Dia = clave.Substring(3, 2),
                Mes = clave.Substring(5, 2),
                Anio = clave.Substring(7, 2),
                Cedula = clave.Substring(9, 12),
                Consecutivo = clave.Substring(21, 20),
                Situacion = clave.Substring(41, 1),
                CodigoSeguridad = clave.Substring(42, 8)
            }
        });
    }
}
```

## Ejemplo 2: Prueba con Diferentes Tipos de Cédula

```csharp
public async Task<List<string>> GenerarClavesConDiferentesCedulas()
{
    var claves = new List<string>();

    // Caso 1: Cédula Física (9 dígitos)
    var empresa1 = new Empresa
    {
        NumeroIdentificacion = "123456789" // Se convierte a 000123456789
    };

    // Caso 2: Cédula Jurídica (10 dígitos)
    var empresa2 = new Empresa
    {
        NumeroIdentificacion = "3101234567" // Se convierte a 003101234567
    };

    // Caso 3: DIMEX (11 dígitos)
    var empresa3 = new Empresa
    {
        NumeroIdentificacion = "12345678901" // Se convierte a 012345678901
    };

    // Caso 4: DIMEX (12 dígitos - ya tiene longitud correcta)
    var empresa4 = new Empresa
    {
        NumeroIdentificacion = "123456789012" // Se mantiene 123456789012
    };

    foreach (var empresa in new[] { empresa1, empresa2, empresa3, empresa4 })
    {
        var documento = new Documento
        {
            FechaEmision = DateTime.Now,
            NumeroConsecutivo = "001-00001-01-0000000001",
            Empresa = empresa
        };

        var clave = await _claveGenerador.GenerarClaveAsync(documento, 1);
        claves.Add(clave);

        Console.WriteLine($"Cédula original: {empresa.NumeroIdentificacion}");
        Console.WriteLine($"Cédula en clave: {clave.Substring(9, 12)}");
        Console.WriteLine($"Clave completa: {clave}");
        Console.WriteLine();
    }

    return claves;
}
```

## Ejemplo 3: Prueba de Situaciones (Normal, Contingencia, Sin Internet)

```csharp
public async Task<Dictionary<string, string>> GenerarClavesConDiferentesSituaciones()
{
    var claves = new Dictionary<string, string>();

    var documento = new Documento
    {
        FechaEmision = new DateTime(2025, 11, 29),
        NumeroConsecutivo = "001-00001-01-0000000001",
        Empresa = new Empresa { NumeroIdentificacion = "3101234567" }
    };

    // Situación 1: Normal
    var claveNormal = await _claveGenerador.GenerarClaveAsync(documento, situacion: 1);
    claves.Add("Normal", claveNormal);
    Console.WriteLine($"Clave Normal (situación=1): {claveNormal}");
    Console.WriteLine($"  Situación: {claveNormal.Substring(41, 1)}");

    // Situación 2: Contingencia
    var claveContingencia = await _claveGenerador.GenerarClaveAsync(documento, situacion: 2);
    claves.Add("Contingencia", claveContingencia);
    Console.WriteLine($"Clave Contingencia (situación=2): {claveContingencia}");
    Console.WriteLine($"  Situación: {claveContingencia.Substring(41, 1)}");

    // Situación 3: Sin Internet
    var claveSinInternet = await _claveGenerador.GenerarClaveAsync(documento, situacion: 3);
    claves.Add("SinInternet", claveSinInternet);
    Console.WriteLine($"Clave Sin Internet (situación=3): {claveSinInternet}");
    Console.WriteLine($"  Situación: {claveSinInternet.Substring(41, 1)}");

    return claves;
}
```

## Ejemplo 4: Prueba de Unicidad de Código de Seguridad

```csharp
public async Task VerificarUnicidadCodigoSeguridad()
{
    var documento = new Documento
    {
        FechaEmision = new DateTime(2025, 11, 29),
        NumeroConsecutivo = "001-00001-01-0000000001",
        Empresa = new Empresa { NumeroIdentificacion = "3101234567" }
    };

    var claves = new HashSet<string>();
    var codigosSeguridad = new HashSet<string>();

    // Generar 1000 claves para el mismo documento
    for (int i = 0; i < 1000; i++)
    {
        var clave = await _claveGenerador.GenerarClaveAsync(documento, 1);
        var codigoSeguridad = clave.Substring(42, 8);

        claves.Add(clave);
        codigosSeguridad.Add(codigoSeguridad);
    }

    Console.WriteLine($"Claves generadas: 1000");
    Console.WriteLine($"Claves únicas: {claves.Count}");
    Console.WriteLine($"Códigos de seguridad únicos: {codigosSeguridad.Count}");
    Console.WriteLine($"Tasa de unicidad: {(codigosSeguridad.Count / 1000.0) * 100:F2}%");

    // Mostrar algunos ejemplos de códigos de seguridad
    Console.WriteLine("\nEjemplos de códigos de seguridad:");
    foreach (var codigo in codigosSeguridad.Take(10))
    {
        Console.WriteLine($"  {codigo}");
    }
}
```

## Ejemplo 5: Prueba de Validación de Claves

```csharp
public void ProbarValidacionClaves()
{
    var pruebas = new Dictionary<string, (string clave, bool esperado, string razon)>
    {
        // Clave válida
        {
            "Válida",
            ("50629112500000310123456700100001010000000001187654321", true, "Formato correcto")
        },

        // Longitud incorrecta
        {
            "Muy corta",
            ("506291125000031012345670010000101000000000118765432", false, "Solo 49 dígitos")
        },
        {
            "Muy larga",
            ("5062911250000031012345670010000101000000000011876543211", false, "51 dígitos")
        },

        // País incorrecto
        {
            "País incorrecto",
            ("00629112500000310123456700100001010000000001187654321", false, "País no es 506")
        },

        // Día inválido
        {
            "Día 00",
            ("50600112500000310123456700100001010000000001187654321", false, "Día es 00")
        },
        {
            "Día 32",
            ("50632112500000310123456700100001010000000001187654321", false, "Día es 32")
        },

        // Mes inválido
        {
            "Mes 00",
            ("50629002500000310123456700100001010000000001187654321", false, "Mes es 00")
        },
        {
            "Mes 13",
            ("50629132500000310123456700100001010000000001187654321", false, "Mes es 13")
        },

        // Situación inválida
        {
            "Situación 0",
            ("50629112500000310123456700100001010000000000087654321", false, "Situación es 0")
        },
        {
            "Situación 4",
            ("50629112500000310123456700100001010000000004487654321", false, "Situación es 4")
        },

        // Caracteres no numéricos
        {
            "Con letras",
            ("5062911250000031012345670010000101000000000A87654321", false, "Contiene letra A")
        },
        {
            "Con guiones",
            ("506-29-11-25-000003101234567-00100001010000000001-1-87654321", false, "Contiene guiones")
        }
    };

    Console.WriteLine("PRUEBAS DE VALIDACIÓN DE CLAVES\n");
    Console.WriteLine($"{"Prueba",-20} {"Resultado",-10} {"Esperado",-10} {"Estado",-10} {"Razón"}");
    Console.WriteLine(new string('-', 100));

    foreach (var (nombre, (clave, esperado, razon)) in pruebas)
    {
        var resultado = _claveGenerador.ValidarClave(clave);
        var estado = resultado == esperado ? "✓ PASS" : "✗ FAIL";

        Console.WriteLine($"{nombre,-20} {resultado,-10} {esperado,-10} {estado,-10} {razon}");
    }
}
```

## Ejemplo 6: Prueba Completa de Generación para un Documento Real

```csharp
public async Task<ResultadoGeneracionClave> GenerarClaveDocumentoCompleto(Guid documentoId)
{
    var documento = await _context.Documentos
        .Include(d => d.Empresa)
        .Include(d => d.Sucursal)
        .Include(d => d.Terminal)
        .FirstOrDefaultAsync(d => d.Id == documentoId);

    if (documento == null)
    {
        throw new Exception($"Documento {documentoId} no encontrado");
    }

    var resultado = new ResultadoGeneracionClave
    {
        DocumentoId = documentoId,
        FechaGeneracion = DateTime.Now
    };

    try
    {
        // Generar clave
        var inicio = DateTime.Now;
        var clave = await _claveGenerador.GenerarClaveAsync(documento, situacion: 1);
        var duracion = DateTime.Now - inicio;

        // Validar clave
        var esValida = _claveGenerador.ValidarClave(clave);

        // Llenar resultado
        resultado.Clave = clave;
        resultado.EsValida = esValida;
        resultado.TiempoGeneracion = duracion;
        resultado.Exitoso = true;

        // Desglosar componentes
        resultado.Componentes = new ComponentesClave
        {
            Pais = clave.Substring(0, 3),
            Dia = clave.Substring(3, 2),
            Mes = clave.Substring(5, 2),
            Anio = clave.Substring(7, 2),
            Cedula = clave.Substring(9, 12),
            Consecutivo = clave.Substring(21, 20),
            Situacion = clave.Substring(41, 1),
            CodigoSeguridad = clave.Substring(42, 8)
        };

        // Información del documento
        resultado.InfoDocumento = new InfoDocumento
        {
            FechaEmision = documento.FechaEmision,
            NumeroConsecutivo = documento.NumeroConsecutivo,
            CedulaEmpresa = documento.Empresa?.NumeroIdentificacion,
            NombreEmpresa = documento.Empresa?.NombreComercial
        };

    }
    catch (Exception ex)
    {
        resultado.Exitoso = false;
        resultado.Error = ex.Message;
    }

    return resultado;
}

// DTOs para el resultado
public class ResultadoGeneracionClave
{
    public Guid DocumentoId { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public string? Clave { get; set; }
    public bool EsValida { get; set; }
    public bool Exitoso { get; set; }
    public string? Error { get; set; }
    public TimeSpan TiempoGeneracion { get; set; }
    public ComponentesClave? Componentes { get; set; }
    public InfoDocumento? InfoDocumento { get; set; }
}

public class ComponentesClave
{
    public string Pais { get; set; } = string.Empty;
    public string Dia { get; set; } = string.Empty;
    public string Mes { get; set; } = string.Empty;
    public string Anio { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Consecutivo { get; set; } = string.Empty;
    public string Situacion { get; set; } = string.Empty;
    public string CodigoSeguridad { get; set; } = string.Empty;
}

public class InfoDocumento
{
    public DateTime FechaEmision { get; set; }
    public string? NumeroConsecutivo { get; set; }
    public string? CedulaEmpresa { get; set; }
    public string? NombreEmpresa { get; set; }
}
```

## Ejemplo 7: Script SQL para Validar Claves en Base de Datos

```sql
-- Verificar claves generadas en documentos
SELECT
    Id,
    NumeroConsecutivo,
    Clave,
    LEN(Clave) AS LongitudClave,
    LEFT(Clave, 3) AS Pais,
    SUBSTRING(Clave, 4, 2) AS Dia,
    SUBSTRING(Clave, 6, 2) AS Mes,
    SUBSTRING(Clave, 8, 2) AS Anio,
    SUBSTRING(Clave, 10, 12) AS Cedula,
    SUBSTRING(Clave, 22, 20) AS Consecutivo,
    SUBSTRING(Clave, 42, 1) AS Situacion,
    SUBSTRING(Clave, 43, 8) AS CodigoSeguridad,
    CASE
        WHEN LEN(Clave) = 50 THEN 'OK'
        ELSE 'ERROR: Longitud incorrecta'
    END AS ValidacionLongitud,
    CASE
        WHEN LEFT(Clave, 3) = '506' THEN 'OK'
        ELSE 'ERROR: País incorrecto'
    END AS ValidacionPais
FROM Documentos
WHERE Clave IS NOT NULL
ORDER BY FechaCreacion DESC;

-- Verificar unicidad de claves
SELECT
    Clave,
    COUNT(*) AS Cantidad
FROM Documentos
WHERE Clave IS NOT NULL
GROUP BY Clave
HAVING COUNT(*) > 1;

-- Verificar códigos de seguridad duplicados
SELECT
    SUBSTRING(Clave, 43, 8) AS CodigoSeguridad,
    COUNT(*) AS Cantidad
FROM Documentos
WHERE Clave IS NOT NULL
GROUP BY SUBSTRING(Clave, 43, 8)
HAVING COUNT(*) > 1;
```

## Ejemplo 8: Prueba de Rendimiento

```csharp
public async Task<ResultadoPrueba> ProbarRendimientoGeneracion()
{
    var resultado = new ResultadoPrueba();
    var tiempos = new List<TimeSpan>();

    var documento = new Documento
    {
        FechaEmision = DateTime.Now,
        NumeroConsecutivo = "001-00001-01-0000000001",
        Empresa = new Empresa { NumeroIdentificacion = "3101234567" }
    };

    // Generar 1000 claves y medir tiempo
    for (int i = 0; i < 1000; i++)
    {
        var inicio = DateTime.Now;
        var clave = await _claveGenerador.GenerarClaveAsync(documento, 1);
        var duracion = DateTime.Now - inicio;
        tiempos.Add(duracion);
    }

    resultado.TotalGeneradas = 1000;
    resultado.TiempoTotal = tiempos.Sum(t => t.TotalMilliseconds);
    resultado.TiempoPromedio = tiempos.Average(t => t.TotalMilliseconds);
    resultado.TiempoMinimo = tiempos.Min(t => t.TotalMilliseconds);
    resultado.TiempoMaximo = tiempos.Max(t => t.TotalMilliseconds);

    Console.WriteLine($"Claves generadas: {resultado.TotalGeneradas}");
    Console.WriteLine($"Tiempo total: {resultado.TiempoTotal:F2} ms");
    Console.WriteLine($"Tiempo promedio: {resultado.TiempoPromedio:F4} ms");
    Console.WriteLine($"Tiempo mínimo: {resultado.TiempoMinimo:F4} ms");
    Console.WriteLine($"Tiempo máximo: {resultado.TiempoMaximo:F4} ms");

    return resultado;
}

public class ResultadoPrueba
{
    public int TotalGeneradas { get; set; }
    public double TiempoTotal { get; set; }
    public double TiempoPromedio { get; set; }
    public double TiempoMinimo { get; set; }
    public double TiempoMaximo { get; set; }
}
```

## Cómo Ejecutar las Pruebas

### Opción 1: Crear un Endpoint Temporal

```csharp
[ApiController]
[Route("api/[controller]")]
public class PruebaClaveController : Controller
{
    private readonly IClaveGeneradorService _claveGenerador;

    public PruebaClaveController(IClaveGeneradorService claveGenerador)
    {
        _claveGenerador = claveGenerador;
    }

    [HttpGet("generar-ejemplo")]
    public async Task<IActionResult> GenerarEjemplo()
    {
        // Copiar el código de cualquier ejemplo aquí
        // ...
        return Ok(resultado);
    }
}
```

Luego acceder via: `GET https://localhost:5001/api/pruebaclave/generar-ejemplo`

### Opción 2: Usar Swagger

1. Ejecutar el backend: `dotnet run`
2. Abrir Swagger: `https://localhost:5001/swagger`
3. Buscar el endpoint de prueba
4. Ejecutar y ver resultados

### Opción 3: Usar Unit Tests (Recomendado)

Crear un proyecto de pruebas:

```bash
dotnet new xunit -n Facturacion.Tests
dotnet add Facturacion.Tests/Facturacion.Tests.csproj reference Facturacion.Backend/Facturacion.Backend.csproj
```

Crear archivo de prueba:

```csharp
using Xunit;
using Facturacion.Backend.Services.Implementations;

public class ClaveGeneradorServiceTests
{
    [Fact]
    public async Task GenerarClave_DebeRetornar50Digitos()
    {
        // Arrange
        var service = new ClaveGeneradorService(mockContext);
        var documento = CrearDocumentoEjemplo();

        // Act
        var clave = await service.GenerarClaveAsync(documento, 1);

        // Assert
        Assert.Equal(50, clave.Length);
    }

    [Fact]
    public void ValidarClave_ConClaveValida_DebeRetornarTrue()
    {
        // Arrange
        var service = new ClaveGeneradorService(mockContext);
        var claveValida = "50629112500000310123456700100001010000000001187654321";

        // Act
        var resultado = service.ValidarClave(claveValida);

        // Assert
        Assert.True(resultado);
    }
}
```

---

**Nota:** Estos ejemplos son para fines de prueba y desarrollo. En producción, las claves se generan automáticamente durante el proceso de envío a Hacienda a través de `DocumentoHaciendaService`.

---

**Fecha:** 29 de noviembre de 2025
**Versión:** 1.0
**Estado:** Listo para pruebas
