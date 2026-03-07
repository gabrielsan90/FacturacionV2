# Hacienda Module - Quick Start Guide

## Para Desarrolladores

### Enviar un Documento a Hacienda (Código de Ejemplo)

```csharp
// Inyectar el servicio en el constructor
private readonly IDocumentoHaciendaService _haciendaService;

public MyController(IDocumentoHaciendaService haciendaService)
{
    _haciendaService = haciendaService;
}

// Método para procesar y enviar
public async Task<IActionResult> EnviarAHacienda(Guid documentoId)
{
    // Procesar y enviar (todo en uno)
    var resultado = await _haciendaService.ProcesarYEnviarAsync(documentoId);

    if (resultado.Exitoso)
    {
        // Documento aceptado o en procesamiento
        Console.WriteLine($"Clave: {resultado.Clave}");
        Console.WriteLine($"Estado: {resultado.Estado}");
        return Ok(resultado);
    }
    else
    {
        // Documento rechazado o error
        Console.WriteLine($"Errores: {string.Join(", ", resultado.Errores)}");
        return BadRequest(resultado);
    }
}
```

### Consultar Estado

```csharp
public async Task<IActionResult> ConsultarEstado(Guid documentoId)
{
    var resultado = await _haciendaService.ConsultarEstadoAsync(documentoId);

    if (resultado.Exitoso)
    {
        Console.WriteLine($"Estado: {resultado.Estado}");
        return Ok(resultado);
    }

    return BadRequest(resultado);
}
```

### Generar XML sin Enviar (Preview)

```csharp
public async Task<IActionResult> GenerarXml(Guid documentoId)
{
    var xml = await _haciendaService.GenerarXmlAsync(documentoId);

    // Devolver XML para previsualización
    return Content(xml, "application/xml");
}
```

### Validar Antes de Enviar

```csharp
public async Task<IActionResult> Validar(Guid documentoId)
{
    var errores = await _haciendaService.ValidarDocumentoAsync(documentoId);

    if (!errores.Any())
    {
        return Ok(new { Valido = true, Mensaje = "Documento válido" });
    }

    return BadRequest(new { Valido = false, Errores = errores });
}
```

## Para el Frontend (Llamadas HTTP)

### 1. Procesar y Enviar

```javascript
async function enviarAHacienda(documentoId) {
    const response = await fetch(`/api/documentos/${documentoId}/procesar`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    const resultado = await response.json();

    if (resultado.exitoso) {
        console.log('Documento aceptado:', resultado.clave);
    } else {
        console.error('Errores:', resultado.errores);
    }
}
```

### 2. Consultar Estado

```javascript
async function consultarEstado(documentoId) {
    const response = await fetch(`/api/documentos/${documentoId}/consultar`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    const resultado = await response.json();
    console.log('Estado:', resultado.estado);
}
```

### 3. Validar Documento

```javascript
async function validarDocumento(documentoId) {
    const response = await fetch(`/api/documentos/${documentoId}/validar`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    const resultado = await response.json();

    if (resultado.valido) {
        console.log('✓ Documento válido');
    } else {
        console.warn('✗ Errores:', resultado.errores);
    }
}
```

### 4. Ver XML Generado

```javascript
async function verXml(documentoId) {
    const response = await fetch(`/api/documentos/${documentoId}/xml`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    const xml = await response.text();
    console.log(xml);
}
```

## Configurar Empresa para Hacienda

### Base de Datos (SQL)

```sql
UPDATE Empresas
SET
    UsuarioHacienda = 'cpj-3-123456789',
    ClaveHacienda = 'password_atv',
    Ambiente = 1, -- 1=Pruebas, 2=Producción
    CertificadoDigital = @certificadoBytes,
    PinCertificado = 'pin_del_certificado'
WHERE Id = @empresaId;
```

### Desde el Frontend

