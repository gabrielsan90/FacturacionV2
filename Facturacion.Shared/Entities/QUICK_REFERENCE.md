# Hacienda v4.4 - Quick Reference Card

## Document Types

| Code | Type | Description | Requires |
|------|------|-------------|----------|
| 01 | FE | Factura Electrónica | Cliente |
| 02 | ND | Nota de Débito | Cliente + Referencias |
| 03 | NC | Nota de Crédito | Cliente + Referencias |
| 04 | TE | Tiquete Electrónico | Cliente (optional) |
| 05 | ND Compra | Nota Débito Compra | Proveedor + Referencias |
| 06 | NC Compra | Nota Crédito Compra | Proveedor + Referencias |
| 07 | CCE | Comprobante Compra | Proveedor |
| 08 | FEC | Factura Electrónica Compra | Proveedor |
| 09 | FEE | Factura Exportación | Cliente + Exportacion |

## Key Formats

| Field | Format | Example |
|-------|--------|---------|
| Clave | 50 digits | 50620251500110111234567001001011234567890100000000112345678 |
| Consecutivo | XXX-YYYYY-ZZ-AAAAAAAAAA | 001-00001-01-0000000001 |
| CAByS | 13 digits | 1010101010101 |
| CIIU4 | 6 digits | 620100 |

## State Workflow

```
Borrador → Pendiente → Procesando → Aceptado
                              ↓
                         Rechazado

Contingencia → (resolved) → Procesando
```

## Required v4.4 Fields

### NEW in v4.4
- `ReceptorActividadEconomica` (mandatory in invoices)
- `ReceptorEmails` (up to 4 emails, comma-separated)
- SINPE Móvil payment method (code 06)
- `CodigoCabys` (13 digits, mandatory from 01/06/2025)
- `ActividadEconomica` (CIIU4, 6 digits)

### Pharmaceutical Products (from 01/12/2024)
- `NumeroRegistroMedicamento`
- `FormaFarmaceutica`

### Vehicles
- `NumeroVIN`

## Payment Methods (MedioPago)

| Code | Description |
|------|-------------|
| 01 | Efectivo |
| 02 | Tarjeta |
| 03 | Cheque |
| 04 | Transferencia |
| 05 | Recaudado por terceros |
| 06 | SINPE Móvil (NEW v4.4) |
| 99 | Otros |

## Sale Conditions (CondicionVenta)

| Code | Description |
|------|-------------|
| 01 | Contado |
| 02 | Crédito |
| 03 | Consignación |
| 04 | Apartado |
| 05 | Arrendamiento con opción de compra |
| 06 | Arrendamiento en función financiera |
| 07 | Cobro a favor de un tercero |
| 08 | Servicios prestados al Estado a crédito |
| 09 | Pago del servicios prestado al Estado |
| 10 | Mercancía no nacionalizada (NEW v4.4) |
| 99 | Otros |

## Tax Codes (Common)

| Code | Description | Rate |
|------|-------------|------|
| 01 | IVA Tarifa General | 13% |
| 02 | IVA Tarifa Reducida | 1%, 2%, 4% |
| 03 | IVA Tarifa Cero | 0% |
| 04 | Exento | 0% |
| 07 | Impuesto de Consumo | Varies |

## Reference Types (ND/NC)

| Code | Description |
|------|-------------|
| 01 | Anula Documento de Referencia |
| 02 | Corrige el texto |
| 03 | Corrige el monto |
| 04 | Referencia a otro documento |
| 05 | Sustituye comprobante provisional |
| 99 | Otros |

## Decimal Precision

| Type | Precision | Example |
|------|-----------|---------|
| Monetary | 5 decimals | 1234.56789 |
| Quantity | 3 decimals | 123.456 |
| Tax Rate | 2 decimals | 13.00 |

## Common Calculations

### Line Total
```
MontoTotal = PrecioUnitario × Cantidad
Subtotal = MontoTotal - MontoDescuento
MontoImpuesto = Subtotal × (Tarifa / 100)
MontoTotalLinea = Subtotal + MontoImpuesto
```

### Document Total
```
TotalVenta = Subtotal - TotalDescuentos + TotalImpuestos + TotalOtrosCargos
```

## Multi-Tenant Structure

```
Empresa (tenant)
  └── Sucursal (branch)
        └── Terminal (POS)
              └── Documento (invoices)
                    └── DocumentoDetalle (lines)
```

## Important Deadlines

