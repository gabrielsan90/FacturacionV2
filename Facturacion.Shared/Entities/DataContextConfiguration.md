# Entity Framework Core Configuration for Hacienda v4.4 Electronic Documents

## Overview

This document provides the complete Entity Framework Core configuration for the Hacienda v4.4 electronic invoicing system entities. Follow these configurations in your `DataContext` class to ensure proper relationships, indexes, and constraints.

## DbSet Declarations

Add these DbSet properties to your DataContext:

```csharp
// Electronic Documents (Hacienda v4.4)
public DbSet<Documento> Documentos { get; set; }
public DbSet<DocumentoDetalle> DocumentoDetalles { get; set; }
public DbSet<DocumentoDetalleImpuesto> DocumentoDetalleImpuestos { get; set; }
public DbSet<DocumentoDetalleDescuento> DocumentoDetalleDescuentos { get; set; }
public DbSet<DocumentoDescuento> DocumentoDescuentos { get; set; }
public DbSet<DocumentoReferencia> DocumentoReferencias { get; set; }
public DbSet<DocumentoMedioPago> DocumentoMediosPago { get; set; }
public DbSet<DocumentoOtraInformacion> DocumentoOtraInformacion { get; set; }
public DbSet<DocumentoExportacion> DocumentoExportaciones { get; set; }
public DbSet<DocumentoReceptorMensaje> DocumentoReceptorMensajes { get; set; }
```

## OnModelCreating Configuration

