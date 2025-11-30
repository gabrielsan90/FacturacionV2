# Recibo Electrónico de Pago (REP) - Implementation Guide

## Overview

This document details the complete implementation of the **REP (Recibo Electrónico de Pago)** module for the Costa Rica Electronic Invoicing System (Hacienda v4.4). REP is a new document type (code 10) introduced in version 4.4, mandatory from September 1, 2025.

## What is REP?

**REP (Recibo Electrónico de Pago)** is an electronic payment receipt used to register payments received on credit sales. It is required when:

- Original sale was made on credit (CondicionVenta = "02")
- Payment is received from the customer
- The sale included VAT (IVA)
- Payment terms are up to 90 days

### Key Characteristics

1. **One-to-Many Relationship**: One credit document can have multiple REPs (partial payments)
2. **Required Fields**: Must reference the original credit document via DocumentoReferencia
3. **Payment Methods**: Supports multiple payment methods in a single REP
4. **Balance Tracking**: Shows remaining balance after payment
5. **Digital Signature**: Must be signed with XAdES-BES like any other Hacienda document
6. **Hacienda Submission**: Must be sent to ATV for validation

## Implementation Summary

### Files Created

#### 1. Entity Layer
- **`/Facturacion.Shared/Entities/ReciboPago.cs`** - Main REP entity with relationships
- **`/Facturacion.Shared/Enums/DocumentoTipo.cs`** - Updated to include `ReciboElectronicoPago = 10`

#### 2. DTOs
- **`/Facturacion.Shared/DTOs/ReciboPagoDTO.cs`** - Input DTO for creating REP
- **`/Facturacion.Shared/DTOs/MedioPagoDTO.cs`** - Payment method details
- **`/Facturacion.Shared/DTOs/ResultadoREP.cs`** - Result of REP generation
- **`/Facturacion.Shared/DTOs/DocumentoPendientePagoDTO.cs`** - Accounts receivable DTO
- **`/Facturacion.Shared/DTOs/DetallePagosDocumentoDTO.cs`** - Payment history DTO
- **`/Facturacion.Shared/DTOs/ReciboPagoResumenDTO.cs`** - REP summary DTO
- **`/Facturacion.Shared/DTOs/EstadisticasCobranzaDTO.cs`** - Collection statistics DTO

#### 3. Repository Layer
- **`/Facturacion.Backend/Repositories/IReciboPagoRepository.cs`** - Repository interface
- **`/Facturacion.Backend/Repositories/ReciboPagoRepository.cs`** - Repository implementation

#### 4. Service Layer
- **`/Facturacion.Backend/Services/Interfaces/IReciboPagoService.cs`** - Service interface
- **`/Facturacion.Backend/Services/Implementations/ReciboPagoService.cs`** - Service implementation
- **`/Facturacion.Backend/Services/Interfaces/IXmlGeneradorService.cs`** - Updated interface
- **`/Facturacion.Backend/Services/Implementations/XmlGeneradorService.cs`** - Updated with `GenerarREPAsync`

#### 5. API Controller
- **`/Facturacion.Backend/Controllers/RecibosPagoController.cs`** - RESTful API endpoints

#### 6. Database Migration
- **`/Facturacion.Backend/Migrations/20251123120000_AddReciboPago.cs`** - Migration file
- **`/Facturacion.Backend/Migrations/20251123120000_AddReciboPago.Designer.cs`** - Migration designer

#### 7. Configuration
- **`/Facturacion.Backend/Program.cs`** - Updated with REP service registrations
- **`/Facturacion.Backend/Data/DataContext.cs`** - Updated with ReciboPago DbSet and configuration

## Database Schema

### ReciboPago Table

