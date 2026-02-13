# Diagnóstico: Consulta Automática de Documentos a Hacienda

## Fecha
2026-02-10

## Problema Reportado
Los documentos se quedan en estado "Procesando" indefinidamente hasta que el usuario hace clic manualmente en "Consultar a Hacienda".

## Análisis del Código Actual

### 1. Background Service Configurado ✓

El `DocumentoEnvioBackgroundService` está:
- **Registrado en DI:** Sí (Program.cs:291)
- **Intervalo de ejecución:** 30 segundos
- **Método de consulta:** `VerificarDocumentosEnProcesoAsync`

### 2. Lógica de Consulta Automática ✓

El método `VerificarDocumentosEnProcesoAsync` (líneas 160-322):

```csharp
// Obtener documentos en proceso:
// - Sin respuesta final (FechaRespuestaHacienda == null)
// - O que llevan más de 5 minutos en Procesando (por si acaso)
var documentosEnProceso = await context.Documentos
    .Include(d => d.Empresa)
    .Where(d => !d.IsDeleted &&
               d.Estado == EstadoDocumento.Procesando &&
               d.FechaEnvioHacienda != null &&
               (d.FechaRespuestaHacienda == null || d.FechaEnvioHacienda < tiempoLimite))
    .OrderBy(d => d.FechaEnvioHacienda)
    .Take(10)
    .ToListAsync(stoppingToken);
```

**Criterios de consulta:**
- Estado = "Procesando"
- FechaEnvioHacienda != null
- FechaRespuestaHacienda == null (respuesta pendiente)
- O FechaEnvioHacienda < 5 minutos atrás (documentos "atascados")

### 3. Actualización de Estado ✓

El servicio actualiza correctamente el estado según la respuesta:

```csharp
switch (estado.Estado.ToLowerInvariant())
{
    case "aceptado":
        documento.Estado = EstadoDocumento.Aceptado;
        documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;
        // Envío automático de correo en producción
        break;

    case "rechazado":
        documento.Estado = EstadoDocumento.Rechazado;
        documento.FechaRespuestaHacienda = FechaCostaRicaHelper.Ahora;
        break;

    case "procesando":
    case "enviado":
        // Todavía en proceso, NO establecer FechaRespuestaHacienda
        documento.MensajeHacienda = "Documento aún en proceso de validación por Hacienda";
        break;
}
```

## Posibles Causas del Problema

### Causa 1: BackgroundService No Está Corriendo
**Probabilidad:** BAJA
**Verificación:**
- Revisar logs del servidor al iniciar
- Buscar: "DocumentoEnvioBackgroundService iniciado"

**Solución:**
- Verificar que el servicio está registrado en Program.cs
- Reiniciar el backend

### Causa 2: FechaRespuestaHacienda Se Establece Prematuramente
**Probabilidad:** MEDIA
**Verificación:**
- Revisar en BD si documentos en "Procesando" tienen FechaRespuestaHacienda != null
- Si la tienen, el BackgroundService no los consultará

**Solución:**
- Verificar que `DocumentoHaciendaService.ProcesarYEnviarAsync` NO establece FechaRespuestaHacienda para estado "enviado" o "procesando"
- Código actual es correcto (líneas 267-278)

### Causa 3: Error en ConsultarEstadoAsync
**Probabilidad:** ALTA ⚠️
**Verificación:**
- Revisar logs del BackgroundService
- Buscar errores en método `ConsultarEstadoAsync`

**Problema Potencial:**
El método `ConsultarEstadoAsync` puede estar lanzando excepciones que se capturan silenciosamente.

### Causa 4: Hacienda No Responde con Estado Final
**Probabilidad:** MEDIA
**Verificación:**
- Revisar si Hacienda realmente está procesando el documento
- Algunos documentos pueden quedarse en "procesando" en Hacienda por horas

**Solución:**
- Implementar timeout más largo (actualmente 5 minutos)
- Agregar alerta para documentos en "Procesando" por más de X horas

## Mejoras Sugeridas

### Mejora 1: Logging Más Detallado

Agregar logs para cada intento de consulta:

```csharp
_logger.LogInformation(
    "Consultando estado para documento {Clave} (ID: {DocumentoId}). " +
    "FechaEnvio: {FechaEnvio}, FechaRespuesta: {FechaRespuesta}",
    documento.Clave,
    documento.Id,
    documento.FechaEnvioHacienda,
    documento.FechaRespuestaHacienda);
```

### Mejora 2: Manejo de Errores Más Robusto

Capturar y registrar errores específicos:

```csharp
catch (HttpRequestException ex)
{
    _logger.LogError(ex,
        "Error HTTP al consultar documento {Clave}: {Message}",
        documento.Clave, ex.Message);
}
catch (TaskCanceledException ex)
{
    _logger.LogError(ex,
        "Timeout al consultar documento {Clave}",
        documento.Clave);
}
```

