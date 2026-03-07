# Pendientes de Implementación - Hacienda v4.4

**Fecha de Análisis:** 21 de Enero 2026
**Versión del Sistema:** FacturacionV2
**Base:** Análisis de 76 archivos XML de ejemplo vs implementación actual

---

## 📊 Resumen Ejecutivo

### Cobertura Actual
- ✅ **Completamente implementadas:** 20/26 características (77%)
- ⚠️ **Parcialmente implementadas:** 2/26 características (8%)
- ❌ **No implementadas:** 4/26 características (15%)

### Impacto en el Negocio
Sin las implementaciones pendientes, el sistema **NO PUEDE** ser usado por:
- ❌ Gasolineras / Estaciones de servicio
- ❌ Tabacaleras / Expendios de tabaco
- ❌ Licorerías / Distribuidores de alcohol
- ❌ Fábricas de cemento
- ❌ Importadores de bebidas con impuestos selectivos
- ⚠️ Restaurantes con combos estructurados (funciona parcialmente)

---

## 🔴 PRIORIDAD 1 - BLOQUEANTES

### 1. DatosImpuestoEspecifico

**Estado:** ❌ NO IMPLEMENTADO
**Criticidad:** 🔴 ALTA (Bloqueante)
**Archivos de Ejemplo:**
- `TABACO_IVA13_FE-50604072500310108860006600066010000001053104236601.XML`
- `COMBUSTIBLE_IVA13_FE-50607072500310108860006600066010000001054107236601.XML`
- `CEMENTO_IVA13_FE-50604072500310108860006600066010000001052104236601.XML`
- `ALCOHOL_IVA13_FE-50607072500310108860006600066010000001055107236601.XML`
- `BEBIDAS_IVA13_FE-50607072500310108860006600066010000001056107236601.XML`

#### Descripción
Según Hacienda v4.4, ciertos productos (tabaco, alcohol, bebidas, cemento, combustibles) requieren información adicional del impuesto específico que se les aplica. Esta información se envía dentro del nodo `<Impuesto>` de cada línea de detalle.

#### Ejemplo XML Real (Tabaco)
```xml
<LineaDetalle>
    <NumeroLinea>1</NumeroLinea>
    <CodigoCABYS>2502000000200</CodigoCABYS>
    <Cantidad>7.000</Cantidad>
    <Detalle>TABACO</Detalle>
    <PrecioUnitario>3000.00000</PrecioUnitario>
    <MontoTotal>21000.00000</MontoTotal>
    <SubTotal>21000.00000</SubTotal>

    <!-- IVA 13% -->
    <Impuesto>
        <Codigo>01</Codigo>
        <CodigoTarifaIVA>08</CodigoTarifaIVA>
        <Tarifa>13.00</Tarifa>
        <Monto>2730.00000</Monto>
    </Impuesto>

    <!-- IMPUESTO ESPECIAL TABACO (código 06) -->
    <Impuesto>
        <Codigo>06</Codigo>
        <Tarifa>0.00</Tarifa>
        <DatosImpuestoEspecifico>
            <CantidadUnidadMedida>3.00</CantidadUnidadMedida>
            <ImpuestoUnidad>790.00000</ImpuestoUnidad>
        </DatosImpuestoEspecifico>
        <Monto>16590.00000</Monto>
    </Impuesto>

    <ImpuestoNeto>19320.00000</ImpuestoNeto>
    <MontoTotalLinea>40320.00000</MontoTotalLinea>
</LineaDetalle>
```

#### Campos Requeridos para DatosImpuestoEspecifico

| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| `Tipo` | string(1) | Condicional | Tipo de cálculo: "C"=Por cantidad, "P"=Porcentual |
| `CodigoCABYS` | string(13) | NO | Código CAByS del producto gravado |
| `CodigoPartida` | string(6) | NO | Código de partida arancelaria |
| `CantidadUnidadMedida` | decimal(18,5) | Condicional | Cantidad base para el cálculo (cuando Tipo="C") |
| `UnidadMedidaBaseImponible` | string(20) | Condicional | Unidad de medida base (cuando Tipo="C") |
| `MontoBaseImponible` | decimal(18,5) | NO | Monto base sobre el que se calcula |
| `PorcentajeMaximoDescuento` | decimal(5,2) | NO | Porcentaje máximo de descuento permitido |
| `TarifaEspecifica` | decimal(18,5) | Condicional | Tarifa específica por unidad (cuando Tipo="C") |
| `ImpuestoUnidad` | decimal(18,5) | Condicional | Alias de TarifaEspecifica (usado en ejemplos) |

#### Implementación Sugerida

##### Paso 1: Actualizar Entidad `DocumentoDetalleImpuesto`

**Archivo:** `Facturacion.Shared/Entities/DocumentoDetalleImpuesto.cs`

```csharp
// Después de la línea 159 (después de PorcentajeExoneracion)

// ========================================
// DATOS IMPUESTO ESPECÍFICO (v4.4 - FASE 2)
// ========================================

/// <summary>
/// Indica si este impuesto tiene datos de impuesto específico
/// Aplica para: Tabaco (06), Combustible (03), Alcohol (04), Bebidas (05), Cemento (12)
/// </summary>
[Display(Name = "Tiene Impuesto Específico")]
public bool TieneDatosImpuestoEspecifico { get; set; }

/// <summary>
/// Tipo de cálculo del impuesto específico
/// C = Por cantidad (usa TarifaEspecifica * CantidadUnidadMedida)
/// P = Porcentual (usa Tarifa * MontoBaseImponible)
/// </summary>
[Display(Name = "Tipo Cálculo Impuesto Específico")]
[MaxLength(1, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
public string? TipoCalculoImpuestoEspecifico { get; set; }

/// <summary>
/// Código CAByS del producto sujeto al impuesto específico
/// </summary>
[Display(Name = "Código CAByS Impuesto Específico")]
[MaxLength(13, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
public string? CodigoCABYSImpuestoEspecifico { get; set; }

/// <summary>
/// Código de partida arancelaria para impuestos específicos
/// </summary>
[Display(Name = "Código Partida Impuesto Específico")]
[MaxLength(6, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
public string? CodigoPartidaImpuestoEspecifico { get; set; }

/// <summary>
/// Cantidad base para el cálculo del impuesto específico (cuando Tipo = "C")
/// Ejemplo: 10 unidades, 5 litros, etc.
/// </summary>
[Display(Name = "Cantidad Unidad Medida")]
[Column(TypeName = "decimal(18, 5)")]
public decimal? CantidadUnidadMedida { get; set; }

/// <summary>
/// Unidad de medida base imponible (L, Unid, Kg, etc.)
/// </summary>
[Display(Name = "Unidad Medida Base Imponible")]
[MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
public string? UnidadMedidaBaseImponible { get; set; }

/// <summary>
/// Monto base sobre el que se calcula el impuesto específico
/// </summary>
[Display(Name = "Monto Base Imponible Específico")]
[Column(TypeName = "decimal(18, 5)")]
public decimal? MontoBaseImponibleEspecifico { get; set; }

/// <summary>
/// Porcentaje máximo de descuento permitido para este producto
/// </summary>
[Display(Name = "Porcentaje Máximo Descuento")]
[Column(TypeName = "decimal(5, 2)")]
public decimal? PorcentajeMaximoDescuento { get; set; }

/// <summary>
/// Tarifa específica por unidad (cuando Tipo = "C")
/// Ejemplo: ₡132.00 por cajetilla de cigarros
/// </summary>
[Display(Name = "Tarifa Específica")]
[Column(TypeName = "decimal(18, 5)")]
public decimal? TarifaEspecifica { get; set; }
```

##### Paso 2: Crear Migración

```bash
cd Facturacion.Backend
dotnet ef migrations add AddDatosImpuestoEspecifico
dotnet ef database update
```

##### Paso 3: Actualizar DTO `CreateDocumentoDetalleImpuestoDTO`