| Date | Event |
|------|-------|
| 2025-06-01 | CAByS 2025 mandatory |
| 2025-08-31 | Last day for CIIU3 |
| **2025-09-01** | **v4.4 MANDATORY** |
| 2025-10-06 | Only CIIU4 accepted |

## Entities Quick Map

| Entity | Purpose | Parent | Cascade Delete |
|--------|---------|--------|----------------|
| Documento | Main document | - | No |
| DocumentoDetalle | Line items | Documento | Yes |
| DocumentoDetalleImpuesto | Tax per line | DocumentoDetalle | Yes |
| DocumentoDetalleDescuento | Discount per line | DocumentoDetalle | Yes |
| DocumentoDescuento | Document discount | Documento | Yes |
| DocumentoReferencia | Doc references | Documento | Yes |
| DocumentoMedioPago | Payment methods | Documento | Yes |
| DocumentoOtraInformacion | Extra info | Documento | Yes |
| DocumentoExportacion | Export data | Documento | Yes (1:1) |
| DocumentoReceptorMensaje | Responses | Documento | No |

## API Endpoints (Hacienda)

### Production
- Recepción: `https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion`
- Consulta: `https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion/{clave}`

### Testing (Sandbox)
- Recepción: `https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1/recepcion`
- Consulta: `https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1/recepcion/{clave}`

## Digital Signature

- **Standard**: XAdES-BES
- **Algorithm**: SHA-256
- **Certificate**: Stored in `Empresa.CertificadoDigital`
- **PIN**: Stored in `Empresa.PinCertificado` (encrypted)

## Contingency Rules

- **Trigger**: When unable to connect to Hacienda
- **Max Duration**: 48 hours
- **Resolution**: Send all contingency documents when connection restored
- **Field**: `EsContingencia = true`

## Common Validations

### Clave
- ✓ Exactly 50 digits
- ✓ Numeric only
- ✓ Unique globally
- ✓ Valid ATV-10 verification digit

### NumeroConsecutivo
- ✓ Format: XXX-YYYYY-ZZ-AAAAAAAAAA
- ✓ Sequential (no gaps)
- ✓ Within Terminal range

### Totals
- ✓ Sum of lines = Document total
- ✓ All decimals rounded to 5 places
- ✓ TotalVenta = Subtotal - Descuentos + Impuestos

### Required Navigation
- ✓ FE/TE/FEE: Must have ClienteId
- ✓ FEC/CCE: Must have ProveedorId
- ✓ ND/NC: Must have Referencias
- ✓ FEE: Must have Exportacion

## EF Core Query Examples

### Get document with all details
```csharp
var documento = await context.Documentos
    .Include(d => d.Detalles)
        .ThenInclude(det => det.Impuestos)
    .Include(d => d.Detalles)
        .ThenInclude(det => det.Descuentos)
    .Include(d => d.Descuentos)
    .Include(d => d.Referencias)
    .Include(d => d.MediosPago)
    .Include(d => d.Cliente)
    .Include(d => d.Empresa)
    .FirstOrDefaultAsync(d => d.Id == id);
```

### Get pending documents
```csharp
var pendientes = await context.Documentos
    .Where(d => d.Estado == EstadoDocumento.Pendiente)
    .Where(d => d.EmpresaId == empresaId)
    .OrderBy(d => d.FechaEmision)
    .ToListAsync();
```

### Get documents in contingency
```csharp
var contingencia = await context.Documentos
    .Where(d => d.EsContingencia == true)
    .Where(d => d.FechaResolucionContingencia == null)
    .ToListAsync();
```

## Migration Commands

### Create migration
```bash
dotnet ef migrations add AddHaciendaDocuments --project Facturacion.Backend
```

### Update database
```bash
dotnet ef database update --project Facturacion.Backend
```

### Rollback migration
```bash
dotnet ef database update PreviousMigration --project Facturacion.Backend
```

## Testing Checklist

- [ ] Generate Clave with correct format and verification
- [ ] Create consecutive numbers without gaps
- [ ] Calculate taxes correctly (5 decimal precision)
- [ ] Apply discounts at line and document level
- [ ] Generate XML matching XSD schema
- [ ] Sign XML with XAdES-BES
- [ ] Send to Hacienda sandbox
- [ ] Parse response and update status
- [ ] Handle rejections gracefully
- [ ] Generate PDF with QR code
- [ ] Test contingency mode
- [ ] Validate all 9 document types
- [ ] Test export documents with foreign buyer
- [ ] Test ND/NC with references
- [ ] Update inventory on acceptance

---

**Quick Reference v1.0** | Created for Hacienda v4.4 mandatory implementation September 1, 2025
