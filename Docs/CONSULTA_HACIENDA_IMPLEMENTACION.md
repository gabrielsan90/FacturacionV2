# Implementación de Consulta de Comprobantes a Hacienda

## Resumen

Se ha mejorado la implementación del servicio de consulta de comprobantes a Hacienda basándose en el código de ejemplo proporcionado. Los cambios permiten parsear correctamente las respuestas de Hacienda, decodificar el XML de respuesta y manejar todos los códigos HTTP relevantes.

## Archivos Creados

### 1. DTOs Nuevos

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/DTOs/HaciendaConsultaRespuesta.cs`

DTO para la respuesta JSON de Hacienda al consultar el estado de un documento:

```csharp
public class HaciendaConsultaRespuesta
{
    [JsonPropertyName("clave")]
    public string Clave { get; set; }

    [JsonPropertyName("fecha")]
    public string Fecha { get; set; }

    [JsonPropertyName("ind-estado")]
    public string IndEstado { get; set; }

    [JsonPropertyName("respuesta-xml")]
    public string? RespuestaXml { get; set; }  // XML en Base64
}
```

#### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/DTOs/HaciendaMensajeRespuestaXml.cs`

DTO para el XML de respuesta de Hacienda (decodificado de Base64):

```csharp
public class HaciendaMensajeRespuestaXml
{
    public string Clave { get; set; }
    public string NombreEmisor { get; set; }
    public string TipoIdentificacionEmisor { get; set; }
    public string NumeroCedulaEmisor { get; set; }
    public string Mensaje { get; set; }  // 1=Aceptado, 2=Parcial, 3=Rechazado
    public string DetalleMensaje { get; set; }
    public decimal? MontoTotalImpuesto { get; set; }
    public decimal? TotalFactura { get; set; }

    // Propiedades calculadas
    public bool EsAceptado => Mensaje == "1";
    public bool EsAceptadoParcial => Mensaje == "2";
    public bool EsRechazado => Mensaje == "3";
    public string EstadoTexto => ...;
}
```

## Archivos Modificados

### `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Services/Implementations/HaciendaApiService.cs`

#### Mejoras Realizadas:

1. **Método `ProcesarRespuestaHaciendaAsync` mejorado**:
   - Maneja HTTP 200 (documento consultado con respuesta de Hacienda)
   - Maneja HTTP 202 (documento aceptado para procesamiento)
   - Maneja HTTP 404 (documento NO encontrado en Hacienda)
   - Parsea el JSON de respuesta de Hacienda
   - Decodifica y parsea el XML de respuesta

2. **Nuevo método `ParsearXmlRespuestaHacienda`**:
   - Decodifica el XML desde Base64
   - Parsea el XML con estructura `<MensajeHacienda>`
   - Extrae campos: Mensaje, DetalleMensaje, Clave, etc.
   - Maneja errores de parseo y decodificación

3. **Nuevo método `CrearRespuestaError`**:
   - Helper para crear respuestas de error consistentes
   - Reduce código duplicado

## Flujo de Consulta

### 1. Consulta Exitosa (HTTP 200/202)

```
Cliente → GET /recepcion/{clave} con Bearer Token
         ↓
Hacienda → Responde JSON con respuesta-xml en Base64
         ↓
Sistema → Deserializa JSON
        → Decodifica respuesta-xml de Base64
        → Parsea XML <MensajeHacienda>
        → Extrae Mensaje (1/2/3) y DetalleMensaje
        → Determina estado: aceptado/aceptado_parcial/rechazado
         ↓
Cliente ← HaciendaRespuesta con estado y mensajes parseados
```

### 2. Documento No Encontrado (HTTP 404)

```
Cliente → GET /recepcion/{clave} con Bearer Token
         ↓
Hacienda → HTTP 404 Not Found
         ↓
Sistema → Crea HaciendaRespuesta con IndEstado = "no_encontrado"
        → Marca como disponible para reenvío
         ↓
Cliente ← Puede reintentar envío del documento
```

### 3. Otros Errores (400, 401, 403, 429, 50x)

```
Cliente → Petición a Hacienda
         ↓
Hacienda → Error HTTP con header X-Error-Cause
         ↓
Sistema → Extrae X-Error-Cause del header
        → Crea HaciendaRespuesta con mensaje de error
         ↓
Cliente ← Error detallado con causa específica
```

## Códigos HTTP Manejados

