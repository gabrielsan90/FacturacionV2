# Mejoras al Envío de Documentos a Hacienda

## Resumen de Cambios

Se realizó una revisión completa y mejora de la implementación del envío de documentos electrónicos a Hacienda, basándose en las especificaciones oficiales y mejores prácticas.

---

## 1. Actualización de URLs de API (v4.3 → v1)

### Cambios en `HaciendaApiService.cs`

**ANTES:**
```csharp
private const string UrlRecepcionStag = "https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v4.3/recepcion";
private const string UrlRecepcionProd = "https://api.comprobanteselectronicos.go.cr/recepcion/v4.3/recepcion";
```

**AHORA:**
```csharp
private const string UrlRecepcionStag = "https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/recepcion";
private const string UrlRecepcionProd = "https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion";
```

**Razón:** El endpoint correcto para recepción de documentos es `/recepcion/v1/recepcion`, no v4.3. Además, el ambiente sandbox usa el subdominio `api-sandbox` en lugar de `recepcion-sandbox`.

---

## 2. Integración con IHaciendaTokenService (OAuth2)

### Nuevos métodos implementados

Se agregaron dos nuevos métodos que usan OAuth2 Bearer token en lugar de Basic auth:

#### `EnviarDocumentoConTokenAsync`
```csharp
public async Task<HaciendaRespuesta> EnviarDocumentoConTokenAsync(
    string clave,
    string xmlFirmado,
    Guid empresaId,
    string ambiente = "stag")
```

**Características:**
- Obtiene automáticamente el token válido usando `IHaciendaTokenService`
- Maneja automáticamente el refresco de tokens expirados
- Usa autenticación OAuth2 Bearer en lugar de Basic auth
- Es el método **recomendado** para nuevas implementaciones

#### `ConsultarEstadoConTokenAsync`
```csharp
public async Task<HaciendaRespuesta> ConsultarEstadoConTokenAsync(
    string clave,
    Guid empresaId,
    string ambiente = "stag")
```

**Compatibilidad hacia atrás:**
Los métodos antiguos (`EnviarDocumentoAsync` y `ConsultarEstadoAsync` con usuario/contraseña) se mantienen funcionando para compatibilidad hacia atrás, pero están marcados como "legacy".

---

## 3. Manejo Correcto de Códigos HTTP

Se implementó el método `ProcesarRespuestaHaciendaAsync` que maneja correctamente todos los códigos HTTP según la especificación de Hacienda:

### Códigos de Éxito

| Código | Significado | Acción |
|--------|-------------|--------|
| **201 Created** | Documento creado exitosamente | Estado: `enviado` → `Procesando` |
| **202 Accepted** | Documento aceptado para procesamiento | Estado: `enviado` → `Procesando` |

### Códigos de Error

| Código | Significado | Detalle |
|--------|-------------|---------|
| **400 Bad Request** | Error de validación del documento | Estado: `rechazado`. Se extrae el error del header `X-Error-Cause` |
| **401 Unauthorized** | Token inválido o expirado | Estado: `error`. Indica que se debe obtener un nuevo token |
| **403 Forbidden** | Usuario bloqueado | Estado: `error`. Usuario bloqueado temporalmente por intentos fallidos |
| **429 Too Many Requests** | Rate limit excedido | Estado: `error`. Demasiadas peticiones en poco tiempo |
| **50x Server Error** | Error del servidor de Hacienda | Estado: `error`. Se extrae mensaje del header `X-Error-Cause` |

---

## 4. Extracción del Header X-Error-Cause

Se implementó el método `ObtenerHeaderXErrorCause` que extrae el mensaje de error detallado que envía Hacienda en el header `X-Error-Cause`:

```csharp
private string? ObtenerHeaderXErrorCause(HttpResponseMessage response)
{
    if (response.Headers.TryGetValues("X-Error-Cause", out var values))
    {
        return string.Join("; ", values);
    }

    // Algunos servidores pueden usar minúsculas
    if (response.Headers.TryGetValues("x-error-cause", out var valuesLower))
    {
        return string.Join("; ", valuesLower);
    }

    return null;
}
```

