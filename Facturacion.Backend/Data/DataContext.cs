using Facturacion.Shared.Entities;
using Facturacion.Shared.Entities.Catalogos;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    // DbSets - Catálogos
    public DbSet<ActividadEconomica> ActividadesEconomicas { get; set; }
    public DbSet<Modulo> Modulos { get; set; }
    public DbSet<Privilegio> Privilegios { get; set; }
    public new DbSet<Rol> Roles { get; set; }

    // DbSets - Catálogos de Hacienda
    public DbSet<Provincia> Provincias { get; set; }
    public DbSet<Canton> Cantones { get; set; }
    public DbSet<Distrito> Distritos { get; set; }
    public DbSet<Barrio> Barrios { get; set; }
    public DbSet<TipoCodigo> TiposCodigo { get; set; }
    public DbSet<TipoDocumento> TiposDocumento { get; set; }
    public DbSet<UnidadMedida> UnidadesMedida { get; set; }
    public DbSet<Impuesto> Impuestos { get; set; }
    public DbSet<CodigoExoneracion> CodigosExoneracion { get; set; }
    public DbSet<CondicionVenta> CondicionesVenta { get; set; }
    public DbSet<MedioPago> MediosPago { get; set; }

    // DbSets - Catálogos de Hacienda v4.4 (Nuevos)
    public DbSet<CAByS> CatalogosCAByS { get; set; }
    public DbSet<TipoDescuentoHacienda> TiposDescuentoHacienda { get; set; }
    public DbSet<CodigoReferencia> CodigosReferencia { get; set; }
    public DbSet<TipoDocumentoReferencia> TiposDocumentoReferencia { get; set; }
    public DbSet<TarifaIVA> TarifasIVA { get; set; }
    public DbSet<FormaFarmaceutica> FormasFarmaceuticas { get; set; } // NUEVO v4.4 - M7

    // DbSets - Empresas y Contactos
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Sucursal> Sucursales { get; set; }
    public DbSet<Terminal> Terminales { get; set; }
    public DbSet<Consecutivo> Consecutivos { get; set; }
    public DbSet<Telefono> Telefonos { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }

    // DbSets - Productos y Categorías
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Producto> Productos { get; set; }

    // DbSets - Inventario
    public DbSet<Inventario> Inventarios { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

    // DbSets - Documentos Electrónicos (Hacienda v4.4)
    public DbSet<Documento> Documentos { get; set; }
    public DbSet<DocumentoDetalle> DocumentoDetalles { get; set; }
    public DbSet<DocumentoDetalleImpuesto> DocumentoDetalleImpuestos { get; set; }
    public DbSet<DocumentoDetalleDescuento> DocumentoDetalleDescuentos { get; set; }
    public DbSet<DocumentoDetalleVIN> DocumentoDetalleVINs { get; set; } // NUEVO v4.4 - M6
    public DbSet<DocumentoDescuento> DocumentoDescuentos { get; set; }
    public DbSet<DocumentoReferencia> DocumentoReferencias { get; set; }
    public DbSet<DocumentoMedioPago> DocumentoMediosPago { get; set; }
    public DbSet<DocumentoOtraInformacion> DocumentoOtraInformacion { get; set; }
    public DbSet<DocumentoExportacion> DocumentoExportaciones { get; set; }
    public DbSet<DocumentoReceptorMensaje> DocumentoReceptorMensajes { get; set; }
    public DbSet<ReciboPago> RecibosPago { get; set; }

    // DbSets - Tablas Intermedias
    public DbSet<EmpresaActividadEconomica> EmpresasActividadesEconomicas { get; set; }
    public DbSet<RolPrivilegio> RolesPrivilegios { get; set; }
    public DbSet<UsuarioEmpresa> UsuariosEmpresas { get; set; }

    // DbSets - Gastos
    public DbSet<CategoriaGasto> CategoriasGasto { get; set; }
    public DbSet<Gasto> Gastos { get; set; }

    // DbSets - Notificaciones
    public DbSet<Notificacion> Notificaciones { get; set; }

    // DbSets - Auditoría
    public DbSet<Auditoria> Auditorias { get; set; }

    // DbSets - Hacienda Tokens
    public DbSet<HaciendaToken> HaciendaTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =============================================
        // 1. UNIQUE INDEXES
        // =============================================

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Document)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Empresa>()
            .HasIndex(e => e.NumeroIdentificacion)
            .IsUnique();

        modelBuilder.Entity<ActividadEconomica>()
            .HasIndex(a => a.CodigoCIIU4)
            .IsUnique();

        modelBuilder.Entity<Modulo>()
            .HasIndex(m => m.Nombre)
            .IsUnique();

        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.Nombre)
            .IsUnique();

        modelBuilder.Entity<Email>()
            .HasIndex(e => e.DireccionEmail);

        // Catálogos de Hacienda - Unique Indexes
        modelBuilder.Entity<Provincia>()
            .HasIndex(p => p.Codigo)
            .IsUnique();

        modelBuilder.Entity<Canton>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        modelBuilder.Entity<Distrito>()
            .HasIndex(d => d.Codigo)
            .IsUnique();

        modelBuilder.Entity<Barrio>()
            .HasIndex(b => b.Codigo)
            .IsUnique();

        modelBuilder.Entity<TipoCodigo>()
            .HasIndex(t => t.Codigo)
            .IsUnique();

        modelBuilder.Entity<TipoDocumento>()
            .HasIndex(t => t.Codigo)
            .IsUnique();

        modelBuilder.Entity<UnidadMedida>()
            .HasIndex(u => u.Codigo)
            .IsUnique();

        modelBuilder.Entity<CodigoExoneracion>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        modelBuilder.Entity<CondicionVenta>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        modelBuilder.Entity<MedioPago>()
            .HasIndex(m => m.Codigo)
            .IsUnique();

        modelBuilder.Entity<Impuesto>()
            .HasIndex(i => i.Codigo)
            .IsUnique();

        // =============================================
        // 2. COMPOSITE INDEXES
        // =============================================

        modelBuilder.Entity<Sucursal>()
            .HasIndex(s => new { s.EmpresaId, s.Codigo })
            .IsUnique();

        modelBuilder.Entity<Terminal>()
            .HasIndex(t => new { t.SucursalId, t.Codigo })
            .IsUnique();

        modelBuilder.Entity<Consecutivo>()
            .HasIndex(c => c.ClaveNumeracion)
            .IsUnique();

        // Índice compuesto para Consecutivo: Terminal + TipoDocumento + Ambiente + Activo
        // Esto permite tener consecutivos separados por ambiente (Pruebas vs Producción)
        modelBuilder.Entity<Consecutivo>()
            .HasIndex(c => new { c.TerminalId, c.TipoDocumento, c.Ambiente, c.Activo })
            .HasFilter("[IsDeleted] = 0 AND [Activo] = 1");

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => new { c.EmpresaId, c.NumeroIdentificacion })
            .IsUnique();

        modelBuilder.Entity<Proveedor>()
            .HasIndex(p => new { p.EmpresaId, p.NumeroIdentificacion })
            .IsUnique();

        modelBuilder.Entity<Privilegio>()
            .HasIndex(p => new { p.ModuloId, p.Accion });

        modelBuilder.Entity<EmpresaActividadEconomica>()
            .HasIndex(ea => new { ea.EmpresaId, ea.ActividadEconomicaId })
            .IsUnique();

        modelBuilder.Entity<RolPrivilegio>()
            .HasIndex(rp => new { rp.RolId, rp.PrivilegioId })
            .IsUnique();

        modelBuilder.Entity<UsuarioEmpresa>()
            .HasIndex(ue => new { ue.UserId, ue.EmpresaId })
            .IsUnique();

        modelBuilder.Entity<Categoria>()
            .HasIndex(c => new { c.EmpresaId, c.Nombre })
            .IsUnique();

        modelBuilder.Entity<Producto>()
            .HasIndex(p => new { p.EmpresaId, p.Codigo })
            .IsUnique();

        modelBuilder.Entity<Inventario>()
            .HasIndex(i => new { i.ProductoId, i.SucursalId })
            .IsUnique();

        modelBuilder.Entity<MovimientoInventario>()
            .HasIndex(m => new { m.InventarioId, m.Fecha });

        modelBuilder.Entity<MovimientoInventario>()
            .HasIndex(m => m.TipoMovimiento);

        // =============================================
        // 3. DECIMAL PRECISION
        // =============================================

        modelBuilder.Entity<Impuesto>()
            .Property(i => i.Porcentaje)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioVenta)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Producto>()
            .Property(p => p.Costo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Producto>()
            .Property(p => p.StockMinimo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.SaldoActual)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.LimiteCredito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Inventario>()
            .Property(i => i.CantidadActual)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Inventario>()
            .Property(i => i.CantidadReservada)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoInventario>()
            .Property(m => m.Cantidad)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoInventario>()
            .Property(m => m.CantidadAnterior)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoInventario>()
            .Property(m => m.CantidadNueva)
            .HasPrecision(18, 2);

        // =============================================
        // 4. ONE-TO-MANY RELATIONSHIPS
        // =============================================

        // Empresa - Teléfonos
        modelBuilder.Entity<Telefono>()
            .HasOne(t => t.Empresa)
            .WithMany(e => e.Telefonos)
            .HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Empresa - Emails
        modelBuilder.Entity<Email>()
            .HasOne(e => e.Empresa)
            .WithMany(emp => emp.Emails)
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Modulo - Privilegios
        modelBuilder.Entity<Privilegio>()
            .HasOne(p => p.Modulo)
            .WithMany(m => m.Privilegios)
            .HasForeignKey(p => p.ModuloId)
            .OnDelete(DeleteBehavior.Restrict);

        // Empresa - Usuario Creación
        modelBuilder.Entity<Empresa>()
            .HasOne(e => e.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Empresa - Usuario Modificación
        modelBuilder.Entity<Empresa>()
            .HasOne(e => e.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Empresa - Usuario Eliminación
        modelBuilder.Entity<Empresa>()
            .HasOne(e => e.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Rol - Usuario Creación
        modelBuilder.Entity<Rol>()
            .HasOne(r => r.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cliente - Empresa
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Empresa)
            .WithMany(e => e.Clientes)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cliente - Usuario Creación
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cliente - Usuario Modificación
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cliente - Usuario Eliminación
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cliente - Teléfonos
        modelBuilder.Entity<Telefono>()
            .HasOne(t => t.Cliente)
            .WithMany(c => c.Telefonos)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cliente - Emails
        modelBuilder.Entity<Email>()
            .HasOne(e => e.Cliente)
            .WithMany(c => c.Emails)
            .HasForeignKey(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Proveedor - Empresa
        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Proveedores)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Proveedor - Usuario Creación
        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Proveedor - Usuario Modificación
        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Proveedor - Usuario Eliminación
        modelBuilder.Entity<Proveedor>()
            .HasOne(p => p.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Proveedor - Teléfonos
        modelBuilder.Entity<Telefono>()
            .HasOne(t => t.Proveedor)
            .WithMany(p => p.Telefonos)
            .HasForeignKey(t => t.ProveedorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Proveedor - Emails
        modelBuilder.Entity<Email>()
            .HasOne(e => e.Proveedor)
            .WithMany(p => p.Emails)
            .HasForeignKey(e => e.ProveedorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Categoria - Empresa
        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.Empresa)
            .WithMany(e => e.Categorias)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Categoria - Usuario Creación
        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Categoria - Usuario Modificación
        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Categoria - Usuario Eliminación
        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Producto - Empresa
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Empresa)
            .WithMany(e => e.Productos)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Producto - UnidadMedida
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.UnidadMedida)
            .WithMany()
            .HasForeignKey(p => p.UnidadMedidaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Producto - Impuesto
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Impuesto)
            .WithMany()
            .HasForeignKey(p => p.ImpuestoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Producto - Categoria
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Producto - Usuario Creación
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Producto - Usuario Modificación
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Producto - Usuario Eliminación
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sucursal - Empresa
        modelBuilder.Entity<Sucursal>()
            .HasOne(s => s.Empresa)
            .WithMany(e => e.Sucursales)
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sucursal - Usuario Creación
        modelBuilder.Entity<Sucursal>()
            .HasOne(s => s.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(s => s.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sucursal - Usuario Modificación
        modelBuilder.Entity<Sucursal>()
            .HasOne(s => s.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(s => s.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sucursal - Usuario Eliminación
        modelBuilder.Entity<Sucursal>()
            .HasOne(s => s.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(s => s.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Terminal - Sucursal
        modelBuilder.Entity<Terminal>()
            .HasOne(t => t.Sucursal)
            .WithMany(s => s.Terminales)
            .HasForeignKey(t => t.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Terminal - Usuario Creación
        modelBuilder.Entity<Terminal>()
            .HasOne(t => t.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Terminal - Usuario Modificación
        modelBuilder.Entity<Terminal>()
            .HasOne(t => t.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Terminal - Usuario Eliminación
        modelBuilder.Entity<Terminal>()
            .HasOne(t => t.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventario - Producto
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.Producto)
            .WithMany()
            .HasForeignKey(i => i.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventario - Sucursal
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.Sucursal)
            .WithMany()
            .HasForeignKey(i => i.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventario - Usuario Creación
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(i => i.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventario - Usuario Modificación
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(i => i.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventario - Usuario Eliminación
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(i => i.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimientoInventario - Inventario
        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.Inventario)
            .WithMany(i => i.Movimientos)
            .HasForeignKey(m => m.InventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimientoInventario - Sucursal Origen
        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.SucursalOrigen)
            .WithMany()
            .HasForeignKey(m => m.SucursalOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimientoInventario - Sucursal Destino
        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.SucursalDestino)
            .WithMany()
            .HasForeignKey(m => m.SucursalDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimientoInventario - Cliente
        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.Cliente)
            .WithMany()
            .HasForeignKey(m => m.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimientoInventario - Proveedor
        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.Proveedor)
            .WithMany()
            .HasForeignKey(m => m.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // MovimientoInventario - Usuario Creación
        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(m => m.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================================
        // 5. MANY-TO-MANY RELATIONSHIPS
        // =============================================

        // EmpresaActividadEconomica - Empresa
        modelBuilder.Entity<EmpresaActividadEconomica>()
            .HasOne(ea => ea.Empresa)
            .WithMany(e => e.ActividadesEconomicas)
            .HasForeignKey(ea => ea.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // EmpresaActividadEconomica - ActividadEconomica
        modelBuilder.Entity<EmpresaActividadEconomica>()
            .HasOne(ea => ea.ActividadEconomica)
            .WithMany(a => a.EmpresasActividades)
            .HasForeignKey(ea => ea.ActividadEconomicaId)
            .OnDelete(DeleteBehavior.Restrict);

        // RolPrivilegio - Rol
        modelBuilder.Entity<RolPrivilegio>()
            .HasOne(rp => rp.Rol)
            .WithMany(r => r.RolesPrivilegios)
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        // RolPrivilegio - Privilegio
        modelBuilder.Entity<RolPrivilegio>()
            .HasOne(rp => rp.Privilegio)
            .WithMany(p => p.RolesPrivilegios)
            .HasForeignKey(rp => rp.PrivilegioId)
            .OnDelete(DeleteBehavior.Restrict);

        // UsuarioEmpresa - User
        modelBuilder.Entity<UsuarioEmpresa>()
            .HasOne(ue => ue.User)
            .WithMany()
            .HasForeignKey(ue => ue.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UsuarioEmpresa - Empresa
        modelBuilder.Entity<UsuarioEmpresa>()
            .HasOne(ue => ue.Empresa)
            .WithMany(e => e.Usuarios)
            .HasForeignKey(ue => ue.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // UsuarioEmpresa - AsignadoPor
        modelBuilder.Entity<UsuarioEmpresa>()
            .HasOne(ue => ue.AsignadoPor)
            .WithMany()
            .HasForeignKey(ue => ue.AsignadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Catálogos de Hacienda - Geographical Relationships
        modelBuilder.Entity<Canton>()
            .HasOne(c => c.Provincia)
            .WithMany(p => p.Cantones)
            .HasForeignKey(c => c.ProvinciaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Distrito>()
            .HasOne(d => d.Canton)
            .WithMany(c => c.Distritos)
            .HasForeignKey(d => d.CantonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Barrio>()
            .HasOne(b => b.Distrito)
            .WithMany(d => d.Barrios)
            .HasForeignKey(b => b.DistritoId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================================
        // 6. COMPUTED PROPERTIES
        // =============================================

        // Inventario - CantidadDisponible es computada (CantidadActual - CantidadReservada)
        modelBuilder.Entity<Inventario>()
            .Ignore(i => i.CantidadDisponible);

        // =============================================
        // 7. DEFAULT VALUES
        // =============================================

        modelBuilder.Entity<Empresa>()
            .Property(e => e.Activa)
            .HasDefaultValue(true);

        modelBuilder.Entity<Empresa>()
            .Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Empresa>()
            .Property(e => e.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<ActividadEconomica>()
            .Property(a => a.Activa)
            .HasDefaultValue(true);

        modelBuilder.Entity<Modulo>()
            .Property(m => m.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Rol>()
            .Property(r => r.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Rol>()
            .Property(r => r.EsSistema)
            .HasDefaultValue(false);

        modelBuilder.Entity<Rol>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Telefono>()
            .Property(t => t.EsPrincipal)
            .HasDefaultValue(false);

        modelBuilder.Entity<Email>()
            .Property(e => e.EsPrincipal)
            .HasDefaultValue(false);

        modelBuilder.Entity<EmpresaActividadEconomica>()
            .Property(ea => ea.EsPrincipal)
            .HasDefaultValue(false);

        modelBuilder.Entity<UsuarioEmpresa>()
            .Property(ue => ue.FechaAsignacion)
            .HasDefaultValueSql("GETDATE()");

        // Catálogos de Hacienda - Default Values
        modelBuilder.Entity<Provincia>()
            .Property(p => p.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Canton>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Distrito>()
            .Property(d => d.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Barrio>()
            .Property(b => b.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<TipoCodigo>()
            .Property(t => t.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<TipoDocumento>()
            .Property(t => t.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<UnidadMedida>()
            .Property(u => u.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CodigoExoneracion>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CondicionVenta>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<MedioPago>()
            .Property(m => m.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Impuesto>()
            .Property(i => i.Activo)
            .HasDefaultValue(true);

        // Categoria - Default Values
        modelBuilder.Entity<Categoria>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Categoria>()
            .Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Categoria>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Producto - Default Values
        modelBuilder.Entity<Producto>()
            .Property(p => p.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Producto>()
            .Property(p => p.ControlarInventario)
            .HasDefaultValue(true);

        modelBuilder.Entity<Producto>()
            .Property(p => p.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Producto>()
            .Property(p => p.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Cliente - Default Values
        modelBuilder.Entity<Cliente>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Cliente>()
            .Property(c => c.FechaRegistro)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Cliente>()
            .Property(c => c.SaldoActual)
            .HasDefaultValue(0);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.LimiteCredito)
            .HasDefaultValue(0);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.Moneda)
            .HasDefaultValue(Facturacion.Shared.Enums.TipoMoneda.CRC);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.TipoDocumento)
            .HasDefaultValue(1);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.TipoVenta)
            .HasDefaultValue(1);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.TipoPago)
            .HasDefaultValue(1);

        // Proveedor - Default Values
        modelBuilder.Entity<Proveedor>()
            .Property(p => p.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Sucursal - Default Values
        modelBuilder.Entity<Sucursal>()
            .Property(s => s.EsPrincipal)
            .HasDefaultValue(false);

        modelBuilder.Entity<Sucursal>()
            .Property(s => s.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Sucursal>()
            .Property(s => s.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Sucursal>()
            .Property(s => s.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Terminal - Default Values
        modelBuilder.Entity<Terminal>()
            .Property(t => t.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Terminal>()
            .Property(t => t.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Terminal>()
            .Property(t => t.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Consecutivo - Default Values
        modelBuilder.Entity<Consecutivo>()
            .Property(c => c.NumeroActual)
            .HasDefaultValue(0);

        modelBuilder.Entity<Consecutivo>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Consecutivo>()
            .Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Consecutivo>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Inventario - Default Values
        modelBuilder.Entity<Inventario>()
            .Property(i => i.CantidadActual)
            .HasDefaultValue(0);

        modelBuilder.Entity<Inventario>()
            .Property(i => i.UltimaActualizacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Inventario>()
            .Property(i => i.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Inventario>()
            .Property(i => i.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // MovimientoInventario - Default Values
        modelBuilder.Entity<MovimientoInventario>()
            .Property(m => m.Fecha)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<MovimientoInventario>()
            .Property(m => m.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 8. DOCUMENTOS ELECTRÓNICOS (HACIENDA V4.4)
        // =============================================

        // Documento - Main Document Entity
        modelBuilder.Entity<Documento>()
            .HasIndex(e => e.Clave)
            .IsUnique()
            .HasDatabaseName("IX_Documento_Clave");

        modelBuilder.Entity<Documento>()
            .HasIndex(e => e.NumeroConsecutivo)
            .HasDatabaseName("IX_Documento_NumeroConsecutivo");

        modelBuilder.Entity<Documento>()
            .HasIndex(e => e.EmpresaId)
            .HasDatabaseName("IX_Documento_EmpresaId");

        modelBuilder.Entity<Documento>()
            .HasIndex(e => new { e.EmpresaId, e.TipoDocumento, e.Estado })
            .HasDatabaseName("IX_Documento_Empresa_Tipo_Estado");

        modelBuilder.Entity<Documento>()
            .HasIndex(e => new { e.EmpresaId, e.Ambiente, e.TipoDocumento })
            .HasDatabaseName("IX_Documento_Empresa_Ambiente_Tipo");

        modelBuilder.Entity<Documento>()
            .HasIndex(e => e.FechaEmision)
            .HasDatabaseName("IX_Documento_FechaEmision");

        modelBuilder.Entity<Documento>()
            .HasQueryFilter(e => !e.IsDeleted);

        // Documento - Relationships
        modelBuilder.Entity<Documento>()
            .HasOne(d => d.Empresa)
            .WithMany(e => e.Documentos)
            .HasForeignKey(d => d.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Documento>()
            .HasOne(d => d.Sucursal)
            .WithMany(s => s.Documentos)
            .HasForeignKey(d => d.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Documento>()
            .HasOne(d => d.Terminal)
            .WithMany(t => t.Documentos)
            .HasForeignKey(d => d.TerminalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Documento>()
            .HasOne(d => d.Cliente)
            .WithMany(c => c.Documentos)
            .HasForeignKey(d => d.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Documento>()
            .HasOne(d => d.Proveedor)
            .WithMany(p => p.Documentos)
            .HasForeignKey(d => d.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Documento>()
            .HasOne(d => d.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Documento>()
            .HasOne(d => d.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Documento>()
            .HasOne(d => d.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Documento - Decimal Precision
        modelBuilder.Entity<Documento>()
            .Property(d => d.TipoCambio)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalMercanciasGravadas)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalMercanciasExentas)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalMercanciasExoneradas)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalGravado)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalExento)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalExonerado)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalVenta)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalDescuentos)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.Subtotal)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalImpuestos)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.IVADevuelto)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Documento>()
            .Property(d => d.TotalOtrosCargos)
            .HasPrecision(18, 5);

        // Documento - Default Values
        modelBuilder.Entity<Documento>()
            .Property(d => d.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Documento>()
            .Property(d => d.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // DocumentoDetalle
        modelBuilder.Entity<DocumentoDetalle>()
            .HasIndex(e => e.DocumentoId)
            .HasDatabaseName("IX_DocumentoDetalle_DocumentoId");

        modelBuilder.Entity<DocumentoDetalle>()
            .HasIndex(e => new { e.DocumentoId, e.NumeroLinea })
            .HasDatabaseName("IX_DocumentoDetalle_Documento_Linea");

        modelBuilder.Entity<DocumentoDetalle>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoDetalle>()
            .HasOne(d => d.Documento)
            .WithMany(doc => doc.Detalles)
            .HasForeignKey(d => d.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoDetalle>()
            .HasOne(d => d.UnidadMedida)
            .WithMany()
            .HasForeignKey(d => d.UnidadMedidaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoDetalle>()
            .HasOne(d => d.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // DocumentoDetalle - Decimal Precision
        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.Cantidad)
            .HasPrecision(18, 3);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.PrecioUnitario)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.MontoTotal)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.Subtotal)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.BaseImponible)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.MontoTotalLinea)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.MontoDescuento)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.MontoImpuesto)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalle>()
            .Property(d => d.ImpuestoNeto)
            .HasPrecision(18, 5);

        // DocumentoDetalleImpuesto
        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .HasIndex(e => e.DocumentoDetalleId)
            .HasDatabaseName("IX_DocumentoDetalleImpuesto_DetalleId");

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .HasOne(d => d.DocumentoDetalle)
            .WithMany(det => det.Impuestos)
            .HasForeignKey(d => d.DocumentoDetalleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .HasOne(d => d.Impuesto)
            .WithMany()
            .HasForeignKey(d => d.ImpuestoId)
            .OnDelete(DeleteBehavior.Restrict);

        // DocumentoDetalleImpuesto - Decimal Precision
        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .Property(d => d.Tarifa)
            .HasPrecision(5, 2);

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .Property(d => d.FactorIVADevuelto)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .Property(d => d.MontoBase)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .Property(d => d.MontoImpuesto)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .Property(d => d.MontoExoneracion)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoDetalleImpuesto>()
            .Property(d => d.PorcentajeExoneracion)
            .HasPrecision(5, 2);

        // DocumentoDetalleDescuento
        modelBuilder.Entity<DocumentoDetalleDescuento>()
            .HasIndex(e => e.DocumentoDetalleId)
            .HasDatabaseName("IX_DocumentoDetalleDescuento_DetalleId");

        modelBuilder.Entity<DocumentoDetalleDescuento>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoDetalleDescuento>()
            .HasOne(d => d.DocumentoDetalle)
            .WithMany(det => det.Descuentos)
            .HasForeignKey(d => d.DocumentoDetalleId)
            .OnDelete(DeleteBehavior.Cascade);

        // DocumentoDetalleDescuento - Decimal Precision
        modelBuilder.Entity<DocumentoDetalleDescuento>()
            .Property(d => d.MontoDescuento)
            .HasPrecision(18, 5);

        // DocumentoDescuento
        modelBuilder.Entity<DocumentoDescuento>()
            .HasIndex(e => e.DocumentoId)
            .HasDatabaseName("IX_DocumentoDescuento_DocumentoId");

        modelBuilder.Entity<DocumentoDescuento>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoDescuento>()
            .HasOne(d => d.Documento)
            .WithMany(doc => doc.Descuentos)
            .HasForeignKey(d => d.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // DocumentoDescuento - Decimal Precision
        modelBuilder.Entity<DocumentoDescuento>()
            .Property(d => d.MontoDescuento)
            .HasPrecision(18, 5);

        // DocumentoReferencia
        modelBuilder.Entity<DocumentoReferencia>()
            .HasIndex(e => e.DocumentoId)
            .HasDatabaseName("IX_DocumentoReferencia_DocumentoId");

        modelBuilder.Entity<DocumentoReferencia>()
            .HasIndex(e => e.NumeroDocumentoReferenciado)
            .HasDatabaseName("IX_DocumentoReferencia_NumeroReferenciado");

        modelBuilder.Entity<DocumentoReferencia>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoReferencia>()
            .HasOne(d => d.Documento)
            .WithMany(doc => doc.Referencias)
            .HasForeignKey(d => d.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoReferencia>()
            .HasOne(d => d.DocumentoReferenciado)
            .WithMany()
            .HasForeignKey(d => d.DocumentoReferenciadoId)
            .OnDelete(DeleteBehavior.Restrict);

        // DocumentoMedioPago
        modelBuilder.Entity<DocumentoMedioPago>()
            .HasIndex(e => e.DocumentoId)
            .HasDatabaseName("IX_DocumentoMedioPago_DocumentoId");

        modelBuilder.Entity<DocumentoMedioPago>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoMedioPago>()
            .HasOne(d => d.Documento)
            .WithMany(doc => doc.MediosPago)
            .HasForeignKey(d => d.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoMedioPago>()
            .HasOne(d => d.MedioPago)
            .WithMany()
            .HasForeignKey(d => d.MedioPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        // DocumentoOtraInformacion
        modelBuilder.Entity<DocumentoOtraInformacion>()
            .HasIndex(e => e.DocumentoId)
            .HasDatabaseName("IX_DocumentoOtraInformacion_DocumentoId");

        modelBuilder.Entity<DocumentoOtraInformacion>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoOtraInformacion>()
            .HasOne(d => d.Documento)
            .WithMany(doc => doc.OtraInformacion)
            .HasForeignKey(d => d.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // DocumentoExportacion
        modelBuilder.Entity<DocumentoExportacion>()
            .HasIndex(e => e.DocumentoId)
            .IsUnique()
            .HasDatabaseName("IX_DocumentoExportacion_DocumentoId");

        modelBuilder.Entity<DocumentoExportacion>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoExportacion>()
            .HasOne(d => d.Documento)
            .WithOne(doc => doc.Exportacion)
            .HasForeignKey<DocumentoExportacion>(d => d.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // DocumentoReceptorMensaje
        modelBuilder.Entity<DocumentoReceptorMensaje>()
            .HasIndex(e => e.ClaveMensaje)
            .IsUnique()
            .HasDatabaseName("IX_DocumentoReceptorMensaje_Clave");

        modelBuilder.Entity<DocumentoReceptorMensaje>()
            .HasIndex(e => e.DocumentoOriginalId)
            .HasDatabaseName("IX_DocumentoReceptorMensaje_DocumentoOriginal");

        modelBuilder.Entity<DocumentoReceptorMensaje>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoReceptorMensaje>()
            .HasOne(d => d.DocumentoOriginal)
            .WithMany()
            .HasForeignKey(d => d.DocumentoOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoReceptorMensaje>()
            .Property(d => d.MontoTotalAceptado)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoReceptorMensaje>()
            .Property(d => d.MontoTotalImpuestoAceptado)
            .HasPrecision(18, 5);

        // ReciboPago (REP - Nuevo en v4.4)
        modelBuilder.Entity<ReciboPago>()
            .HasIndex(e => e.DocumentoId)
            .HasDatabaseName("IX_ReciboPago_DocumentoId");

        modelBuilder.Entity<ReciboPago>()
            .HasIndex(e => e.DocumentoOriginalId)
            .HasDatabaseName("IX_ReciboPago_DocumentoOriginal");

        modelBuilder.Entity<ReciboPago>()
            .HasIndex(e => e.ClaveDocumentoOriginal)
            .HasDatabaseName("IX_ReciboPago_ClaveOriginal");

        modelBuilder.Entity<ReciboPago>()
            .HasIndex(e => new { e.DocumentoOriginalId, e.FechaPago })
            .HasDatabaseName("IX_ReciboPago_Documento_Fecha");

        modelBuilder.Entity<ReciboPago>()
            .HasQueryFilter(e => !e.IsDeleted);

        // ReciboPago - Relationships
        modelBuilder.Entity<ReciboPago>()
            .HasOne(r => r.Documento)
            .WithMany()
            .HasForeignKey(r => r.DocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReciboPago>()
            .HasOne(r => r.DocumentoOriginal)
            .WithMany()
            .HasForeignKey(r => r.DocumentoOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReciboPago>()
            .HasOne(r => r.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReciboPago>()
            .HasOne(r => r.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReciboPago>()
            .HasOne(r => r.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // ReciboPago - Decimal Precision
        modelBuilder.Entity<ReciboPago>()
            .Property(r => r.MontoPagado)
            .HasPrecision(18, 5);

        modelBuilder.Entity<ReciboPago>()
            .Property(r => r.SaldoPendiente)
            .HasPrecision(18, 5);

        modelBuilder.Entity<ReciboPago>()
            .Property(r => r.TipoCambio)
            .HasPrecision(18, 5);

        // ReciboPago - Default Values
        modelBuilder.Entity<ReciboPago>()
            .Property(r => r.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<ReciboPago>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 9. MAXIMUM LENGTH
        // =============================================

        // Definido en las entidades con Data Annotations

        // =============================================
        // 10. GASTOS MODULE
        // =============================================

        // CategoriaGasto - Index
        modelBuilder.Entity<CategoriaGasto>()
            .HasIndex(c => c.Nombre)
            .IsUnique();

        // Gasto - Indexes
        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.EmpresaId)
            .HasDatabaseName("IX_Gasto_EmpresaId");

        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.ProveedorId)
            .HasDatabaseName("IX_Gasto_ProveedorId");

        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.CategoriaGastoId)
            .HasDatabaseName("IX_Gasto_CategoriaGastoId");

        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.FechaGasto)
            .HasDatabaseName("IX_Gasto_FechaGasto");

        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.EstadoPago)
            .HasDatabaseName("IX_Gasto_EstadoPago");

        modelBuilder.Entity<Gasto>()
            .HasIndex(g => new { g.EmpresaId, g.NumeroDocumento })
            .IsUnique()
            .HasDatabaseName("IX_Gasto_Empresa_NumeroDocumento");

        // Gasto - Soft Delete Query Filter
        modelBuilder.Entity<Gasto>()
            .HasQueryFilter(g => !g.IsDeleted);

        // Gasto - Relationships
        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.Empresa)
            .WithMany()
            .HasForeignKey(g => g.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.Proveedor)
            .WithMany()
            .HasForeignKey(g => g.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.CategoriaGasto)
            .WithMany(c => c.Gastos)
            .HasForeignKey(g => g.CategoriaGastoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(g => g.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(g => g.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(g => g.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Gasto>()
            .HasOne(g => g.UsuarioAprobacion)
            .WithMany()
            .HasForeignKey(g => g.UsuarioAprobacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Gasto - Decimal Precision
        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoSubtotal)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoImpuesto)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoTotal)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.TipoCambio)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoPagado)
            .HasPrecision(18, 5);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.SaldoPendiente)
            .HasPrecision(18, 5);

        // Gasto - Default Values
        modelBuilder.Entity<Gasto>()
            .Property(g => g.Aprobado)
            .HasDefaultValue(false);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoPagado)
            .HasDefaultValue(0);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.SaldoPendiente)
            .HasDefaultValue(0);

        modelBuilder.Entity<Gasto>()
            .Property(g => g.MontoImpuesto)
            .HasDefaultValue(0);

        // CategoriaGasto - Default Values
        modelBuilder.Entity<CategoriaGasto>()
            .Property(c => c.Activa)
            .HasDefaultValue(true);

        // =============================================
        // 11. NOTIFICACIONES MODULE
        // =============================================

        // Notificacion - Indexes
        modelBuilder.Entity<Notificacion>()
            .HasIndex(n => n.EmpresaId)
            .HasDatabaseName("IX_Notificacion_EmpresaId");

        modelBuilder.Entity<Notificacion>()
            .HasIndex(n => n.UsuarioId)
            .HasDatabaseName("IX_Notificacion_UsuarioId");

        modelBuilder.Entity<Notificacion>()
            .HasIndex(n => new { n.UsuarioId, n.EmpresaId, n.Leida })
            .HasDatabaseName("IX_Notificacion_Usuario_Empresa_Leida");

        modelBuilder.Entity<Notificacion>()
            .HasIndex(n => n.FechaCreacion)
            .HasDatabaseName("IX_Notificacion_FechaCreacion");

        modelBuilder.Entity<Notificacion>()
            .HasIndex(n => n.FechaExpiracion)
            .HasDatabaseName("IX_Notificacion_FechaExpiracion");

        modelBuilder.Entity<Notificacion>()
            .HasIndex(n => new { n.TipoNotificacion, n.Leida })
            .HasDatabaseName("IX_Notificacion_Tipo_Leida");

        // Notificacion - Relationships
        modelBuilder.Entity<Notificacion>()
            .HasOne(n => n.Empresa)
            .WithMany()
            .HasForeignKey(n => n.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notificacion>()
            .HasOne(n => n.Usuario)
            .WithMany()
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notificacion - Default Values
        modelBuilder.Entity<Notificacion>()
            .Property(n => n.Leida)
            .HasDefaultValue(false);

        modelBuilder.Entity<Notificacion>()
            .Property(n => n.Importante)
            .HasDefaultValue(false);

        modelBuilder.Entity<Notificacion>()
            .Property(n => n.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 12. CATÁLOGOS HACIENDA V4.4 - NUEVOS
        // =============================================

        // CAByS - Catálogo de Bienes y Servicios
        modelBuilder.Entity<CAByS>()
            .HasIndex(c => c.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_CAByS_Codigo");

        modelBuilder.Entity<CAByS>()
            .Property(c => c.TarifaImpuesto)
            .HasPrecision(5, 2);

        modelBuilder.Entity<CAByS>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        // TipoDescuentoHacienda
        modelBuilder.Entity<TipoDescuentoHacienda>()
            .HasIndex(t => t.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_TipoDescuentoHacienda_Codigo");

        modelBuilder.Entity<TipoDescuentoHacienda>()
            .Property(t => t.Activo)
            .HasDefaultValue(true);

        // CodigoReferencia
        modelBuilder.Entity<CodigoReferencia>()
            .HasIndex(c => c.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_CodigoReferencia_Codigo");

        modelBuilder.Entity<CodigoReferencia>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        // TipoDocumentoReferencia
        modelBuilder.Entity<TipoDocumentoReferencia>()
            .HasIndex(t => t.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_TipoDocumentoReferencia_Codigo");

        modelBuilder.Entity<TipoDocumentoReferencia>()
            .Property(t => t.Activo)
            .HasDefaultValue(true);

        // TarifaIVA - Catálogo de Tarifas de IVA v4.4
        modelBuilder.Entity<TarifaIVA>()
            .HasIndex(t => t.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_TarifaIVA_Codigo");

        modelBuilder.Entity<TarifaIVA>()
            .Property(t => t.Porcentaje)
            .HasPrecision(5, 2);

        modelBuilder.Entity<TarifaIVA>()
            .Property(t => t.Activo)
            .HasDefaultValue(true);

        // NUEVO v4.4 - M7: FormaFarmaceutica - Catálogo de Formas Farmacéuticas
        modelBuilder.Entity<FormaFarmaceutica>()
            .HasIndex(f => f.Codigo)
            .IsUnique()
            .HasDatabaseName("IX_FormaFarmaceutica_Codigo");

        modelBuilder.Entity<FormaFarmaceutica>()
            .Property(f => f.Activo)
            .HasDefaultValue(true);

        // NUEVO v4.4 - M6: DocumentoDetalleVIN - Múltiples VINs por línea de detalle
        modelBuilder.Entity<DocumentoDetalleVIN>()
            .HasIndex(v => v.DocumentoDetalleId)
            .HasDatabaseName("IX_DocumentoDetalleVIN_DetalleId");

        modelBuilder.Entity<DocumentoDetalleVIN>()
            .HasIndex(v => new { v.DocumentoDetalleId, v.NumeroOrden })
            .IsUnique()
            .HasDatabaseName("IX_DocumentoDetalleVIN_Detalle_Orden");

        modelBuilder.Entity<DocumentoDetalleVIN>()
            .HasOne(v => v.DocumentoDetalle)
            .WithMany(d => d.NumerosVIN)
            .HasForeignKey(v => v.DocumentoDetalleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoDetalleVIN>()
            .HasOne(v => v.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(v => v.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoDetalleVIN>()
            .Property(v => v.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // NUEVO v4.4 - M7: Relación DocumentoDetalle -> FormaFarmaceutica
        modelBuilder.Entity<DocumentoDetalle>()
            .HasOne(d => d.FormaFarmaceuticaNavigation)
            .WithMany()
            .HasForeignKey(d => d.FormaFarmaceuticaId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================================
        // 13. AUDITORÍA
        // =============================================

        modelBuilder.Entity<Auditoria>()
            .HasIndex(a => a.EmpresaId)
            .HasDatabaseName("IX_Auditoria_EmpresaId");

        modelBuilder.Entity<Auditoria>()
            .HasIndex(a => a.UsuarioId)
            .HasDatabaseName("IX_Auditoria_UsuarioId");

        modelBuilder.Entity<Auditoria>()
            .HasIndex(a => a.Fecha)
            .HasDatabaseName("IX_Auditoria_Fecha");

        modelBuilder.Entity<Auditoria>()
            .HasIndex(a => a.Tabla)
            .HasDatabaseName("IX_Auditoria_Tabla");

        modelBuilder.Entity<Auditoria>()
            .HasIndex(a => new { a.EmpresaId, a.Tabla, a.Fecha })
            .HasDatabaseName("IX_Auditoria_Empresa_Tabla_Fecha");

        modelBuilder.Entity<Auditoria>()
            .HasIndex(a => new { a.UsuarioId, a.Fecha })
            .HasDatabaseName("IX_Auditoria_Usuario_Fecha");

        // Auditoria - Relationships
        modelBuilder.Entity<Auditoria>()
            .HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Auditoria>()
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auditoria - Default Values
        modelBuilder.Entity<Auditoria>()
            .Property(a => a.Exitoso)
            .HasDefaultValue(true);

        modelBuilder.Entity<Auditoria>()
            .Property(a => a.Fecha)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 14. HACIENDA TOKENS
        // =============================================

        // HaciendaToken - Indexes
        modelBuilder.Entity<HaciendaToken>()
            .HasIndex(h => new { h.EmpresaId, h.Ambiente, h.Activo })
            .HasDatabaseName("IX_HaciendaToken_Empresa_Ambiente_Activo");

        modelBuilder.Entity<HaciendaToken>()
            .HasIndex(h => h.FechaExpiracionToken)
            .HasDatabaseName("IX_HaciendaToken_FechaExpiracion");

        modelBuilder.Entity<HaciendaToken>()
            .HasIndex(h => h.FechaExpiracionRefreshToken)
            .HasDatabaseName("IX_HaciendaToken_FechaExpiracionRefresh");

        // HaciendaToken - Relationship
        modelBuilder.Entity<HaciendaToken>()
            .HasOne(h => h.Empresa)
            .WithMany()
            .HasForeignKey(h => h.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // HaciendaToken - Default Values
        modelBuilder.Entity<HaciendaToken>()
            .Property(h => h.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<HaciendaToken>()
            .Property(h => h.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");
    }
}
