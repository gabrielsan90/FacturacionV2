# Diagnóstico Error 400 - POST /api/Documentos

## Problemas Identificados

### 1. Falta Configuración de camelCase en el Backend
**Ubicación**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Program.cs`

**Problema**: El backend no tenía configurado el `PropertyNamingPolicy` para manejar camelCase.

**Solución Aplicada**: Se agregó la configuración de camelCase y el convertidor de enums a strings:
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
```

**Impacto**: Ahora el API acepta JSON en camelCase y también permite enviar enums como strings.

### 2. Valor de `moneda` Incorrecto en el JSON Original

**JSON Enviado**:
```json
"moneda": "CRC"
```

**Valor Correcto Opción 1** (con el convertidor de enums agregado):
```json
"moneda": "CRC"
```

**Valor Correcto Opción 2** (usando el valor numérico del enum):
```json
"moneda": 1
```

**Explicación**:
- El DTO espera un `TipoMoneda` (enum)
- Los valores del enum son: CRC = 1, USD = 2, EUR = 3
- Con el `JsonStringEnumConverter` agregado, ahora se puede enviar como string "CRC" o como número 1

### 3. Campo Obligatorio Faltante: `receptorActividadEconomica`

**JSON Original**: No incluía este campo
**JSON Corregido**: Se agregó `"receptorActividadEconomica": "861001"`

**Explicación**:
- Para Facturas Electrónicas (tipoDocumento = 1), la actividad económica del receptor es OBLIGATORIA según la versión 4.4
- Debe tener exactamente 6 dígitos (código CIIU4)
- En el ejemplo, "861001" corresponde a "Actividades de hospitales y clínicas, con internación"

### 4. Otras Diferencias en el JSON

El JSON original ya tenía los valores correctos para estos campos, pero vale la pena documentarlos:

- **tipoDocumento**: 1 (FacturaElectronica) ✓ Correcto
- **receptorTipoIdentificacion**: 2 (Juridica) ✓ Correcto
- **tipoCambio**: Se recomienda usar `1.0` en lugar de `1` para que quede claro que es decimal
- **impuestos[].monto**: Valor calculado automáticamente por el backend, pero se puede enviar

## JSON Corregido

Ver archivo: `/mnt/d/Proyectos/2/Facturacion/JSON_CORREGIDO.json`

## Comparación Campo por Campo

| Campo JSON | Tipo Esperado (DTO) | Valor Enviado | ¿Correcto? | Observaciones |
|------------|---------------------|---------------|------------|---------------|
| tipoDocumento | DocumentoTipo (enum) | 1 | ✓ | 1 = FacturaElectronica |
| empresaId | Guid | "d7eb6eb4-9463-41a7-5df7-08de2b7440bd" | ✓ | |
| sucursalId | Guid | "d65eb67d-024e-4577-2126-08de2ebd8144" | ✓ | |
| terminalId | Guid | "01f8bcbd-77db-4f3a-1074-08de2edaf606" | ✓ | |
| actividadEconomica | string (max 6) | "620900" | ✓ | |
| clienteId | Guid? | "238bf6b8-b4f6-4122-0791-08de2f0974e4" | ✓ | |
| condicionVenta | string (max 2) | "04" | ✓ | 04 = Otros |
| medioPago | string (max 2) | "04" | ✓ | 04 = Transferencia |
| plazoCreditoDias | int? | null | ✓ | |
| observaciones | string? | null | ✓ | |
| moneda | TipoMoneda (enum) | "CRC" | ✓ | Ahora funciona con string o número |
| tipoCambio | decimal? | 1 | ✓ | Se recomienda 1.0 |
| receptorNombre | string? | "3DX IMAGENES DENTALES..." | ✓ | |
| receptorTipoIdentificacion | TipoIdentificacion? (enum) | 2 | ✓ | 2 = Juridica |
| receptorNumeroIdentificacion | string? | "3101601306" | ✓ | |
| receptorActividadEconomica | string? | FALTABA | ✗ → ✓ | OBLIGATORIO para FE v4.4, agregado "861001" |
| receptorEmails | string? | "gsanchez@smarttechcr.com" | ✓ | |
| receptorTelefono | string? | "87026858" | ✓ | |
| receptorProvincia | int? | 1 | ✓ | |
| receptorCanton | int? | 101 | ✓ | |
| receptorDistrito | int? | 10103 | ✓ | |
| receptorOtrasSenas | string? | "100 sur 200 norte" | ✓ | |
| detalles | List<CreateDocumentoDetalleDTO> | [...] | ✓ | |
| detalles[0].numeroLinea | int | 1 | ✓ | |
| detalles[0].codigo | string? | "002" | ✓ | |
| detalles[0].codigoCabys | string? | "1010101010101" | ✓ | |
| detalles[0].descripcion | string | "Materia Prima 3" | ✓ | |
| detalles[0].cantidad | decimal | 1 | ✓ | |
| detalles[0].unidadMedidaId | int | 1 | ✓ | |
| detalles[0].precioUnitario | decimal | 5000 | ✓ | |
| detalles[0].impuestos | List<CreateDocumentoDetalleImpuestoDTO> | [...] | ✓ | |
| detalles[0].impuestos[0].codigoTarifa | string | "08" | ✓ | |
| detalles[0].impuestos[0].tarifa | decimal | 13 | ✓ | |
| detalles[0].impuestos[0].monto | decimal | 650 | ✓ | |