Este header contiene información detallada sobre el error y es **crítico** para debugging.

---

## 5. Actualización de DocumentoHaciendaService

Se mejoró el procesamiento de respuestas para distinguir entre diferentes estados:

### Estados Procesados

```csharp
if (estadoRespuesta == "enviado")
{
    // 201/202: Documento recibido por Hacienda y en proceso de validación
    documento.Estado = EstadoDocumento.Procesando;
    documento.MensajeHacienda = "Documento enviado exitosamente a Hacienda y en proceso de validación";
}
else if (estadoRespuesta == "rechazado")
{
    // 400 Bad Request: Error de validación
    documento.Estado = EstadoDocumento.Rechazado;
    // Se incluyen tanto mensajes como detalles del error
}
else if (estadoRespuesta == "error")
{
    // 401/403/429/50x: Error técnico (no del documento)
    // Se registra el error pero no se cambia el estado del documento
}
```

**Mejora clave:** Ahora se distingue entre:
- **Errores de validación del documento** (400) → Documento rechazado
- **Errores técnicos** (401, 403, 429, 50x) → Error de comunicación, el documento no cambió de estado

---

## 6. Actualización de appsettings.json

```json
"HaciendaApi": {
  "UrlRecepcionStaging": "https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/recepcion",
  "UrlRecepcionProduction": "https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion",
  "UrlConsultaStaging": "https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/recepcion",
  "UrlConsultaProduction": "https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion",
  "Timeout": 30,
  "Comments": {
    "Version": "v1 es la versión correcta para el endpoint de recepción",
    "Staging": "Sandbox usa el subdominio api-sandbox",
    "Production": "Producción usa el subdominio api",
    "CodigosHTTP": "201/202=Exitoso, 400=Error validación, 401=Token inválido, 403=Bloqueado, 429=Rate limit, 50x=Error servidor",
    "HeaderError": "Los errores se obtienen del header X-Error-Cause"
  }
}
```

---

## 7. Refactorización de Inyección de Dependencias

### Cambio en Program.cs

**ANTES:**
```csharp
builder.Services.AddHttpClient<IHaciendaApiService, HaciendaApiService>(client => {
    var baseUrl = builder.Configuration["HaciendaApi:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl);
});
```

**AHORA:**
```csharp
builder.Services.AddScoped<IHaciendaApiService, HaciendaApiService>();
```

**Razón:** El servicio ahora usa `IHttpClientFactory` directamente para mayor flexibilidad y mejor gestión de conexiones.

---

## Estructura del Payload JSON

El payload enviado a Hacienda tiene la siguiente estructura:

```json
{
  "clave": "50628112400310112345600100001010000000001199999999",
  "fecha": "2024-11-29T10:30:00-06:00",
  "emisor": {
    "tipoIdentificacion": "01",
    "numeroIdentificacion": "3101123456"
  },
  "comprobanteXml": "BASE64_DEL_XML_FIRMADO"
}
```

**Notas:**
- La información del emisor se extrae automáticamente de la clave (posiciones 9-11 para tipo, 11-23 para número)
- El receptor no es obligatorio en todos los tipos de documento (por ejemplo, en tiquetes)
- El XML debe estar firmado digitalmente y codificado en Base64

---

## Flujo de Autenticación Recomendado

### Usando OAuth2 (Nuevo - Recomendado)

1. El sistema obtiene automáticamente el token usando `IHaciendaTokenService`
2. El token se almacena en BD con su fecha de expiración
3. Si el token expiró, se refresca automáticamente
4. Si el refresh token expiró, se obtiene un nuevo token completo
5. El token se envía como `Authorization: Bearer {token}`

### Ventajas sobre Basic Auth

- **Seguridad:** No se envían credenciales en cada request
- **Eficiencia:** El token se reutiliza mientras sea válido
- **Manejo automático:** El servicio maneja automáticamente la expiración y refresco

---

## Archivos Modificados

### Backend

1. `/Facturacion.Backend/Services/Interfaces/IHaciendaApiService.cs`
   - Agregados métodos `EnviarDocumentoConTokenAsync` y `ConsultarEstadoConTokenAsync`

