# Solución: Dropdowns de Unidad de Medida e Impuesto vacíos en Productos

## Problema Identificado

Los dropdowns de "Unidad de Medida" e "Impuesto" en la página de Productos no estaban cargando datos debido a dos problemas principales:

1. **Backend retornaba datos incompletos**: Los endpoints `/api/catalogos/unidades-medida` y `/api/catalogos/impuestos` no incluían el campo `Id` que el frontend necesitaba para construir los selectores.

2. **Falta de datos de Impuestos**: El seeder (`SeedDb.cs`) no incluía la inicialización de la tabla `Impuestos`, por lo que aunque el endpoint funcionara correctamente, no había datos para mostrar.

## Archivos Modificados

### 1. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Controllers/CatalogosController.cs`

**Cambios realizados:**

#### Endpoint de Unidades de Medida (líneas 383-406)
```csharp
// ANTES:
var unidades = await _context.Set<UnidadMedida>()
    .Where(u => u.Activo)
    .OrderBy(u => u.Codigo)
    .Select(u => new
    {
        u.Codigo,
        u.Descripcion
    })
    .ToListAsync();

return Ok(new { success = true, data = unidades });

// DESPUÉS:
var unidades = await _context.Set<UnidadMedida>()
    .Where(u => u.Activo)
    .OrderBy(u => u.Codigo)
    .Select(u => new
    {
        u.Id,           // ✅ Agregado
        u.Codigo,
        u.Descripcion
    })
    .ToListAsync();

return Ok(unidades);  // ✅ Retorna directamente el array
```

#### Endpoint de Impuestos (líneas 411-435)
```csharp
// ANTES:
var impuestos = await _context.Set<Impuesto>()
    .Where(i => i.Activo)
    .OrderBy(i => i.Codigo)
    .Select(i => new
    {
        i.Codigo,
        i.Descripcion,
        Tarifa = i.Porcentaje
    })
    .ToListAsync();

return Ok(new { success = true, data = impuestos });

// DESPUÉS:
var impuestos = await _context.Set<Impuesto>()
    .Where(i => i.Activo)
    .OrderBy(i => i.Codigo)
    .Select(i => new
    {
        i.Id,           // ✅ Agregado
        i.Codigo,
        i.Descripcion,
        Tarifa = i.Porcentaje
    })
    .ToListAsync();

return Ok(impuestos);  // ✅ Retorna directamente el array
```

### 2. `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/Data/SeedDb.cs`

**Cambios realizados:**

#### Agregado método CheckImpuestosAsync (líneas 485-506)
```csharp
private async Task CheckImpuestosAsync()
{
    if (!_context.Impuestos.Any())
    {
        var impuestos = new List<Impuesto>
        {
            new() { Codigo = "01", Descripcion = "Impuesto al Valor Agregado", Porcentaje = 13.00m, Activo = true },
            new() { Codigo = "02", Descripcion = "Impuesto Selectivo de Consumo", Porcentaje = 0.00m, Activo = true },
            new() { Codigo = "03", Descripcion = "Impuesto Único a los Combustibles", Porcentaje = 0.00m, Activo = true },
            new() { Codigo = "04", Descripcion = "Impuesto específico de Bebidas Alcohólicas", Porcentaje = 0.00m, Activo = true },
            new() { Codigo = "05", Descripcion = "Impuesto Específico sobre las bebidas envasadas sin contenido alcohólico y jabones de tocador", Porcentaje = 0.00m, Activo = true },
            new() { Codigo = "06", Descripcion = "Impuesto a los Productos de Tabaco", Porcentaje = 0.00m, Activo = true },
            new() { Codigo = "07", Descripcion = "IVA (cálculo especial)", Porcentaje = 13.00m, Activo = true },
            new() { Codigo = "08", Descripcion = "IVA Régimen de Bienes Usados (Factor)", Porcentaje = 13.00m, Activo = true },
            new() { Codigo = "12", Descripcion = "Impuesto específico al cemento asfáltico", Porcentaje = 0.00m, Activo = true },
            new() { Codigo = "99", Descripcion = "Otros", Porcentaje = 0.00m, Activo = true }
        };

        _context.Impuestos.AddRange(impuestos);
        await _context.SaveChangesAsync();
    }
}
```

#### Actualizado método SeedAsync (línea 39)
```csharp
// ANTES:
await CheckUnidadesMedidaAsync();
await CheckCodigosExoneracionAsync();

// DESPUÉS:
await CheckUnidadesMedidaAsync();
await CheckImpuestosAsync();        // ✅ Agregado
await CheckCodigosExoneracionAsync();
```

## Archivos Creados

### `/mnt/d/Proyectos/2/Facturacion/Scripts/SeedImpuestos.sql`

Script SQL para insertar manualmente el catálogo de impuestos si es necesario:

```sql
-- Script para insertar catálogo de Impuestos de Hacienda Costa Rica
-- Ejecutar solo si la tabla Impuestos está vacía

IF NOT EXISTS (SELECT 1 FROM Impuestos)
BEGIN
    INSERT INTO Impuestos (Codigo, Descripcion, Porcentaje, Activo)
    VALUES
        ('01', 'Impuesto al Valor Agregado', 13.00, 1),
        ('02', 'Impuesto Selectivo de Consumo', 0.00, 1),
        -- ... resto de impuestos
END
GO
```

## Catálogos de Hacienda Implementados

### Unidades de Medida (según catálogo oficial)
- **Sp** - Servicios Profesionales
- **m** - Metro
- **kg** - Kilogramo
- **Unid** - Unidad
- **Lts** - Litros
- **Svc** - Servicio
- Y 29 unidades más...

### Códigos de Impuesto (según catálogo oficial v4.4)
- **01** - Impuesto al Valor Agregado (13%)
- **02** - Impuesto Selectivo de Consumo
- **03** - Impuesto Único a los Combustibles
- **04** - Impuesto específico de Bebidas Alcohólicas
- **05** - Impuesto Específico sobre bebidas envasadas sin alcohol y jabones
- **06** - Impuesto a los Productos de Tabaco
- **07** - IVA (cálculo especial) (13%)
- **08** - IVA Régimen de Bienes Usados (13%)
- **12** - Impuesto específico al cemento asfáltico
- **99** - Otros

## Cómo Probar la Solución

### Opción 1: Reiniciar la aplicación (Recomendado)

Si la base de datos se recrea o se ejecuta el seeder:

```bash
# Detener el backend
# Reiniciar el backend
# El seeder automáticamente insertará los datos
```

### Opción 2: Ejecutar el script SQL manualmente

Si la base de datos ya existe y solo faltan los impuestos:

```bash
# Ejecutar el script en SQL Server Management Studio o Azure Data Studio
/mnt/d/Proyectos/2/Facturacion/Scripts/SeedImpuestos.sql
```

### Opción 3: Verificar datos existentes

Conectarse a la base de datos y verificar:

```sql
-- Verificar Unidades de Medida
SELECT COUNT(*) as Total FROM UnidadesMedida;
SELECT * FROM UnidadesMedida WHERE Activo = 1 ORDER BY Codigo;

-- Verificar Impuestos
SELECT COUNT(*) as Total FROM Impuestos;
SELECT * FROM Impuestos WHERE Activo = 1 ORDER BY Codigo;
```

## Verificación del Frontend

1. Abrir la página de Productos en el navegador
2. Hacer clic en "Nuevo Producto/Servicio"
3. Verificar que el dropdown "Unidad de Medida" muestra opciones como:
   - `Sp - Servicios Profesionales`
   - `Unid - Unidad`
   - `Svc - Servicio`
   - etc.
4. Verificar que el dropdown "Impuesto" muestra opciones como:
   - `Impuesto al Valor Agregado`
   - `Impuesto Selectivo de Consumo`
   - etc.

## Flujo de Datos

```
Frontend (Productos.cshtml)
    ↓
    JavaScript: loadUnidadesMedida() / loadImpuestos()
    ↓
    AJAX: GET ?handler=UnidadesMedida / ?handler=Impuestos
    ↓
PageModel (Productos.cshtml.cs)
    ↓
    OnGetUnidadesMedidaAsync() / OnGetImpuestosAsync()
    ↓
    HTTP: GET /api/catalogos/unidades-medida
    HTTP: GET /api/catalogos/impuestos
    ↓
Backend Controller (CatalogosController.cs)
    ↓
    GetUnidadesMedida() / GetImpuestos()
    ↓
    DataContext → Consulta a base de datos
    ↓
    Retorna: [{ id, codigo, descripcion }, ...]
    ↓
Frontend JavaScript
    ↓
    Construye <option> con value="${item.id}"
```

## Notas Importantes

1. **Formato de respuesta**: Los endpoints ahora retornan directamente el array de datos (sin wrapper `{ success: true, data: [...] }`), lo cual es más consistente con el resto de la API.

2. **Serialización JSON**: ASP.NET Core por defecto serializa propiedades en camelCase, por lo que `Id` → `id`, `Codigo` → `codigo`, etc.

3. **Datos del seeder**: Los datos de Unidades de Medida e Impuestos se insertan automáticamente cuando se ejecuta el seeder al iniciar la aplicación por primera vez.

4. **Validación**: Los campos UnidadMedidaId e ImpuestoId son requeridos en el formulario de productos, por lo que es crítico que estos catálogos estén poblados.

## Estado de Compilación

✅ Backend compilado exitosamente sin errores
✅ Solo warnings de nullable reference (no afectan funcionalidad)
✅ Frontend no requiere cambios (ya estaba correctamente implementado)

## Referencias

- Catálogo oficial de Hacienda Costa Rica v4.4
- Anexo 4.2 - Códigos de impuestos
- Anexo 4.3 - Unidades de medida comercial