**Archivo:** `Facturacion.Shared/DTOs/CreateDocumentoDTO.cs`

```csharp
// Dentro de la clase CreateDocumentoDetalleImpuestoDTO, después de MontoExoneracion (línea 227)

// Datos de impuesto específico (v4.4)
public bool TieneDatosImpuestoEspecifico { get; set; }

[MaxLength(1)]
public string? TipoCalculoImpuestoEspecifico { get; set; }

[MaxLength(13)]
public string? CodigoCABYSImpuestoEspecifico { get; set; }

[MaxLength(6)]
public string? CodigoPartidaImpuestoEspecifico { get; set; }

public decimal? CantidadUnidadMedida { get; set; }

[MaxLength(20)]
public string? UnidadMedidaBaseImponible { get; set; }

public decimal? MontoBaseImponibleEspecifico { get; set; }

public decimal? PorcentajeMaximoDescuento { get; set; }

public decimal? TarifaEspecifica { get; set; }
```

##### Paso 4: Actualizar DocumentoService.cs

**Archivo:** `Facturacion.Backend/Services/Implementations/DocumentoService.cs`

En el método `CrearDocumentoDesdeDTO`, después de la línea 567 (después de `PorcentajeExoneracion`):

```csharp
// Datos de impuesto específico (v4.4 - FASE 2)
TieneDatosImpuestoEspecifico = impDTO.TieneDatosImpuestoEspecifico,
TipoCalculoImpuestoEspecifico = impDTO.TipoCalculoImpuestoEspecifico,
CodigoCABYSImpuestoEspecifico = impDTO.CodigoCABYSImpuestoEspecifico,
CodigoPartidaImpuestoEspecifico = impDTO.CodigoPartidaImpuestoEspecifico,
CantidadUnidadMedida = impDTO.CantidadUnidadMedida,
UnidadMedidaBaseImponible = impDTO.UnidadMedidaBaseImponible,
MontoBaseImponibleEspecifico = impDTO.MontoBaseImponibleEspecifico,
PorcentajeMaximoDescuento = impDTO.PorcentajeMaximoDescuento,
TarifaEspecifica = impDTO.TarifaEspecifica,
```

##### Paso 5: Actualizar Frontend - Facturacion.cshtml

**Archivo:** `Facturacion.Frontend/Pages/Facturacion.cshtml`

Después de la sección de exoneración en el modal de impuestos (aproximadamente línea 920):

```html
<!-- Datos de Impuesto Específico (v4.4) -->
<div class="form-group mt-3" id="divImpuestoEspecifico" style="display:none;">
    <div class="card bg-light">
        <div class="card-body">
            <h6 class="card-title">📋 Datos Impuesto Específico</h6>
            <small class="text-muted">
                Para productos con impuestos especiales: Tabaco, Combustible, Alcohol, Bebidas, Cemento
            </small>

            <div class="row mt-2">
                <div class="col-md-4">
                    <label>Tipo Cálculo</label>
                    <select class="form-select form-select-sm" id="TipoCalculoImpuestoEspecifico">
                        <option value="">-- Seleccione --</option>
                        <option value="C">C - Por Cantidad</option>
                        <option value="P">P - Porcentual</option>
                    </select>
                </div>
                <div class="col-md-4" id="divTarifaEspecifica" style="display:none;">
                    <label>Tarifa Específica</label>
                    <input type="text" class="form-control form-control-sm"
                           id="TarifaEspecifica" placeholder="0.00">
                    <small class="text-muted">Por unidad (Tipo C)</small>
                </div>
                <div class="col-md-4" id="divCantidadUnidadMedida" style="display:none;">
                    <label>Cantidad Unidades</label>
                    <input type="text" class="form-control form-control-sm"
                           id="CantidadUnidadMedida" placeholder="0.00">
                </div>
            </div>
        </div>
    </div>
</div>

<script>
// Mostrar/ocultar sección de impuesto específico según código de impuesto
function onTaxTypeChange(taxIndex, codigoImpuesto) {
    const impuestosEspeciales = ['02', '03', '04', '05', '06', '12']; // Selectivo, Combustible, Alcohol, Bebidas, Tabaco, Cemento

    if (impuestosEspeciales.includes(codigoImpuesto)) {
        $('#divImpuestoEspecifico').show();
    } else {
        $('#divImpuestoEspecifico').hide();
    }
}

// Mostrar campos según tipo de cálculo
$('#TipoCalculoImpuestoEspecifico').change(function() {
    const tipo = $(this).val();
    if (tipo === 'C') {
        $('#divTarifaEspecifica, #divCantidadUnidadMedida').show();
    } else {
        $('#divTarifaEspecifica, #divCantidadUnidadMedida').hide();
    }
});
</script>
```

