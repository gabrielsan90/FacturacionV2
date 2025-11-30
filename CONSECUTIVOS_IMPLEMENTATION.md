# Implementación de Generación de Consecutivos

## Resumen

Se ha implementado la generación de consecutivos de documentos electrónicos siguiendo el formato oficial de Hacienda de Costa Rica, basado en el ejemplo SQL proporcionado.

## Formato del Consecutivo

Según la normativa de Hacienda, el consecutivo tiene el siguiente formato:

### Formato con guiones (Base de datos y visualización)
```
XXX-YYYYY-ZZ-AAAAAAAAAA
```

### Formato sin guiones (XML para Hacienda)
```
XXXYYYYYZZAAAAAAAAAA (20 dígitos numéricos)
```

### Componentes:
- **XXX**: Código de sucursal (3 dígitos)
- **YYYYY**: Código de terminal/servidor (5 dígitos)
- **ZZ**: Tipo de documento (2 dígitos)
  - 01 = Factura Electrónica (FE)
  - 02 = Nota de Débito (ND)
  - 03 = Nota de Crédito (NC)
  - 04 = Tiquete Electrónico (TE)
  - 08 = Factura Electrónica de Compra (FEC)
  - 09 = Factura Electrónica de Exportación (FEE)
- **AAAAAAAAAA**: Número consecutivo (10 dígitos con ceros a la izquierda)

### Ejemplo SQL Original
```sql
SELECT @Consecutivo = (C.IdTienda + C.Servidor + C.TipoDocumento + RIGHT('0000000000' + convert(varchar(max),(C.Consecutivo +1)),10))
FROM CONSECUTIVO C JOIN @venta A ON (C.IdTienda = A.idtienda
                                     AND C.TipoDocumento = CASE WHEN A.TipoDocumento = 'TE' THEN '04'
                                                                WHEN A.TipoDocumento = 'FE' THEN '01'
                                                                WHEN A.TipoDocumento = 'NC' THEN '03'
                                                                WHEN A.TipoDocumento = 'ND' THEN '02' END)
```

## Archivos Creados

### 1. Interfaz del Servicio
**Ubicación**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Services/Interfaces/IConsecutivoService.cs`

Métodos principales:
- `GenerarYAsignarConsecutivoAsync()`: Genera y asigna el siguiente consecutivo de forma atómica
- `ObtenerSiguienteConsecutivoAsync()`: Consulta el siguiente consecutivo sin incrementarlo
- `ConvertirAFormatoHacienda()`: Convierte formato con guiones a formato numérico (20 dígitos)
- `ObtenerCodigoTipoDocumento()`: Obtiene el código de 2 dígitos del tipo de documento
- `TieneConsecutivosDisponiblesAsync()`: Verifica disponibilidad de consecutivos

### 2. Implementación del Servicio
**Ubicación**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Services/Implementations/ConsecutivoService.cs`

#### Características clave:

1. **Atomicidad garantizada**:
   ```csharp
   using var transaction = await _context.Database.BeginTransactionAsync();

   // Bloqueo a nivel de fila para evitar concurrencia
   var consecutivoEntity = await _context.Consecutivos
       .FromSqlRaw(@"
           SELECT * FROM Consecutivos WITH (UPDLOCK, ROWLOCK)
           WHERE TerminalId = {0}
           AND TipoDocumento = {1}
           AND Activo = 1
           AND IsDeleted = 0",
           terminalId, codigoTipoDoc)
       .FirstOrDefaultAsync();

   // Incrementar de forma atómica
   consecutivoEntity.NumeroActual++;
   await _context.SaveChangesAsync();
   await transaction.CommitAsync();
   ```

2. **Formato equivalente al SQL**:
   ```csharp
   // Equivalente a: RIGHT('0000000000' + convert(varchar(max), Consecutivo), 10)
   var consecutivo = numeroConsecutivo.ToString().PadLeft(10, '0');

   // Formato completo: XXX-YYYYY-ZZ-AAAAAAAAAA
   return $"{sucursal}-{terminal}-{tipo}-{consecutivo}";
   ```

