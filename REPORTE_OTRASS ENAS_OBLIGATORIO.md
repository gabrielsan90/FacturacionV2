# REPORTE: Campo "Otras Señas" (OtrasSenas) - Obligatoriedad según Hacienda v4.4

## Fecha de Investigación
29 de noviembre de 2025

## Resumen Ejecutivo
Se verificó la obligatoriedad del campo "Otras Señas" (OtrasSenas) en el sistema de facturación electrónica v4.4 de Costa Rica. **CONCLUSIÓN: El campo ES OBLIGATORIO** según el esquema XSD oficial de Hacienda.

---

## 1. HALLAZGOS DE LA INVESTIGACIÓN

### 1.1 Esquema XSD Oficial de Hacienda v4.4

**Archivo**: `/mnt/d/Proyectos/2/Facturacion/4.4/FacturaElectronica_V4.4.xsd`

**Tipo Complejo**: `UbicacionType`

**Campos definidos**:
```
Provincia:   OBLIGATORIO (minOccurs=1)
Canton:      OBLIGATORIO (minOccurs=1)
Distrito:    OBLIGATORIO (minOccurs=1)
Barrio:      OPCIONAL    (minOccurs=0)
OtrasSenas:  OBLIGATORIO (minOccurs=1) <-- CONFIRMADO
```

**Longitud máxima**: 250 caracteres (según `<xs:maxLength value="250"/>`)

### 1.2 Documentación Oficial

Según `/mnt/d/Proyectos/2/Facturacion/DOCUMENTACION_CAMPOS_V44.md`:

| Campo | FE | TE | NC | ND | FEC | FEE | REP |
|-------|----|----|----|----|-----|-----|-----|
| Ubicacion del Receptor | 1 | 3 | 2 | 2 | 2 | 2 | 4 |

**Condición 1 = DATO OBLIGATORIO**: El dato DEBE estar en el documento SIEMPRE

**Nota importante**:
- "Ubicacion" se deshabilita (condición 4) cuando el receptor es "Extranjero No Domiciliado"
- En ese caso se usa el campo "Otras Señas Extranjero"

### 1.3 Estado Previo del Sistema

**Cliente.cs** (INCORRECTO):
```csharp
[MaxLength(500)]  // <-- Longitud incorrecta (debe ser 250)
public string? OtrasSenas { get; set; }  // <-- Nullable (debe ser required)
```

**UbicacionType.cs** (INCORRECTO):
```csharp
[StringLength(250)]
public string? OtrasSenas { get; set; }  // <-- Nullable (debe ser required)
```

**Facturacion.cshtml**:
```html
<label>Otras Señas <span class="text-danger">*</span></label>
<textarea id="ClienteOtrasSenas" maxlength="500" required>  <!-- Marcado como required pero maxlength incorrecto -->
```

**Validación JavaScript**: Presente y correcta

---

## 2. CAMBIOS REALIZADOS

### 2.1 Entidad Cliente.cs

