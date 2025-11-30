# Ejemplos de Prueba - Generación de Consecutivos

## Escenario 1: Generar Consecutivo para Factura Electrónica

### Configuración inicial en la base de datos

```sql
-- Crear una empresa
INSERT INTO Empresas (Id, Nombre, Identificacion, ...)
VALUES ('guid-empresa-1', 'Mi Empresa S.A.', '3101234567', ...);

-- Crear una sucursal
INSERT INTO Sucursales (Id, EmpresaId, Codigo, Nombre, Activo)
VALUES ('guid-sucursal-1', 'guid-empresa-1', '001', 'Casa Matriz', 1);

-- Crear un terminal
INSERT INTO Terminales (Id, SucursalId, Codigo, Nombre, Activo)
VALUES ('guid-terminal-1', 'guid-sucursal-1', '00001', 'Terminal Principal', 1);

-- Configurar consecutivo para Facturas Electrónicas (tipo 01)
INSERT INTO Consecutivos (
    Id,
    TerminalId,
    TipoDocumento,
    ClaveNumeracion,
    NumeroInicio,
    NumeroFin,
    NumeroActual,
    Activo,
    FechaCreacion
)
VALUES (
    NEWID(),
    'guid-terminal-1',
    '01',                           -- Factura Electrónica
    '00100001010000000001',         -- Clave de numeración inicial
    1,                              -- Inicia en 1
    9999999999,                     -- Fin en 9,999,999,999 (10 dígitos)
    0,                              -- Actual: 0 (primer documento será 1)
    1,                              -- Activo
    GETUTCDATE()
);
```

### Código C# de prueba

```csharp
// Inyectar el servicio en el constructor
public class DocumentosController : Controller
{
    private readonly IConsecutivoService _consecutivoService;

    public DocumentosController(IConsecutivoService consecutivoService)
    {
        _consecutivoService = consecutivoService;
    }

    // Método de prueba
    public async Task<IActionResult> PruebaGenerarConsecutivo()
    {
        try
        {
            var terminalId = Guid.Parse("guid-terminal-1");
            var tipoDocumento = DocumentoTipo.FacturaElectronica;

            // 1. Verificar disponibilidad
            var hayDisponibles = await _consecutivoService.TieneConsecutivosDisponiblesAsync(
                terminalId,
                tipoDocumento
            );

            if (!hayDisponibles)
            {
                return BadRequest("No hay consecutivos disponibles");
            }

            // 2. Consultar el siguiente (sin incrementar)
            var siguiente = await _consecutivoService.ObtenerSiguienteConsecutivoAsync(
                terminalId,
                tipoDocumento
            );
            // Resultado esperado: "001-00001-01-0000000001"

            // 3. Generar y asignar el consecutivo
            var consecutivo = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
                terminalId,
                tipoDocumento
            );
            // Resultado esperado: "001-00001-01-0000000001"

            // 4. Convertir a formato Hacienda (XML)
            var formatoXML = _consecutivoService.ConvertirAFormatoHacienda(consecutivo);
            // Resultado esperado: "00100001010000000001" (20 dígitos)

            return Ok(new
            {
                ConsecutivoConGuiones = consecutivo,
                ConsecutivoParaXML = formatoXML,
                SiguienteDisponible = await _consecutivoService.ObtenerSiguienteConsecutivoAsync(terminalId, tipoDocumento)
                // Resultado esperado: "001-00001-01-0000000002"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

### Resultado esperado de la prueba

```json
{
  "consecutivoConGuiones": "001-00001-01-0000000001",
  "consecutivoParaXML": "00100001010000000001",
  "siguienteDisponible": "001-00001-01-0000000002"
}
```

### Estado en la base de datos después de la prueba

```sql
SELECT * FROM Consecutivos WHERE TerminalId = 'guid-terminal-1' AND TipoDocumento = '01';

-- Resultado esperado:
-- NumeroActual = 1 (se incrementó de 0 a 1)
```

## Escenario 2: Múltiples Tipos de Documento

### Configuración inicial

```sql
-- Configurar consecutivos para diferentes tipos de documentos
-- Todos en el mismo terminal

-- Facturas Electrónicas (01)
INSERT INTO Consecutivos (..., TipoDocumento, NumeroActual, ...)
VALUES (..., '01', 0, ...);

-- Notas de Crédito (03)
INSERT INTO Consecutivos (..., TipoDocumento, NumeroActual, ...)
VALUES (..., '03', 0, ...);