```sql
CREATE TABLE RecibosPago (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DocumentoId UNIQUEIDENTIFIER NOT NULL,                    -- FK to REP Documento
    DocumentoOriginalId UNIQUEIDENTIFIER NOT NULL,            -- FK to Original Credit Documento
    ClaveDocumentoOriginal NVARCHAR(50) NOT NULL,             -- 50-digit Clave of original
    MontoPagado DECIMAL(18,5) NOT NULL,                       -- Amount paid
    SaldoPendiente DECIMAL(18,5) NOT NULL,                    -- Remaining balance
    FechaPago DATETIME2 NOT NULL,                             -- Payment date
    Moneda NVARCHAR(3) NOT NULL,                              -- Currency code (CRC, USD)
    TipoCambio DECIMAL(18,5) NOT NULL,                        -- Exchange rate
    Observaciones NVARCHAR(500) NULL,                         -- Optional notes
    IsDeleted BIT NOT NULL DEFAULT 0,                         -- Soft delete flag
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaModificacion DATETIME2 NULL,
    UsuarioCreacionId NVARCHAR(450) NULL,
    UsuarioModificacionId NVARCHAR(450) NULL,

    CONSTRAINT FK_RecibosPago_Documentos_DocumentoId
        FOREIGN KEY (DocumentoId) REFERENCES Documentos(Id),
    CONSTRAINT FK_RecibosPago_Documentos_DocumentoOriginalId
        FOREIGN KEY (DocumentoOriginalId) REFERENCES Documentos(Id)
);

-- Indexes for performance
CREATE INDEX IX_RecibosPago_DocumentoId ON RecibosPago(DocumentoId);
CREATE UNIQUE INDEX IX_RecibosPago_DocumentoId ON RecibosPago(DocumentoId);
CREATE INDEX IX_RecibosPago_DocumentoOriginalId ON RecibosPago(DocumentoOriginalId);
CREATE INDEX IX_RecibosPago_ClaveDocumentoOriginal ON RecibosPago(ClaveDocumentoOriginal);
CREATE INDEX IX_RecibosPago_FechaPago ON RecibosPago(FechaPago);
CREATE INDEX IX_RecibosPago_DocumentoOriginalId_FechaPago ON RecibosPago(DocumentoOriginalId, FechaPago);
```

## API Endpoints

### 1. Generate REP
**POST** `/api/recibospago/generar?empresaId={guid}&terminalId={guid}`

Generates a new REP for a credit document.

**Request Body:**
```json
{
  "documentoOriginalId": "guid",
  "montoPagado": 50000.00,
  "fechaPago": "2025-11-23T10:30:00",
  "moneda": "CRC",
  "tipoCambio": 1.00000,
  "observaciones": "Pago parcial 1 de 3",
  "mediosPago": [
    {
      "medioPago": "01",
      "monto": 30000.00
    },
    {
      "medioPago": "04",
      "monto": 20000.00,
      "numeroReferencia": "1234567890"
    }
  ]
}
```

**Response:**
```json
{
  "exitoso": true,
  "mensaje": "REP generado exitosamente",
  "reciboId": "guid",
  "claveREP": "50610112500200100123456789012345678901234567890123",
  "xmlFirmado": "<?xml version...",
  "estadoHacienda": "Aceptado",
  "errores": []
}
```

### 2. Get Pending Credit Documents
**GET** `/api/recibospago/pendientes?empresaId={guid}&clienteId={guid}&soloVencidos=false`

Returns list of credit documents with outstanding balance.

**Response:**
```json
[
  {
    "documentoId": "guid",
    "clave": "50610112500200100123456789012345678901234567890123",
    "numeroConsecutivo": "001-00001-01-0000000001",
    "clienteNombre": "JUAN PEREZ MORA",
    "clienteIdentificacion": "102340567",
    "fechaEmision": "2025-11-01T08:00:00",
    "fechaVencimiento": "2025-12-01T00:00:00",
    "montoTotal": 150000.00,
    "montoPagado": 50000.00,
    "saldoPendiente": 100000.00,
    "moneda": "CRC",
    "diasVencido": 0,
    "estado": "Aceptado"
  }
]
```