2. `/Facturacion.Backend/Services/Implementations/HaciendaApiService.cs`
   - Actualizado constructor para usar `IHttpClientFactory` e `IHaciendaTokenService`
   - Actualizadas URLs de API a v1
   - Implementados nuevos métodos con OAuth2
   - Agregado método `ProcesarRespuestaHaciendaAsync` con manejo completo de códigos HTTP
   - Agregado método `ObtenerHeaderXErrorCause`

3. `/Facturacion.Backend/Services/Implementations/DocumentoHaciendaService.cs`
   - Mejorado procesamiento de respuestas para distinguir entre estados
   - Agregado manejo del estado "enviado" (201/202)
   - Agregado manejo del estado "error" para errores técnicos

4. `/Facturacion.Backend/appsettings.json`
   - Actualizadas URLs con la versión correcta (v1)
   - Agregados comentarios explicativos

5. `/Facturacion.Backend/Program.cs`
   - Cambiado registro de servicio a `AddScoped` para usar `IHttpClientFactory`

---

## Migración de Código Existente

### Opción 1: Usar los nuevos métodos (Recomendado)

```csharp
// ANTES (Basic auth)
var respuesta = await _haciendaApi.EnviarDocumentoAsync(
    clave,
    xmlFirmado,
    empresa.UsuarioHacienda,
    empresa.ClaveHacienda,
    ambiente);

// AHORA (OAuth2)
var respuesta = await _haciendaApi.EnviarDocumentoConTokenAsync(
    clave,
    xmlFirmado,
    empresa.Id,  // Solo necesita el ID de la empresa
    ambiente);
```

### Opción 2: Mantener compatibilidad

Los métodos antiguos siguen funcionando, por lo que no es necesario cambiar código existente si no se desea.

---

## Pruebas Recomendadas

1. **Envío exitoso (201/202)**
   - Verificar que el estado cambia a "Procesando"
   - Verificar que se registra la fecha de envío

2. **Error de validación (400)**
   - Verificar que se extrae correctamente el mensaje del header X-Error-Cause
   - Verificar que el estado cambia a "Rechazado"

3. **Token inválido (401)**
   - Verificar que se detecta y se solicita un nuevo token
   - Verificar que el estado del documento no cambia

4. **Rate limit (429)**
   - Verificar que se maneja correctamente el error
   - Implementar retry con backoff exponencial si es necesario

5. **Error del servidor (500-599)**
   - Verificar que se extrae el mensaje del header X-Error-Cause
   - Verificar que se registra para análisis

---

## Próximos Pasos Sugeridos

1. **Migrar a OAuth2:** Actualizar el código que llama a los métodos antiguos para usar `EnviarDocumentoConTokenAsync` y `ConsultarEstadoConTokenAsync`

2. **Implementar reintentos:** Agregar lógica de retry con backoff exponencial para errores 429 y 50x

3. **Monitoreo:** Implementar logging detallado de todas las respuestas de Hacienda para análisis

4. **Alertas:** Configurar alertas para errores 403 (usuario bloqueado) y 50x (problemas del servidor)

5. **Testing:** Crear tests unitarios y de integración para todos los códigos HTTP

---

## Referencias

- **Especificación de Hacienda:** Documento oficial de integración con la API de recepción
- **OAuth2:** RFC 6749 - The OAuth 2.0 Authorization Framework
- **IHaciendaTokenService:** Servicio implementado para gestión de tokens OAuth2

---

## Notas Importantes

1. **Ambiente Sandbox:** Para pruebas usar `ambiente = "stag"` y la URL con `api-sandbox`
2. **Ambiente Producción:** Para producción usar `ambiente = "prod"` y la URL estándar
3. **Timeout:** El timeout por defecto es 30 segundos, configurable en appsettings.json
4. **Logs:** Todos los métodos registran logs detallados para debugging

---

**Fecha de implementación:** 2025-11-29
**Versión de .NET:** 9.0
**Estado:** Compilación exitosa, listo para pruebas
