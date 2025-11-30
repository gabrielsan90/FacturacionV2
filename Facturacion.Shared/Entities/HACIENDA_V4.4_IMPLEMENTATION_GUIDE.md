# Hacienda v4.4 Electronic Invoicing - Implementation Guide

## Overview

This guide provides a complete overview of the entity structure implemented for Costa Rica's Hacienda v4.4 electronic invoicing system, compliant with resolution MH-DGT-RES-0027-2024.

**Implementation Date**: The system is designed for the mandatory v4.4 implementation starting September 1, 2025.

---

## Entity Structure Summary

### Core Document Entity

**Documento** - Main document entity supporting all 9 document types:
- 01 - Factura Electrónica (FE)
- 02 - Nota de Débito Electrónica (ND)
- 03 - Nota de Crédito Electrónica (NC)
- 04 - Tiquete Electrónico (TE)
- 05 - Nota de Débito Electrónica de Compra
- 06 - Nota de Crédito Electrónica de Compra
- 07 - Comprobante de Compra Electrónico (CCE)
- 08 - Factura Electrónica de Compra (FEC)
- 09 - Factura Electrónica de Exportación (FEE)

### Supporting Entities

1. **DocumentoDetalle** - Line items (products/services)
2. **DocumentoDetalleImpuesto** - Tax lines per detail
3. **DocumentoDetalleDescuento** - Discounts per detail line
4. **DocumentoDescuento** - Document-level discounts
5. **DocumentoReferencia** - References to other documents (for ND/NC)
6. **DocumentoMedioPago** - Payment methods (supports mixed payments)
7. **DocumentoOtraInformacion** - Additional key-value information
8. **DocumentoExportacion** - Export-specific data (for FEE type 09)
9. **DocumentoReceptorMensaje** - Response messages (types 05-07)

### Enumerations

1. **DocumentoTipo** - Document types (01-09)
2. **EstadoDocumento** - Document states (Borrador, Pendiente, Procesando, Aceptado, Rechazado, Contingencia, Anulado)
3. **TipoMoneda** - Currency types (CRC, USD, EUR)
4. **TipoReferenciaDocumento** - Reference types for ND/NC
5. **TipoDescuento** - Discount types (MontoAbsoluto, Porcentaje)
6. **NivelDescuento** - Discount level (Linea, Documento)

---

## Key Hacienda v4.4 Features Implemented

### 1. Mandatory Fields (v4.4)

#### NEW in v4.4:
- **actividadEconomicaReceptor**: Now mandatory in invoices (field `ReceptorActividadEconomica` in Documento)
- **SINPE Móvil**: New payment method code 06 (supported in `MedioPago` field)
- **Multiple emails**: Up to 4 email addresses (field `ReceptorEmails` stores comma-separated emails)
- **CIIU4**: 6-digit economic activity codes (field `ActividadEconomica` with 6-char limit)
- **CAByS 2025**: 13-digit product codes (field `CodigoCabys` in DocumentoDetalle, mandatory from 01/06/2025)

#### Pharmaceutical Products (mandatory from 01/12/2024):
- **NumeroRegistroMedicamento**: Medicine registration number
- **FormaFarmaceutica**: Pharmaceutical form code

#### Vehicles:
- **NumeroVIN**: Vehicle Identification Number (mandatory for vehicle sales)

### 2. Consecutive Number System

The system integrates with your existing Terminal entity which manages consecutive number authorization from Hacienda.

**Format**: XXX-YYYYY-ZZ-AAAAAAAAAA (20 digits total)
- XXX = Branch code (3 digits)
- YYYYY = Terminal code (5 digits)
- ZZ = Document type (2 digits)
- AAAAAAAAAA = Sequential number (10 digits)

**Implementation**: The `NumeroConsecutivo` field stores this formatted number.

### 3. Clave Generation (50-digit Numeric Key)

