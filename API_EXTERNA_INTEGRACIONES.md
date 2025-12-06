# Integraciones con APIs Externas - Facturación Electrónica v4.4

Este documento describe las integraciones implementadas con APIs externas de Hacienda y BCCR para el sistema de facturación electrónica de Costa Rica v4.4.

---

## Servicios Implementados

### 1. Servicio CABYS (Catálogo de Bienes y Servicios)

**Propósito:** Consultar y validar códigos CABYS de productos y servicios.

**API:** `https://api.hacienda.go.cr/fe/cabys`

**Archivos:**
- Interfaz: `/Facturacion.Backend/Services/Interfaces/ICabysService.cs`
- Implementación: `/Facturacion.Backend/Services/Implementations/CabysService.cs`
- Controlador: `/Facturacion.Backend/Controllers/CabysController.cs`
- DTOs: `/Facturacion.Shared/DTOs/CabysDTO.cs`

**Características:**
- Búsqueda de códigos CABYS por código exacto o descripción
- Validación de formato (13 dígitos)
- Obtención de porcentaje de impuesto
- Caching de 7 días para optimizar rendimiento
- Manejo de errores de conexión con timeouts

**Endpoints Disponibles:**

```http
# Obtener información de un código CABYS específico
GET /api/Cabys/1234567890123
Authorization: Bearer {token}

# Validar si un código CABYS existe
GET /api/Cabys/validar/1234567890123
Authorization: Bearer {token}

# Buscar códigos CABYS por descripción
GET /api/Cabys/buscar/descripcion?q=cafe&top=10
Authorization: Bearer {token}

# Búsqueda avanzada
POST /api/Cabys/buscar
Authorization: Bearer {token}
Content-Type: application/json

{
  "codigo": "1234567890123",
  "descripcion": "cafe",
  "top": 10
}

# Obtener porcentaje de impuesto
GET /api/Cabys/1234567890123/impuesto
Authorization: Bearer {token}

# Limpiar caché (solo SuperUser/Administrador)
POST /api/Cabys/limpiar-cache
Authorization: Bearer {token}
```

**Ejemplo de Respuesta:**

```json
{
  "codigo": "1234567890123",
  "descripcion": "Café tostado en grano",
  "impuesto": 13.00,
  "categoria": "Alimentos y bebidas"
}
```

---

### 2. Servicio Actividades Económicas (CIIU)

**Propósito:** Consultar y validar actividades económicas según clasificación CIIU.

**API:** `https://api.hacienda.go.cr/fe/ae`

**Archivos:**
- Interfaz: `/Facturacion.Backend/Services/Interfaces/IActividadEconomicaService.cs`
- Implementación: `/Facturacion.Backend/Services/Implementations/ActividadEconomicaService.cs`
- Controlador: `/Facturacion.Backend/Controllers/ActividadesEconomicasController.cs`
- DTOs: `/Facturacion.Shared/DTOs/ActividadEconomicaDTO.cs`

**Características:**
- Búsqueda por código o descripción
- Validación de actividades activas
- Obtención de todas las actividades económicas
- Caching de 30 días
- Filtrado por estado (activa/inactiva)

**Endpoints Disponibles:**

```http
# Obtener información de una actividad económica específica
GET /api/ActividadesEconomicas/620101
Authorization: Bearer {token}

# Validar si un código de actividad económica está activo
GET /api/ActividadesEconomicas/validar/620101
Authorization: Bearer {token}

# Buscar actividades económicas por descripción
GET /api/ActividadesEconomicas/buscar/descripcion?q=programacion&top=10
Authorization: Bearer {token}

# Búsqueda avanzada
POST /api/ActividadesEconomicas/buscar
Authorization: Bearer {token}
Content-Type: application/json

{
  "codigo": "620101",
  "descripcion": "programacion",
  "top": 10
}

# Obtener todas las actividades económicas activas
GET /api/ActividadesEconomicas/todas
Authorization: Bearer {token}

# Limpiar caché (solo SuperUser/Administrador)
POST /api/ActividadesEconomicas/limpiar-cache
Authorization: Bearer {token}
```