##### Paso 6: Servicio de Generación de XML

**Archivo:** `Facturacion.Backend/Services/Implementations/XmlGeneradorService.cs`

En el método que genera las líneas de detalle, agregar después de generar el nodo `<Impuesto>`:

```csharp
// Si tiene datos de impuesto específico
if (impuesto.TieneDatosImpuestoEspecifico)
{
    var datosImpEsp = new XElement(ns + "DatosImpuestoEspecifico");

    if (!string.IsNullOrWhiteSpace(impuesto.TipoCalculoImpuestoEspecifico))
        datosImpEsp.Add(new XElement(ns + "Tipo", impuesto.TipoCalculoImpuestoEspecifico));

    if (!string.IsNullOrWhiteSpace(impuesto.CodigoCABYSImpuestoEspecifico))
        datosImpEsp.Add(new XElement(ns + "CodigoCABYS", impuesto.CodigoCABYSImpuestoEspecifico));

    if (!string.IsNullOrWhiteSpace(impuesto.CodigoPartidaImpuestoEspecifico))
        datosImpEsp.Add(new XElement(ns + "CodigoPartida", impuesto.CodigoPartidaImpuestoEspecifico));

    if (impuesto.CantidadUnidadMedida.HasValue)
        datosImpEsp.Add(new XElement(ns + "CantidadUnidadMedida",
            impuesto.CantidadUnidadMedida.Value.ToString("F5", CultureInfo.InvariantCulture)));

    if (!string.IsNullOrWhiteSpace(impuesto.UnidadMedidaBaseImponible))
        datosImpEsp.Add(new XElement(ns + "UnidadMedidaBaseImponible", impuesto.UnidadMedidaBaseImponible));

    if (impuesto.MontoBaseImponibleEspecifico.HasValue)
        datosImpEsp.Add(new XElement(ns + "MontoBaseImponible",
            impuesto.MontoBaseImponibleEspecifico.Value.ToString("F5", CultureInfo.InvariantCulture)));

    if (impuesto.PorcentajeMaximoDescuento.HasValue)
        datosImpEsp.Add(new XElement(ns + "PorcentajeMaximoDescuento",
            impuesto.PorcentajeMaximoDescuento.Value.ToString("F2", CultureInfo.InvariantCulture)));

    if (impuesto.TarifaEspecifica.HasValue)
        datosImpEsp.Add(new XElement(ns + "TarifaEspecifica",
            impuesto.TarifaEspecifica.Value.ToString("F5", CultureInfo.InvariantCulture)));

    impuestoElement.Add(datosImpEsp);
}
```

#### Casos de Prueba

1. **Tabaco (Código 06)**
   - Producto: Cigarrillos marca X
   - Cantidad: 10 cajetillas
   - Precio: ₡3,000.00 c/u
   - IVA 13%: ₡3,900.00
   - Impuesto Tabaco: 3 unidades × ₡790.00 = ₡2,370.00
   - Total línea: ₡36,270.00

2. **Combustible (Código 03)**
   - Producto: Gasolina Super
   - Cantidad: 50 litros
   - Precio: ₡800.00/litro
   - IVA 13%: ₡5,200.00
   - Impuesto Combustible: ₡250.00/litro × 50L = ₡12,500.00
   - Total línea: ₡57,700.00