**Format**: CCPPPPDDDDDDDDDDSSSTTTNNNNNNNNNNNNNNNNNNNNSSSSSSSSC
- CC = Country code (always "506" for Costa Rica)
- PPPP = Emission date (day + month, 4 digits)
- DDDDDDDDDD = Issuer identification (10 digits, padded)
- SSS = Branch code (3 digits)
- TTT = Terminal code (3 digits)
- NN...N = Consecutive number (20 digits)
- SSSSSSSS = Security code (8 random digits)
- C = Verification digit (ATV-10 algorithm)

**Implementation**: The `Clave` field stores this 50-character key. You'll need to implement the generation logic.

### 4. Tax Calculations

The system supports:
- Multiple taxes per line (IVA, consumption tax, etc.)
- Tax on tax calculations
- Proper rounding to 5 decimals as per Hacienda spec
- Exoneration tracking with document details

**Key fields**:
- `MontoBase`: Base amount for tax calculation
- `MontoImpuesto`: Calculated tax amount
- `Tarifa`: Tax rate percentage
- Exoneration fields for tax-exempt sales

### 5. Multi-Currency Support

Fields:
- `Moneda`: Currency type (CRC, USD, EUR)
- `TipoCambio`: Exchange rate (mandatory if not CRC, up to 5 decimals)

All monetary amounts are stored with 5 decimal precision (`decimal(18,5)`).

### 6. Document Workflow

```
Borrador → Pendiente → Procesando → Aceptado/Rechazado
                    ↓
              Contingencia → (resolved) → Procesando
```

**States**:
1. **Borrador**: Draft, being edited
2. **Pendiente**: Ready to send to Hacienda
3. **Procesando**: Sent to Hacienda, awaiting response
4. **Aceptado**: Accepted by Hacienda (valid document)
5. **Rechazado**: Rejected by Hacienda (needs correction)
6. **Contingencia**: Generated offline (to be sent later)
7. **Anulado**: Canceled document

---

## Database Schema Structure

### Primary Tables

```
Documentos (main table)
├── DocumentoDetalles (1:N - line items)
│   ├── DocumentoDetalleImpuestos (1:N - taxes per line)
│   └── DocumentoDetalleDescuentos (1:N - discounts per line)
├── DocumentoDescuentos (1:N - document-level discounts)
├── DocumentoReferencias (1:N - document references)
├── DocumentoMediosPago (1:N - payment methods)
├── DocumentoOtraInformacion (1:N - additional info)
└── DocumentoExportaciones (1:1 - export data, only for FEE)

DocumentoReceptorMensajes (separate table for response messages)
└── References Documentos (N:1)
```

### Foreign Key Relationships

**Documento** relates to:
- Empresa (N:1) - Multi-tenant root
- Sucursal (N:1) - Branch
- Terminal (N:1) - POS terminal
- Cliente (N:1) - Customer (for sales documents)
- Proveedor (N:1) - Supplier (for purchase documents)
- User entities (for audit trail)

---

## Critical Business Rules

### 1. Document Type Validation

| Document Type | Requires Cliente | Requires Proveedor | Requires Referencias | Can Have Exportacion |
|---------------|-----------------|-------------------|---------------------|---------------------|
| FE (01)       | Yes             | No                | No                  | No                  |
| ND (02)       | Yes             | No                | Yes (mandatory)     | No                  |
| NC (03)       | Yes             | No                | Yes (mandatory)     | No                  |
| TE (04)       | Optional        | No                | No                  | No                  |
| ND Compra (05)| No              | Yes               | Yes (mandatory)     | No                  |
| NC Compra (06)| No              | Yes               | Yes (mandatory)     | No                  |
| CCE (07)      | No              | Yes               | No                  | No                  |
| FEC (08)      | No              | Yes               | No                  | No                  |
| FEE (09)      | Yes             | No                | No                  | Yes (mandatory)     |

### 2. Consecutive Number Management