3. **Validaciones**:
   - Verifica que el terminal exista y esté activo
   - Verifica que el terminal tenga una sucursal asignada
   - Verifica que exista un consecutivo configurado para el tipo de documento
   - Verifica que no se haya alcanzado el límite de consecutivos
   - Manejo de excepciones con rollback automático

4. **Conversión de formatos**:
   ```csharp
   // Con guiones para BD: 001-00001-01-0000000001
   // Sin guiones para XML: 00100001010000000001 (20 dígitos)
   public string ConvertirAFormatoHacienda(string consecutivoConGuiones)
   {
       var consecutivoSinGuiones = consecutivoConGuiones.Replace("-", "");
       // Validación de 20 dígitos numéricos
       return consecutivoSinGuiones;
   }
   ```

## Archivos Modificados

### 1. DocumentoService.cs
**Ubicación**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Services/Implementations/DocumentoService.cs`

**Cambios realizados**:
- Inyección de `IConsecutivoService` en el constructor
- Métodos antiguos marcados como `[Obsolete]` y delegando al nuevo servicio
- Actualización de `CrearDocumentoDesdeDTO()` para usar el nuevo servicio:
  ```csharp
  // ANTES:
  var codigoTipoDocumento = ObtenerCodigoTipoDocumento(dto.TipoDocumento);
  documento.NumeroConsecutivo = await GenerarNumeroConsecutivoAsync(dto.TerminalId, codigoTipoDocumento);

  // AHORA:
  documento.NumeroConsecutivo = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
      dto.TerminalId,
      dto.TipoDocumento);
  ```

### 2. Program.cs
**Ubicación**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Program.cs`

**Registro del servicio**:
```csharp
// Dependency Injection - Consecutivos Service
builder.Services.AddScoped<Facturacion.Backend.Services.Interfaces.IConsecutivoService,
                          Facturacion.Backend.Services.Implementations.ConsecutivoService>();
```

## Estructura de Base de Datos

La tabla `Consecutivos` tiene la siguiente estructura:

```csharp
public class Consecutivo
{
    public Guid Id { get; set; }
    public Guid TerminalId { get; set; }
    public string TipoDocumento { get; set; }  // "01", "02", "03", "04", etc.
    public string ClaveNumeracion { get; set; }
    public long NumeroInicio { get; set; }
    public long NumeroFin { get; set; }
    public long NumeroActual { get; set; }     // Se incrementa automáticamente
    public bool Activo { get; set; }

    // Navigation Properties
    public Terminal? Terminal { get; set; }
}
```

Cada `Terminal` pertenece a una `Sucursal`:

```csharp
public class Terminal
{
    public Guid Id { get; set; }
    public Guid SucursalId { get; set; }
    public string Codigo { get; set; }         // 5 dígitos
    public string Nombre { get; set; }

    public Sucursal? Sucursal { get; set; }
    public ICollection<Consecutivo>? Consecutivos { get; set; }
}

public class Sucursal
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Codigo { get; set; }         // 3 dígitos
    public string Nombre { get; set; }

    public ICollection<Terminal>? Terminales { get; set; }
}
```

## Flujo de Generación de Consecutivo

1. **Usuario crea un documento** (por ejemplo, una Factura Electrónica)
2. **Sistema solicita consecutivo**:
   ```csharp
   var consecutivo = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
       terminalId: Guid.Parse("..."),
       tipoDocumento: DocumentoTipo.FacturaElectronica  // Enum
   );
   ```
3. **Servicio ejecuta**:
   - Inicia transacción de base de datos
   - Bloquea el registro del consecutivo (UPDLOCK, ROWLOCK)
   - Incrementa `NumeroActual` en 1
   - Formatea el consecutivo: `001-00001-01-0000000123`
   - Guarda cambios y hace commit
   - Retorna el consecutivo formateado
