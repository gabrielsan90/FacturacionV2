# Mejoras de Consulta Automática Implementadas

## Fecha
2026-02-10

## Problema Original
Los documentos se quedaban en estado "Procesando" hasta que el usuario hacía clic manualmente en "Consultar a Hacienda".

## Solución Implementada

### 1. Consulta Inmediata con Reintentos Progresivos

Se implementó un sistema de consulta inmediata después del envío que:

- **Inicia automáticamente** después de enviar un documento a Hacienda
- **Reintentos progresivos** con intervalos crecientes: 5s, 10s, 20s, 40s, 60s
- **Total de tiempo de reintentos**: ~2.5 minutos
- **Detección automática** de estados finales (aceptado/rechazado)
- **Fallback al BackgroundService** si no se obtiene respuesta en los reintentos

#### Ventajas
1. El usuario obtiene la respuesta de Hacienda en segundos (en lugar de esperar hasta 30s)
2. Reduce la carga del BackgroundService al resolver la mayoría de casos inmediatamente
3. No interfiere con el BackgroundService existente (actúa como complemento)
4. Maneja errores gracefully sin afectar el envío del documento

### 2. Logging Mejorado

Se agregó logging detallado en el BackgroundService:

```csharp
_logger.LogInformation(
    "Consultando estado en Hacienda para documento {Clave} (ID: {DocumentoId}). " +
    "FechaEnvio: {FechaEnvio}, TiempoEnProceso: {TiempoMinutos} minutos, " +
    "FechaRespuesta: {FechaRespuesta}",
    documento.Clave,
    documento.Id,
    documento.FechaEnvioHacienda,
    tiempoEnProceso.TotalMinutes,
    documento.FechaRespuestaHacienda);
```

#### Beneficios
- Permite diagnosticar problemas fácilmente
- Muestra tiempo en proceso de cada documento
- Identifica documentos "atascados"

### 3. Manejo de Errores Específicos

Se implementaron catch específicos para diferentes tipos de errores:

#### HttpRequestException
```csharp
catch (HttpRequestException ex)
{
    _logger.LogError(ex,
        "Error HTTP al consultar documento {Clave} (ID: {DocumentoId}): {Message}. " +
        "Posible problema de conectividad con Hacienda",
        documento.Clave, documento.Id, ex.Message);
}
```

#### TaskCanceledException (Timeout)
```csharp
catch (TaskCanceledException ex)
{
    _logger.LogError(ex,
        "Timeout al consultar documento {Clave} (ID: {DocumentoId}). " +
        "Hacienda no respondió a tiempo",
        documento.Clave, documento.Id);
}
```

#### InvalidOperationException (Credenciales)
```csharp
catch (InvalidOperationException ex) when (ex.Message.Contains("token") || ex.Message.Contains("credentials"))
{
    _logger.LogError(ex,
        "Error de autenticación al consultar documento {Clave} (ID: {DocumentoId}): {Message}. " +
        "Verifique las credenciales de Hacienda de la empresa",
        documento.Clave, documento.Id, ex.Message);
}
```

#### Beneficios
- Mensajes de error claros y accionables
- Permite identificar la causa raíz rápidamente
- Facilita el soporte y debugging

## Archivos Modificados

### 1. DocumentoHaciendaService.cs

**Ubicación:** `/Facturacion.Backend/Services/Implementations/DocumentoHaciendaService.cs`

**Cambios:**
- Líneas 279-284: Agregado inicio de consultas automáticas con reintentos
- Líneas 843-939: Nuevo método `ConsultarEstadoConReintentosAsync`

**Código agregado:**
```csharp
// En ProcesarYEnviarAsync, después de enviar con éxito
if (estadoRespuesta == "enviado" || estadoRespuesta == "procesando")
{
    // ... código existente ...

    // MEJORA: Iniciar consultas inmediatas en background con reintentos progresivos
    _ = Task.Run(async () =>
    {
        await ConsultarEstadoConReintentosAsync(documentoId);
    });
}
```

### 2. DocumentoEnvioBackgroundService.cs

**Ubicación:** `/Facturacion.Backend/Services/BackgroundServices/DocumentoEnvioBackgroundService.cs`

