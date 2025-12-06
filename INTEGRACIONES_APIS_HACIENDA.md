# Integraciones con APIs de Hacienda - Sistema de Facturación Electrónica v4.4

## Resumen de Implementación

Se han implementado las integraciones con las APIs oficiales de Hacienda de Costa Rica para el sistema de facturación electrónica v4.4. Estas integraciones permiten validar información crítica para la generación de comprobantes electrónicos.

## Módulos Implementados

### M2: API de CABYS (Códigos de Actividades, Bienes y Servicios)

**URL API:** `https://api.hacienda.go.cr/fe/cabys`

**Archivos creados:**
- `/Facturacion.Shared/DTOs/CabysDTO.cs` - DTOs para consultas CABYS
- `/Facturacion.Backend/Services/Interfaces/ICabysService.cs` - Interfaz del servicio
- `/Facturacion.Backend/Services/Implementations/CabysService.cs` - Implementación del servicio

**Funcionalidades:**
- `ValidarCodigoAsync(string codigo)` - Valida que un código CABYS de 13 dígitos existe y es válido
- `ObtenerPorCodigoAsync(string codigo)` - Obtiene información detallada de un código CABYS
- `BuscarPorDescripcionAsync(string descripcion, int top)` - Busca códigos por descripción
- `BuscarAsync(CabysBusquedaDTO parametros)` - Búsqueda general con múltiples criterios
- `ObtenerPorcentajeImpuestoAsync(string codigo)` - Obtiene el porcentaje de impuesto aplicable
- `LimpiarCache()` - Limpia la caché de códigos CABYS

**Características:**
- Validación de formato (13 dígitos numéricos)
- Caching de resultados por 7 días para mejorar rendimiento
- Manejo robusto de errores y timeout de 30 segundos
- Logging completo de operaciones

**Ejemplo de uso:**
```csharp
// Validar un código CABYS
var esValido = await _cabysService.ValidarCodigoAsync("4321100000000");

// Obtener información completa
var cabys = await _cabysService.ObtenerPorCodigoAsync("4321100000000");
if (cabys != null)
{
    Console.WriteLine($"Descripción: {cabys.Descripcion}");
    Console.WriteLine($"Impuesto: {cabys.Impuesto}%");
}

// Buscar por descripción
var resultados = await _cabysService.BuscarPorDescripcionAsync("software", top: 10);
foreach (var item in resultados.Cabys)
{
    Console.WriteLine($"{item.Codigo} - {item.Descripcion}");
}
```

---

### M3: API de Actividades Económicas (CIIU)

**URL API:** `https://api.hacienda.go.cr/fe/ae`

**Archivos creados:**
- `/Facturacion.Shared/DTOs/ActividadEconomicaDTO.cs` - DTOs para actividades económicas
- `/Facturacion.Backend/Services/Interfaces/IActividadEconomicaService.cs` - Interfaz del servicio
- `/Facturacion.Backend/Services/Implementations/ActividadEconomicaService.cs` - Implementación del servicio

**Funcionalidades:**
- `ValidarCodigoAsync(string codigo)` - Valida que un código de actividad económica existe y está activo
- `ObtenerPorCodigoAsync(string codigo)` - Obtiene información detallada de una actividad
- `BuscarPorDescripcionAsync(string descripcion, int top)` - Busca actividades por descripción
- `BuscarAsync(ActividadEconomicaBusquedaDTO parametros)` - Búsqueda general con múltiples criterios
- `ObtenerTodasActivasAsync()` - Obtiene todas las actividades económicas activas (útil para combos)
- `LimpiarCache()` - Limpia la caché de actividades económicas

**Características:**
- Validación de formato (solo dígitos)
- Caching de resultados por 30 días
- Validación de estado activo/inactivo
- Manejo robusto de errores y timeout de 30 segundos
- Logging completo de operaciones

**Ejemplo de uso:**
```csharp
// Validar un código de actividad económica
var esValido = await _actividadService.ValidarCodigoAsync("620101");

// Obtener información completa
var actividad = await _actividadService.ObtenerPorCodigoAsync("620101");
if (actividad != null && actividad.Activa)
{
    Console.WriteLine($"Descripción: {actividad.Descripcion}");
}

// Obtener todas las actividades para un combo
var todasActivas = await _actividadService.ObtenerTodasActivasAsync();

// Buscar por descripción
var resultados = await _actividadService.BuscarPorDescripcionAsync("programación", top: 10);
```

---

### M4: API de Tipo de Cambio BCCR

**NOTA:** Este servicio ya estaba implementado en el proyecto.