| Código | Descripción | Estado Retornado | Acción |
|--------|-------------|------------------|---------|
| 200 | Documento consultado | `aceptado`/`rechazado`/`aceptado_parcial` | Parsear XML de respuesta |
| 201 | Documento creado | `enviado` | Documento enviado exitosamente |
| 202 | Aceptado para procesamiento | `procesando` o estado del XML | Parsear XML si existe |
| 400 | Bad Request | `rechazado` | Error de validación |
| 401 | Unauthorized | `error` | Token inválido/expirado |
| 403 | Forbidden | `error` | Usuario bloqueado |
| 404 | Not Found | `no_encontrado` | Documento no existe - puede reenviarse |
| 429 | Too Many Requests | `error` | Rate limit excedido |
| 50x | Server Error | `error` | Error del servidor |

## Ejemplo de Uso

### Consultar estado de documento

```csharp
var haciendaApiService = serviceProvider.GetService<IHaciendaApiService>();

var respuesta = await haciendaApiService.ConsultarEstadoConTokenAsync(
    clave: "50628112400310112345600100001010000000001199999999",
    empresaId: empresaGuid,
    ambiente: "stag"
);

if (respuesta.IndEstado == "aceptado")
{
    // Documento aceptado por Hacienda
    var mensaje = respuesta.Mensajes.FirstOrDefault();
    Console.WriteLine($"Documento aceptado: {mensaje?.Detalle}");
}
else if (respuesta.IndEstado == "rechazado")
{
    // Documento rechazado
    var mensaje = respuesta.Mensajes.FirstOrDefault();
    Console.WriteLine($"Documento rechazado: {mensaje?.Detalle}");
}
else if (respuesta.IndEstado == "no_encontrado")
{
    // Documento no existe en Hacienda - puede reenviarse
    Console.WriteLine("Documento no encontrado. Considere reenviarlo.");
}
```

## Estructura XML de Respuesta de Hacienda

```xml
<MensajeHacienda>
  <Clave>50628112400310112345600100001010000000001199999999</Clave>
  <NombreEmisor>Empresa XYZ</NombreEmisor>
  <TipoIdentificacionEmisor>01</TipoIdentificacionEmisor>
  <NumeroCedulaEmisor>3101123456</NumeroCedulaEmisor>
  <Mensaje>1</Mensaje>  <!-- 1=Aceptado, 2=Parcial, 3=Rechazado -->
  <DetalleMensaje>Documento aceptado correctamente</DetalleMensaje>
  <MontoTotalImpuesto>1300.00</MontoTotalImpuesto>
  <TotalFactura>11300.00</TotalFactura>
</MensajeHacienda>
```

## Mapeo de Códigos de Mensaje

| Código Mensaje | Estado | Descripción |
|----------------|--------|-------------|
| 1 | Aceptado | Documento aceptado correctamente |
| 2 | Aceptado Parcial | Documento aceptado con observaciones |
| 3 | Rechazado | Documento rechazado por errores |

## Ventajas de la Implementación

1. **Parseo automático**: El XML de respuesta se parsea automáticamente
2. **Manejo de 404**: Detecta documentos no encontrados para reenvío
3. **Extracción de X-Error-Cause**: Obtiene mensajes de error detallados
4. **Logging completo**: Registra todas las operaciones para debugging
5. **Manejo de errores robusto**: Catch de excepciones con mensajes claros
6. **Estados consistentes**: Mapeo uniforme de estados de Hacienda

## Próximos Pasos Sugeridos

1. Actualizar el servicio de background para usar la nueva lógica de consulta
2. Implementar reenvío automático cuando se reciba estado `no_encontrado`
3. Agregar reintentos con backoff exponencial para errores 429 y 50x
4. Crear notificaciones al usuario sobre el estado final del documento
5. Guardar el XML de respuesta de Hacienda en la base de datos

## Testing

Para probar la implementación:

1. Enviar un documento a Hacienda
2. Esperar unos segundos
3. Consultar el estado del documento
4. Verificar que el estado se parsea correctamente
5. Probar con documentos que no existen (404)
6. Verificar el manejo de errores (token inválido, etc.)

## Notas Importantes

- El servicio usa `IHaciendaTokenService` para obtener tokens OAuth2 automáticamente
- Los tokens se refrescan automáticamente cuando expiran
- El XML de respuesta se guarda en Base64 en `HaciendaRespuesta.RespuestaXml`
- El método `ConsultarEstadoConTokenAsync` es el recomendado (usa OAuth2)
- El método `ConsultarEstadoAsync` (Basic auth) se mantiene por compatibilidad

## Compilación

El proyecto compila exitosamente sin errores:

```bash
cd /mnt/d/Proyectos/2/Facturacion/Facturacion.Backend
dotnet build
```

Build succeeded - 0 Error(s)