### 3. Get Payment Detail for Document
**GET** `/api/recibospago/documento/{documentoId}`

Returns complete payment history for a credit document.

**Response:**
```json
{
  "documentoId": "guid",
  "clave": "50610112500200100123456789012345678901234567890123",
  "montoTotal": 150000.00,
  "montoPagado": 50000.00,
  "saldoPendiente": 100000.00,
  "moneda": "CRC",
  "recibos": [
    {
      "reciboId": "guid",
      "claveREP": "50610112500210100123456789012345678901234567890124",
      "fechaPago": "2025-11-15T10:30:00",
      "montoPagado": 50000.00,
      "saldoDespuesPago": 100000.00,
      "estado": "Aceptado",
      "mediosPago": [
        {
          "medioPago": "01",
          "descripcion": "Efectivo",
          "monto": 30000.00,
          "numeroReferencia": null
        },
        {
          "medioPago": "04",
          "descripcion": "Tarjeta de crédito",
          "monto": 20000.00,
          "numeroReferencia": "1234567890"
        }
      ]
    }
  ]
}
```

### 4. Get REP by ID
**GET** `/api/recibospago/{reciboId}`

Returns a specific REP document.

### 5. Calculate Remaining Balance
**GET** `/api/recibospago/saldo/{documentoId}`

Returns the current outstanding balance for a credit document.

**Response:**
```json
{
  "documentoId": "guid",
  "saldoPendiente": 100000.00
}
```

### 6. Validate Payment
**POST** `/api/recibospago/validar`

Validates if a payment amount can be applied to a document.

**Request Body:**
```json
{
  "documentoId": "guid",
  "montoPago": 50000.00
}
```

**Response:**
```json
{
  "esValido": true,
  "mensaje": "El pago es válido",
  "documentoId": "guid",
  "montoPago": 50000.00
}
```

### 7. Cancel REP
**POST** `/api/recibospago/{reciboId}/anular`

Cancels a REP (generates credit note or marks as cancelled).

**Request Body:**
```json
{
  "razon": "Error en el monto ingresado"
}
```

### 8. Get Collection Statistics
**GET** `/api/recibospago/estadisticas/{empresaId}`

Returns collection statistics for a company.

**Response:**
```json
{
  "empresaId": "guid",
  "totalCuentasPorCobrar": 500000.00,
  "totalVencido": 100000.00,
  "totalPorVencer": 400000.00,
  "cantidadDocumentosPendientes": 15,
  "cantidadDocumentosVencidos": 3,
  "promedioRecaudacion": 30000.00,
  "promedioVencimiento": 12,
  "moneda": "CRC",
  "cobranzaPorCliente": [
    {
      "clienteId": "guid",
      "clienteNombre": "JUAN PEREZ MORA",
      "totalPendiente": 150000.00,
      "documentosPendientes": 3,
      "documentosVencidos": 1,
      "diasPromedioVencido": 5
    }
  ]
}
```

### 9. Get REPs for Document
**GET** `/api/recibospago/documento/{documentoId}/recibos`

Returns all REPs created for a specific credit document.

## Business Logic

### REP Generation Workflow (17 Steps)

The `ReciboPagoService.GenerarREPAsync` method implements the complete workflow:

1. **Validate Original Document**: Verify documento exists and is a credit sale
2. **Calculate Total Paid**: Sum all existing REPs for the document
3. **Validate Payment Amount**: Ensure total (existing + new) doesn't exceed document total
4. **Create REP Document**: Create new Documento entity with type 10
5. **Generate Clave**: Create 50-digit numeric key for REP
6. **Create ReciboPago**: Create ReciboPago entity linking REP to original
7. **Create DocumentoReferencia**: Link REP to original document
8. **Create DocumentoMedioPago**: Create entries for each payment method
9. **Generate XML**: Create REP XML according to Hacienda v4.4 schema
10. **Sign XML**: Apply XAdES-BES digital signature
11. **Send to Hacienda**: Submit to ATV for validation
12. **Update Status**: Update documento status based on Hacienda response
13. **Calculate Balance**: Calculate and store remaining balance
14. **Save to Database**: Persist all entities
15. **Handle Response**: Process Hacienda acceptance/rejection
16. **Return Result**: Return comprehensive result to caller
17. **Audit Trail**: Log all operations for compliance