## Validaciones Adicionales del Backend

El controlador `DocumentosController.PostAsync` realiza las siguientes validaciones:

1. **ModelState.IsValid**: Verifica que todos los campos requeridos estén presentes y sean válidos
2. **Usuario Autenticado**: Verifica que el JWT sea válido y obtenga el `userId`
3. **Creación de Documento**: `DocumentoService.CrearDocumentoDesdeDTO` convierte el DTO en entidad
4. **Validación de Negocio**: `DocumentoService.ValidarDocumentoAsync` verifica:
   - Al menos una línea de detalle
   - Actividad económica de 6 dígitos
   - Receptor obligatorio para facturas (no para tiquetes)
   - Actividad económica del receptor para Facturas Electrónicas (v4.4)
   - Cliente O Proveedor (no ambos)
   - Referencias para NC/ND
   - Plazo de crédito si condición es "02"
   - Tipo de cambio si moneda no es CRC
   - Total > 0
   - Validaciones de cada línea de detalle

## Pasos para Probar

1. **Reiniciar el Backend**: Es necesario reiniciar el backend para que tome la nueva configuración del `Program.cs`
   ```bash
   # Detener el backend
   # Iniciar nuevamente el backend
   ```

2. **Probar con el JSON Corregido**: Usar el archivo `JSON_CORREGIDO.json` para hacer la prueba

3. **Verificar Headers**: Asegurarse de incluir:
   ```
   Content-Type: application/json
   Authorization: Bearer {tu-jwt-token}
   ```

## Archivos Modificados

- `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Program.cs` - Agregada configuración de camelCase y JsonStringEnumConverter

## Archivos Creados

- `/mnt/d/Proyectos/2/Facturacion/JSON_CORREGIDO.json` - JSON con los valores correctos
- `/mnt/d/Proyectos/2/Facturacion/DIAGNOSTICO_ERROR_400.md` - Este documento

## Próximos Pasos

1. Reiniciar el backend
2. Probar con el JSON corregido
3. Si persiste el error, revisar los logs del backend para ver el error específico de validación
4. Verificar que existan en la base de datos:
   - La empresa con ID: d7eb6eb4-9463-41a7-5df7-08de2b7440bd
   - La sucursal con ID: d65eb67d-024e-4577-2126-08de2ebd8144
   - El terminal con ID: 01f8bcbd-77db-4f3a-1074-08de2edaf606
   - El cliente con ID: 238bf6b8-b4f6-4122-0791-08de2f0974e4
   - La unidad de medida con ID: 1
   - Un consecutivo activo para el terminal y tipo de documento "01"
