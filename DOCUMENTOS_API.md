# API de Documentos Electrónicos - Hacienda v4.4

## Descripción General

Esta API proporciona endpoints completos para la gestión de documentos electrónicos según la especificación v4.4 del Ministerio de Hacienda de Costa Rica (Resolución MH-DGT-RES-0027-2024).

La API maneja todo el ciclo de vida de los documentos electrónicos:
1. Creación de documentos (con generación automática de consecutivos y cálculo de totales)
2. Generación de XML según esquemas Hacienda
3. Firma digital con certificados XAdES-BES
4. Envío a la API de Hacienda
5. Consulta de estado y respuestas
6. Descarga de XMLs firmados y PDFs

## Autenticación

Todos los endpoints requieren autenticación JWT:

```http
Authorization: Bearer {token}
```

## Endpoints Principales

### 1. Crear Documento

**POST** `/api/Documentos`

Crea un nuevo documento electrónico usando DTOs. Este endpoint:
- Genera automáticamente el número consecutivo
- Calcula todos los totales (subtotales, impuestos, descuentos)
- Valida el documento según reglas de Hacienda v4.4
- Guarda el documento en estado `Borrador`

**Request Body:**
```json
{
  "empresaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sucursalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "terminalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tipoDocumento": 1,
  "actividadEconomica": "620100",
  "fechaEmision": "2025-01-23T10:30:00",
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "receptorActividadEconomica": "471101",
  "condicionVenta": "01",
  "medioPago": "01",
  "moneda": 1,
  "detalles": [
    {
      "numeroLinea": 1,
      "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "codigoCabys": "1010101010101",
      "descripcion": "Producto de prueba",
      "cantidad": 2,
      "unidadMedidaId": 1,
      "precioUnitario": 10000.00,
      "impuestos": [
        {
          "codigoTarifa": "08",
          "tarifa": 13.00,
          "monto": 2600.00
        }
      ]
    }
  ]
}
```

**Response:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clave": null,
  "numeroConsecutivo": "001-00001-01-0000000001",
  "tipoDocumento": 1,
  "estado": 1,
  "fechaEmision": "2025-01-23T10:30:00",
  "subtotal": 20000.00,
  "totalImpuestos": 2600.00,
  "totalVenta": 22600.00
}
```

**Validaciones Automáticas:**
- Actividad económica del emisor (6 dígitos CIIU4)
- Actividad económica del receptor (obligatoria en FE desde v4.4)
- Al menos un detalle con descripción y cantidad
- Código CAByS (13 dígitos, obligatorio desde 01/06/2025)
- Referencias obligatorias en NC/ND
- Plazo de crédito si condición de venta es "02"
- Tipo de cambio si moneda != CRC

### 2. Obtener Documento por ID

**GET** `/api/Documentos/{id}`

Obtiene un documento con todos sus detalles, impuestos, descuentos y referencias.

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clave": "50601231120210001001000100010000000013218203498",
  "numeroConsecutivo": "001-00001-01-0000000001",
  "tipoDocumento": 1,
  "estado": 4,
  "empresa": { ... },
  "sucursal": { ... },
  "terminal": { ... },
  "cliente": { ... },
  "detalles": [ ... ],
  "totalVenta": 22600.00
}
```

### 3. Listar Documentos de una Empresa

**GET** `/api/Documentos/empresa/{empresaId}`

Obtiene todos los documentos de una empresa.

**Parámetros opcionales:**
- `estado`: Filtrar por estado (1-7)
- `fechaInicio`: Fecha inicial del rango
- `fechaFin`: Fecha final del rango

**Response:** `200 OK`
```json
[
  {
    "id": "...",
    "numeroConsecutivo": "001-00001-01-0000000001",
    "fechaEmision": "2025-01-23T10:30:00",
    "estado": 4,
    "totalVenta": 22600.00
  }
]
```

### 4. Obtener Siguiente Consecutivo

**GET** `/api/Documentos/consecutivo/{terminalId}/siguiente?tipoDocumento=01`

Obtiene el próximo número consecutivo disponible sin incrementar el contador.

**Response:** `200 OK`
```json
{
  "terminalId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tipoDocumento": "01",
  "siguienteConsecutivo": "001-00001-01-0000000042"
}
```

### 5. Procesar y Enviar a Hacienda

**POST** `/api/Documentos/{id}/procesar`