```javascript
async function configurarHacienda(empresaId, datos) {
    const response = await fetch(`/api/empresas/${empresaId}`, {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            usuarioHacienda: datos.usuarioATV,
            claveHacienda: datos.claveATV,
            ambiente: datos.ambiente, // 1=Pruebas, 2=Producción
            pinCertificado: datos.pinCertificado
        })
    });

    return response.json();
}

// Subir certificado digital
async function subirCertificado(empresaId, archivo) {
    const formData = new FormData();
    formData.append('certificado', archivo);

    const response = await fetch(`/api/empresas/${empresaId}/certificado`, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${token}`
        },
        body: formData
    });

    return response.json();
}
```

## Códigos de Estado de Hacienda

| Estado | Descripción | Acción |
|--------|-------------|--------|
| `aceptado` | Documento aprobado | Documento válido |
| `rechazado` | Documento con errores | Corregir y reenviar |
| `procesando` | En validación | Consultar después |
| `error` | Error de conexión | Reintentar |

## Campos Obligatorios v4.4

### Para Factura Electrónica (FE)

```javascript
{
    // Emisor
    actividadEconomica: "123456", // CIIU4 - 6 dígitos

    // Receptor
    receptorActividadEconomica: "654321", // NUEVO v4.4 - OBLIGATORIO
    receptorNombre: "Cliente SA",

    // Documento
    condicionVenta: "01", // 01=Contado, 02=Crédito
    medioPago: "01", // 01=Efectivo, 02=Tarjeta, 06=SINPE Móvil

    // Detalles
    detalles: [
        {
            codigoCabys: "1234567890123", // 13 dígitos (obligatorio desde 01/06/2025)
            cantidad: 1.000,
            precioUnitario: 100.00000,
            // ...
        }
    ]
}
```

## Errores Comunes y Soluciones

### Error 401 - No Autorizado
```
Causa: Usuario o clave ATV incorrectos
Solución: Verificar credenciales en Empresa
```

### Error 400 - Clave duplicada
```
Causa: La clave ya fue enviada
Solución: Regenerar clave (el sistema lo hace automáticamente)
```

### Error 400 - XML inválido
```
Causa: Estructura XML no cumple con XSD
Solución: Validar campos obligatorios, formatos de fecha, decimales
```

### Estado: Rechazado
```
Causa: Errores de validación de Hacienda
Solución: Revisar resultado.respuestaHacienda.mensajes
          Corregir errores
          Reenviar con /api/documentos/{id}/reenviar
```

## Formato de Números

```csharp
// Montos (5 decimales)
decimal total = 113.00000m;
string xml = $"<TotalComprobante>{total:F5}</TotalComprobante>";
// Output: <TotalComprobante>113.00000</TotalComprobante>

// Cantidades (3 decimales)
decimal cantidad = 1.000m;
string xml = $"<Cantidad>{cantidad:F3}</Cantidad>";
// Output: <Cantidad>1.000</Cantidad>

// Fechas (ISO 8601 con zona horaria)
DateTime fecha = DateTime.Now;
string xml = $"<FechaEmision>{fecha:yyyy-MM-ddTHH:mm:sszzz}</FechaEmision>";
// Output: <FechaEmision>2025-01-15T10:30:00-06:00</FechaEmision>
```

## Testing con Postman

### 1. Procesar y Enviar

```
POST http://localhost:7030/api/documentos/{id}/procesar
Authorization: Bearer {token}
```

### 2. Consultar Estado

```
GET http://localhost:7030/api/documentos/{id}/consultar
Authorization: Bearer {token}
```

### 3. Ver XML

```
GET http://localhost:7030/api/documentos/{id}/xml
Authorization: Bearer {token}
```

## Logs y Debugging

El sistema registra logs en:
- Generación de clave
- Generación de XML
- Firma digital
- Envío a Hacienda
- Respuesta de Hacienda

Buscar en logs por:
```
- "Procesando y enviando documento"
- "Documento aceptado por Hacienda"
- "Documento rechazado por Hacienda"
- "Error al procesar documento"
```

## Checklist de Implementación

- [ ] Configurar credenciales ATV en Empresa
- [ ] Subir certificado digital (.p12/.pfx)
- [ ] Configurar PIN del certificado
- [ ] Seleccionar ambiente (Pruebas/Producción)
- [ ] Crear documento de prueba
- [ ] Validar documento
- [ ] Procesar y enviar a Hacienda (sandbox)
- [ ] Verificar respuesta "aceptado"
- [ ] Probar consulta de estado
- [ ] Probar reenvío de documento rechazado
- [ ] Migrar a producción

## Contacto de Soporte

**Hacienda Costa Rica:**
- Portal: https://www.hacienda.go.cr/facturae
- Teléfono: 800-HACIENDA

**Documentación Técnica:**
- Archivo: `/HACIENDA_MODULE_IMPLEMENTATION.md`
- XSD Schemas: https://www.hacienda.go.cr/facturae

---

**Última actualización:** 2025-11-22