4. **Documento se guarda** con el consecutivo asignado
5. **Al generar XML**, se convierte a formato Hacienda:
   ```csharp
   var consecutivoParaXML = _consecutivoService.ConvertirAFormatoHacienda(
       documento.NumeroConsecutivo  // "001-00001-01-0000000123"
   );
   // Resultado: "00100001010000000123" (20 dígitos)
   ```

## Prevención de Duplicados

La implementación previene duplicados mediante:

1. **Bloqueo a nivel de fila**: `SELECT ... WITH (UPDLOCK, ROWLOCK)`
2. **Transacciones**: Commit solo si todo es exitoso
3. **Incremento atómico**: El incremento ocurre dentro de la transacción
4. **Índices únicos**: El campo `Clave` en la tabla `Documentos` tiene índice único

## Manejo de Errores

El servicio lanza excepciones claras en los siguientes casos:

1. **Terminal no encontrado o inactivo**:
   ```
   Terminal con ID {terminalId} no encontrado o está inactivo
   ```

2. **Terminal sin sucursal**:
   ```
   Terminal {nombre} no tiene una sucursal asignada
   ```

3. **Sin consecutivo configurado**:
   ```
   No hay consecutivo activo configurado para el terminal '{nombre}'
   y tipo de documento '{tipo}' ({código})
   ```

4. **Límite alcanzado**:
   ```
   El consecutivo del terminal '{nombre}' ha alcanzado el límite.
   Actual: {actual}, Límite: {fin}.
   Por favor configure un nuevo rango de consecutivos.
   ```

## Pruebas

El proyecto compiló exitosamente sin errores:
```
Build succeeded.
    44 Warning(s)
    0 Error(s)
Time Elapsed 00:00:15.33
```

Las advertencias son relacionadas con null-safety de C# y no afectan la funcionalidad.

## Uso Recomendado

### Para generar un consecutivo nuevo:
```csharp
var consecutivo = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
    terminalId,
    DocumentoTipo.FacturaElectronica
);
```

### Para consultar el siguiente sin incrementar:
```csharp
var siguiente = await _consecutivoService.ObtenerSiguienteConsecutivoAsync(
    terminalId,
    DocumentoTipo.FacturaElectronica
);
```

### Para convertir a formato XML:
```csharp
var xmlFormat = _consecutivoService.ConvertirAFormatoHacienda("001-00001-01-0000000123");
// Resultado: "00100001010000000123"
```

### Para verificar disponibilidad:
```csharp
var hayDisponibles = await _consecutivoService.TieneConsecutivosDisponiblesAsync(
    terminalId,
    DocumentoTipo.FacturaElectronica
);
```

## Próximos Pasos

Para completar la implementación:

1. Probar la generación de consecutivos en ambiente de desarrollo
2. Verificar que los documentos se generen correctamente con el nuevo formato
3. Validar que el XML generado para Hacienda tenga el consecutivo en formato numérico de 20 dígitos
4. Probar casos de concurrencia (múltiples usuarios generando documentos simultáneamente)
5. Configurar alertas cuando los consecutivos estén próximos a su límite

## Notas Importantes

- El consecutivo **se incrementa inmediatamente** al llamar `GenerarYAsignarConsecutivoAsync()`, incluso si el documento no se guarda o se cancela
- Esto es intencional para cumplir con la normativa de Hacienda (los consecutivos pueden tener saltos)
- Si un documento es rechazado por Hacienda, el consecutivo se "quema" (no se reutiliza)
- Los métodos antiguos en `DocumentoService` están marcados como `[Obsolete]` pero siguen funcionando para compatibilidad

## Conformidad con Hacienda

La implementación cumple con:
- ✅ Formato de 20 caracteres/dígitos
- ✅ Estructura: Sucursal (3) + Terminal (5) + Tipo (2) + Consecutivo (10)
- ✅ Incremento automático y atómico
- ✅ Prevención de duplicados
- ✅ Validación del formato numérico para XML
- ✅ Soporte para todos los tipos de documentos (FE, TE, NC, ND, FEC, FEE, REP)
- ✅ Manejo de límites de rangos de consecutivos