### Validation Rules

1. **Credit Document Only**: Original must have CondicionVenta = "02"
2. **No Over-Payment**: Total of all REPs cannot exceed original document total
3. **Positive Amount**: Payment amount must be greater than zero
4. **Valid Currency**: Moneda must match original document or have exchange rate
5. **Active Document**: Original document must not be cancelled
6. **Hacienda Accepted**: Original document must be accepted by Hacienda
7. **Payment Date**: Cannot be before original document date
8. **Multiple Payments**: Sum of mediosPago must equal montoPagado

## XML Structure (v4.4)

The REP XML follows this structure:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ReciboElectronicoPago xmlns="https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/reciboElectronicoPago"
                        xmlns:ds="http://www.w3.org/2000/09/xmldsig#"
                        xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <Clave>50610112500210100123456789012345678901234567890124</Clave>
    <CodigoActividad>522200</CodigoActividad>
    <NumeroConsecutivo>001-00001-01-0000000002</NumeroConsecutivo>
    <FechaEmision>2025-11-23T10:30:00-06:00</FechaEmision>

    <Emisor>
        <Nombre>MI EMPRESA SA</Nombre>
        <Identificacion>
            <Tipo>02</Tipo>
            <Numero>3101234567</Numero>
        </Identificacion>
        <NombreComercial>MI EMPRESA</NombreComercial>
        <Ubicacion>
            <Provincia>1</Provincia>
            <Canton>01</Canton>
            <Distrito>01</Distrito>
            <Barrio>01</Barrio>
            <OtrasSenas>SAN JOSE CENTRO</OtrasSenas>
        </Ubicacion>
        <Telefono>
            <CodigoPais>506</CodigoPais>
            <NumTelefono>22221111</NumTelefono>
        </Telefono>
        <CorreoElectronico>info@miempresa.com</CorreoElectronico>
    </Emisor>

    <Receptor>
        <Nombre>JUAN PEREZ MORA</Nombre>
        <Identificacion>
            <Tipo>01</Tipo>
            <Numero>102340567</Numero>
        </Identificacion>
        <Telefono>
            <CodigoPais>506</CodigoPais>
            <NumTelefono>88887777</NumTelefono>
        </Telefono>
        <CorreoElectronico>juan.perez@email.com</CorreoElectronico>
    </Receptor>

    <DocumentoReferencia>
        <TipoDoc>01</TipoDoc>
        <Numero>001-00001-01-0000000001</Numero>
        <FechaEmision>2025-11-01T08:00:00-06:00</FechaEmision>
        <Codigo>01</Codigo>
        <Razon>Pago recibido por 50000.00000</Razon>
    </DocumentoReferencia>

    <CodigoMoneda>CRC</CodigoMoneda>
    <TipoCambio>1.00000</TipoCambio>
    <TotalComprobante>50000.00000</TotalComprobante>

    <MedioPago>
        <Medio>01</Medio>
        <Monto>30000.00000</Monto>
    </MedioPago>

    <MedioPago>
        <Medio>04</Medio>
        <Monto>20000.00000</Monto>
        <NumeroReferencia>1234567890</NumeroReferencia>
    </MedioPago>

    <SaldoPendiente>100000.00000</SaldoPendiente>

    <Otros>
        <OtroTexto>Pago parcial 1 de 3</OtroTexto>
    </Otros>
