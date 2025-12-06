# Implementación de Validación XSD v4.4 para Documentos Electrónicos

## Resumen

Se ha implementado exitosamente el servicio de validación XSD que valida los documentos XML generados contra los esquemas oficiales v4.4 de Hacienda de Costa Rica antes de enviarlos.

## Archivos Creados

### 1. Interfaz del Servicio
**Ubicación:** `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Services/Interfaces/IXsdValidacionService.cs`

Define los métodos principales:
- `ValidarXmlContraXsdAsync(string xml, DocumentoTipo tipoDocumento)` - Valida un XML contra el XSD correspondiente
- `ObtenerRutaXsd(DocumentoTipo tipoDocumento)` - Obtiene la ruta del archivo XSD según el tipo de documento
- `ValidarExistenciaEsquemasXsd()` - Verifica que todos los archivos XSD existan

### 2. Implementación del Servicio
**Ubicación:** `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Services/Implementations/XsdValidacionService.cs`

Características principales:
- Validación completa contra esquemas XSD v4.4
- Manejo de errores y advertencias de validación
- Soporte para todos los tipos de documentos electrónicos
- Logging detallado para diagnóstico
- Resolución automática de dependencias entre esquemas

### 3. DTO de Resultado
**Ubicación:** `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/DTOs/ResultadoValidacionXsd.cs`

Estructura del resultado:
```csharp
{
    EsValido: bool,
    Errores: List<string>,
    Advertencias: List<string>,
    RutaXsd: string,
    TipoDocumento: string,
    Mensaje: string
}
```

## Integración en el Flujo de Envío a Hacienda

El servicio de validación XSD se integró en el método `ProcesarYEnviarAsync` del `DocumentoHaciendaService`:

**Flujo de procesamiento actualizado:**
1. Obtener documento
2. Validar estado del documento
3. Validar datos de negocio
4. Generar clave numérica
5. Generar XML
6. **NUEVO: Validar XML contra XSD v4.4** ⬅️
7. Firmar XML digitalmente
8. Enviar a Hacienda

**Comportamiento:**
- Si el XML **NO es válido** según el XSD:
  - Se detiene el proceso
  - El documento pasa a estado `Error`
  - Se retornan los errores de validación
  - NO se envía a Hacienda

- Si el XML **es válido**:
  - Se continúa con la firma digital
  - Se envía a Hacienda normalmente
  - Las advertencias se registran en logs pero no bloquean el envío

## Mapeo de Tipos de Documento a Esquemas XSD

| Tipo de Documento | Archivo XSD |
|-------------------|-------------|
| FacturaElectronica | FacturaElectronica_V4.4.xsd |
| TiqueteElectronico | TiqueteElectronico_V4.4.xsd |
| NotaCreditoElectronica | NotaCreditoElectronica_V4.4.xsd |
| NotaDebitoElectronica | NotaDebitoElectronica_V4.4.xsd |
| FacturaElectronicaExportacion | FacturaElectronicaExportacion_V4.4.xsd |
| FacturaElectronicaCompra | FacturaElectronicaCompra_V4.4.xsd |
| NotaCreditoElectronicaCompra | NotaCreditoElectronica_V4.4.xsd |
| NotaDebitoElectronicaCompra | NotaDebitoElectronica_V4.4.xsd |
| ReciboElectronicoPago | ReciboElectronicoPago_V4.4.xsd |

## Ubicación de Archivos XSD

Los esquemas XSD deben estar en:
```
/mnt/d/Proyectos/2/Facturacion/4.4/
```

Archivos requeridos:
- FacturaElectronica_V4.4.xsd ✓
- TiqueteElectronico_V4.4.xsd ✓
- NotaCreditoElectronica_V4.4.xsd ✓
- NotaDebitoElectronica_V4.4.xsd ✓
- FacturaElectronicaExportacion_V4.4.xsd ✓
- FacturaElectronicaCompra_V4.4.xsd ✓
- ReciboElectronicoPago_V4.4.xsd ✓
- MensajeHacienda_V4.4.xsd ✓
- MensajeReceptor_V4.4.xsd ✓

## Registro en Dependency Injection

El servicio se registró en `Program.cs`:

```csharp
builder.Services.AddScoped<IXsdValidacionService, XsdValidacionService>();
```

## Endpoint de Diagnóstico

Se agregó un endpoint para verificar la disponibilidad de los esquemas XSD:

**Endpoint:** `GET /api/documentos/diagnostico-xsd`