-- Tiquetes Electrónicos (04)
INSERT INTO Consecutivos (..., TipoDocumento, NumeroActual, ...)
VALUES (..., '04', 0, ...);
```

### Código de prueba

```csharp
public async Task<IActionResult> PruebaMultiplesTipos()
{
    var terminalId = Guid.Parse("guid-terminal-1");

    // Generar FE
    var fe = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
        terminalId,
        DocumentoTipo.FacturaElectronica
    );
    // Resultado: "001-00001-01-0000000001"

    // Generar NC
    var nc = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
        terminalId,
        DocumentoTipo.NotaCreditoElectronica
    );
    // Resultado: "001-00001-03-0000000001"

    // Generar TE
    var te = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
        terminalId,
        DocumentoTipo.TiqueteElectronico
    );
    // Resultado: "001-00001-04-0000000001"

    return Ok(new { fe, nc, te });
}
```

### Resultado esperado

```json
{
  "fe": "001-00001-01-0000000001",
  "nc": "001-00001-03-0000000001",
  "te": "001-00001-04-0000000001"
}
```

**Nota**: Cada tipo de documento tiene su propio contador independiente.

## Escenario 3: Múltiples Terminales

### Configuración inicial

```sql
-- Terminal 1 en Sucursal Casa Matriz (001)
INSERT INTO Terminales (Id, SucursalId, Codigo, Nombre, Activo)
VALUES ('guid-terminal-1', 'guid-sucursal-1', '00001', 'Terminal 1', 1);

-- Terminal 2 en Sucursal Casa Matriz (001)
INSERT INTO Terminales (Id, SucursalId, Codigo, Nombre, Activo)
VALUES ('guid-terminal-2', 'guid-sucursal-1', '00002', 'Terminal 2', 1);

-- Sucursal San José (002)
INSERT INTO Sucursales (Id, EmpresaId, Codigo, Nombre, Activo)
VALUES ('guid-sucursal-2', 'guid-empresa-1', '002', 'Sucursal San José', 1);

-- Terminal 1 en Sucursal San José (002)
INSERT INTO Terminales (Id, SucursalId, Codigo, Nombre, Activo)
VALUES ('guid-terminal-3', 'guid-sucursal-2', '00001', 'Terminal SJ', 1);

-- Consecutivos para cada terminal (tipo 01 - FE)
INSERT INTO Consecutivos (..., TerminalId, TipoDocumento, NumeroActual, ...)
VALUES (..., 'guid-terminal-1', '01', 0, ...);

INSERT INTO Consecutivos (..., TerminalId, TipoDocumento, NumeroActual, ...)
VALUES (..., 'guid-terminal-2', '01', 0, ...);