**URL API:** `https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx`

**Archivos existentes:**
- `/Facturacion.Backend/Services/Interfaces/ITipoCambioBCCRService.cs`
- `/Facturacion.Backend/Services/Implementations/TipoCambioBCCRService.cs`

**Funcionalidades:**
- Obtención de tipo de cambio compra/venta del dólar
- Obtención de tipo de cambio para EUR
- Caching por 24 horas
- Fallback automático a días anteriores si falla la consulta

---

### M5: API de Exoneraciones

**URL API:** `https://api.hacienda.go.cr/fe/ex`

**Archivos creados:**
- `/Facturacion.Shared/DTOs/ExoneracionDTO.cs` - DTOs para exoneraciones
- `/Facturacion.Backend/Services/Interfaces/IExoneracionService.cs` - Interfaz del servicio
- `/Facturacion.Backend/Services/Implementations/ExoneracionService.cs` - Implementación del servicio

**Funcionalidades:**
- `ValidarExoneracionAsync(string numeroDocumento, string nombreInstitucion, DateTime? fechaValidacion)` - Valida que una exoneración existe y está vigente
- `EstaVigenteAsync(string numeroDocumento, string nombreInstitucion, DateTime? fecha)` - Verifica si está vigente en una fecha específica
- `ObtenerExoneracionAsync(string numeroDocumento, string nombreInstitucion)` - Obtiene información detallada
- `ObtenerExoneracionesPorBeneficiarioAsync(string identificacion)` - Obtiene exoneraciones vigentes de un beneficiario
- `LimpiarCache()` - Limpia la caché de exoneraciones

**Características:**
- Validación de vigencia basada en fechas de emisión y vencimiento
- Caching de resultados por 24 horas
- Información detallada del beneficiario
- Porcentaje de exoneración
- Manejo robusto de errores y timeout de 30 segundos
- Logging completo de operaciones

**Ejemplo de uso:**
```csharp
// Validar una exoneración
var validacion = await _exoneracionService.ValidarExoneracionAsync(
    "DOC-2024-001",
    "Ministerio de Hacienda",
    DateTime.Now
);

if (validacion.EsValida)
{
    Console.WriteLine($"Exoneración válida: {validacion.Mensaje}");
    Console.WriteLine($"Porcentaje: {validacion.Exoneracion.PorcentajeExoneracion}%");
}
else
{
    Console.WriteLine($"Exoneración no válida: {validacion.Mensaje}");
}

// Verificar vigencia simple
var vigente = await _exoneracionService.EstaVigenteAsync("DOC-2024-001", "Ministerio de Hacienda");

// Obtener exoneraciones de un beneficiario
var exoneraciones = await _exoneracionService.ObtenerExoneracionesPorBeneficiarioAsync("301234567");
```

---

## Registro en Dependency Injection

Todos los servicios han sido registrados en `/Facturacion.Backend/Program.cs`:

```csharp
// Dependency Injection - Servicios de APIs de Hacienda (CABYS, Actividades Económicas, Exoneraciones)
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.ICabysService, Facturacion.Backend.Services.Implementations.CabysService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IActividadEconomicaService, Facturacion.Backend.Services.Implementations.ActividadEconomicaService>();
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IExoneracionService, Facturacion.Backend.Services.Implementations.ExoneracionService>();
```

---

## Dependencias Requeridas

Todos los servicios utilizan las siguientes dependencias ya configuradas en el proyecto:

- `IHttpClientFactory` - Para realizar llamadas HTTP a las APIs
- `IMemoryCache` - Para almacenar resultados en caché
- `ILogger<T>` - Para logging de operaciones

Estas dependencias ya están configuradas en `Program.cs`:
```csharp
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
```

---

## Características Comunes

Todos los servicios implementados comparten las siguientes características:

1. **Caching Inteligente**
   - CABYS: 7 días
   - Actividades Económicas: 30 días
   - Exoneraciones: 24 horas
   - Tipo de Cambio: 24 horas

2. **Manejo de Errores**
   - Try-catch en todas las operaciones
   - Mensajes de error descriptivos
   - Logging de todas las excepciones
   - Timeout de 30 segundos en llamadas HTTP

3. **Validaciones**
   - Validación de formatos antes de consultar APIs
   - Validación de datos de entrada
   - Manejo de respuestas nulas o vacías

4. **Logging**
   - Registro de todas las operaciones
   - Niveles apropiados (Debug, Information, Warning, Error)
   - Información de contexto en cada log

5. **Async/Await**
   - Todas las operaciones son asíncronas
   - No bloquean el hilo de ejecución
   - Optimizadas para escalabilidad