**Ejemplo de Respuesta:**

```json
{
  "codigo": "620101",
  "descripcion": "Programación informática",
  "categoria": "Tecnología",
  "activa": true
}
```

---

### 3. Servicio Tipo de Cambio (BCCR)

**Propósito:** Consultar el tipo de cambio oficial del Banco Central de Costa Rica.

**API:** `https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx`

**Archivos:**
- Interfaz: `/Facturacion.Backend/Services/Interfaces/ITipoCambioBCCRService.cs`
- Implementación: `/Facturacion.Backend/Services/Implementations/TipoCambioBCCRService.cs`
- Controlador: `/Facturacion.Backend/Controllers/TipoCambioController.cs`

**Características:**
- Consulta de tipo de cambio de compra y venta
- Soporte para USD, EUR, CRC
- Caching diario (24 horas)
- Fallback automático a fechas anteriores si no hay datos
- Precisión de 5 decimales

**Endpoints Disponibles:**

```http
# Obtener tipo de cambio de compra
GET /api/TipoCambio/compra?fecha=2025-12-01
Authorization: Bearer {token}

# Obtener tipo de cambio de venta
GET /api/TipoCambio/venta?fecha=2025-12-01
Authorization: Bearer {token}

# Obtener ambos tipos de cambio
GET /api/TipoCambio?fecha=2025-12-01
Authorization: Bearer {token}

# Obtener tipo de cambio para una moneda específica
GET /api/TipoCambio/moneda/USD?fecha=2025-12-01
Authorization: Bearer {token}

# Obtener tipos de cambio para un rango de fechas
POST /api/TipoCambio/rango
Authorization: Bearer {token}
Content-Type: application/json

{
  "fechaInicio": "2025-12-01",
  "fechaFin": "2025-12-31"
}
```

**Ejemplo de Respuesta:**

```json
{
  "fecha": "2025-12-01",
  "compra": 505.00000,
  "venta": 515.00000,
  "moneda": "USD"
}
```

---

### 4. Servicio Exoneraciones

**Propósito:** Validar documentos de exoneración tributaria.

**API:** `https://api.hacienda.go.cr/fe/ex`

**Archivos:**
- Interfaz: `/Facturacion.Backend/Services/Interfaces/IExoneracionService.cs`
- Implementación: `/Facturacion.Backend/Services/Implementations/ExoneracionService.cs`
- Controlador: `/Facturacion.Backend/Controllers/ExoneracionesController.cs`
- DTOs: `/Facturacion.Shared/DTOs/ExoneracionDTO.cs`

**Características:**
- Validación de documentos de exoneración
- Verificación de vigencia por fecha
- Consulta por beneficiario
- Validación múltiple
- Caching de 24 horas

**Endpoints Disponibles:**

```http
# Obtener información de una exoneración
GET /api/Exoneraciones?numeroDocumento=EX-2025-001&nombreInstitucion=Ministerio de Hacienda
Authorization: Bearer {token}

# Validar exoneración
POST /api/Exoneraciones/validar
Authorization: Bearer {token}
Content-Type: application/json

{
  "numeroDocumento": "EX-2025-001",
  "nombreInstitucion": "Ministerio de Hacienda",
  "fechaValidacion": "2025-12-01"
}

# Verificar si está vigente
GET /api/Exoneraciones/vigente?numeroDocumento=EX-2025-001&nombreInstitucion=Ministerio de Hacienda&fecha=2025-12-01
Authorization: Bearer {token}

# Obtener exoneraciones de un beneficiario
GET /api/Exoneraciones/beneficiario/304560789
Authorization: Bearer {token}

# Validar múltiples exoneraciones
POST /api/Exoneraciones/validar-multiple
Authorization: Bearer {token}
Content-Type: application/json

[
  {
    "numeroDocumento": "EX-2025-001",
    "nombreInstitucion": "Ministerio de Hacienda"
  },
  {
    "numeroDocumento": "EX-2025-002",
    "nombreInstitucion": "Ministerio de Salud"
  }
]

# Limpiar caché (solo SuperUser/Administrador)
POST /api/Exoneraciones/limpiar-cache
Authorization: Bearer {token}
```