INSERT INTO Consecutivos (..., TerminalId, TipoDocumento, NumeroActual, ...)
VALUES (..., 'guid-terminal-3', '01', 0, ...);
```

### Código de prueba

```csharp
public async Task<IActionResult> PruebaMultiplesTerminales()
{
    var tipoDocumento = DocumentoTipo.FacturaElectronica;

    // Terminal 1 de Casa Matriz (001)
    var t1 = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
        Guid.Parse("guid-terminal-1"),
        tipoDocumento
    );
    // Resultado: "001-00001-01-0000000001"

    // Terminal 2 de Casa Matriz (001)
    var t2 = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
        Guid.Parse("guid-terminal-2"),
        tipoDocumento
    );
    // Resultado: "001-00002-01-0000000001"

    // Terminal 1 de San José (002)
    var t3 = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
        Guid.Parse("guid-terminal-3"),
        tipoDocumento
    );
    // Resultado: "002-00001-01-0000000001"

    return Ok(new { terminal1 = t1, terminal2 = t2, terminal3 = t3 });
}
```

### Resultado esperado

```json
{
  "terminal1": "001-00001-01-0000000001",
  "terminal2": "001-00002-01-0000000001",
  "terminal3": "002-00001-01-0000000001"
}
```

**Nota**: Cada terminal tiene su propio rango de consecutivos independiente.

## Escenario 4: Límite de Consecutivos

### Configuración inicial

```sql
-- Configurar consecutivo con límite bajo para prueba
INSERT INTO Consecutivos (
    Id,
    TerminalId,
    TipoDocumento,
    ClaveNumeracion,
    NumeroInicio,
    NumeroFin,
    NumeroActual,
    Activo
)
VALUES (
    NEWID(),
    'guid-terminal-1',
    '01',
    '00100001010000000001',
    1,
    5,           -- Solo 5 consecutivos disponibles
    0,
    1
);
```

### Código de prueba

```csharp
public async Task<IActionResult> PruebaLimiteConsecutivos()
{
    var terminalId = Guid.Parse("guid-terminal-1");
    var tipoDocumento = DocumentoTipo.FacturaElectronica;

    var resultados = new List<object>();

    // Generar 5 consecutivos (hasta el límite)
    for (int i = 1; i <= 5; i++)
    {
        try
        {
            var consecutivo = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
                terminalId,
                tipoDocumento
            );

            resultados.Add(new
            {
                Numero = i,
                Consecutivo = consecutivo,
                Estado = "Éxito"
            });
        }
        catch (Exception ex)
        {
            resultados.Add(new
            {
                Numero = i,
                Error = ex.Message,
                Estado = "Fallo"
            });
        }
    }

    // Intentar generar el 6to (debe fallar)
    try
    {
        var consecutivo6 = await _consecutivoService.GenerarYAsignarConsecutivoAsync(
            terminalId,
            tipoDocumento
        );

        resultados.Add(new
        {
            Numero = 6,
            Consecutivo = consecutivo6,
            Estado = "Éxito (NO DEBERÍA PASAR)"
        });
    }
    catch (Exception ex)
    {
        resultados.Add(new
        {
            Numero = 6,
            Error = ex.Message,
            Estado = "Fallo Esperado"
        });
    }

    return Ok(resultados);
}
```

### Resultado esperado

```json
[
  { "numero": 1, "consecutivo": "001-00001-01-0000000001", "estado": "Éxito" },
  { "numero": 2, "consecutivo": "001-00001-01-0000000002", "estado": "Éxito" },
  { "numero": 3, "consecutivo": "001-00001-01-0000000003", "estado": "Éxito" },
  { "numero": 4, "consecutivo": "001-00001-01-0000000004", "estado": "Éxito" },
  { "numero": 5, "consecutivo": "001-00001-01-0000000005", "estado": "Éxito" },
  {
    "numero": 6,
    "error": "El consecutivo del terminal 'Terminal Principal' ha alcanzado el límite. Actual: 5, Límite: 5. Por favor configure un nuevo rango de consecutivos.",
    "estado": "Fallo Esperado"
  }
]
```

## Escenario 5: Concurrencia (2 usuarios simultáneos)

### Código de prueba (simular con Task.WhenAll)

```csharp
public async Task<IActionResult> PruebaConcurrencia()
{
    var terminalId = Guid.Parse("guid-terminal-1");
    var tipoDocumento = DocumentoTipo.FacturaElectronica;

    // Simular 10 usuarios generando consecutivos simultáneamente
    var tareas = new List<Task<string>>();

    for (int i = 0; i < 10; i++)
    {
        tareas.Add(_consecutivoService.GenerarYAsignarConsecutivoAsync(
            terminalId,
            tipoDocumento
        ));
    }

    // Ejecutar todas las tareas en paralelo
    var consecutivos = await Task.WhenAll(tareas);

    // Verificar que no hay duplicados
    var duplicados = consecutivos
        .GroupBy(c => c)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .ToList();

    return Ok(new
    {
        Consecutivos = consecutivos.OrderBy(c => c).ToList(),
        TotalGenerados = consecutivos.Length,
        Duplicados = duplicados,
        PruebaPasada = duplicados.Count == 0
    });
}
```

### Resultado esperado

```json
{
  "consecutivos": [
    "001-00001-01-0000000001",
    "001-00001-01-0000000002",
    "001-00001-01-0000000003",
    "001-00001-01-0000000004",
    "001-00001-01-0000000005",
    "001-00001-01-0000000006",
    "001-00001-01-0000000007",
    "001-00001-01-0000000008",
    "001-00001-01-0000000009",
    "001-00001-01-0000000010"
  ],
  "totalGenerados": 10,
  "duplicados": [],
  "pruebaPasada": true
}
```

**Nota**: El bloqueo `WITH (UPDLOCK, ROWLOCK)` garantiza que no haya duplicados incluso con acceso concurrente.

## Escenario 6: Conversión de Formatos

### Código de prueba

```csharp
public IActionResult PruebaConversionFormatos()
{
    var casos = new[]
    {
        "001-00001-01-0000000001",
        "002-00005-04-0000000123",
        "010-12345-03-9999999999"
    };

    var resultados = casos.Select(consecutivo => new
    {
        ConGuiones = consecutivo,
        SinGuiones = _consecutivoService.ConvertirAFormatoHacienda(consecutivo),
        Longitud = _consecutivoService.ConvertirAFormatoHacienda(consecutivo).Length
    }).ToList();

    return Ok(resultados);
}
```

### Resultado esperado

```json
[
  {
    "conGuiones": "001-00001-01-0000000001",
    "sinGuiones": "00100001010000000001",
    "longitud": 20
  },
  {
    "conGuiones": "002-00005-04-0000000123",
    "sinGuiones": "00200005040000000123",
    "longitud": 20
  },
  {
    "conGuiones": "010-12345-03-9999999999",
    "sinGuiones": "01012345039999999999",
    "longitud": 20
  }
]
```

## Script de Configuración Completa

```sql
-- Script para configurar un ambiente de prueba completo