---

## Uso en Validación de Documentos

Estos servicios pueden integrarse en el proceso de validación de documentos:

```csharp
// En ValidacionDocumentoService o DocumentoService

// Validar código CABYS de cada línea de detalle
foreach (var linea in documento.Lineas)
{
    var esValidoCabys = await _cabysService.ValidarCodigoAsync(linea.CodigoCabys);
    if (!esValidoCabys)
    {
        errores.Add($"Línea {linea.NumeroLinea}: Código CABYS {linea.CodigoCabys} no es válido");
    }
}

// Validar actividad económica del emisor
var actividadValida = await _actividadEconomicaService.ValidarCodigoAsync(documento.CodigoActividad);
if (!actividadValida)
{
    errores.Add($"Código de actividad económica {documento.CodigoActividad} no es válido");
}

// Validar exoneraciones si aplica
if (documento.Exoneraciones != null)
{
    foreach (var exo in documento.Exoneraciones)
    {
        var validacion = await _exoneracionService.ValidarExoneracionAsync(
            exo.NumeroDocumento,
            exo.NombreInstitucion,
            documento.Fecha
        );

        if (!validacion.EsValida)
        {
            errores.Add($"Exoneración {exo.NumeroDocumento} no es válida: {validacion.Mensaje}");
        }
    }
}
```

---

## Notas Importantes

1. **URLs de las APIs**: Las URLs están basadas en la guía de facturación electrónica v4.4. Si Hacienda actualiza las URLs, deben modificarse las constantes en cada servicio.

2. **Autenticación**: Actualmente las APIs públicas de consulta no requieren autenticación. Si Hacienda implementa autenticación en el futuro, deberá agregarse.

3. **Rate Limiting**: Las APIs de Hacienda pueden tener límites de consultas. El caching ayuda a reducir el número de llamadas.

4. **Ambiente de Pruebas**: Para pruebas, Hacienda puede proporcionar URLs alternativas. Estas pueden configurarse en `appsettings.json` en el futuro.

5. **Disponibilidad**: Las APIs de Hacienda pueden no estar disponibles 24/7. El manejo de errores está preparado para esto.

---

## Testing

Para probar los servicios, se pueden crear endpoints de prueba o tests unitarios:

```csharp
// Endpoint de prueba en un controller
[HttpGet("test-cabys/{codigo}")]
public async Task<IActionResult> TestCabys(string codigo)
{
    try
    {
        var cabys = await _cabysService.ObtenerPorCodigoAsync(codigo);
        return Ok(cabys);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

[HttpGet("test-actividad/{codigo}")]
public async Task<IActionResult> TestActividad(string codigo)
{
    try
    {
        var actividad = await _actividadEconomicaService.ObtenerPorCodigoAsync(codigo);
        return Ok(actividad);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

[HttpGet("test-exoneracion")]
public async Task<IActionResult> TestExoneracion(string numeroDoc, string institucion)
{
    try
    {
        var validacion = await _exoneracionService.ValidarExoneracionAsync(numeroDoc, institucion);
        return Ok(validacion);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```

---

## Estado de Implementación

| Módulo | Estado | Descripción |
|--------|--------|-------------|
| M2 - CABYS | Completado | Validación de códigos de productos y servicios |
| M3 - Actividades Económicas | Completado | Validación de códigos CIIU |
| M4 - Tipo de Cambio | Ya existía | Consulta al BCCR |
| M5 - Exoneraciones | Completado | Validación de documentos de exoneración |

---

## Próximos Pasos Recomendados

1. **Crear Controllers de Prueba**: Crear endpoints de prueba para validar las integraciones
2. **Integrar en Validación**: Incorporar las validaciones en el proceso de creación de documentos
3. **Configuración**: Mover las URLs a `appsettings.json` para facilitar cambios de ambiente
4. **Monitoring**: Agregar métricas de uso y disponibilidad de las APIs
5. **Tests Unitarios**: Crear tests unitarios y de integración para cada servicio
6. **Frontend**: Crear componentes en el frontend para búsqueda de códigos CABYS y actividades

---

## Conclusión

Se han implementado exitosamente las integraciones con las APIs de Hacienda (M2, M3, M5) complementando el servicio de tipo de cambio (M4) que ya existía. Todos los servicios están listos para ser utilizados en el proceso de validación y generación de documentos electrónicos, cumpliendo con los requisitos de la facturación electrónica v4.4 de Costa Rica.

El proyecto compila exitosamente y todos los servicios están registrados en el contenedor de Dependency Injection.