Ejecuta el proceso completo de Hacienda:
1. Genera la Clave numérica (50 dígitos)
2. Genera el XML según esquemas v4.4
3. Firma digitalmente el XML (XAdES-BES)
4. Envía a la API de Hacienda
5. Procesa la respuesta

**Response:** `200 OK`
```json
{
  "exitoso": true,
  "mensaje": "Documento aceptado exitosamente por Hacienda",
  "clave": "50601231120210001001000100010000000013218203498",
  "estado": "Aceptado",
  "xmlGenerado": "<FacturaElectronica>...</FacturaElectronica>",
  "xmlFirmado": "<FacturaElectronica>...<Signature>...</Signature></FacturaElectronica>",
  "respuestaHacienda": {
    "indEstado": "aceptado",
    "mensajes": []
  }
}
```

**Posibles Estados de Respuesta:**
- `Aceptado`: Documento validado y aceptado por Hacienda
- `Rechazado`: Documento rechazado con lista de errores
- `Procesando`: Enviado correctamente, en proceso de validación

**Errores Comunes:**
```json
{
  "exitoso": false,
  "mensaje": "El documento tiene errores de validación",
  "errores": [
    "Falta la actividad económica del receptor (obligatorio en v4.4)",
    "El código CAByS debe tener 13 dígitos"
  ]
}
```

### 6. Consultar Estado en Hacienda

**GET** `/api/Documentos/{id}/consultar`

Consulta el estado actual del documento en Hacienda y actualiza la base de datos.

**Response:** `200 OK`
```json
{
  "exitoso": true,
  "mensaje": "Estado actual: aceptado",
  "clave": "50601231120210001001000100010000000013218203498",
  "estado": "aceptado",
  "fechaConsulta": "2025-01-23T15:45:00",
  "respuestaHacienda": {
    "indEstado": "aceptado",
    "mensajes": []
  }
}
```

### 7. Reenviar Documento

**POST** `/api/Documentos/{id}/reenviar`

Reenvía un documento rechazado o en contingencia a Hacienda.

**Condiciones:**
- Solo documentos en estado `Rechazado` o `Contingencia`
- Mantiene la misma Clave y XML

**Response:** `200 OK`
```json
{
  "exitoso": true,
  "mensaje": "Documento reenviado exitosamente"
}
```

### 8. Validar Documento

**GET** `/api/Documentos/{id}/validar`

Valida el documento según todas las reglas de Hacienda v4.4 sin enviarlo.

**Response:** `200 OK`
```json
{
  "valido": true,
  "mensaje": "El documento es válido y puede ser enviado a Hacienda"
}
```

O si hay errores:
```json
{
  "valido": false,
  "errores": [
    "La actividad económica del receptor es obligatoria para Facturas Electrónicas (v4.4)",
    "La línea 2 debe tener código CAByS (obligatorio desde 01/06/2025)"
  ]
}
```

### 9. Generar XML (Sin Enviar)

**GET** `/api/Documentos/{id}/xml`

Genera el XML del documento para previsualización o testing, sin enviarlo a Hacienda.

**Response:** `200 OK`
```xml
Content-Type: application/xml

<?xml version="1.0" encoding="utf-8"?>
<FacturaElectronica xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica">
  <Clave>50601231120210001001000100010000000013218203498</Clave>
  <NumeroConsecutivo>001-00001-01-0000000001</NumeroConsecutivo>
  ...
</FacturaElectronica>
```

### 10. Descargar XML Firmado

**GET** `/api/Documentos/{id}/descargar-xml`

Descarga el XML firmado digitalmente.

**Response:** `200 OK`
```
Content-Type: application/xml
Content-Disposition: attachment; filename="00100010010000000001.xml"
```

### 11. Anular Documento

**POST** `/api/Documentos/{id}/anular`

Anula un documento previamente aceptado.

**Request Body (opcional):**
```json
{
  "motivo": "Error en precio, se emitirá nueva factura"
}
```

**Condiciones:**
- Solo documentos en estado `Aceptado`
- Cambia el estado a `Anulado`
- TODO: Genera Mensaje Receptor (tipo 05) automáticamente

**Response:** `200 OK`
```json
{
  "mensaje": "Documento anulado exitosamente",
  "documentoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "estado": 7
}
```

### 12. Actualizar Documento

**PUT** `/api/Documentos/{id}`

Actualiza un documento existente. Solo permitido en estado `Borrador`.