**Rules**:
- Each Terminal has its own consecutive number sequence
- Must be sequential (no gaps allowed)
- Format must match Terminal configuration
- Terminal validates range (NumeroInicio to NumeroFin)

**Implementation**:
```csharp
// Pseudo-code for consecutive number generation
var terminal = await GetTerminalById(terminalId);
var nextNumber = terminal.NumeroConsecutivoActual + 1;

if (nextNumber > terminal.NumeroFin)
    throw new Exception("Consecutive number range exhausted");

var numeroConsecutivo = $"{terminal.Sucursal.Codigo:D3}-{terminal.Codigo:D5}-{documentType:D2}-{nextNumber:D10}";

terminal.NumeroConsecutivoActual = nextNumber;
await UpdateTerminal(terminal);
```

### 3. Tax Calculation Example

```csharp
// Line item calculation with IVA 13%
var precioUnitario = 1000.00m;
var cantidad = 2;
var montoTotal = precioUnitario * cantidad; // 2000.00

// Apply line discount
var montoDescuento = 200.00m; // 10% discount
var subtotal = montoTotal - montoDescuento; // 1800.00

// Calculate IVA 13%
var montoBase = subtotal; // 1800.00
var tarifa = 13.00m;
var montoImpuesto = Math.Round(montoBase * (tarifa / 100), 5); // 234.00000

// Line total
var montoTotalLinea = subtotal + montoImpuesto; // 2034.00
```

### 4. Totals Calculation

```csharp
// Document totals aggregation
documento.Subtotal = detalles.Sum(d => d.Subtotal);
documento.TotalDescuentos = detalles.Sum(d => d.MontoDescuento) +
                            descuentosDocumento.Sum(d => d.MontoDescuento);
documento.TotalImpuestos = detalles.Sum(d => d.MontoImpuesto);
documento.TotalVenta = documento.Subtotal - documento.TotalDescuentos + documento.TotalImpuestos;

// Category totals (required by Hacienda)
documento.TotalMercanciasGravadas = detalles
    .Where(d => d.Producto.Tipo == TipoProducto.Mercancia && d.Impuestos.Any())
    .Sum(d => d.MontoTotal);

documento.TotalServiciosGravados = detalles
    .Where(d => d.Producto.Tipo == TipoProducto.Servicio && d.Impuestos.Any())
    .Sum(d => d.MontoTotal);
// ... similar for Exentos and Exonerados
```

### 5. Inventory Integration

For sales documents (FE, TE, FEE), you must update inventory:

```csharp
// After document is accepted by Hacienda
if (documento.Estado == EstadoDocumento.Aceptado)
{
    foreach (var detalle in documento.Detalles.Where(d => d.Producto.ControlarInventario))
    {
        var inventario = await GetInventario(detalle.ProductoId, documento.SucursalId);
        inventario.CantidadActual -= detalle.Cantidad;

        // Register movement
        var movimiento = new MovimientoInventario
        {
            ProductoId = detalle.ProductoId,
            SucursalId = documento.SucursalId,
            TipoMovimiento = TipoMovimientoInventario.Venta,
            Cantidad = -detalle.Cantidad,
            DocumentoId = documento.Id,
            // ... other fields
        };

        await SaveMovimiento(movimiento);
    }
}
```

---

## Data Precision Requirements

### Decimal Precision by Field Type

| Field Type | Precision | Example |
|-----------|-----------|---------|
| Monetary amounts | decimal(18, 5) | 1234.56789 |
| Quantities | decimal(18, 3) | 123.456 |
| Tax rates | decimal(5, 2) | 13.00 |
| Exchange rates | decimal(18, 5) | 567.12345 |

### String Length Requirements

| Field | Max Length | Format |
|-------|-----------|--------|
| Clave | 50 chars (exact) | Numeric only |
| NumeroConsecutivo | 20 chars | XXX-YYYYY-ZZ-AAAAAAAAAA |
| CodigoCabys | 13 chars (exact) | Numeric only |
| ActividadEconomica | 6 chars | CIIU4 code |
| CodigoImpuesto | 2 chars | Hacienda catalog |
| MedioPago | 2 chars | Hacienda catalog |