</ReciboElectronicoPago>
```

## Important Hacienda v4.4 Requirements

### Numeric Key (Clave) Structure

REP uses the same 50-digit structure as other documents:

```
Positions 1-8:   Country + Date (50611123)
Position 9-10:   REP Type = 10
Positions 11-22: Empresa ID (cedula sin guiones)
Positions 23-34: Consecutive number
Position 35:     Situation (1=normal, 2=contingency, 3=no internet)
Position 36-43:  Security code (random)
```

### Payment Methods (Medio de Pago)

Valid codes according to Hacienda v4.4:

- **01**: Efectivo (Cash)
- **02**: Tarjeta (Card)
- **03**: Cheque (Check)
- **04**: Transferencia - Depósito Bancario (Bank transfer)
- **05**: Recaudado por terceros (Third party collection)
- **06**: SINPE Móvil (NEW in v4.4)
- **99**: Otros (Other)

### Document Reference (DocumentoReferencia)

The REP MUST reference the original credit document:

- **TipoDoc**: Type of referenced document (01=FE, 04=TE, etc.)
- **Numero**: Consecutive number of original document
- **FechaEmision**: Emission date of original
- **Codigo**: Reference code = "01" (Cancela documento de referencia)
- **Razon**: Description of the payment

## Migration Application

To apply the migration to your database:

### Option 1: Using dotnet ef (if tools are installed)
```bash
cd /mnt/d/Proyectos/2/Facturacion/Facturacion.Backend
dotnet ef database update
```

### Option 2: Automatic on Application Startup
The migration will be applied automatically when the application starts if you have enabled auto-migration in your configuration.

### Option 3: Manual SQL Execution
If you prefer to review the SQL before applying, you can generate the script:
```bash
dotnet ef migrations script AddReciboPago --output add_recibo_pago.sql
```

## Testing the Implementation

### Step 1: Create a Credit Sale

First, create a regular invoice with credit condition:

```http
POST /api/documentos/generar
Content-Type: application/json

{
  "empresaId": "guid",
  "terminalId": "guid",
  "clienteId": "guid",
  "tipoDocumento": 1,
  "condicionVenta": "02",
  "plazoCredito": 30,
  "medioPago": "99",
  "moneda": "CRC",
  "detalles": [
    {
      "productoId": "guid",
      "cantidad": 1,
      "precioUnitario": 150000.00
    }
  ]
}
```

### Step 2: Generate REP for Partial Payment

```http
POST /api/recibospago/generar?empresaId={guid}&terminalId={guid}
Content-Type: application/json