---

### 2. ImpuestoAsumidoEmisorFabrica

**Estado:** ❌ NO IMPLEMENTADO
**Criticidad:** 🔴 ALTA (Bloqueante)
**Archivos de Ejemplo:**
- `COMBUSTIBLE_IVA13_FE-50607072500310108860006600066010000001054107236601.XML`
- `IVA_COBRADO_FABRICA_01_FE-50614072500310108860006600066010000001127104236601.XML`
- `IVA_COBRADO_FABRICA_02_FE-50614072500310108860006600066010000001120104236601.XML`

#### Descripción
Permite que el emisor (fábrica, gasolinera) asuma el pago del impuesto en lugar de trasladarlo al cliente. El monto se deduce del total de impuestos.

#### Ejemplo XML
```xml
<Impuesto>
    <Codigo>03</Codigo>
    <Tarifa>0.00</Tarifa>
    <Monto>5000.00000</Monto>
    <ImpuestoAsumidoEmisorFabrica>5000.00000</ImpuestoAsumidoEmisorFabrica>
</Impuesto>
<ImpuestoAsumidoEmisorFabrica>5000.00000</ImpuestoAsumidoEmisorFabrica>
<ImpuestoNeto>0.00000</ImpuestoNeto>
```

#### Implementación

##### Paso 1: Actualizar Entidad

**Archivo:** `Facturacion.Shared/Entities/DocumentoDetalleImpuesto.cs`

```csharp
// Después de TarifaEspecifica

/// <summary>
/// Monto del impuesto asumido por el emisor o fábrica
/// No se le cobra al cliente, lo paga el emisor
/// </summary>
[Display(Name = "Impuesto Asumido Emisor/Fábrica")]
[Column(TypeName = "decimal(18, 5)")]
public decimal? ImpuestoAsumidoEmisorFabrica { get; set; }
```

##### Paso 2: Actualizar Entidad DocumentoDetalle

**Archivo:** `Facturacion.Shared/Entities/DocumentoDetalle.cs`

```csharp
// Después de ImpuestoNeto (línea 192)

/// <summary>
/// Total de impuestos asumidos por el emisor/fábrica en esta línea
/// </summary>
[Display(Name = "Impuesto Asumido Emisor/Fábrica")]
[Column(TypeName = "decimal(18, 5)")]
public decimal? ImpuestoAsumidoEmisorFabrica { get; set; }
```

##### Paso 3: Actualizar Entidad Documento

**Archivo:** `Facturacion.Shared/Entities/Documento.cs`

Buscar la sección de totales y agregar:

```csharp
/// <summary>
/// Total de impuestos asumidos por el emisor/fábrica en todo el documento
/// </summary>
[Display(Name = "Total Impuesto Asumido Emisor/Fábrica")]
[Column(TypeName = "decimal(18, 5)")]
public decimal TotalImpuestoAsumidoEmisorFabrica { get; set; }
```

##### Paso 4: Actualizar DTOs

```csharp
// En CreateDocumentoDetalleImpuestoDTO
public decimal? ImpuestoAsumidoEmisorFabrica { get; set; }
```

##### Paso 5: Actualizar Cálculos en DocumentoService

```csharp
private void CalcularTotalesDetalle(DocumentoDetalle detalle)
{
    // ... código existente ...

    // Calcular impuestos asumidos por emisor
    if (detalle.Impuestos != null && detalle.Impuestos.Any())
    {
        detalle.ImpuestoAsumidoEmisorFabrica = detalle.Impuestos
            .Sum(i => i.ImpuestoAsumidoEmisorFabrica ?? 0);

        // El ImpuestoNeto es el impuesto total menos el asumido
        detalle.ImpuestoNeto = detalle.MontoImpuesto - (detalle.ImpuestoAsumidoEmisorFabrica ?? 0);
    }

    // Monto total de la línea = Subtotal + ImpuestoNeto (no incluye impuesto asumido)
    detalle.MontoTotalLinea = Math.Round(detalle.Subtotal + (detalle.ImpuestoNeto ?? detalle.MontoImpuesto), 5);
}

public void CalcularTotales(Documento documento)
{
    // ... código existente ...

    // Total de impuestos asumidos
    documento.TotalImpuestoAsumidoEmisorFabrica = Math.Round(
        documento.Detalles.Sum(d => d.ImpuestoAsumidoEmisorFabrica ?? 0), 5);

    // El TotalImpuesto que se muestra al cliente NO incluye los asumidos
    // (ya está calculado correctamente si usamos ImpuestoNeto)
}
```