**Request Body:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "observaciones": "Actualización de datos del cliente",
  "receptorEmails": "cliente@example.com,facturacion@example.com"
}
```

**Response:** `204 No Content`

### 13. Eliminar Documento (Soft Delete)

**DELETE** `/api/Documentos/{id}`

Elimina lógicamente un documento. Solo permitido en estado `Borrador`.

**Response:** `204 No Content`

## Estados del Documento

| Código | Estado | Descripción |
|--------|--------|-------------|
| 1 | Borrador | Documento en edición, no enviado a Hacienda |
| 2 | Pendiente | Documento generado, listo para enviar |
| 3 | Procesando | Enviado a Hacienda, esperando respuesta |
| 4 | Aceptado | Aceptado por Hacienda (estado final) |
| 5 | Rechazado | Rechazado por Hacienda |
| 6 | Contingencia | Generado en modo contingencia (sin conexión) |
| 7 | Anulado | Documento cancelado |

## Tipos de Documento

| Código | Tipo | Descripción |
|--------|------|-------------|
| 01 | FE | Factura Electrónica |
| 02 | ND | Nota de Débito Electrónica |
| 03 | NC | Nota de Crédito Electrónica |
| 04 | TE | Tiquete Electrónico |
| 08 | FEC | Factura Electrónica de Compra |
| 09 | FEE | Factura Electrónica de Exportación |
| 10 | REP | Recibo Electrónico de Pago (NUEVO v4.4) |

## Condiciones de Venta

| Código | Descripción |
|--------|-------------|
| 01 | Contado |
| 02 | Crédito (requiere PlazoCreditoDias) |
| 03 | Consignación |
| 04 | Apartado |
| 05 | Arrendamiento con opción de compra |
| 06 | Arrendamiento en función financiera |
| 07 | Cobro a favor de un tercero |
| 08 | Servicios prestados al Estado a crédito |
| 09 | Pago de servicios prestados al Estado |
| 10 | Mercancía no nacionalizada (NUEVO v4.4) |
| 99 | Otros |

## Medios de Pago

| Código | Descripción |
|--------|-------------|
| 01 | Efectivo |
| 02 | Tarjeta |
| 03 | Cheque |
| 04 | Transferencia - depósito bancario |
| 05 | Recaudado por terceros |
| 06 | SINPE Móvil (NUEVO v4.4) |
| 99 | Otros |

## Cambios Importantes v4.4

### Obligatorios desde 01/04/2025
- **actividadEconomicaReceptor**: Obligatoria en Facturas Electrónicas (FE)
- **SINPE Móvil**: Nuevo medio de pago (código 06)
- **Emails múltiples**: Hasta 4 direcciones separadas por coma

### Obligatorios desde 01/06/2025
- **CAByS 2025**: Código de 13 dígitos obligatorio en todas las líneas de detalle
- **CIIU4**: Códigos de actividad económica de 6 dígitos (reemplaza CIIU3)

### Obligatorios desde 01/12/2024
- **Productos farmacéuticos**: NumeroRegistroMedicamento y FormaFarmaceutica
- **Vehículos**: Número VIN obligatorio

### Nuevo Documento: REP (tipo 10)
- Recibo Electrónico de Pago
- Requerido para ventas a crédito con IVA (plazo hasta 90 días)
- Registra pagos parciales o totales sobre facturas a crédito

## Flujo de Trabajo Completo

```
1. Crear Documento (POST /api/Documentos)
   └─> Estado: Borrador
   └─> Consecutivo generado automáticamente
   └─> Totales calculados

2. Validar (GET /api/Documentos/{id}/validar)
   └─> Verifica reglas de Hacienda v4.4

3. Procesar y Enviar (POST /api/Documentos/{id}/procesar)
   ├─> Genera Clave (50 dígitos)
   ├─> Genera XML
   ├─> Firma Digital (XAdES-BES)
   └─> Envía a Hacienda
       ├─> Aceptado → Estado: Aceptado
       ├─> Rechazado → Estado: Rechazado (puede reenviar)
       └─> Procesando → Consultar estado después

4. Consultar Estado (GET /api/Documentos/{id}/consultar)
   └─> Si cambió a Aceptado/Rechazado, actualiza BD

5. Descargar (GET /api/Documentos/{id}/descargar-xml)
   └─> Descarga XML firmado para cliente