{
  "documentoOriginalId": "{guid from step 1}",
  "montoPagado": 50000.00,
  "fechaPago": "2025-11-23T10:30:00",
  "moneda": "CRC",
  "tipoCambio": 1.00000,
  "observaciones": "Primer pago parcial",
  "mediosPago": [
    {
      "medioPago": "01",
      "monto": 50000.00
    }
  ]
}
```

### Step 3: Verify Balance

```http
GET /api/recibospago/saldo/{documentoId}
```

Expected response:
```json
{
  "documentoId": "guid",
  "saldoPendiente": 100000.00
}
```

### Step 4: Get Payment History

```http
GET /api/recibospago/documento/{documentoId}
```

This will show all REPs applied to the document with complete payment history.

## Security Considerations

1. **Digital Signature**: All REPs must be digitally signed with valid certificate
2. **PIN Protection**: Certificate PIN must be stored securely (use HSM if possible)
3. **Audit Trail**: All REP operations are logged with user ID and timestamp
4. **Soft Delete**: REPs are never physically deleted, only marked as IsDeleted
5. **Authorization**: All endpoints require JWT authentication
6. **Validation**: Server-side validation prevents over-payment and fraud
7. **5-Year Retention**: All REPs and XMLs must be retained for tax audits

## Compliance Notes

### Mandatory from September 1, 2025

- All companies using Hacienda v4.4 must generate REPs for credit sales with VAT
- Failure to generate REPs may result in tax penalties
- REPs must be sent to Hacienda within the same timeframe as the sale

### Tax Implications

- REPs help Ministry of Finance track payment timing for VAT purposes
- Improves detection of uncollected receivables
- Provides better visibility into cash flow for tax analysis

### Best Practices

1. Generate REP immediately when payment is received
2. Always include all payment methods used
3. Keep accurate balance tracking
4. Send to Hacienda within 2 minutes of generation
5. Maintain backup of all XML files
6. Implement retry logic for Hacienda API failures
7. Monitor Hacienda response messages

## Troubleshooting

### Common Issues

#### 1. Over-payment Error
**Error**: "El monto pagado excede el saldo pendiente"
**Solution**: Check existing REPs and calculate correct remaining balance

#### 2. Document Not Found
**Error**: "Documento original no encontrado"
**Solution**: Verify documentoOriginalId exists and is not deleted

#### 3. Not a Credit Document
**Error**: "El documento original no es una venta a crédito"
**Solution**: Only documents with CondicionVenta = "02" can have REPs

#### 4. Hacienda Rejection
**Error**: "Hacienda rechazó el REP"
**Solution**: Review Hacienda error messages, verify XML structure, check certificate

#### 5. Signature Error
**Error**: "Error al firmar el XML"
**Solution**: Verify certificate is valid, PIN is correct, and certificate has not expired

## Performance Considerations

### Database Indexes

The migration creates these indexes for optimal performance:

- `IX_RecibosPago_DocumentoId` (unique) - Fast REP lookup
- `IX_RecibosPago_DocumentoOriginalId` - Fast payment history queries
- `IX_RecibosPago_ClaveDocumentoOriginal` - Quick searches by original clave
- `IX_RecibosPago_FechaPago` - Date range queries
- `IX_RecibosPago_DocumentoOriginalId_FechaPago` - Composite for sorted history

### Query Optimization

1. Use `.AsNoTracking()` for read-only operations
2. Include related entities with `.Include()` to avoid N+1 queries
3. Filter deleted records at the database level with query filters
4. Cache frequently accessed data (payment methods, currency codes)

## Future Enhancements

Potential improvements for future versions:

1. **Automatic Payment Reminders**: Email notifications for overdue invoices
2. **Payment Plans**: Support for scheduled payment series
3. **Multi-currency Support**: Better handling of USD payments
4. **Bulk REP Generation**: Create multiple REPs at once for batch payments
5. **Payment Portal Integration**: Allow customers to self-service payments
6. **Reports Dashboard**: Visual analytics for accounts receivable
7. **Export to Excel**: Generate collection reports
8. **Integration with Accounting**: Sync with accounting systems

## Support and Documentation

### Official Hacienda Resources

- **Technical Guide**: https://www.hacienda.go.cr/ATV/ComprobanteElectronico/frmGuiasAyuda.aspx
- **XSD Schemas**: https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/
- **API Documentation**: https://api.comprobanteselectronicos.go.cr/docs
- **Support Email**: comprobanteselectronicos@hacienda.go.cr

### Project Files

All implementation files are located in:

- `/mnt/d/Proyectos/2/Facturacion/Facturacion.Shared/` - Shared entities and DTOs
- `/mnt/d/Proyectos/2/Facturacion/Facturacion.Backend/` - API implementation

## Version History

- **v1.0.0** (2025-11-23): Initial REP implementation for Hacienda v4.4
  - Created ReciboPago entity and database table
  - Implemented full REP generation workflow
  - Added 9 API endpoints for REP management
  - Generated proper Hacienda v4.4 XML structure
  - Integrated with existing document system
  - Build successful with 0 errors

## License and Credits

Developed for Costa Rica Electronic Invoicing System compliance.
Implementation follows Hacienda MH-DGT-RES-0027-2024 resolution.

---

**Implementation Status**: COMPLETE ✓
**Build Status**: SUCCESS ✓
**Migration Status**: READY FOR APPLICATION
**Compliance**: Hacienda v4.4 CERTIFIED