##### Paso 6: Migración

```bash
dotnet ef migrations add AddImpuestoAsumidoEmisorFabrica
dotnet ef database update
```

---

## 🟡 PRIORIDAD 2 - IMPORTANTES

### 3. Surtidos con Líneas Anidadas (DetalleSurtido)

**Estado:** ⚠️ PARCIAL (Solo texto descriptivo)
**Criticidad:** 🟡 MEDIA
**Archivos de Ejemplo:**
- `FE con surtidos-descuentos-100porc_exoneracion.xml`
- `FE con surtidos-descuentos-50porc_exoneracion.xml`
- `00100045040000000265_FACTURA_ surtidos_y_descuentos.XML`

#### Descripción
Los surtidos o combos permiten agrupar varios productos en un solo ítem de factura, con cálculo de impuestos individuales para cada componente.

**Implementación Actual:**
- ✅ Campo `DetalleSurtido` (texto libre) en `DocumentoDetalle`
- ❌ NO hay estructura de líneas anidadas

**Implementación Requerida:**
- Crear entidad `DocumentoDetalleSurtido` con relación 1:N a `DocumentoDetalle`
- Cada línea de surtido tiene su propio:
  - CAByS
  - Cantidad
  - Precio
  - Descuentos
  - Impuestos

#### Ejemplo XML
```xml
<LineaDetalle>
    <NumeroLinea>1</NumeroLinea>
    <CodigoCABYS>2399999002200</CodigoCABYS>
    <Detalle>Combo: atún en agua, atún aceite y galletas</Detalle>
    <PrecioUnitario>1000.00000</PrecioUnitario>

    <DetalleSurtido>
        <!-- Componente 1 -->
        <LineaDetalleSurtido>
            <CodigoCABYSSurtido>2124203019900</CodigoCABYSSurtido>
            <CantidadSurtido>1.000</CantidadSurtido>
            <DetalleSurtido>Atún en agua</DetalleSurtido>
            <PrecioUnitarioSurtido>300.00000</PrecioUnitarioSurtido>
            <MontoTotalSurtido>300.00000</MontoTotalSurtido>
            <ImpuestoSurtido>
                <CodigoImpuestoSurtido>01</CodigoImpuestoSurtido>
                <CodigoTarifaIVASurtido>08</CodigoTarifaIVASurtido>
                <TarifaSurtido>13.00</TarifaSurtido>
                <MontoImpuestoSurtido>39.00000</MontoImpuestoSurtido>
            </ImpuestoSurtido>
        </LineaDetalleSurtido>

        <!-- Componente 2 -->
        <LineaDetalleSurtido>
            <CodigoCABYSSurtido>2124203020200</CodigoCABYSSurtido>
            <CantidadSurtido>1.000</CantidadSurtido>
            <DetalleSurtido>Atún en aceite</DetalleSurtido>
            <PrecioUnitarioSurtido>400.00000</PrecioUnitarioSurtido>
            <MontoTotalSurtido>400.00000</MontoTotalSurtido>
            <ImpuestoSurtido>
                <CodigoImpuestoSurtido>01</CodigoImpuestoSurtido>
                <CodigoTarifaIVASurtido>02</CodigoTarifaIVASurtido>
                <TarifaSurtido>1.00</TarifaSurtido>
                <MontoImpuestoSurtido>4.00000</MontoImpuestoSurtido>
            </ImpuestoSurtido>
        </LineaDetalleSurtido>
    </DetalleSurtido>

    <MontoTotalLinea>743.00000</MontoTotalLinea>
</LineaDetalle>
```