**Respuesta:**
```json
{
  "exitoso": true,
  "mensaje": "Todos los esquemas XSD v4.4 están disponibles",
  "esquemasValidados": [
    "FacturaElectronica_V4.4.xsd",
    "TiqueteElectronico_V4.4.xsd",
    ...
  ]
}
```

## Logging

El servicio genera logs detallados en los siguientes niveles:

- **Information:** Validaciones exitosas, archivos XSD encontrados
- **Warning:** Advertencias de validación, archivos XSD faltantes
- **Error:** Errores de validación, problemas al cargar esquemas

Ejemplos de logs:
```
[Information] XsdValidacionService inicializado. Ruta base XSD: /mnt/d/Proyectos/2/Facturacion/4.4
[Information] Validando XML contra XSD: /mnt/d/Proyectos/2/Facturacion/4.4/FacturaElectronica_V4.4.xsd
[Information] Validación XSD exitosa para documento tipo FacturaElectronica
[Warning] Advertencia de validación XSD: Línea 45, Posición 12: ...
[Error] Validación XSD falló para documento {DocumentoId}. Errores: ...
```

## Manejo de Errores

### Errores de Validación XSD
Los errores incluyen información detallada:
- Número de línea y posición en el XML
- Descripción del error según el esquema
- Elemento o atributo que causó el error

Ejemplo:
```
Línea 156, Posición 23: El elemento 'CodigoCABYS' no es válido según su tipo de datos 'xs:string' - El valor debe tener exactamente 13 caracteres.
```

### Errores del Sistema
- Archivo XSD no encontrado → Documento pasa a estado Error
- Error al parsear XML → Se reporta el error de sintaxis
- Excepción durante validación → Se registra y el envío continúa (modo permisivo para evitar bloqueos completos)

## Casos de Uso

### 1. Validación Exitosa
```
Usuario envía documento → XML generado → Validación XSD ✓ → Firma digital → Envío a Hacienda
```

### 2. Validación Fallida
```
Usuario envía documento → XML generado → Validación XSD ✗ → Documento en estado Error
                                                         → Errores retornados al usuario
                                                         → NO se envía a Hacienda
```

### 3. Diagnóstico del Sistema
```
Administrador llama endpoint /diagnostico-xsd → Verifica archivos XSD → Retorna estado
```

## Ventajas de la Implementación

1. **Detección Temprana de Errores:** Los problemas se detectan antes de enviar a Hacienda, ahorrando tiempo y cuota de envíos
2. **Cumplimiento Normativo:** Garantiza que los XML cumplen exactamente con los estándares v4.4
3. **Mensajes Claros:** Los errores de validación son específicos y ayudan a corregir el problema
4. **Diagnóstico Fácil:** El endpoint de diagnóstico permite verificar rápidamente la configuración
5. **Logging Completo:** Todos los eventos quedan registrados para auditoría y debugging

## Configuración en Producción

Para desplegar en producción, asegúrese de:

1. **Copiar los archivos XSD** a la carpeta `4.4/` en el servidor
2. **Verificar permisos** de lectura en los archivos XSD
3. **Llamar al endpoint** `/api/documentos/diagnostico-xsd` para confirmar que todos los esquemas están disponibles
4. **Revisar los logs** durante las primeras validaciones para detectar problemas

## Soporte y Mantenimiento

### Actualización de Esquemas XSD
Cuando Hacienda publique una nueva versión de los esquemas:
1. Descargar los nuevos archivos XSD
2. Reemplazar en la carpeta `4.4/`
3. Reiniciar la aplicación
4. Ejecutar el endpoint de diagnóstico

### Troubleshooting

**Problema:** "No se encontró el archivo XSD"
- Verificar que la carpeta `4.4/` existe en la ubicación correcta
- Confirmar permisos de lectura
- Revisar la ruta en los logs

**Problema:** "Error al parsear el XML"
- El XML generado tiene un error de sintaxis
- Revisar el servicio XmlGeneradorService
- Verificar que los datos del documento son válidos

**Problema:** "Validación XSD falló"
- Leer los errores específicos retornados
- Corregir los datos del documento según los errores
- El problema está en los datos o en la lógica de generación del XML

## Conclusión

La validación XSD está completamente implementada e integrada en el flujo de envío a Hacienda. Proporciona una capa adicional de calidad y conformidad que reduce significativamente la probabilidad de rechazos por parte de Hacienda debido a errores de estructura o formato en los documentos XML.