**Ejemplo de Respuesta:**

```json
{
  "numeroDocumento": "EX-2025-001",
  "nombreInstitucion": "Ministerio de Hacienda",
  "fechaEmision": "2025-01-01",
  "fechaVencimiento": "2025-12-31",
  "porcentajeExoneracion": 100,
  "tipoDocumento": "01",
  "identificacionBeneficiario": "304560789",
  "nombreBeneficiario": "Juan Pérez",
  "vigente": true,
  "razonNoVigente": null
}
```

---

## Configuración

### Requisitos

Los siguientes servicios ya están registrados en `Program.cs` (líneas 18-19, 209-212):

```csharp
// Servicios requeridos
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Servicios de APIs externas
builder.Services.AddScoped<ICabysService, CabysService>();
builder.Services.AddScoped<IActividadEconomicaService, ActividadEconomicaService>();
builder.Services.AddScoped<IExoneracionService, ExoneracionService>();
builder.Services.AddScoped<ITipoCambioBCCRService, TipoCambioBCCRService>();
```

### Autenticación

Todos los endpoints requieren autenticación JWT. Incluir el token en el header:

```http
Authorization: Bearer {token}
```

### Permisos

- Endpoints de consulta: Cualquier usuario autenticado
- Endpoints de limpieza de caché: Solo `SuperUser` o `Administrador`

---

## Manejo de Errores

### Códigos de Estado HTTP

- `200 OK`: Operación exitosa
- `400 Bad Request`: Parámetros inválidos
- `401 Unauthorized`: Token JWT inválido o ausente
- `403 Forbidden`: Usuario sin permisos suficientes
- `404 Not Found`: Recurso no encontrado
- `503 Service Unavailable`: Error de conexión con API externa
- `500 Internal Server Error`: Error interno del servidor

### Mensajes de Error

Todos los endpoints devuelven mensajes de error descriptivos:

```json
{
  "message": "No se pudo conectar con la API de CABYS de Hacienda. Verifique su conexión a internet."
}
```

---

## Caching

### Estrategia de Caché

Todos los servicios implementan caching en memoria para optimizar rendimiento:

| Servicio | Duración de Caché | Razón |
|----------|-------------------|-------|
| CABYS | 7 días | Códigos estables, cambios poco frecuentes |
| Actividades Económicas | 30 días | Catálogo muy estable |
| Tipo de Cambio | 24 horas | Actualización diaria |
| Exoneraciones | 24 horas | Vigencia puede cambiar |

### Limpieza Manual

Los administradores pueden forzar la limpieza de caché mediante los endpoints `POST /limpiar-cache`.

---

## Logging

Todos los servicios implementan logging detallado:

- **Debug**: Operaciones normales, cache hits
- **Information**: Consultas exitosas a APIs externas
- **Warning**: Errores de conexión, respuestas inesperadas
- **Error**: Errores críticos

---

## Mejores Prácticas

### 1. Uso del Caching

Aprovechar el caching automático para reducir llamadas a APIs externas:

```csharp
// El servicio maneja el cache automáticamente
var cabys = await _cabysService.ObtenerPorCodigoAsync("1234567890123");
```

### 2. Manejo de Errores de Conexión

Los servicios lanzan `InvalidOperationException` cuando no pueden conectarse:

```csharp
try
{
    var tipoCambio = await _tipoCambioService.ObtenerTipoCambioVentaAsync(DateTime.Now);
}
catch (InvalidOperationException ex)
{
    // Manejar error de conexión
    // El servicio ya intentó fallback automático
}
```