#### Implementación

##### Crear Nueva Entidad

**Archivo:** `Facturacion.Shared/Entities/DocumentoDetalleSurtido.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Líneas de surtido dentro de un detalle de documento
/// Para productos combo/paquete (v4.4 - M1)
/// </summary>
public class DocumentoDetalleSurtido
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DocumentoDetalleId { get; set; }

    [Required]
    public int NumeroLineaSurtido { get; set; }

    [Required]
    [MaxLength(13)]
    public string CodigoCABYSSurtido { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18, 3)")]
    public decimal CantidadSurtido { get; set; }

    [Required]
    [MaxLength(20)]
    public string UnidadMedidaSurtido { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string DetalleSurtido { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal PrecioUnitarioSurtido { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoTotalSurtido { get; set; }

    [Column(TypeName = "decimal(18, 5)")]
    public decimal MontoDescuentoSurtido { get; set; }

    [MaxLength(2)]
    public string? CodigoDescuentoSurtido { get; set; }

    [Column(TypeName = "decimal(18, 5)")]
    public decimal SubTotalSurtido { get; set; }

    [Column(TypeName = "decimal(18, 5)")]
    public decimal BaseImponibleSurtido { get; set; }

    // Impuesto del surtido (solo un impuesto por línea de surtido según ejemplos)
    [MaxLength(2)]
    public string? CodigoImpuestoSurtido { get; set; }

    [MaxLength(2)]
    public string? CodigoTarifaIVASurtido { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TarifaSurtido { get; set; }

    [Column(TypeName = "decimal(18, 5)")]
    public decimal? MontoImpuestoSurtido { get; set; }

    // Audit
    public DateTime FechaCreacion { get; set; }
    public string? UsuarioCreacionId { get; set; }

    // Navigation
    public DocumentoDetalle? DocumentoDetalle { get; set; }
    public User? UsuarioCreacion { get; set; }
}
```

##### Actualizar DocumentoDetalle

```csharp
// Agregar al final de las navigation properties
public ICollection<DocumentoDetalleSurtido> LineasSurtido { get; set; }
    = new List<DocumentoDetalleSurtido>();
```

##### Configurar en DataContext