**Cambios:**
- Líneas 195-209: Logging detallado con cálculo de tiempo en proceso
- Líneas 328-354: Manejo de errores específicos con logging mejorado

## Flujo de Consulta Mejorado

### Escenario Normal (Hacienda responde rápido)

```
1. Usuario envía documento
2. Backend envía a Hacienda → HTTP 201/202 (Procesando)
3. Documento cambia a estado "Procesando"
4. Se inicia Task.Run con ConsultarEstadoConReintentosAsync
   ├── Espera 5 segundos
   ├── Consulta estado → "Aceptado"
   └── Actualiza documento y finaliza
5. Usuario ve el documento "Aceptado" en ~5 segundos
6. BackgroundService (30s) no necesita hacer nada (ya está aceptado)
```

### Escenario con Reintentos (Hacienda tarda en responder)

```
1. Usuario envía documento
2. Backend envía a Hacienda → HTTP 201/202 (Procesando)
3. Documento cambia a estado "Procesando"
4. Se inicia Task.Run con ConsultarEstadoConReintentosAsync
   ├── Espera 5s → Consulta → "Procesando"
   ├── Espera 10s → Consulta → "Procesando"
   ├── Espera 20s → Consulta → "Aceptado"
   └── Actualiza documento y finaliza
5. Usuario ve el documento "Aceptado" en ~35 segundos
6. BackgroundService (30s) puede haber consultado también (sin problema)
```

### Escenario de Error (Sin conectividad)

```
1. Usuario envía documento
2. Backend envía a Hacienda → HTTP 201/202 (Procesando)
3. Documento cambia a estado "Procesando"
4. Se inicia Task.Run con ConsultarEstadoConReintentosAsync
   ├── Espera 5s → Consulta → HttpRequestException
   ├── Espera 10s → Consulta → HttpRequestException
   ├── Espera 20s → Consulta → HttpRequestException
   ├── Espera 40s → Consulta → HttpRequestException
   ├── Espera 60s → Consulta → HttpRequestException
   └── Finaliza después de 5 intentos (log registrado)
5. BackgroundService toma el relevo cada 30s hasta obtener respuesta
```

## Estrategia de Reintentos

Los intervalos fueron seleccionados basándose en:

1. **5 segundos**: Hacienda normalmente responde entre 3-10 segundos
2. **10 segundos**: Para documentos complejos que tardan más
3. **20 segundos**: Backoff exponencial moderado
4. **40 segundos**: Casos de alta carga en Hacienda
5. **60 segundos**: Último intento antes de delegar al BackgroundService

**Total acumulado:** 5 + 10 + 20 + 40 + 60 = 135 segundos (~2.5 minutos)

Después de 2.5 minutos, el BackgroundService toma el control y continúa consultando cada 30 segundos.

## Monitoreo y Logs

### Logs Exitosos

```
[INFO] Documento {guid} enviado exitosamente a Hacienda (HTTP 201/202) - será consultado automáticamente
[INFO] Iniciando consultas automáticas con reintentos progresivos para documento {guid}
[INFO] Intento 1/5: Consultando estado de documento {guid}
[INFO] Documento {guid} obtuvo estado final 'Aceptado' en el intento 1. Finalizando reintentos automáticos
```

### Logs con Reintentos

```
[INFO] Intento 1/5: Consultando estado de documento {guid}
[DEBUG] Documento {guid} aún en estado 'Procesando'. Continuando reintentos...
[INFO] Intento 2/5: Consultando estado de documento {guid}
[INFO] Documento {guid} obtuvo estado final 'Aceptado' en el intento 2. Finalizando reintentos automáticos
```

### Logs de Error

```
[WARN] Error HTTP en intento 1/5 para documento {guid}: Connection timeout. Continuando con siguientes reintentos...
[WARN] Timeout en intento 2/5 para documento {guid}. Continuando con siguientes reintentos...
[INFO] Finalizados los 5 reintentos automáticos para documento {guid}. El BackgroundService continuará consultando cada 30 segundos
```

## Ventajas del Enfoque Implementado

### 1. Doble Capa de Seguridad
- **Capa 1:** Reintentos inmediatos (rápidos, específicos del documento)
- **Capa 2:** BackgroundService (lento, procesa múltiples documentos)