---

## Hacienda Communication Fields

### XML Storage

- **XmlGenerado**: Stores the unsigned XML document
- **XmlFirmado**: Stores the digitally signed XML (XAdES-BES standard)
- **FechaFirma**: Timestamp of digital signature

### API Communication

- **FechaEnvioHacienda**: When the document was sent to ATV
- **FechaRespuestaHacienda**: When Hacienda responded
- **MensajeHacienda**: Response message from Hacienda
- **XmlRespuestaHacienda**: Full XML response from Hacienda

### Status Tracking

Track the document through these states:
1. Generate XML → `XmlGenerado` populated
2. Sign XML → `XmlFirmado` populated, `FechaFirma` set
3. Send to Hacienda → `Estado` = Procesando, `FechaEnvioHacienda` set
4. Receive response → `Estado` = Aceptado/Rechazado, `FechaRespuestaHacienda` set

---

## Contingency Mode

When unable to connect to Hacienda, documents can be generated in contingency mode:

```csharp
documento.EsContingencia = true;
documento.Estado = EstadoDocumento.Contingencia;
// Generate and sign normally, but don't send to Hacienda yet

// Later, when connection is restored:
await EnviarDocumentosContingencia();
documento.FechaResolucionContingencia = DateTime.Now;
documento.Estado = EstadoDocumento.Procesando;
```

**Important**: Contingency documents must be sent to Hacienda within 48 hours.

---

## Export Documents (FEE - Type 09)

For exports, populate the `DocumentoExportacion` entity with:

**Required**:
- NombreComprador (foreign buyer name)
- Pais (country code ISO 3166-1)

**Recommended**:
- Incoterm (EXW, FOB, CIF, etc.)
- NumeroDUA (export declaration number)
- Transport information (carrier, shipping details)
- Commercial information (packing list, bill of lading)

**Financial**:
- TotalFOB, Flete, Seguro, TotalCIF

---

## Response Messages (Types 05-07)

Customers can respond to received documents with:

**Type 05 - Acceptance**: Document fully accepted
**Type 06 - Partial Acceptance**: Document partially accepted (specify amounts)
**Type 07 - Rejection**: Document rejected (must provide reason)

Use the `DocumentoReceptorMensaje` entity to track these responses.

---

## Next Implementation Steps

### Phase 1: Database Setup
1. Apply the Entity Framework configuration from `DataContextConfiguration.md`
2. Create and run migrations
3. Verify database schema and indexes
4. Seed Hacienda catalog data (MedioPago, CondicionVenta, Impuestos, etc.)

### Phase 2: Business Logic
1. Implement consecutive number generation service
2. Implement Clave (50-digit key) generation with ATV-10 verification
3. Create document calculation service (totals, taxes, discounts)
4. Implement validation rules per document type
5. Create inventory update service

### Phase 3: XML Generation
1. Implement XML serialization per Hacienda XSD schemas
2. Create namespace and schema handlers
3. Implement date/time formatting (ISO 8601 with timezone)
4. Handle special characters and encoding

### Phase 4: Digital Signature
1. Integrate with digital certificate (stored in Empresa.CertificadoDigital)
2. Implement XAdES-BES signature standard
3. Handle HSM integration if applicable
4. Implement signature verification

### Phase 5: Hacienda API Integration
1. Implement authentication (OAuth2 with Empresa credentials)
2. Create API client for document submission
3. Implement response parsing and status tracking
4. Handle errors and retries
5. Implement contingency mode workflow

### Phase 6: PDF Generation
1. Create PDF templates for each document type
2. Include QR code with document verification data
3. Implement branding with Empresa.Logo
4. Generate printable formats

### Phase 7: Testing
1. Unit tests for calculations
2. Integration tests with Hacienda test environment (Ambiente.Pruebas)
3. Validate all 9 document types
4. Test edge cases (contingency, exonerations, exports)
5. Performance testing for bulk operations