-- 1. Empresa
DECLARE @EmpresaId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Empresas (Id, Nombre, Identificacion, TipoIdentificacion, Activo, FechaCreacion)
VALUES (@EmpresaId, 'Empresa de Prueba S.A.', '3101234567', '02', 1, GETUTCDATE());

-- 2. Sucursal Casa Matriz
DECLARE @SucursalId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Sucursales (Id, EmpresaId, Codigo, Nombre, Activo, FechaCreacion)
VALUES (@SucursalId, @EmpresaId, '001', 'Casa Matriz', 1, GETUTCDATE());

-- 3. Terminal Principal
DECLARE @TerminalId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Terminales (Id, SucursalId, Codigo, Nombre, Activo, FechaCreacion)
VALUES (@TerminalId, @SucursalId, '00001', 'Terminal Principal', 1, GETUTCDATE());

-- 4. Consecutivos para todos los tipos de documentos
INSERT INTO Consecutivos (Id, TerminalId, TipoDocumento, ClaveNumeracion, NumeroInicio, NumeroFin, NumeroActual, Activo, FechaCreacion)
VALUES
    (NEWID(), @TerminalId, '01', '00100001010000000001', 1, 9999999999, 0, 1, GETUTCDATE()), -- FE
    (NEWID(), @TerminalId, '02', '00100001020000000001', 1, 9999999999, 0, 1, GETUTCDATE()), -- ND
    (NEWID(), @TerminalId, '03', '00100001030000000001', 1, 9999999999, 0, 1, GETUTCDATE()), -- NC
    (NEWID(), @TerminalId, '04', '00100001040000000001', 1, 9999999999, 0, 1, GETUTCDATE()), -- TE
    (NEWID(), @TerminalId, '08', '00100001080000000001', 1, 9999999999, 0, 1, GETUTCDATE()), -- FEC
    (NEWID(), @TerminalId, '09', '00100001090000000001', 1, 9999999999, 0, 1, GETUTCDATE()); -- FEE

-- Consultar para obtener los IDs generados
SELECT
    'EmpresaId' AS Tipo, @EmpresaId AS Id
UNION ALL SELECT 'SucursalId', @SucursalId
UNION ALL SELECT 'TerminalId', @TerminalId;

-- Verificar configuración
SELECT
    s.Codigo AS Sucursal,
    t.Codigo AS Terminal,
    c.TipoDocumento,
    c.NumeroActual,
    c.NumeroFin,
    c.Activo
FROM Consecutivos c
INNER JOIN Terminales t ON c.TerminalId = t.Id
INNER JOIN Sucursales s ON t.SucursalId = s.Id
WHERE s.Id = @SucursalId;
```

## Verificación Post-Implementación

### Checklist de pruebas

- [ ] Generar consecutivo para FE (Factura Electrónica)
- [ ] Generar consecutivo para TE (Tiquete Electrónico)
- [ ] Generar consecutivo para NC (Nota de Crédito)
- [ ] Verificar que cada tipo tiene su contador independiente
- [ ] Verificar formato con guiones: XXX-YYYYY-ZZ-AAAAAAAAAA
- [ ] Verificar conversión a formato XML: 20 dígitos sin guiones
- [ ] Probar concurrencia (10 usuarios simultáneos)
- [ ] Verificar que no hay duplicados
- [ ] Probar límite de consecutivos (debe lanzar excepción)
- [ ] Verificar que el `NumeroActual` se incrementa en la BD
- [ ] Probar con múltiples terminales en la misma sucursal
- [ ] Probar con terminales en diferentes sucursales