### 2. Experiencia de Usuario Mejorada
- Respuesta inmediata en la mayoría de casos (5-35 segundos)
- No requiere acción manual del usuario
- Feedback en tiempo real

### 3. Resiliencia
- Maneja errores de red sin afectar el envío
- Continúa intentando hasta obtener respuesta
- No depende de un solo mecanismo

### 4. Eficiencia
- Reduce carga del BackgroundService
- Consultas específicas solo para documentos nuevos
- BackgroundService solo procesa documentos "atascados"

### 5. Observabilidad
- Logs detallados en cada paso
- Métricas de tiempo de respuesta
- Identificación de problemas de conectividad

## Consideraciones de Rendimiento

### Consumo de Recursos

**Task.Run por documento:**
- **Memoria:** Mínima (~KB por task)
- **CPU:** Mínima (mayoría del tiempo en await)
- **Red:** 5 requests HTTP máximo por documento

**BackgroundService:**
- **Sin cambios:** Sigue procesando cada 30 segundos
- **Carga reducida:** Menos documentos en cola (ya fueron resueltos por reintentos)

### Concurrencia

- Las consultas inmediatas son independientes (no bloquean)
- El BackgroundService puede consultar el mismo documento (sin problema, es idempotente)
- Si ambos consultan simultáneamente, el último que guarde gana (estado correcto de todas formas)

## Testing Recomendado

### Test 1: Respuesta Rápida de Hacienda
1. Enviar documento a Hacienda (staging)
2. Verificar logs: debe aparecer "Intento 1/5"
3. Verificar que el documento cambia a "Aceptado" en ~5 segundos
4. Verificar que los reintentos se detienen después del primer intento exitoso

### Test 2: Respuesta Lenta de Hacienda
1. Enviar documento cuando Hacienda está bajo carga
2. Verificar logs: deben aparecer múltiples intentos
3. Verificar que eventualmente se obtiene respuesta
4. Medir tiempo total hasta obtener respuesta final

### Test 3: Error de Conectividad
1. Desconectar red después de enviar documento
2. Verificar logs de error HTTP
3. Reconectar red
4. Verificar que el BackgroundService toma el control
5. Verificar que eventualmente se obtiene respuesta

### Test 4: Múltiples Documentos Simultáneos
1. Enviar 10 documentos simultáneamente
2. Verificar que todos inician sus propios reintentos
3. Verificar que no hay errores de concurrencia
4. Verificar que todos obtienen respuesta final

## Rollback (si es necesario)

Si surge algún problema con los reintentos automáticos:

### Opción 1: Deshabilitar solo los reintentos inmediatos

Comentar las líneas 279-284 en `DocumentoHaciendaService.cs`:

```csharp
// MEJORA: Iniciar consultas inmediatas en background con reintentos progresivos
// _ = Task.Run(async () =>
// {
//     await ConsultarEstadoConReintentosAsync(documentoId);
// });
```

El BackgroundService seguirá funcionando normalmente.

### Opción 2: Ajustar intervalos de reintento

Modificar el array de intervalos en línea 852:

```csharp
// Más agresivo (más rápido)
var intervalos = new[] { 3, 5, 10, 15, 20 }; // segundos

// Más conservador (menos carga)
var intervalos = new[] { 10, 20, 40, 60, 120 }; // segundos
```

## Próximos Pasos Sugeridos

1. **Monitorear logs en producción** durante 1 semana
2. **Analizar métricas:**
   - Tiempo promedio hasta obtener respuesta
   - Porcentaje de documentos resueltos en primer intento
   - Tasa de errores HTTP
3. **Ajustar intervalos** basándose en métricas reales
4. **Implementar dashboard** con estadísticas de consultas
5. **Agregar alertas** para documentos que no obtienen respuesta en X tiempo

## Referencias

- Documentación de Hacienda: Tiempos de respuesta esperados (3-10 segundos típicamente)
- Patrón de Reintentos: Exponential Backoff
- Background Services: ASP.NET Core Hosted Services

## Estado

- [x] Implementado
- [x] Compilación exitosa
- [ ] Testing en staging
- [ ] Testing en producción
- [ ] Monitoreo de métricas
- [ ] Ajuste de intervalos basado en datos reales