### Phase 8: Production Deployment
1. Migrate to production Hacienda environment
2. Implement monitoring and alerting
3. Create operational dashboards
4. Train users
5. Document troubleshooting procedures

---

## Important Deadlines (v4.4)

| Date | Requirement |
|------|-------------|
| 2024-12-01 | Pharmaceutical fields mandatory |
| 2025-04-01 | v4.4 voluntary period begins |
| 2025-06-01 | CAByS 2025 mandatory |
| 2025-06-02 | Migration to Tribu-CR system |
| 2025-08-31 | Last day for CIIU3, must use CIIU4 |
| **2025-09-01** | **v4.4 MANDATORY for all** |
| 2025-10-06 | Only CIIU4 accepted |

---

## Security Considerations

1. **Certificate Storage**: Never commit certificates or PINs to source control
2. **HSM Recommended**: Use Hardware Security Module for production certificates
3. **Encryption**: Encrypt sensitive fields (PinCertificado, ClaveHacienda) at rest
4. **Access Control**: Implement role-based access for document operations
5. **Audit Trail**: All operations tracked with user IDs and timestamps
6. **Data Retention**: Keep documents for 5 years as per Costa Rican tax law

---

## Performance Optimization

1. **Indexes**: All critical indexes defined in configuration
2. **Eager Loading**: Load related entities efficiently
3. **Caching**: Cache Hacienda catalogs (they rarely change)
4. **Batch Operations**: Support bulk document generation
5. **Async/Await**: Use async operations for database and API calls
6. **Connection Pooling**: Configure EF Core connection pooling

---

## Support and Resources

- **Hacienda Documentation**: https://tribunet.hacienda.go.cr
- **Technical Specification**: Resolution MH-DGT-RES-0027-2024
- **XSD Schemas**: Available from Hacienda developer portal
- **Test Environment**: https://api.comprobanteselectronicos.go.cr/recepcion-sandbox/v1/recepcion

---

## File Structure Summary

### Entities Created (10 files)
```
/Facturacion.Shared/Entities/
├── Documento.cs                      (Main document)
├── DocumentoDetalle.cs               (Line items)
├── DocumentoDetalleImpuesto.cs       (Tax lines)
├── DocumentoDetalleDescuento.cs      (Line discounts)
├── DocumentoDescuento.cs             (Document discounts)
├── DocumentoReferencia.cs            (Document references)
├── DocumentoMedioPago.cs             (Payment methods)
├── DocumentoOtraInformacion.cs       (Additional info)
├── DocumentoExportacion.cs           (Export data)
└── DocumentoReceptorMensaje.cs       (Response messages)
```

### Enums Created (6 files)
```
/Facturacion.Shared/Enums/
├── DocumentoTipo.cs                  (Document types 01-09)
├── EstadoDocumento.cs                (Document states)
├── TipoMoneda.cs                     (CRC, USD, EUR)
├── TipoReferenciaDocumento.cs        (Reference types)
├── TipoDescuento.cs                  (Discount types)
└── NivelDescuento.cs                 (Discount levels)
```

### Updated Entities (4 files)
```
/Facturacion.Shared/Entities/
├── Cliente.cs                        (Added Documentos navigation)
├── Proveedor.cs                      (Added Documentos navigation)
├── Empresa.cs                        (Added Documentos navigation)
├── Sucursal.cs                       (Added Documentos navigation)
└── Terminal.cs                       (Added Documentos navigation)
```

### Documentation (2 files)
```
/Facturacion.Shared/Entities/
├── DataContextConfiguration.md       (EF Core configuration)
└── HACIENDA_V4.4_IMPLEMENTATION_GUIDE.md (This file)
```

---

**End of Implementation Guide**

For questions or clarifications about the entity structure, refer to the inline documentation in each entity class or the DataContextConfiguration.md file.