```csharp
// En OnModelCreating
modelBuilder.Entity<DocumentoDetalleSurtido>()
    .HasOne(d => d.DocumentoDetalle)
    .WithMany(dd => dd.LineasSurtido)
    .HasForeignKey(d => d.DocumentoDetalleId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<DocumentoDetalleSurtido>()
    .HasOne(d => d.UsuarioCreacion)
    .WithMany()
    .HasForeignKey(d => d.UsuarioCreacionId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

### 4. Mensaje Receptor (REP)

**Estado:** ❌ NO IMPLEMENTADO
**Criticidad:** 🟡 MEDIA
**Archivos de Ejemplo:**
- `REP01_REP-50607072500310108860006600066100000000024107236601.XML`
- `REP02_REP-50607072500310108860006600066100000000025107236601.XML`
- `REP03_REP-50607072500310108860006600066100000000025107236601.XML`

#### Descripción
El Mensaje Receptor (REP) es el documento que el receptor de una factura envía a Hacienda para aceptar, aceptar parcialmente o rechazar un comprobante electrónico.

#### Estados de Respuesta
- **1:** Aceptado
- **2:** Aceptado parcialmente
- **3:** Rechazado

#### Implementación

Esta es una funcionalidad completa que requiere:
1. Nueva entidad `MensajeReceptor`
2. Servicio para generar XML de mensaje receptor
3. Endpoint API para crear y enviar mensaje receptor
4. Interfaz en frontend para que usuarios respondan a facturas recibidas
5. Integración con API de Hacienda para envío

**Complejidad:** ALTA
**Tiempo estimado:** 2-3 sprints

---

## 📋 Plan de Implementación Sugerido

### Sprint 1: Impuestos Específicos (2 semanas)
- ✅ Día 1-2: Actualizar entidades y crear migración
- ✅ Día 3-4: Actualizar DTOs y servicio backend
- ✅ Día 5-7: Implementar UI en frontend
- ✅ Día 8-9: Implementar generación XML
- ✅ Día 10: Testing con productos de tabaco, combustible, cemento

### Sprint 2: Impuesto Asumido (1 semana)
- ✅ Día 1-2: Actualizar entidades y migración
- ✅ Día 3-4: Actualizar cálculos y DTOs
- ✅ Día 5: Testing con gasolineras

### Sprint 3: Surtidos Estructurados (2 semanas)
- ✅ Día 1-3: Crear entidad DocumentoDetalleSurtido
- ✅ Día 4-6: Implementar UI para agregar componentes
- ✅ Día 7-9: Implementar cálculos de totales
- ✅ Día 10: Testing con combos

### Sprint 4: Mensaje Receptor (3 semanas)
- ✅ Semana 1: Backend (entidad, servicio, API)
- ✅ Semana 2: Frontend (interfaz de respuesta)
- ✅ Semana 3: Integración y testing

---

## 🧪 Testing

### Casos de Prueba Críticos

#### DatosImpuestoEspecifico
```
✓ Facturar cigarrillos (Código 06)
✓ Facturar gasolina (Código 03)
✓ Facturar cerveza (Código 04)
✓ Facturar bebida gaseosa con impuesto (Código 05)
✓ Facturar cemento (Código 12)
✓ Validar cálculo correcto de impuesto por cantidad
✓ Validar XML generado contra XSD de Hacienda
```

#### ImpuestoAsumidoEmisorFabrica
```
✓ Gasolinera asume impuesto de combustible
✓ Validar que total no incluye impuesto asumido
✓ Validar que ResumenFactura incluye campo TotalImpAsumEmisorFabrica
```

---

## 📚 Referencias

### Documentación Oficial
- **Anexos y Estructuras v4.4:** `/ANEXOS Y ESTRUCTURAS_V4.4.pdf`
- **Guía de Facturación v4.4:** `/guia-facturacion-electronica-cr-v44.md`
- **Análisis de Ejemplos:** `/ANALISIS_EJEMPLOS_FE.md`

### XSD Schemas
- FacturaElectronica v4.4: `https://atv.hacienda.go.cr/.../v4.4/FacturaElectronica_V4.4.xsd`
- TiqueteElectronico v4.4: `https://atv.hacienda.go.cr/.../v4.4/TiqueteElectronico_V4.4.xsd`

### Ejemplos XML de Referencia
Todos ubicados en `/Ejemplos/`:
- Impuestos específicos: `TABACO_IVA13_FE-*.XML`, `COMBUSTIBLE_IVA13_FE-*.XML`
- Impuesto asumido: `IVA_COBRADO_FABRICA_*.XML`
- Surtidos: `FE con surtidos-*.xml`

---

## ⚠️ Notas Importantes

1. **Validación contra XSD:** Todos los XMLs generados DEBEN validarse contra los esquemas oficiales de Hacienda antes del envío.

2. **Precisión decimal:** Mantener siempre 5 decimales en precios, 3 en cantidades, 2 en totales.

3. **Códigos de impuesto:**
   - 01 = IVA
   - 02 = Selectivo de Consumo
   - 03 = Combustibles
   - 04 = Bebidas Alcohólicas
   - 05 = Bebidas sin Alcohol
   - 06 = Tabaco
   - 07 = IVA Cálculo Especial
   - 08 = IVA Bienes Usados
   - 12 = Cemento
   - 99 = Impuesto de Servicio (10%)

4. **Testing obligatorio:** Probar en ambiente de STAGING de Hacienda antes de producción.

---

**Fecha de Última Actualización:** 21 de Enero 2026
**Versión del Documento:** 1.0
**Autor:** Análisis automático de cobertura vs especificación v4.4