### 3. Validación de Datos

Siempre validar los datos antes de enviar a los servicios:

```csharp
// Validar formato de código CABYS
if (codigo.Length != 13 || !codigo.All(char.IsDigit))
{
    return BadRequest("Código CABYS inválido");
}
```

---

## Pruebas

### Swagger UI

Todos los endpoints están disponibles en Swagger UI para pruebas:

```
https://localhost:7001/swagger
```

### Ejemplos de Prueba

**1. Buscar código CABYS:**
```bash
curl -X GET "https://localhost:7001/api/Cabys/buscar/descripcion?q=cafe&top=10" \
  -H "Authorization: Bearer {token}"
```

**2. Obtener tipo de cambio:**
```bash
curl -X GET "https://localhost:7001/api/TipoCambio?fecha=2025-12-01" \
  -H "Authorization: Bearer {token}"
```

**3. Validar exoneración:**
```bash
curl -X POST "https://localhost:7001/api/Exoneraciones/validar" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "numeroDocumento": "EX-2025-001",
    "nombreInstitucion": "Ministerio de Hacienda"
  }'
```

---

## Troubleshooting

### Problema: Error 503 Service Unavailable

**Causa:** No se puede conectar con la API externa de Hacienda o BCCR.

**Solución:**
1. Verificar conexión a internet
2. Verificar que las URLs de las APIs estén accesibles
3. Revisar logs para detalles específicos

### Problema: Caché no se actualiza

**Causa:** Los datos cacheados tienen una duración específica.

**Solución:**
1. Esperar a que expire el caché automáticamente
2. Usar endpoint `/limpiar-cache` (requiere permisos de administrador)

### Problema: Token JWT inválido

**Causa:** Token expirado o formato incorrecto.

**Solución:**
1. Obtener un nuevo token mediante `/api/Accounts/Login`
2. Incluir el token en el header: `Authorization: Bearer {token}`

---

## Notas Importantes

1. **URLs de APIs Externas:** Las URLs de las APIs de Hacienda pueden cambiar. Verificar la documentación oficial si hay errores de conexión.

2. **Límites de Rate Limiting:** Aunque no hay límites documentados oficialmente, el caching ayuda a reducir la carga en las APIs de Hacienda.

3. **Tipo de Cambio BCCR:** El servicio del BCCR puede tener datos retrasados en fines de semana y feriados. El servicio implementa fallback automático a fechas anteriores.

4. **Exoneraciones:** La API de exoneraciones de Hacienda puede no estar disponible en todos los ambientes (ATV vs Producción).

---

## Arquitectura

### Patrón de Diseño

Los servicios siguen el patrón establecido en el proyecto:

1. **Interfaz** en `/Services/Interfaces/` define el contrato
2. **Implementación** en `/Services/Implementations/` con lógica de negocio
3. **Controlador** en `/Controllers/` expone endpoints REST
4. **DTOs** en `Facturacion.Shared/DTOs/` para transferencia de datos

### Inyección de Dependencias

Todos los servicios usan inyección de dependencias:

```csharp
public class CabysService : ICabysService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CabysService> _logger;

    public CabysService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<CabysService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _cache = cache;
        _logger = logger;
    }
}
```

---

## Referencias

- Documentación oficial Hacienda v4.4: https://www.hacienda.go.cr/ATV/ComprobanteElectronico/frmAnexosyEstructuras.aspx
- API CABYS: https://api.hacienda.go.cr/fe/cabys
- API Actividades Económicas: https://api.hacienda.go.cr/fe/ae
- API Exoneraciones: https://api.hacienda.go.cr/fe/ex
- Servicio Web BCCR: https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx

---

**Fecha de creación:** 01 de diciembre de 2025
**Versión:** 1.0
**Estado:** Implementado y Funcional