```

## Ejemplo Completo: Crear y Enviar Factura

```javascript
// 1. Crear factura
const crearFactura = await fetch('/api/Documentos', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    empresaId: "empresa-guid",
    sucursalId: "sucursal-guid",
    terminalId: "terminal-guid",
    tipoDocumento: 1, // FE
    actividadEconomica: "620100",
    clienteId: "cliente-guid",
    receptorActividadEconomica: "471101", // OBLIGATORIO v4.4
    condicionVenta: "01", // Contado
    medioPago: "01", // Efectivo
    moneda: 1, // CRC
    detalles: [
      {
        numeroLinea: 1,
        codigoCabys: "8523904200000", // 13 dígitos
        descripcion: "Servicio de desarrollo web",
        cantidad: 1,
        unidadMedidaId: 1, // Servicio
        precioUnitario: 500000.00,
        impuestos: [
          {
            codigoTarifa: "08", // IVA 13%
            tarifa: 13.00,
            monto: 65000.00
          }
        ]
      }
    ]
  })
});

const factura = await crearFactura.json();
// factura.id = "guid"
// factura.numeroConsecutivo = "001-00001-01-0000000042"
// factura.totalVenta = 565000.00

// 2. Validar antes de enviar
const validar = await fetch(`/api/Documentos/${factura.id}/validar`, {
  headers: { 'Authorization': `Bearer ${token}` }
});

const validacion = await validar.json();
if (!validacion.valido) {
  console.error("Errores:", validacion.errores);
  return;
}

// 3. Procesar y enviar a Hacienda
const procesar = await fetch(`/api/Documentos/${factura.id}/procesar`, {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` }
});

const resultado = await procesar.json();
if (resultado.exitoso) {
  console.log("Clave:", resultado.clave);
  console.log("Estado:", resultado.estado); // "Aceptado"

  // 4. Descargar XML firmado
  window.open(`/api/Documentos/${factura.id}/descargar-xml`);
} else {
  console.error("Errores:", resultado.errores);
}
```

## Manejo de Errores

### Códigos HTTP
- `200 OK`: Operación exitosa
- `201 Created`: Documento creado exitosamente
- `204 No Content`: Actualización/eliminación exitosa
- `400 Bad Request`: Datos inválidos o reglas de negocio violadas
- `401 Unauthorized`: Token JWT inválido o expirado
- `403 Forbidden`: Usuario sin permisos para la empresa
- `404 Not Found`: Documento no encontrado
- `500 Internal Server Error`: Error del servidor

### Errores de Validación Comunes

**Actividad Económica del Receptor:**
```json
{
  "mensaje": "El documento tiene errores de validación",
  "errores": [
    "La actividad económica del receptor es obligatoria para Facturas Electrónicas (v4.4)"
  ]
}
```

**Código CAByS:**
```json
{
  "errores": [
    "La línea 1 debe tener código CAByS (obligatorio desde 01/06/2025)",
    "La línea 2 tiene un código CAByS inválido (debe tener 13 dígitos)"
  ]
}
```

**Referencias Faltantes (NC/ND):**
```json
{
  "errores": [
    "Las Notas de Crédito y Débito requieren al menos una referencia al documento original"
  ]
}
```

**Terminal sin Consecutivos:**
```json
{
  "mensaje": "El terminal Terminal 1 ha alcanzado el límite de consecutivos. Actual: 9999999999, Límite: 9999999999"
}
```

## Notas Importantes

1. **Consecutivos**: Se generan automáticamente al crear el documento. El terminal debe tener configurados los rangos NumeroInicio y NumeroFin.

2. **Cálculo de Totales**: Los totales se calculan automáticamente. No es necesario enviarlos en el request, pero deben coincidir si se envían.

3. **Estado Borrador**: Solo se pueden modificar o eliminar documentos en estado Borrador. Una vez procesados, son inmutables.

4. **Contingencia**: Si no hay conexión con Hacienda, crear documentos con `esContingencia: true` y enviarlos cuando se recupere la conexión.

5. **Certificado Digital**: La empresa debe tener configurado el certificado digital (byte[]) y el PIN antes de procesar documentos.

6. **Ambiente**: Usar ambiente de Pruebas para desarrollo y testing. Cambiar a Producción solo cuando esté listo.

7. **Retención**: Los XMLs firmados se almacenan en la base de datos por los 5 años requeridos por ley.

8. **Emails**: El campo receptorEmails permite hasta 4 direcciones separadas por coma (nuevo en v4.4).

## Soporte y Contacto

Para más información sobre la especificación v4.4:
- Resolución: MH-DGT-RES-0027-2024
- Fecha obligatoriedad: 01/09/2025
- Ministerio de Hacienda: https://www.hacienda.go.cr