**Archivo**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/Entities/Cliente.cs`

```csharp
// ANTES:
[Display(Name = "Otras Señas")]
[MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
public string? OtrasSenas { get; set; }

// DESPUÉS:
[Display(Name = "Otras Señas")]
[MaxLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
[Required(ErrorMessage = "El campo {0} es obligatorio según Hacienda v4.4.")]
public string OtrasSenas { get; set; } = null!;
```

**Cambios**:
1. Reducida longitud máxima de 500 a 250 caracteres (según XSD)
2. Agregado atributo `[Required]`
3. Cambiado de nullable (`string?`) a non-nullable (`string`)
4. Inicializador con `= null!`

### 2.2 UbicacionType.cs

**Archivo**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/Entities/DocumentosElectronicos/V44/TiposComunes/UbicacionType.cs`

```csharp
// ANTES:
/// <summary>
/// Otras señas de la ubicación exacta
/// </summary>
[XmlElement("OtrasSenas")]
[StringLength(250)]
public string? OtrasSenas { get; set; }

// DESPUÉS:
/// <summary>
/// Otras señas de la ubicación exacta (OBLIGATORIO según XSD v4.4)
/// </summary>
[XmlElement("OtrasSenas")]
[Required(ErrorMessage = "Las otras señas son obligatorias según Hacienda v4.4")]
[StringLength(250, ErrorMessage = "Las otras señas no pueden exceder 250 caracteres")]
public string OtrasSenas { get; set; } = null!;
```

**Cambios**:
1. Actualizada documentación XML
2. Agregado atributo `[Required]`
3. Agregado mensaje de error en `[StringLength]`
4. Cambiado de nullable a non-nullable

### 2.3 Formulario Facturacion.cshtml

**Archivo**: `/mnt/d/Proyectos/2/Facturacion/Facturacion.Frontend/Pages/Facturacion.cshtml`

```html
<!-- ANTES: -->
<label for="ClienteOtrasSenas" class="form-label">
    Otras Señas <span class="text-danger">*</span>
    <i class="fas fa-info-circle text-info" data-bs-toggle="tooltip"
       title="Dirección exacta: Barrio, número de casa, referencias"></i>
</label>
<textarea class="form-control" id="ClienteOtrasSenas" rows="2"
          maxlength="500" required
          placeholder="Ejemplo: Del Más x Menos 100 metros al norte, casa amarilla portón negro"></textarea>
<div class="invalid-feedback">Las otras señas son obligatorias.</div>
<small class="text-muted">Dirección exacta requerida para facturación electrónica Costa Rica</small>

<!-- DESPUÉS: -->
<label for="ClienteOtrasSenas" class="form-label">
    Otras Señas <span class="text-danger">*</span>
    <i class="fas fa-info-circle text-info" data-bs-toggle="tooltip"
       title="Dirección exacta: Barrio, número de casa, referencias. OBLIGATORIO según XSD Hacienda v4.4"></i>
</label>
<textarea class="form-control" id="ClienteOtrasSenas" rows="2"
          maxlength="250" required
          placeholder="Ejemplo: Del Más x Menos 100 metros al norte, casa amarilla portón negro"></textarea>
<div class="invalid-feedback">Las otras señas son obligatorias según Hacienda v4.4.</div>
<small class="text-muted">Dirección exacta requerida (máx. 250 caracteres según XSD)</small>
```

**Cambios**:
1. Actualizado tooltip para indicar que es obligatorio según XSD
2. Reducido `maxlength` de 500 a 250
3. Actualizado mensaje de error
4. Actualizado texto de ayuda

### 2.4 Validación JavaScript

```javascript
// ANTES:
if (!$('#ClienteOtrasSenas').val()) {
    Swal.fire('Validación', 'Las otras señas (dirección exacta) son obligatorias.', 'warning');
    return false;
}

// DESPUÉS:
// Validar OtrasSenas (OBLIGATORIO según XSD v4.4: minOccurs=1, maxLength=250)
if (!$('#ClienteOtrasSenas').val()) {
    Swal.fire('Validación', 'Las otras señas (dirección exacta) son obligatorias según Hacienda v4.4.', 'warning');
    return false;
}
```

**Cambios**:
1. Agregado comentario explicativo con referencia al XSD
2. Actualizado mensaje de validación

---

## 3. CASOS ESPECIALES

### 3.1 Receptores Extranjeros No Domiciliados

Según la documentación:
- Cuando el tipo de identificación es "Extranjero No Domiciliado" (código 05)
- El campo "Ubicacion" completo se DESHABILITA (condición 4)
- En su lugar se usa "Otras Señas Extranjero" (campo diferente, condición 2 - condicional)

**Implicación**: El campo "OtrasSenas" dentro de "Ubicacion" NO aplica para extranjeros no domiciliados, porque toda la sección "Ubicacion" se omite.

### 3.2 Tiquetes Electrónicos (TE)

- "Ubicacion del Receptor": Condición 3 (OPCIONAL)
- El TE puede ir SIN receptor completamente
- Si se incluye receptor, las validaciones normales aplican

### 3.3 Recibo Electrónico de Pago (REP)

- "Ubicacion del Receptor": Condición 4 (INEXISTENTE)
- El REP NO incluye ubicación del receptor en absoluto

---

## 4. VALIDACIÓN DE CUMPLIMIENTO

### 4.1 Verificación contra XSD

```python
# Script utilizado para verificar
import xml.etree.ElementTree as ET

tree = ET.parse('/mnt/d/Proyectos/2/Facturacion/4.4/FacturaElectronica_V4.4.xsd')
root = tree.getroot()
ns = {'xs': 'http://www.w3.org/2001/XMLSchema'}

for complex_type in root.findall('.//xs:complexType[@name="UbicacionType"]', ns):
    for element in complex_type.findall('.//xs:element', ns):
        name = element.get('name')
        min_occurs = element.get('minOccurs', '1')
        print(f"{name}: minOccurs={min_occurs}")
```

**Resultado**:
```
Provincia: minOccurs=1
Canton: minOccurs=1
Distrito: minOccurs=1
Barrio: minOccurs=0
OtrasSenas: minOccurs=1  <-- CONFIRMADO OBLIGATORIO
```

### 4.2 Tipos de Documento y Obligatoriedad

| Tipo Documento | Ubicación Completa | OtrasSenas dentro de Ubicación |
|----------------|--------------------|---------------------------------|
| FE (01) | Obligatorio (1) | Obligatorio si hay ubicación |
| TE (04) | Opcional (3) | Obligatorio si hay ubicación |
| NC (03) | Condicional (2) | Obligatorio si hay ubicación |
| ND (02) | Condicional (2) | Obligatorio si hay ubicación |
| FEC (08) | Condicional (2) | Obligatorio si hay ubicación |
| FEE (09) | Condicional (2) | Obligatorio si hay ubicación |
| REP (10) | No existe (4) | No aplica |

---

## 5. ACCIONES PENDIENTES

### 5.1 Migración de Base de Datos

Se requiere crear y aplicar una migración para:
1. Cambiar columna `OtrasSenas` de nullable a NOT NULL
2. Actualizar longitud máxima de 500 a 250 caracteres
3. Proporcionar valor por defecto para registros existentes con NULL

**Comando**:
```bash
cd Facturacion.Backend
dotnet ef migrations add MakeOtrasSenasRequired --context DataContext
dotnet ef database update
```

**ADVERTENCIA**: El backend debe estar detenido antes de ejecutar la migración (actualmente hay bloqueo de archivos).

### 5.2 Actualización de Datos Existentes

Antes de aplicar la migración, se debe:
1. Identificar clientes con `OtrasSenas` NULL o vacío
2. Solicitar actualización de datos o asignar valor por defecto ("Sin especificar" o similar)

**Query SQL para verificar**:
```sql
SELECT Id, Nombre, NumeroIdentificacion, OtrasSenas
FROM Clientes
WHERE OtrasSenas IS NULL OR OtrasSenas = ''
```

### 5.3 Actualización de Documentación del Usuario

Actualizar manuales/ayudas para indicar:
- El campo "Otras Señas" es obligatorio
- Longitud máxima: 250 caracteres
- Ejemplos de valores válidos
- Excepciones (extranjeros, REP)

---

## 6. REFERENCIAS TÉCNICAS

### 6.1 Fuentes Oficiales
1. **XSD Oficial Hacienda v4.4**: `/4.4/FacturaElectronica_V4.4.xsd`
2. **Resolución**: MH-DGT-RES-0027-2024
3. **Documentación del Sistema**: `/DOCUMENTACION_CAMPOS_V44.md`

### 6.2 Archivos Modificados
1. `/Facturacion.Shared/Entities/Cliente.cs`
2. `/Facturacion.Shared/Entities/DocumentosElectronicos/V44/TiposComunes/UbicacionType.cs`
3. `/Facturacion.Frontend/Pages/Facturacion.cshtml`

### 6.3 Archivos de Prueba para Verificación
- Probar creación de cliente sin "Otras Señas" -> Debe rechazar
- Probar creación de factura con cliente sin "Otras Señas" -> Debe rechazar
- Probar con más de 250 caracteres -> Debe rechazar
- Probar con extranjero no domiciliado -> No debe requerir ubicación

---

## 7. CONCLUSIONES

1. **El campo "Otras Señas" ES OBLIGATORIO** según el XSD oficial de Hacienda v4.4
2. **La longitud máxima es 250 caracteres**, no 500
3. **Los cambios realizados alinean el sistema con las especificaciones oficiales**
4. **Se requiere migración de BD** para completar la implementación
5. **Pueden existir datos legacy** que necesitan actualización antes de la migración

---

## 8. RECOMENDACIONES

### 8.1 Inmediatas
1. Detener el backend para permitir compilación y migración
2. Ejecutar migración de base de datos
3. Actualizar registros existentes con valores NULL

### 8.2 Corto Plazo
1. Realizar pruebas exhaustivas de creación/edición de clientes
2. Verificar generación de XML contra XSD oficial
3. Probar casos especiales (extranjeros, REP)

### 8.3 Largo Plazo
1. Implementar validación dinámica basada en tipo de documento
2. Crear herramienta de verificación de cumplimiento XSD
3. Mantener sincronización con actualizaciones de Hacienda

---

**Elaborado por**: Sistema de Análisis Técnico
**Fecha**: 29 de noviembre de 2025
**Versión**: 1.0
**Estado**: Cambios aplicados, pendiente migración BD