### 1. Documento Entity Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ============================================
    // DOCUMENTO - Main Document Entity
    // ============================================

    modelBuilder.Entity<Documento>(entity =>
    {
        // Primary Key
        entity.HasKey(e => e.Id);

        // Unique constraint on Clave (50-digit key must be unique)
        entity.HasIndex(e => e.Clave)
              .IsUnique()
              .HasDatabaseName("IX_Documento_Clave");

        // Index on NumeroConsecutivo for fast lookups
        entity.HasIndex(e => e.NumeroConsecutivo)
              .HasDatabaseName("IX_Documento_NumeroConsecutivo");

        // Index on EmpresaId for tenant filtering
        entity.HasIndex(e => e.EmpresaId)
              .HasDatabaseName("IX_Documento_EmpresaId");

        // Composite index for common queries
        entity.HasIndex(e => new { e.EmpresaId, e.TipoDocumento, e.Estado })
              .HasDatabaseName("IX_Documento_Empresa_Tipo_Estado");

        // Index on FechaEmision for date range queries
        entity.HasIndex(e => e.FechaEmision)
              .HasDatabaseName("IX_Documento_FechaEmision");

        // Soft delete filter
        entity.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        entity.HasOne(d => d.Empresa)
              .WithMany(e => e.Documentos)
              .HasForeignKey(d => d.EmpresaId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.Sucursal)
              .WithMany(s => s.Documentos)
              .HasForeignKey(d => d.SucursalId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.Terminal)
              .WithMany(t => t.Documentos)
              .HasForeignKey(d => d.TerminalId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.Cliente)
              .WithMany(c => c.Documentos)
              .HasForeignKey(d => d.ClienteId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.Proveedor)
              .WithMany(p => p.Documentos)
              .HasForeignKey(d => d.ProveedorId)
              .OnDelete(DeleteBehavior.Restrict);

        // User audit relationships
        entity.HasOne(d => d.UsuarioCreacion)
              .WithMany()
              .HasForeignKey(d => d.UsuarioCreacionId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.UsuarioModificacion)
              .WithMany()
              .HasForeignKey(d => d.UsuarioModificacionId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.UsuarioEliminacion)
              .WithMany()
              .HasForeignKey(d => d.UsuarioEliminacionId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    // ============================================
    // DOCUMENTO DETALLE - Document Line Items
    // ============================================

    modelBuilder.Entity<DocumentoDetalle>(entity =>
    {
        entity.HasKey(e => e.Id);

        // Index on DocumentoId for efficient line item retrieval
        entity.HasIndex(e => e.DocumentoId)
              .HasDatabaseName("IX_DocumentoDetalle_DocumentoId");

        // Composite index for line number ordering
        entity.HasIndex(e => new { e.DocumentoId, e.NumeroLinea })
              .HasDatabaseName("IX_DocumentoDetalle_Documento_Linea");

        // Soft delete filter
        entity.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        entity.HasOne(d => d.Documento)
              .WithMany(doc => doc.Detalles)
              .HasForeignKey(d => d.DocumentoId)
              .OnDelete(DeleteBehavior.Cascade); // Cascade delete when document is deleted

        entity.HasOne(d => d.Producto)
              .WithMany()
              .HasForeignKey(d => d.ProductoId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.UnidadMedida)
              .WithMany()
              .HasForeignKey(d => d.UnidadMedidaId)
              .OnDelete(DeleteBehavior.Restrict);

        // User audit relationships
        entity.HasOne(d => d.UsuarioCreacion)
              .WithMany()
              .HasForeignKey(d => d.UsuarioCreacionId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    // ============================================
    // DOCUMENTO DETALLE IMPUESTO - Tax Lines
    // ============================================

    modelBuilder.Entity<DocumentoDetalleImpuesto>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.DocumentoDetalleId)
              .HasDatabaseName("IX_DocumentoDetalleImpuesto_DetalleId");

        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasOne(d => d.DocumentoDetalle)
              .WithMany(det => det.Impuestos)
              .HasForeignKey(d => d.DocumentoDetalleId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(d => d.Impuesto)
              .WithMany()
              .HasForeignKey(d => d.ImpuestoId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    // ============================================
    // DOCUMENTO DETALLE DESCUENTO - Line Discounts
    // ============================================

    modelBuilder.Entity<DocumentoDetalleDescuento>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.DocumentoDetalleId)
              .HasDatabaseName("IX_DocumentoDetalleDescuento_DetalleId");

        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasOne(d => d.DocumentoDetalle)
              .WithMany(det => det.Descuentos)
              .HasForeignKey(d => d.DocumentoDetalleId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // ============================================
    // DOCUMENTO DESCUENTO - Document Level Discounts
    // ============================================

    modelBuilder.Entity<DocumentoDescuento>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.DocumentoId)
              .HasDatabaseName("IX_DocumentoDescuento_DocumentoId");

        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasOne(d => d.Documento)
              .WithMany(doc => doc.Descuentos)
              .HasForeignKey(d => d.DocumentoId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // ============================================
    // DOCUMENTO REFERENCIA - Document References (ND/NC)
    // ============================================

    modelBuilder.Entity<DocumentoReferencia>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.DocumentoId)
              .HasDatabaseName("IX_DocumentoReferencia_DocumentoId");

        entity.HasIndex(e => e.NumeroDocumentoReferenciado)
              .HasDatabaseName("IX_DocumentoReferencia_NumeroReferenciado");

        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasOne(d => d.Documento)
              .WithMany(doc => doc.Referencias)
              .HasForeignKey(d => d.DocumentoId)
              .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship (optional, for documents in the same database)
        entity.HasOne(d => d.DocumentoReferenciado)
              .WithMany()
              .HasForeignKey(d => d.DocumentoReferenciadoId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    // ============================================
    // DOCUMENTO MEDIO PAGO - Payment Methods
    // ============================================

    modelBuilder.Entity<DocumentoMedioPago>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.DocumentoId)
              .HasDatabaseName("IX_DocumentoMedioPago_DocumentoId");

        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasOne(d => d.Documento)
              .WithMany(doc => doc.MediosPago)
              .HasForeignKey(d => d.DocumentoId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(d => d.MedioPago)
              .WithMany()
              .HasForeignKey(d => d.MedioPagoId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    // ============================================
    // DOCUMENTO OTRA INFORMACION - Additional Info
    // ============================================

    modelBuilder.Entity<DocumentoOtraInformacion>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.DocumentoId)
              .HasDatabaseName("IX_DocumentoOtraInformacion_DocumentoId");

        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasOne(d => d.Documento)
              .WithMany(doc => doc.OtraInformacion)
              .HasForeignKey(d => d.DocumentoId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // ============================================
    // DOCUMENTO EXPORTACION - Export Specific Data
    // ============================================

    modelBuilder.Entity<DocumentoExportacion>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.DocumentoId)
              .IsUnique() // 1-to-1 relationship
              .HasDatabaseName("IX_DocumentoExportacion_DocumentoId");

        entity.HasQueryFilter(e => !e.IsDeleted);

        // One-to-one relationship
        entity.HasOne(d => d.Documento)
              .WithOne(doc => doc.Exportacion)
              .HasForeignKey<DocumentoExportacion>(d => d.DocumentoId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // ============================================
    // DOCUMENTO RECEPTOR MENSAJE - Response Messages
    // ============================================

    modelBuilder.Entity<DocumentoReceptorMensaje>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.ClaveMensaje)
              .IsUnique()
              .HasDatabaseName("IX_DocumentoReceptorMensaje_Clave");

        entity.HasIndex(e => e.DocumentoOriginalId)
              .HasDatabaseName("IX_DocumentoReceptorMensaje_DocumentoOriginal");

        entity.HasQueryFilter(e => !e.IsDeleted);

        entity.HasOne(d => d.DocumentoOriginal)
              .WithMany()
              .HasForeignKey(d => d.DocumentoOriginalId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    base.OnModelCreating(modelBuilder);
}
```

## Important Considerations

### 1. Cascade Delete Strategy

- **Cascade Delete**: Used for child entities that cannot exist without their parent (Detalles, Impuestos, Descuentos, etc.)
- **Restrict Delete**: Used for reference entities to prevent accidental data loss (Empresa, Cliente, Proveedor, etc.)

### 2. Indexes for Performance

The configuration includes strategic indexes for:
- **Unique constraints**: Clave (50-digit key) must be globally unique
- **Tenant filtering**: EmpresaId for multi-tenant queries
- **Search operations**: NumeroConsecutivo, FechaEmision
- **Composite indexes**: Common query patterns (Empresa + Tipo + Estado)

### 3. Soft Delete Implementation

All entities use the `HasQueryFilter` to automatically exclude soft-deleted records:
```csharp
entity.HasQueryFilter(e => !e.IsDeleted);
```

To query soft-deleted records explicitly, use:
```csharp
var documents = await _context.Documentos
    .IgnoreQueryFilters()
    .Where(d => d.IsDeleted)
    .ToListAsync();
```

### 4. Precision Configuration

Decimal fields are configured with specific precision:
- **Monetary amounts**: `decimal(18, 5)` - 5 decimals as per Hacienda spec
- **Quantities**: `decimal(18, 3)` - 3 decimals for quantities
- **Tax rates**: `decimal(5, 2)` - percentage with 2 decimals

### 5. String Length Constraints

All string fields have MaxLength attributes matching Hacienda requirements:
- Clave: Exactly 50 characters
- NumeroConsecutivo: 20 characters
- CodigoCabys: 13 characters
- ActividadEconomica: 6 characters (CIIU4)

## Migration Example

Create the initial migration with:

```bash
# From the Backend project
dotnet ef migrations add AddHaciendaDocuments --project ../Facturacion.Backend --startup-project ../Facturacion.Backend
```

Update the database:

```bash
dotnet ef database update --project ../Facturacion.Backend --startup-project ../Facturacion.Backend
```

## Additional Recommendations

### 1. Value Conversions for Enums

Store enums as integers for better performance:
```csharp
modelBuilder.Entity<Documento>()
    .Property(d => d.TipoDocumento)
    .HasConversion<int>();
```

### 2. Computed Columns

Consider adding computed columns for common calculations:
```csharp
modelBuilder.Entity<Documento>()
    .Property(d => d.TotalVenta)
    .HasComputedColumnSql("[Subtotal] - [TotalDescuentos] + [TotalImpuestos]", stored: true);
```

### 3. Temporal Tables (SQL Server 2016+)

Enable temporal tables for complete audit history:
```csharp
modelBuilder.Entity<Documento>()
    .ToTable("Documentos", b => b.IsTemporal());
```

### 4. Database Sequence for Consecutive Numbers

Create a database sequence for thread-safe consecutive number generation:
```csharp
modelBuilder.HasSequence<int>("DocumentoConsecutivo")
    .StartsAt(1)
    .IncrementsBy(1);
```

## Testing the Configuration

After applying migrations, verify the schema with:

```sql
-- Check Documento table structure
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Documentos'
ORDER BY ORDINAL_POSITION;

-- Check indexes
SELECT
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic
    ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE OBJECT_NAME(i.object_id) = 'Documentos'
ORDER BY i.name, ic.key_ordinal;
```

## Next Steps

After implementing these configurations:

1. **Validation**: Create unit tests for entity validation rules
2. **Repository Pattern**: Implement repositories for document operations
3. **Business Logic**: Create services for document generation and calculation
4. **XML Generation**: Implement XML serialization according to Hacienda XSD schemas
5. **Digital Signature**: Integrate XAdES-BES signing with the digital certificate
6. **Hacienda API**: Implement communication with ATV (Virtual Tax Administration)
7. **PDF Generation**: Create PDF templates for document printing