### Mejora 3: Alerta para Documentos Atascados

Notificar cuando un documento lleva más de X horas en "Procesando":

```csharp
var horasEnProceso = (FechaCostaRicaHelper.Ahora - documento.FechaEnvioHacienda.Value).TotalHours;

if (horasEnProceso > 2)
{
    _logger.LogWarning(
        "Documento {Clave} lleva {Horas} horas en estado Procesando",
        documento.Clave, horasEnProceso);

    // Enviar notificación al usuario
}
```

### Mejora 4: Reintentos con Backoff Exponencial

Para errores temporales de Hacienda:

```csharp
private async Task<ResultadoConsulta> ConsultarConReintentosAsync(
    Guid documentoId,
    int maxReintentos = 3)
{
    for (int intento = 1; intento <= maxReintentos; intento++)
    {
        try
        {
            return await ConsultarEstadoAsync(documentoId);
        }
        catch (HttpRequestException ex) when (intento < maxReintentos)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, intento));
            _logger.LogWarning(
                "Intento {Intento}/{Max} falló. Reintentando en {Delay}s",
                intento, maxReintentos, delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }

    throw new InvalidOperationException("Máximo de reintentos alcanzado");
}
```

### Mejora 5: Consulta Inmediata Después de Envío

En lugar de esperar 30 segundos, consultar inmediatamente después de enviar:

```csharp
// En DocumentoHaciendaService.ProcesarYEnviarAsync
if (estadoRespuesta == "enviado" || estadoRespuesta == "procesando")
{
    documento.Estado = EstadoDocumento.Procesando;
    // ...
    await _context.SaveChangesAsync();

    // Consultar inmediatamente después de 5 segundos
    _ = Task.Run(async () =>
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        try
        {
            await ConsultarEstadoAsync(documentoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en consulta inmediata");
        }
    });
}
```

## Plan de Acción Recomendado

### Paso 1: Diagnóstico (5 minutos)
1. Revisar logs del backend al iniciar
2. Verificar que aparece "DocumentoEnvioBackgroundService iniciado"
3. Revisar logs de consultas automáticas

### Paso 2: Verificación en BD (5 minutos)
```sql
-- Documentos en Procesando con FechaRespuestaHacienda
SELECT
    Id,
    Clave,
    NumeroConsecutivo,
    Estado,
    FechaEnvioHacienda,
    FechaRespuestaHacienda,
    MensajeHacienda
FROM Documentos
WHERE Estado = 2 -- EstadoDocumento.Procesando
ORDER BY FechaEnvioHacienda DESC;
```

### Paso 3: Agregar Logging Detallado (10 minutos)
- Modificar `VerificarDocumentosEnProcesoAsync` para agregar más logs
- Reiniciar backend y verificar logs

### Paso 4: Implementar Mejora de Consulta Inmediata (15 minutos)
- Agregar consulta inmediata después de envío
- Probar con un documento nuevo

### Paso 5: Implementar Alertas (10 minutos)
- Notificar documentos en "Procesando" por más de 2 horas
- Enviar notificación al usuario

## Comandos de Verificación

### Verificar si el BackgroundService está corriendo
```bash
# En logs del backend buscar:
grep "DocumentoEnvioBackgroundService" logs.txt

# Debe aparecer:
# DocumentoEnvioBackgroundService iniciado
# Verificando estado de X documentos en proceso
```

### Verificar documentos en Procesando
```sql
SELECT COUNT(*) as TotalProcesando
FROM Documentos
WHERE Estado = 2 AND IsDeleted = 0;
```

### Verificar documentos con respuesta pendiente
```sql
SELECT COUNT(*) as PendientesRespuesta
FROM Documentos
WHERE Estado = 2
  AND FechaEnvioHacienda IS NOT NULL
  AND FechaRespuestaHacienda IS NULL
  AND IsDeleted = 0;
```

## Conclusión

El sistema **SÍ tiene implementada la consulta automática**, pero puede estar fallando por:

1. Errores en la comunicación con Hacienda
2. Problemas de autenticación (token OAuth2)
3. Documentos que realmente están en "procesando" en Hacienda
4. Falta de logging para diagnosticar el problema

**Recomendación:** Implementar primero las mejoras de logging para diagnosticar el problema real, y luego implementar la consulta inmediata después de envío para mejorar la experiencia del usuario.

## Estado de Implementación

- [x] Background Service registrado
- [x] Consulta automática cada 30 segundos
- [x] Actualización automática de estado
- [x] Envío automático de correo (producción)
- [ ] Logging detallado de consultas
- [ ] Consulta inmediata después de envío
- [ ] Alertas para documentos atascados
- [ ] Reintentos con backoff exponencial
