using Facturacion.Shared.Entities;
using Facturacion.Shared.Entities.Catalogos;
using Facturacion.Shared.Enums;
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
    public DbSet<DocumentoOtroCargo> DocumentoOtrosCargos { get; set; }
    public DbSet<DocumentoExportacion> DocumentoExportaciones { get; set; }
    public DbSet<DocumentoReceptorMensaje> DocumentoReceptorMensajes { get; set; }
    public DbSet<ReciboPago> RecibosPago { get; set; }

    // DbSets - Cotizaciones
    public DbSet<Cotizacion> Cotizaciones { get; set; }
    public DbSet<CotizacionDetalle> CotizacionDetalles { get; set; }

    // DbSets - Cuentas Por Cobrar
    public DbSet<CuentaPorCobrar> CuentasPorCobrar { get; set; }
    public DbSet<AbonoCobranza> AbonosCobranza { get; set; }

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

    // DbSets - Migración de Datos
    public DbSet<MigracionIdMapping> MigracionesIdMapping { get; set; }

    // DbSets - Almacenes y Bancos (Fase 1 ERP Integration)
    public DbSet<Bodega> Bodegas { get; set; }
    public DbSet<Banco> Bancos { get; set; }

    // DbSets - Inventario Avanzado (Fase 2 ERP Integration)
    public DbSet<Lote> Lotes { get; set; }
    public DbSet<TrasladoInventario> TrasladosInventario { get; set; }
    public DbSet<TrasladoInventarioDetalle> TrasladosInventarioDetalle { get; set; }
    public DbSet<AjusteInventario> AjustesInventario { get; set; }
    public DbSet<AjusteInventarioDetalle> AjustesInventarioDetalle { get; set; }

    // DbSets - CRM (Fase 3 ERP Integration)
    public DbSet<EtapaPipeline> EtapasPipeline { get; set; }
    public DbSet<Competidor> Competidores { get; set; }
    public DbSet<Oportunidad> Oportunidades { get; set; }
    public DbSet<ActividadCRM> ActividadesCRM { get; set; }
    public DbSet<NotaOportunidad> NotasOportunidad { get; set; }
    public DbSet<HistorialEtapaOportunidad> HistorialEtapasOportunidad { get; set; }

    // DbSets - RRHH y Nómina (Fase 4 ERP Integration)
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Puesto> Puestos { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<ContactoEmergencia> ContactosEmergencia { get; set; }
    public DbSet<ExpedienteDigital> ExpedientesDigitales { get; set; }
    public DbSet<Vacacion> Vacaciones { get; set; }
    public DbSet<Incapacidad> Incapacidades { get; set; }
    public DbSet<AccionPersonal> AccionesPersonal { get; set; }
    public DbSet<Planilla> Planillas { get; set; }
    public DbSet<DetallePlanilla> DetallesPlanilla { get; set; }

    // DbSets - Activos Fijos (Fase 5 ERP Integration)
    public DbSet<CategoriaActivo> CategoriasActivo { get; set; }
    public DbSet<ActivoFijo> ActivosFijos { get; set; }
    public DbSet<DepreciacionActivo> DepreciacionesActivo { get; set; }
    public DbSet<TrasladoActivo> TrasladosActivo { get; set; }

    // DbSets - Compras Avanzadas (Fase 6 ERP Integration)
    public DbSet<Requisicion> Requisiciones { get; set; }
    public DbSet<RequisicionDetalle> RequisicionesDetalle { get; set; }
    public DbSet<CotizacionProveedor> CotizacionesProveedor { get; set; }
    public DbSet<CotizacionProveedorDetalle> CotizacionesProveedorDetalle { get; set; }
    public DbSet<ComparativoCotizacion> ComparativosCotizacion { get; set; }
    public DbSet<ComparativoCotizacionDetalle> ComparativosCotizacionDetalle { get; set; }
    public DbSet<EvaluacionProveedor> EvaluacionesProveedor { get; set; }
    public DbSet<OrdenCompra> OrdenesCompra { get; set; }
    public DbSet<OrdenCompraDetalle> OrdenesCompraDetalle { get; set; }
    public DbSet<RecepcionCompra> RecepcionesCompra { get; set; }
    public DbSet<RecepcionCompraDetalle> RecepcionesCompraDetalle { get; set; }

    // DbSets - Contabilidad (Fase 7 ERP Integration)
    public DbSet<CuentaContable> CuentasContables { get; set; }
    public DbSet<PeriodoFiscal> PeriodosFiscales { get; set; }
    public DbSet<PeriodoContable> PeriodosContables { get; set; }
    public DbSet<AsientoContable> AsientosContables { get; set; }
    public DbSet<MovimientoContable> MovimientosContables { get; set; }
    public DbSet<ConfiguracionContable> ConfiguracionesContables { get; set; }
    public DbSet<CuentaIntegracion> CuentasIntegracion { get; set; }
    public DbSet<PlantillaAsiento> PlantillasAsiento { get; set; }
    public DbSet<PlantillaAsientoLinea> PlantillasAsientoLineas { get; set; }

    // DbSets - Workflow (Fase 8 ERP Integration)
    public DbSet<TipoWorkflow> TiposWorkflow { get; set; }
    public DbSet<NivelAprobacion> NivelesAprobacion { get; set; }
    public DbSet<SolicitudAprobacion> SolicitudesAprobacion { get; set; }
    public DbSet<AccionAprobacion> AccionesAprobacion { get; set; }

    // DbSets - Presupuestos y Conciliación (Fase 9 ERP Integration)
    public DbSet<CentroCosto> CentrosCosto { get; set; }
    public DbSet<Presupuesto> Presupuestos { get; set; }
    public DbSet<LineaPresupuesto> LineasPresupuesto { get; set; }
    public DbSet<CuentaBancaria> CuentasBancarias { get; set; }
    public DbSet<MovimientoBancario> MovimientosBancarios { get; set; }
    public DbSet<ConciliacionBancaria> ConciliacionesBancarias { get; set; }
    public DbSet<ExtractoBancario> ExtractosBancarios { get; set; }
    public DbSet<LineaExtractoBancario> LineasExtractoBancario { get; set; }
    public DbSet<ReglaConciliacion> ReglasConciliacion { get; set; }

    // DbSets - Cuentas por Pagar (Fase 10 ERP Integration)
    public DbSet<CuentaPorPagar> CuentasPorPagar { get; set; }
    public DbSet<AbonoPago> AbonosPago { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Override Identity's unique index on NormalizedName to allow
        // same role name in different empresas (roles personalizados)
        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.NormalizedName)
            .HasDatabaseName("RoleNameIndex")
            .IsUnique(false);

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
            .HasIndex(e => new { e.NumeroIdentificacion, e.Ambiente })
            .IsUnique();

        modelBuilder.Entity<ActividadEconomica>()
            .HasIndex(a => a.CodigoCIIU4)
            .IsUnique();

        modelBuilder.Entity<Modulo>()
            .HasIndex(m => m.Nombre)
            .IsUnique();

        modelBuilder.Entity<Rol>()
            .HasIndex(r => new { r.Nombre, r.EmpresaId })
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

        // Índice único para ClaveNumeracion por Empresa + Sucursal + Terminal + TipoDocumento + Ambiente
        modelBuilder.Entity<Consecutivo>()
            .HasIndex(c => new { c.EmpresaId, c.SucursalId, c.TerminalId, c.TipoDocumento, c.Ambiente, c.ClaveNumeracion })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

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

        modelBuilder.Entity<Cliente>()
            .Property(c => c.PorcentajeExoneracion)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.DescuentoGeneral)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.LimiteCredito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.SaldoPendiente)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.PedidoMinimo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.DescuentoGeneral)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.RetencionIVA)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.RetencionRenta)
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

        // Rol - Empresa (nullable: null para roles de sistema, set para roles personalizados)
        modelBuilder.Entity<Rol>()
            .HasOne(r => r.Empresa)
            .WithMany()
            .HasForeignKey(r => r.EmpresaId)
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

        // Producto - CAByS (Código de Bienes y Servicios)
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Cabys)
            .WithMany()
            .HasForeignKey(p => p.CabysId)
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

        modelBuilder.Entity<Producto>()
            .Property(p => p.Moneda)
            .HasDefaultValue(TipoMoneda.CRC);

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

        modelBuilder.Entity<Cliente>()
            .Property(c => c.DiasCredito)
            .HasDefaultValue(0);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.ExentoIVA)
            .HasDefaultValue(false);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.PorcentajeExoneracion)
            .HasDefaultValue(0);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.DescuentoGeneral)
            .HasDefaultValue(0);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.RequiereOrdenCompra)
            .HasDefaultValue(false);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.EnMora)
            .HasDefaultValue(false);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.Bloqueado)
            .HasDefaultValue(false);

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

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.DiasCredito)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.LimiteCredito)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.SaldoPendiente)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.TiempoEntrega)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.PedidoMinimo)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.DescuentoGeneral)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.EsExtranjero)
            .HasDefaultValue(false);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.RetencionIVA)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.RetencionRenta)
            .HasDefaultValue(0);

        modelBuilder.Entity<Proveedor>()
            .Property(p => p.Bloqueado)
            .HasDefaultValue(false);

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

        // GAP-003: Integración Contable
        modelBuilder.Entity<Documento>()
            .HasOne(d => d.AsientoContable)
            .WithMany()
            .HasForeignKey(d => d.AsientoContableId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // DocumentoOtroCargo (v4.4 - FASE 2)
        modelBuilder.Entity<DocumentoOtroCargo>()
            .HasIndex(e => e.DocumentoId)
            .HasDatabaseName("IX_DocumentoOtroCargo_DocumentoId");

        modelBuilder.Entity<DocumentoOtroCargo>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DocumentoOtroCargo>()
            .HasOne(d => d.Documento)
            .WithMany(doc => doc.OtrosCargos)
            .HasForeignKey(d => d.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentoOtroCargo>()
            .Property(d => d.Monto)
            .HasPrecision(18, 5);

        modelBuilder.Entity<DocumentoOtroCargo>()
            .HasOne(d => d.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoOtroCargo>()
            .HasOne(d => d.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentoOtroCargo>()
            .HasOne(d => d.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

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
        // 8.1 COTIZACIONES (SALES QUOTATIONS)
        // =============================================

        // Cotizacion - Indexes
        modelBuilder.Entity<Cotizacion>()
            .HasIndex(c => c.Numero)
            .IsUnique()
            .HasDatabaseName("IX_Cotizacion_Numero");

        modelBuilder.Entity<Cotizacion>()
            .HasIndex(c => c.EmpresaId)
            .HasDatabaseName("IX_Cotizacion_EmpresaId");

        modelBuilder.Entity<Cotizacion>()
            .HasIndex(c => new { c.EmpresaId, c.Estado })
            .HasDatabaseName("IX_Cotizacion_Empresa_Estado");

        modelBuilder.Entity<Cotizacion>()
            .HasIndex(c => c.FechaEmision)
            .HasDatabaseName("IX_Cotizacion_FechaEmision");

        modelBuilder.Entity<Cotizacion>()
            .HasIndex(c => c.FechaVencimiento)
            .HasDatabaseName("IX_Cotizacion_FechaVencimiento");

        modelBuilder.Entity<Cotizacion>()
            .HasQueryFilter(c => !c.IsDeleted);

        // Cotizacion - Relationships
        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.Sucursal)
            .WithMany()
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.Terminal)
            .WithMany()
            .HasForeignKey(c => c.TerminalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.DocumentoGenerado)
            .WithMany()
            .HasForeignKey(c => c.DocumentoGeneradoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // CotizacionDetalle - Relationships
        modelBuilder.Entity<CotizacionDetalle>()
            .HasOne(cd => cd.Cotizacion)
            .WithMany(c => c.Detalles)
            .HasForeignKey(cd => cd.CotizacionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CotizacionDetalle>()
            .HasOne(cd => cd.Producto)
            .WithMany()
            .HasForeignKey(cd => cd.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // CotizacionDetalle - Indexes
        modelBuilder.Entity<CotizacionDetalle>()
            .HasIndex(cd => cd.CotizacionId)
            .HasDatabaseName("IX_CotizacionDetalle_CotizacionId");

        // =============================================
        // 9. MAXIMUM LENGTH
        // =============================================

        // Definido en las entidades con Data Annotations

        // =============================================
        // 10. GASTOS MODULE
        // =============================================

        // CategoriaGasto - Index (unique per empresa)
        modelBuilder.Entity<CategoriaGasto>()
            .HasIndex(c => new { c.EmpresaId, c.Nombre })
            .IsUnique()
            .HasDatabaseName("IX_CategoriaGasto_Empresa_Nombre");

        modelBuilder.Entity<CategoriaGasto>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // =============================================
        // 15. MIGRACIÓN DE DATOS
        // =============================================

        // MigracionIdMapping - Unique Indexes
        // Índice único compuesto: No pueden existir dos mapeos para la misma entidad con el mismo ID anterior
        modelBuilder.Entity<MigracionIdMapping>()
            .HasIndex(m => new { m.NombreEntidad, m.IdAnterior })
            .IsUnique()
            .HasDatabaseName("IX_MigracionIdMapping_Entidad_IdAnterior");

        // Índice en IdNuevo para búsquedas inversas rápidas
        modelBuilder.Entity<MigracionIdMapping>()
            .HasIndex(m => m.IdNuevo)
            .HasDatabaseName("IX_MigracionIdMapping_IdNuevo");

        // Índice en NombreEntidad para filtrar por tipo de entidad
        modelBuilder.Entity<MigracionIdMapping>()
            .HasIndex(m => m.NombreEntidad)
            .HasDatabaseName("IX_MigracionIdMapping_NombreEntidad");

        // MigracionIdMapping - Default Values
        modelBuilder.Entity<MigracionIdMapping>()
            .Property(m => m.FechaMigracion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 16. BODEGAS (ALMACENES)
        // =============================================

        // Bodega - Unique Indexes
        modelBuilder.Entity<Bodega>()
            .HasIndex(b => new { b.EmpresaId, b.SucursalId, b.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Bodega_Empresa_Sucursal_Codigo");

        // Bodega - Soft Delete Query Filter
        modelBuilder.Entity<Bodega>()
            .HasQueryFilter(b => !b.IsDeleted);

        // Bodega - Relationships
        modelBuilder.Entity<Bodega>()
            .HasOne(b => b.Empresa)
            .WithMany()
            .HasForeignKey(b => b.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bodega>()
            .HasOne(b => b.Sucursal)
            .WithMany()
            .HasForeignKey(b => b.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bodega>()
            .HasOne(b => b.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(b => b.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bodega>()
            .HasOne(b => b.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(b => b.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bodega>()
            .HasOne(b => b.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(b => b.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Bodega - Default Values
        modelBuilder.Entity<Bodega>()
            .Property(b => b.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Bodega>()
            .Property(b => b.EsPrincipal)
            .HasDefaultValue(false);

        modelBuilder.Entity<Bodega>()
            .Property(b => b.PermiteNegativos)
            .HasDefaultValue(false);

        modelBuilder.Entity<Bodega>()
            .Property(b => b.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Bodega>()
            .Property(b => b.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 17. BANCOS
        // =============================================

        // Banco - Unique Indexes (per empresa)
        modelBuilder.Entity<Banco>()
            .HasIndex(b => new { b.EmpresaId, b.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Banco_Empresa_Codigo");

        modelBuilder.Entity<Banco>()
            .HasIndex(b => b.CodigoSINPE)
            .HasDatabaseName("IX_Banco_CodigoSINPE");

        // Banco - Soft Delete Query Filter
        modelBuilder.Entity<Banco>()
            .HasQueryFilter(b => !b.IsDeleted);

        // Banco - Relationships
        modelBuilder.Entity<Banco>()
            .HasOne(b => b.Empresa)
            .WithMany()
            .HasForeignKey(b => b.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Banco>()
            .HasOne(b => b.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(b => b.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Banco>()
            .HasOne(b => b.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(b => b.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Banco>()
            .HasOne(b => b.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(b => b.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Banco - Default Values
        modelBuilder.Entity<Banco>()
            .Property(b => b.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Banco>()
            .Property(b => b.EsNacional)
            .HasDefaultValue(true);

        modelBuilder.Entity<Banco>()
            .Property(b => b.EsEstatal)
            .HasDefaultValue(false);

        modelBuilder.Entity<Banco>()
            .Property(b => b.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Banco>()
            .Property(b => b.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 18. INVENTARIO - EXTENSIONES FASE 2
        // =============================================

        // Inventario - Navigation to Bodega (optional)
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.Bodega)
            .WithMany()
            .HasForeignKey(i => i.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventario - Navigation to Lote (optional)
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.Lote)
            .WithMany(l => l.Inventarios)
            .HasForeignKey(i => i.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventario - Index for Bodega
        modelBuilder.Entity<Inventario>()
            .HasIndex(i => i.BodegaId)
            .HasDatabaseName("IX_Inventario_BodegaId");

        // Inventario - CostoPromedio precision
        modelBuilder.Entity<Inventario>()
            .Property(i => i.CostoPromedio)
            .HasPrecision(18, 6)
            .HasDefaultValue(0m);

        // =============================================
        // 19. LOTES
        // =============================================

        // Lote - Unique Index
        modelBuilder.Entity<Lote>()
            .HasIndex(l => new { l.EmpresaId, l.NumeroLote })
            .IsUnique()
            .HasDatabaseName("IX_Lote_Empresa_NumeroLote");

        // Lote - Soft Delete Query Filter
        modelBuilder.Entity<Lote>()
            .HasQueryFilter(l => !l.IsDeleted);

        // Lote - Relationships
        modelBuilder.Entity<Lote>()
            .HasOne(l => l.Empresa)
            .WithMany()
            .HasForeignKey(l => l.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lote>()
            .HasOne(l => l.Producto)
            .WithMany()
            .HasForeignKey(l => l.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lote>()
            .HasOne(l => l.Proveedor)
            .WithMany()
            .HasForeignKey(l => l.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lote>()
            .HasOne(l => l.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(l => l.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lote>()
            .HasOne(l => l.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(l => l.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lote>()
            .HasOne(l => l.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(l => l.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lote - Precision
        modelBuilder.Entity<Lote>()
            .Property(l => l.CantidadInicial)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Lote>()
            .Property(l => l.CantidadActual)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Lote>()
            .Property(l => l.CostoUnitario)
            .HasPrecision(18, 6);

        // Lote - Default Values
        modelBuilder.Entity<Lote>()
            .Property(l => l.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Lote>()
            .Property(l => l.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Lote>()
            .Property(l => l.EnCuarentena)
            .HasDefaultValue(false);

        modelBuilder.Entity<Lote>()
            .Property(l => l.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 20. TRASLADOS DE INVENTARIO
        // =============================================

        // TrasladoInventario - Unique Index
        modelBuilder.Entity<TrasladoInventario>()
            .HasIndex(t => new { t.EmpresaId, t.Numero })
            .IsUnique()
            .HasDatabaseName("IX_TrasladoInventario_Empresa_Numero");

        // TrasladoInventario - Soft Delete Query Filter
        modelBuilder.Entity<TrasladoInventario>()
            .HasQueryFilter(t => !t.IsDeleted);

        // TrasladoInventario - Relationships
        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.Empresa)
            .WithMany()
            .HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.BodegaOrigen)
            .WithMany()
            .HasForeignKey(t => t.BodegaOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.BodegaDestino)
            .WithMany()
            .HasForeignKey(t => t.BodegaDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.EnviadoPor)
            .WithMany()
            .HasForeignKey(t => t.EnviadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.RecibidoPor)
            .WithMany()
            .HasForeignKey(t => t.RecibidoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventario>()
            .HasOne(t => t.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // TrasladoInventario - Default Values
        modelBuilder.Entity<TrasladoInventario>()
            .Property(t => t.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<TrasladoInventario>()
            .Property(t => t.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // TrasladoInventarioDetalle - Relationships
        modelBuilder.Entity<TrasladoInventarioDetalle>()
            .HasOne(d => d.TrasladoInventario)
            .WithMany(t => t.Detalles)
            .HasForeignKey(d => d.TrasladoInventarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrasladoInventarioDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoInventarioDetalle>()
            .HasOne(d => d.Lote)
            .WithMany()
            .HasForeignKey(d => d.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // TrasladoInventarioDetalle - Precision
        modelBuilder.Entity<TrasladoInventarioDetalle>()
            .Property(d => d.CantidadSolicitada)
            .HasPrecision(18, 4);

        modelBuilder.Entity<TrasladoInventarioDetalle>()
            .Property(d => d.CantidadEnviada)
            .HasPrecision(18, 4);

        modelBuilder.Entity<TrasladoInventarioDetalle>()
            .Property(d => d.CantidadRecibida)
            .HasPrecision(18, 4);

        modelBuilder.Entity<TrasladoInventarioDetalle>()
            .Property(d => d.CostoUnitario)
            .HasPrecision(18, 6);

        // =============================================
        // 21. AJUSTES DE INVENTARIO
        // =============================================

        // AjusteInventario - Unique Index
        modelBuilder.Entity<AjusteInventario>()
            .HasIndex(a => new { a.EmpresaId, a.Numero })
            .IsUnique()
            .HasDatabaseName("IX_AjusteInventario_Empresa_Numero");

        // AjusteInventario - Soft Delete Query Filter
        modelBuilder.Entity<AjusteInventario>()
            .HasQueryFilter(a => !a.IsDeleted);

        // AjusteInventario - Relationships
        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.Bodega)
            .WithMany()
            .HasForeignKey(a => a.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.AprobadoPor)
            .WithMany()
            .HasForeignKey(a => a.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventario>()
            .HasOne(a => a.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // AjusteInventario - Default Values
        modelBuilder.Entity<AjusteInventario>()
            .Property(a => a.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<AjusteInventario>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // AjusteInventarioDetalle - Relationships
        modelBuilder.Entity<AjusteInventarioDetalle>()
            .HasOne(d => d.AjusteInventario)
            .WithMany(a => a.Detalles)
            .HasForeignKey(d => d.AjusteInventarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AjusteInventarioDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AjusteInventarioDetalle>()
            .HasOne(d => d.Lote)
            .WithMany()
            .HasForeignKey(d => d.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // AjusteInventarioDetalle - Precision
        modelBuilder.Entity<AjusteInventarioDetalle>()
            .Property(d => d.CantidadSistema)
            .HasPrecision(18, 4);

        modelBuilder.Entity<AjusteInventarioDetalle>()
            .Property(d => d.CantidadFisica)
            .HasPrecision(18, 4);

        modelBuilder.Entity<AjusteInventarioDetalle>()
            .Property(d => d.Diferencia)
            .HasPrecision(18, 4);

        modelBuilder.Entity<AjusteInventarioDetalle>()
            .Property(d => d.CostoUnitario)
            .HasPrecision(18, 6);

        // =============================================
        // CRM - FASE 3 ERP INTEGRATION
        // =============================================

        // -----------------------------------------
        // EtapaPipeline
        // -----------------------------------------

        // EtapaPipeline - Unique Index
        modelBuilder.Entity<EtapaPipeline>()
            .HasIndex(e => new { e.EmpresaId, e.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_EtapaPipeline_Empresa_Codigo");

        // EtapaPipeline - Soft Delete Query Filter
        modelBuilder.Entity<EtapaPipeline>()
            .HasQueryFilter(e => !e.IsDeleted);

        // EtapaPipeline - Relationships
        modelBuilder.Entity<EtapaPipeline>()
            .HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EtapaPipeline>()
            .HasOne(e => e.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EtapaPipeline>()
            .HasOne(e => e.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EtapaPipeline>()
            .HasOne(e => e.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // EtapaPipeline - Default Values
        modelBuilder.Entity<EtapaPipeline>()
            .Property(e => e.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<EtapaPipeline>()
            .Property(e => e.ProbabilidadSugerida)
            .HasDefaultValue(50);

        modelBuilder.Entity<EtapaPipeline>()
            .Property(e => e.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // Competidor
        // -----------------------------------------

        // Competidor - Unique Index
        modelBuilder.Entity<Competidor>()
            .HasIndex(c => new { c.EmpresaId, c.Nombre })
            .IsUnique()
            .HasDatabaseName("IX_Competidor_Empresa_Nombre");

        // Competidor - Soft Delete Query Filter
        modelBuilder.Entity<Competidor>()
            .HasQueryFilter(c => !c.IsDeleted);

        // Competidor - Relationships
        modelBuilder.Entity<Competidor>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Competidor>()
            .HasOne(c => c.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Competidor>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Competidor>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Competidor - Default Values
        modelBuilder.Entity<Competidor>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Competidor>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // Oportunidad
        // -----------------------------------------

        // Oportunidad - Unique Index
        modelBuilder.Entity<Oportunidad>()
            .HasIndex(o => new { o.EmpresaId, o.Numero })
            .IsUnique()
            .HasDatabaseName("IX_Oportunidad_Empresa_Numero");

        // Oportunidad - Additional Indexes
        modelBuilder.Entity<Oportunidad>()
            .HasIndex(o => o.ClienteId)
            .HasDatabaseName("IX_Oportunidad_ClienteId");

        modelBuilder.Entity<Oportunidad>()
            .HasIndex(o => o.CodigoEtapa)
            .HasDatabaseName("IX_Oportunidad_CodigoEtapa");

        modelBuilder.Entity<Oportunidad>()
            .HasIndex(o => o.VendedorId)
            .HasDatabaseName("IX_Oportunidad_VendedorId");

        // Oportunidad - Soft Delete Query Filter
        modelBuilder.Entity<Oportunidad>()
            .HasQueryFilter(o => !o.IsDeleted);

        // Oportunidad - Relationships
        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.Empresa)
            .WithMany()
            .HasForeignKey(o => o.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.Cliente)
            .WithMany()
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.EtapaPipeline)
            .WithMany(e => e.Oportunidades)
            .HasForeignKey(o => o.EtapaPipelineId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.Vendedor)
            .WithMany()
            .HasForeignKey(o => o.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.Competidor)
            .WithMany(c => c.Oportunidades)
            .HasForeignKey(o => o.CompetidorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(o => o.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(o => o.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Oportunidad>()
            .HasOne(o => o.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(o => o.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Oportunidad - Default Values
        modelBuilder.Entity<Oportunidad>()
            .Property(o => o.CodigoEtapa)
            .HasDefaultValue("PRO");

        modelBuilder.Entity<Oportunidad>()
            .Property(o => o.ProbabilidadCierre)
            .HasDefaultValue(50m);

        modelBuilder.Entity<Oportunidad>()
            .Property(o => o.Prioridad)
            .HasDefaultValue("MED");

        modelBuilder.Entity<Oportunidad>()
            .Property(o => o.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Oportunidad - Precision
        modelBuilder.Entity<Oportunidad>()
            .Property(o => o.MontoEstimado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Oportunidad>()
            .Property(o => o.ProbabilidadCierre)
            .HasPrecision(5, 2);

        // -----------------------------------------
        // ActividadCRM
        // -----------------------------------------

        // ActividadCRM - Indexes
        modelBuilder.Entity<ActividadCRM>()
            .HasIndex(a => new { a.EmpresaId, a.FechaProgramada })
            .HasDatabaseName("IX_ActividadCRM_Empresa_FechaProgramada");

        modelBuilder.Entity<ActividadCRM>()
            .HasIndex(a => a.OportunidadId)
            .HasDatabaseName("IX_ActividadCRM_OportunidadId");

        modelBuilder.Entity<ActividadCRM>()
            .HasIndex(a => a.ClienteId)
            .HasDatabaseName("IX_ActividadCRM_ClienteId");

        modelBuilder.Entity<ActividadCRM>()
            .HasIndex(a => a.AsignadoAId)
            .HasDatabaseName("IX_ActividadCRM_AsignadoAId");

        modelBuilder.Entity<ActividadCRM>()
            .HasIndex(a => a.Estado)
            .HasDatabaseName("IX_ActividadCRM_Estado");

        // ActividadCRM - Soft Delete Query Filter
        modelBuilder.Entity<ActividadCRM>()
            .HasQueryFilter(a => !a.IsDeleted);

        // ActividadCRM - Relationships
        modelBuilder.Entity<ActividadCRM>()
            .HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadCRM>()
            .HasOne(a => a.Oportunidad)
            .WithMany(o => o.Actividades)
            .HasForeignKey(a => a.OportunidadId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadCRM>()
            .HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadCRM>()
            .HasOne(a => a.AsignadoA)
            .WithMany()
            .HasForeignKey(a => a.AsignadoAId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadCRM>()
            .HasOne(a => a.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadCRM>()
            .HasOne(a => a.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActividadCRM>()
            .HasOne(a => a.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // ActividadCRM - Default Values
        modelBuilder.Entity<ActividadCRM>()
            .Property(a => a.Estado)
            .HasDefaultValue("PEN");

        modelBuilder.Entity<ActividadCRM>()
            .Property(a => a.Prioridad)
            .HasDefaultValue("MED");

        modelBuilder.Entity<ActividadCRM>()
            .Property(a => a.DuracionMinutos)
            .HasDefaultValue(30);

        modelBuilder.Entity<ActividadCRM>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // NotaOportunidad
        // -----------------------------------------

        // NotaOportunidad - Index
        modelBuilder.Entity<NotaOportunidad>()
            .HasIndex(n => n.OportunidadId)
            .HasDatabaseName("IX_NotaOportunidad_OportunidadId");

        // NotaOportunidad - Relationships
        modelBuilder.Entity<NotaOportunidad>()
            .HasOne(n => n.Oportunidad)
            .WithMany(o => o.Notas)
            .HasForeignKey(n => n.OportunidadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NotaOportunidad>()
            .HasOne(n => n.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(n => n.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // NotaOportunidad - Default Values
        modelBuilder.Entity<NotaOportunidad>()
            .Property(n => n.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // HistorialEtapaOportunidad
        // -----------------------------------------

        // HistorialEtapaOportunidad - Index
        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .HasIndex(h => h.OportunidadId)
            .HasDatabaseName("IX_HistorialEtapaOportunidad_OportunidadId");

        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .HasIndex(h => h.FechaCambio)
            .HasDatabaseName("IX_HistorialEtapaOportunidad_FechaCambio");

        // HistorialEtapaOportunidad - Relationships
        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .HasOne(h => h.Oportunidad)
            .WithMany(o => o.HistorialEtapas)
            .HasForeignKey(h => h.OportunidadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .HasOne(h => h.EtapaAnteriorNav)
            .WithMany()
            .HasForeignKey(h => h.EtapaAnteriorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .HasOne(h => h.EtapaNuevaNav)
            .WithMany()
            .HasForeignKey(h => h.EtapaNuevaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .HasOne(h => h.CambiadoPor)
            .WithMany()
            .HasForeignKey(h => h.CambiadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        // HistorialEtapaOportunidad - Default Values
        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .Property(h => h.FechaCambio)
            .HasDefaultValueSql("GETDATE()");

        // HistorialEtapaOportunidad - Precision
        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .Property(h => h.MontoAlCambio)
            .HasPrecision(18, 2);

        modelBuilder.Entity<HistorialEtapaOportunidad>()
            .Property(h => h.ProbabilidadAlCambio)
            .HasPrecision(5, 2);

        // =============================================
        // RRHH Y NÓMINA - FASE 4 ERP INTEGRATION
        // =============================================

        // -----------------------------------------
        // Departamento
        // -----------------------------------------

        // Departamento - Unique Index
        modelBuilder.Entity<Departamento>()
            .HasIndex(d => new { d.EmpresaId, d.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Departamento_Empresa_Codigo");

        // Departamento - Soft Delete Query Filter
        modelBuilder.Entity<Departamento>()
            .HasQueryFilter(d => !d.IsDeleted);

        // Departamento - Relationships
        modelBuilder.Entity<Departamento>()
            .HasOne(d => d.Empresa)
            .WithMany()
            .HasForeignKey(d => d.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Departamento>()
            .HasOne(d => d.Jefe)
            .WithMany()
            .HasForeignKey(d => d.JefeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Departamento>()
            .HasOne(d => d.DepartamentoPadre)
            .WithMany(d => d.SubDepartamentos)
            .HasForeignKey(d => d.DepartamentoPadreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Departamento>()
            .HasOne(d => d.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Departamento>()
            .HasOne(d => d.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Departamento>()
            .HasOne(d => d.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Departamento - Default Values
        modelBuilder.Entity<Departamento>()
            .Property(d => d.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Departamento>()
            .Property(d => d.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // Puesto
        // -----------------------------------------

        // Puesto - Unique Index
        modelBuilder.Entity<Puesto>()
            .HasIndex(p => new { p.EmpresaId, p.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Puesto_Empresa_Codigo");

        // Puesto - Soft Delete Query Filter
        modelBuilder.Entity<Puesto>()
            .HasQueryFilter(p => !p.IsDeleted);

        // Puesto - Relationships
        modelBuilder.Entity<Puesto>()
            .HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Puesto>()
            .HasOne(p => p.Departamento)
            .WithMany(d => d.Puestos)
            .HasForeignKey(p => p.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Puesto>()
            .HasOne(p => p.PuestoSuperior)
            .WithMany(p => p.PuestosSubordinados)
            .HasForeignKey(p => p.PuestoSuperiorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Puesto>()
            .HasOne(p => p.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Puesto>()
            .HasOne(p => p.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Puesto>()
            .HasOne(p => p.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Puesto - Default Values
        modelBuilder.Entity<Puesto>()
            .Property(p => p.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Puesto>()
            .Property(p => p.NivelJerarquico)
            .HasDefaultValue(4);

        modelBuilder.Entity<Puesto>()
            .Property(p => p.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Puesto - Precision
        modelBuilder.Entity<Puesto>()
            .Property(p => p.SalarioMinimo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Puesto>()
            .Property(p => p.SalarioMaximo)
            .HasPrecision(18, 2);

        // -----------------------------------------
        // Empleado
        // -----------------------------------------

        // Empleado - Unique Indexes
        modelBuilder.Entity<Empleado>()
            .HasIndex(e => new { e.EmpresaId, e.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Empleado_Empresa_Codigo");

        modelBuilder.Entity<Empleado>()
            .HasIndex(e => new { e.EmpresaId, e.Identificacion })
            .IsUnique()
            .HasDatabaseName("IX_Empleado_Empresa_Identificacion");

        modelBuilder.Entity<Empleado>()
            .HasIndex(e => e.DepartamentoId)
            .HasDatabaseName("IX_Empleado_DepartamentoId");

        modelBuilder.Entity<Empleado>()
            .HasIndex(e => e.PuestoId)
            .HasDatabaseName("IX_Empleado_PuestoId");

        modelBuilder.Entity<Empleado>()
            .HasIndex(e => e.Estado)
            .HasDatabaseName("IX_Empleado_Estado");

        // Empleado - Soft Delete Query Filter
        modelBuilder.Entity<Empleado>()
            .HasQueryFilter(e => !e.IsDeleted);

        // Empleado - Relationships
        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Departamento)
            .WithMany(d => d.Empleados)
            .HasForeignKey(e => e.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Puesto)
            .WithMany(p => p.Empleados)
            .HasForeignKey(e => e.PuestoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Banco)
            .WithMany()
            .HasForeignKey(e => e.BancoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.JefeDirecto)
            .WithMany(e => e.Subordinados)
            .HasForeignKey(e => e.JefeDirectoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Empleado - Default Values
        modelBuilder.Entity<Empleado>()
            .Property(e => e.Estado)
            .HasDefaultValue("ACT");

        modelBuilder.Entity<Empleado>()
            .Property(e => e.TipoContrato)
            .HasDefaultValue("IND");

        modelBuilder.Entity<Empleado>()
            .Property(e => e.FormaPago)
            .HasDefaultValue("QUI");

        modelBuilder.Entity<Empleado>()
            .Property(e => e.AportaCCSS)
            .HasDefaultValue(true);

        modelBuilder.Entity<Empleado>()
            .Property(e => e.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Empleado - Precision
        modelBuilder.Entity<Empleado>()
            .Property(e => e.SalarioBase)
            .HasPrecision(18, 2);

        // -----------------------------------------
        // ContactoEmergencia
        // -----------------------------------------

        modelBuilder.Entity<ContactoEmergencia>()
            .HasIndex(c => c.EmpleadoId)
            .HasDatabaseName("IX_ContactoEmergencia_EmpleadoId");

        modelBuilder.Entity<ContactoEmergencia>()
            .HasOne(c => c.Empleado)
            .WithMany(e => e.ContactosEmergencia)
            .HasForeignKey(c => c.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ContactoEmergencia>()
            .HasOne(c => c.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ContactoEmergencia>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<ContactoEmergencia>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // ExpedienteDigital
        // -----------------------------------------

        modelBuilder.Entity<ExpedienteDigital>()
            .HasIndex(e => e.EmpleadoId)
            .HasDatabaseName("IX_ExpedienteDigital_EmpleadoId");

        modelBuilder.Entity<ExpedienteDigital>()
            .HasIndex(e => e.TipoDocumento)
            .HasDatabaseName("IX_ExpedienteDigital_TipoDocumento");

        modelBuilder.Entity<ExpedienteDigital>()
            .HasOne(e => e.Empleado)
            .WithMany(emp => emp.ExpedientesDigitales)
            .HasForeignKey(e => e.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExpedienteDigital>()
            .HasOne(e => e.VerificadoPor)
            .WithMany()
            .HasForeignKey(e => e.VerificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExpedienteDigital>()
            .HasOne(e => e.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExpedienteDigital>()
            .HasOne(e => e.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(e => e.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExpedienteDigital>()
            .Property(e => e.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<ExpedienteDigital>()
            .Property(e => e.TipoDocumento)
            .HasDefaultValue("OTR");

        modelBuilder.Entity<ExpedienteDigital>()
            .Property(e => e.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // Vacacion
        // -----------------------------------------

        modelBuilder.Entity<Vacacion>()
            .HasIndex(v => v.EmpleadoId)
            .HasDatabaseName("IX_Vacacion_EmpleadoId");

        modelBuilder.Entity<Vacacion>()
            .HasIndex(v => v.Estado)
            .HasDatabaseName("IX_Vacacion_Estado");

        modelBuilder.Entity<Vacacion>()
            .HasIndex(v => new { v.FechaInicio, v.FechaFin })
            .HasDatabaseName("IX_Vacacion_Fechas");

        modelBuilder.Entity<Vacacion>()
            .HasOne(v => v.Empleado)
            .WithMany(e => e.Vacaciones)
            .HasForeignKey(v => v.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vacacion>()
            .HasOne(v => v.AprobadoPor)
            .WithMany()
            .HasForeignKey(v => v.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vacacion>()
            .HasOne(v => v.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(v => v.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vacacion>()
            .HasOne(v => v.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(v => v.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vacacion>()
            .Property(v => v.Estado)
            .HasDefaultValue("SOL");

        modelBuilder.Entity<Vacacion>()
            .Property(v => v.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // -----------------------------------------
        // Incapacidad
        // -----------------------------------------

        modelBuilder.Entity<Incapacidad>()
            .HasIndex(i => i.EmpleadoId)
            .HasDatabaseName("IX_Incapacidad_EmpleadoId");

        modelBuilder.Entity<Incapacidad>()
            .HasIndex(i => i.Tipo)
            .HasDatabaseName("IX_Incapacidad_Tipo");

        modelBuilder.Entity<Incapacidad>()
            .HasIndex(i => new { i.FechaInicio, i.FechaFin })
            .HasDatabaseName("IX_Incapacidad_Fechas");

        modelBuilder.Entity<Incapacidad>()
            .HasOne(i => i.Empleado)
            .WithMany(e => e.Incapacidades)
            .HasForeignKey(i => i.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Incapacidad>()
            .HasOne(i => i.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(i => i.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incapacidad>()
            .HasOne(i => i.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(i => i.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Incapacidad>()
            .Property(i => i.Tipo)
            .HasDefaultValue("ENF");

        modelBuilder.Entity<Incapacidad>()
            .Property(i => i.PorcentajePagoPatrono)
            .HasDefaultValue(100m);

        modelBuilder.Entity<Incapacidad>()
            .Property(i => i.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Incapacidad - Precision
        modelBuilder.Entity<Incapacidad>()
            .Property(i => i.PorcentajePagoCCSS)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Incapacidad>()
            .Property(i => i.PorcentajePagoPatrono)
            .HasPrecision(5, 2);

        // -----------------------------------------
        // AccionPersonal
        // -----------------------------------------

        modelBuilder.Entity<AccionPersonal>()
            .HasIndex(a => a.EmpleadoId)
            .HasDatabaseName("IX_AccionPersonal_EmpleadoId");

        modelBuilder.Entity<AccionPersonal>()
            .HasIndex(a => a.TipoAccion)
            .HasDatabaseName("IX_AccionPersonal_TipoAccion");

        modelBuilder.Entity<AccionPersonal>()
            .HasIndex(a => a.FechaAccion)
            .HasDatabaseName("IX_AccionPersonal_FechaAccion");

        modelBuilder.Entity<AccionPersonal>()
            .HasOne(a => a.Empleado)
            .WithMany(e => e.AccionesPersonal)
            .HasForeignKey(a => a.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AccionPersonal>()
            .HasOne(a => a.AprobadoPor)
            .WithMany()
            .HasForeignKey(a => a.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionPersonal>()
            .HasOne(a => a.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionPersonal>()
            .HasOne(a => a.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionPersonal>()
            .Property(a => a.Estado)
            .HasDefaultValue("PEN");

        modelBuilder.Entity<AccionPersonal>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // AccionPersonal - Precision
        modelBuilder.Entity<AccionPersonal>()
            .Property(a => a.SalarioAnterior)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AccionPersonal>()
            .Property(a => a.SalarioNuevo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AccionPersonal>()
            .Property(a => a.MontoAfectado)
            .HasPrecision(18, 2);

        // -----------------------------------------
        // Planilla
        // -----------------------------------------

        // Planilla - Unique Index
        modelBuilder.Entity<Planilla>()
            .HasIndex(p => new { p.EmpresaId, p.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Planilla_Empresa_Codigo");

        modelBuilder.Entity<Planilla>()
            .HasIndex(p => new { p.EmpresaId, p.Anio, p.Mes, p.Periodo, p.TipoPlanilla })
            .HasDatabaseName("IX_Planilla_Empresa_Periodo");

        // Planilla - Soft Delete Query Filter
        modelBuilder.Entity<Planilla>()
            .HasQueryFilter(p => !p.IsDeleted);

        // Planilla - Relationships
        modelBuilder.Entity<Planilla>()
            .HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Planilla>()
            .HasOne(p => p.AprobadoPor)
            .WithMany()
            .HasForeignKey(p => p.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Planilla>()
            .HasOne(p => p.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Planilla>()
            .HasOne(p => p.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Planilla>()
            .HasOne(p => p.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Planilla - Default Values
        modelBuilder.Entity<Planilla>()
            .Property(p => p.Estado)
            .HasDefaultValue("BOR");

        modelBuilder.Entity<Planilla>()
            .Property(p => p.TipoPlanilla)
            .HasDefaultValue("ORD");

        modelBuilder.Entity<Planilla>()
            .Property(p => p.Periodo)
            .HasDefaultValue(1);

        modelBuilder.Entity<Planilla>()
            .Property(p => p.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Planilla - Precision
        modelBuilder.Entity<Planilla>()
            .Property(p => p.TotalSalarioBruto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Planilla>()
            .Property(p => p.TotalDeducciones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Planilla>()
            .Property(p => p.TotalSalarioNeto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Planilla>()
            .Property(p => p.TotalCargasSociales)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Planilla>()
            .Property(p => p.TotalCCSSPatronal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Planilla>()
            .Property(p => p.TotalINS)
            .HasPrecision(18, 2);

        // -----------------------------------------
        // DetallePlanilla
        // -----------------------------------------

        modelBuilder.Entity<DetallePlanilla>()
            .HasIndex(d => d.PlanillaId)
            .HasDatabaseName("IX_DetallePlanilla_PlanillaId");

        modelBuilder.Entity<DetallePlanilla>()
            .HasIndex(d => d.EmpleadoId)
            .HasDatabaseName("IX_DetallePlanilla_EmpleadoId");

        modelBuilder.Entity<DetallePlanilla>()
            .HasIndex(d => new { d.PlanillaId, d.EmpleadoId })
            .IsUnique()
            .HasDatabaseName("IX_DetallePlanilla_Planilla_Empleado");

        modelBuilder.Entity<DetallePlanilla>()
            .HasOne(d => d.Planilla)
            .WithMany(p => p.Detalles)
            .HasForeignKey(d => d.PlanillaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetallePlanilla>()
            .HasOne(d => d.Empleado)
            .WithMany(e => e.DetallesPlanilla)
            .HasForeignKey(d => d.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        // DetallePlanilla - Default Values
        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DiasLaborados)
            .HasDefaultValue(15m);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.EstadoPago)
            .HasDefaultValue("PEN");

        // DetallePlanilla - Precision (all decimal fields)
        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.SalarioBase)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DiasLaborados)
            .HasPrecision(5, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.SalarioBruto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.HorasExtra)
            .HasPrecision(5, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.MontoHorasExtra)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.Comisiones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.Bonificaciones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.OtrosIngresos)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.TotalIngresos)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DeduccionCCSS)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DeduccionBancoPopular)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DeduccionRenta)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DeduccionPensionAlimenticia)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DeduccionEmbargo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.DeduccionPrestamo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.OtrasDeducciones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.TotalDeducciones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.SalarioNeto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.CargaCCSSPatronal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.CargaOtrasInstituciones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.CargaAguinaldo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.CargaVacaciones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.CargaCesantia)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.CargaINS)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetallePlanilla>()
            .Property(d => d.TotalCargasSociales)
            .HasPrecision(18, 2);

        // =============================================
        // 17. ACTIVOS FIJOS MODULE (Fase 5)
        // =============================================

        // ----- CategoriaActivo -----
        modelBuilder.Entity<CategoriaActivo>()
            .HasIndex(c => new { c.EmpresaId, c.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_CategoriaActivo_Empresa_Codigo");

        modelBuilder.Entity<CategoriaActivo>()
            .HasQueryFilter(c => !c.IsDeleted);

        // CategoriaActivo - Relationships
        modelBuilder.Entity<CategoriaActivo>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CategoriaActivo>()
            .HasOne(c => c.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CategoriaActivo>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CategoriaActivo>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // CategoriaActivo - Precision
        modelBuilder.Entity<CategoriaActivo>()
            .Property(c => c.PorcentajeDepreciacionAnual)
            .HasPrecision(5, 2);

        // CategoriaActivo - Defaults
        modelBuilder.Entity<CategoriaActivo>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CategoriaActivo>()
            .Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<CategoriaActivo>()
            .Property(c => c.VidaUtilAnios)
            .HasDefaultValue(5);

        modelBuilder.Entity<CategoriaActivo>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- ActivoFijo -----
        modelBuilder.Entity<ActivoFijo>()
            .HasIndex(a => new { a.EmpresaId, a.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_ActivoFijo_Empresa_Codigo");

        modelBuilder.Entity<ActivoFijo>()
            .HasIndex(a => a.CategoriaActivoId)
            .HasDatabaseName("IX_ActivoFijo_CategoriaId");

        modelBuilder.Entity<ActivoFijo>()
            .HasIndex(a => a.SucursalId)
            .HasDatabaseName("IX_ActivoFijo_SucursalId");

        modelBuilder.Entity<ActivoFijo>()
            .HasIndex(a => a.ResponsableId)
            .HasDatabaseName("IX_ActivoFijo_ResponsableId");

        modelBuilder.Entity<ActivoFijo>()
            .HasIndex(a => a.Estado)
            .HasDatabaseName("IX_ActivoFijo_Estado");

        modelBuilder.Entity<ActivoFijo>()
            .HasIndex(a => a.NumeroSerie)
            .HasDatabaseName("IX_ActivoFijo_NumeroSerie");

        modelBuilder.Entity<ActivoFijo>()
            .HasQueryFilter(a => !a.IsDeleted);

        // ActivoFijo - Relationships
        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.CategoriaActivo)
            .WithMany(c => c.Activos)
            .HasForeignKey(a => a.CategoriaActivoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.Sucursal)
            .WithMany()
            .HasForeignKey(a => a.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.Responsable)
            .WithMany()
            .HasForeignKey(a => a.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.ProveedorEntity)
            .WithMany()
            .HasForeignKey(a => a.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivoFijo>()
            .HasOne(a => a.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // ActivoFijo - Precision
        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.ValorOriginal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.ValorResidual)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.DepreciacionAcumulada)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.PorcentajeDepreciacionAnual)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.ValorVenta)
            .HasPrecision(18, 2);

        // ActivoFijo - Defaults
        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.Estado)
            .HasDefaultValue("ACT");

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.MetodoDepreciacion)
            .HasDefaultValue("LR");

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.FrecuenciaDepreciacion)
            .HasDefaultValue("MEN");

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.VidaUtilAnios)
            .HasDefaultValue(5);

        modelBuilder.Entity<ActivoFijo>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- DepreciacionActivo -----
        modelBuilder.Entity<DepreciacionActivo>()
            .HasIndex(d => d.ActivoFijoId)
            .HasDatabaseName("IX_DepreciacionActivo_ActivoFijoId");

        modelBuilder.Entity<DepreciacionActivo>()
            .HasIndex(d => new { d.ActivoFijoId, d.Anio, d.Mes })
            .IsUnique()
            .HasDatabaseName("IX_DepreciacionActivo_Activo_Periodo");

        modelBuilder.Entity<DepreciacionActivo>()
            .HasIndex(d => d.Fecha)
            .HasDatabaseName("IX_DepreciacionActivo_Fecha");

        // DepreciacionActivo - Relationships
        modelBuilder.Entity<DepreciacionActivo>()
            .HasOne(d => d.ActivoFijo)
            .WithMany(a => a.Depreciaciones)
            .HasForeignKey(d => d.ActivoFijoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DepreciacionActivo>()
            .HasOne(d => d.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(d => d.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // DepreciacionActivo - Precision
        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.MontoDepreciacion)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.DepreciacionAcumuladaAnterior)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.DepreciacionAcumuladaNueva)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.ValorLibrosAnterior)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.ValorLibrosNuevo)
            .HasPrecision(18, 2);

        // DepreciacionActivo - Defaults
        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.Estado)
            .HasDefaultValue("CAL");

        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.Contabilizado)
            .HasDefaultValue(false);

        modelBuilder.Entity<DepreciacionActivo>()
            .Property(d => d.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- TrasladoActivo -----
        modelBuilder.Entity<TrasladoActivo>()
            .HasIndex(t => new { t.EmpresaId, t.Numero })
            .IsUnique()
            .HasDatabaseName("IX_TrasladoActivo_Empresa_Numero");

        modelBuilder.Entity<TrasladoActivo>()
            .HasIndex(t => t.ActivoFijoId)
            .HasDatabaseName("IX_TrasladoActivo_ActivoFijoId");

        modelBuilder.Entity<TrasladoActivo>()
            .HasIndex(t => t.Fecha)
            .HasDatabaseName("IX_TrasladoActivo_Fecha");

        modelBuilder.Entity<TrasladoActivo>()
            .HasIndex(t => t.Estado)
            .HasDatabaseName("IX_TrasladoActivo_Estado");

        // TrasladoActivo - Relationships
        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.Empresa)
            .WithMany()
            .HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.ActivoFijo)
            .WithMany(a => a.Traslados)
            .HasForeignKey(t => t.ActivoFijoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.SucursalOrigen)
            .WithMany()
            .HasForeignKey(t => t.SucursalOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.SucursalDestino)
            .WithMany()
            .HasForeignKey(t => t.SucursalDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.ResponsableOrigen)
            .WithMany()
            .HasForeignKey(t => t.ResponsableOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.ResponsableDestino)
            .WithMany()
            .HasForeignKey(t => t.ResponsableDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.AprobadoPor)
            .WithMany()
            .HasForeignKey(t => t.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.RecibidoPor)
            .WithMany()
            .HasForeignKey(t => t.RecibidoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrasladoActivo>()
            .HasOne(t => t.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // TrasladoActivo - Defaults
        modelBuilder.Entity<TrasladoActivo>()
            .Property(t => t.Estado)
            .HasDefaultValue("PEN");

        modelBuilder.Entity<TrasladoActivo>()
            .Property(t => t.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // 18. COMPRAS AVANZADAS MODULE (Fase 6)
        // =============================================

        // ----- Requisicion -----
        modelBuilder.Entity<Requisicion>()
            .HasIndex(r => new { r.EmpresaId, r.Numero })
            .IsUnique()
            .HasDatabaseName("IX_Requisicion_Empresa_Numero");

        modelBuilder.Entity<Requisicion>()
            .HasIndex(r => r.SolicitanteId)
            .HasDatabaseName("IX_Requisicion_SolicitanteId");

        modelBuilder.Entity<Requisicion>()
            .HasIndex(r => r.Estado)
            .HasDatabaseName("IX_Requisicion_Estado");

        modelBuilder.Entity<Requisicion>()
            .HasIndex(r => r.Fecha)
            .HasDatabaseName("IX_Requisicion_Fecha");

        modelBuilder.Entity<Requisicion>()
            .HasQueryFilter(r => !r.IsDeleted);

        // Requisicion - Relationships
        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.Empresa)
            .WithMany()
            .HasForeignKey(r => r.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.Solicitante)
            .WithMany()
            .HasForeignKey(r => r.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.Departamento)
            .WithMany()
            .HasForeignKey(r => r.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.Sucursal)
            .WithMany()
            .HasForeignKey(r => r.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.AprobadoPor)
            .WithMany()
            .HasForeignKey(r => r.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Requisicion>()
            .HasOne(r => r.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Requisicion - Precision & Defaults
        modelBuilder.Entity<Requisicion>()
            .Property(r => r.MontoEstimado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Requisicion>()
            .Property(r => r.Estado)
            .HasDefaultValue("BOR");

        modelBuilder.Entity<Requisicion>()
            .Property(r => r.Prioridad)
            .HasDefaultValue("MED");

        modelBuilder.Entity<Requisicion>()
            .Property(r => r.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Requisicion>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- RequisicionDetalle -----
        modelBuilder.Entity<RequisicionDetalle>()
            .HasIndex(d => d.RequisicionId)
            .HasDatabaseName("IX_RequisicionDetalle_RequisicionId");

        modelBuilder.Entity<RequisicionDetalle>()
            .HasOne(d => d.Requisicion)
            .WithMany(r => r.Detalles)
            .HasForeignKey(d => d.RequisicionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RequisicionDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequisicionDetalle>()
            .HasOne(d => d.ProveedorSugerido)
            .WithMany()
            .HasForeignKey(d => d.ProveedorSugeridoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequisicionDetalle>()
            .Property(d => d.CantidadSolicitada)
            .HasPrecision(18, 4);

        modelBuilder.Entity<RequisicionDetalle>()
            .Property(d => d.CantidadCotizada)
            .HasPrecision(18, 4);

        modelBuilder.Entity<RequisicionDetalle>()
            .Property(d => d.CantidadOrdenada)
            .HasPrecision(18, 4);

        modelBuilder.Entity<RequisicionDetalle>()
            .Property(d => d.PrecioEstimado)
            .HasPrecision(18, 4);

        modelBuilder.Entity<RequisicionDetalle>()
            .Property(d => d.SubtotalEstimado)
            .HasPrecision(18, 2);

        // ----- CotizacionProveedor -----
        modelBuilder.Entity<CotizacionProveedor>()
            .HasIndex(c => new { c.EmpresaId, c.Numero })
            .IsUnique()
            .HasDatabaseName("IX_CotizacionProveedor_Empresa_Numero");

        modelBuilder.Entity<CotizacionProveedor>()
            .HasIndex(c => c.RequisicionId)
            .HasDatabaseName("IX_CotizacionProveedor_RequisicionId");

        modelBuilder.Entity<CotizacionProveedor>()
            .HasIndex(c => c.ProveedorId)
            .HasDatabaseName("IX_CotizacionProveedor_ProveedorId");

        modelBuilder.Entity<CotizacionProveedor>()
            .HasIndex(c => c.Estado)
            .HasDatabaseName("IX_CotizacionProveedor_Estado");

        modelBuilder.Entity<CotizacionProveedor>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionProveedor>()
            .HasOne(c => c.Requisicion)
            .WithMany(r => r.Cotizaciones)
            .HasForeignKey(c => c.RequisicionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionProveedor>()
            .HasOne(c => c.Proveedor)
            .WithMany()
            .HasForeignKey(c => c.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionProveedor>()
            .HasOne(c => c.RegistradoPor)
            .WithMany()
            .HasForeignKey(c => c.RegistradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionProveedor>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.MontoSubtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.MontoImpuestos)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.MontoTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.TipoCambio)
            .HasPrecision(18, 4);

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.MontoFlete)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.PuntuacionTotal)
            .HasPrecision(5, 2);

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.Estado)
            .HasDefaultValue("ENV");

        modelBuilder.Entity<CotizacionProveedor>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- CotizacionProveedorDetalle -----
        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .HasIndex(d => d.CotizacionProveedorId)
            .HasDatabaseName("IX_CotizacionProveedorDetalle_CotizacionId");

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .HasOne(d => d.CotizacionProveedor)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CotizacionProveedorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .HasOne(d => d.RequisicionDetalle)
            .WithMany()
            .HasForeignKey(d => d.RequisicionDetalleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.Cantidad)
            .HasPrecision(18, 4);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.PrecioUnitario)
            .HasPrecision(18, 4);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.PorcentajeDescuento)
            .HasPrecision(5, 2);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.MontoDescuento)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.PorcentajeIVA)
            .HasPrecision(5, 2);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.MontoIVA)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CotizacionProveedorDetalle>()
            .Property(d => d.Total)
            .HasPrecision(18, 2);

        // ----- ComparativoCotizacion -----
        modelBuilder.Entity<ComparativoCotizacion>()
            .HasIndex(c => new { c.EmpresaId, c.Numero })
            .IsUnique()
            .HasDatabaseName("IX_ComparativoCotizacion_Empresa_Numero");

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasIndex(c => c.RequisicionId)
            .HasDatabaseName("IX_ComparativoCotizacion_RequisicionId");

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasOne(c => c.Requisicion)
            .WithMany()
            .HasForeignKey(c => c.RequisicionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasOne(c => c.CotizacionSeleccionada)
            .WithMany()
            .HasForeignKey(c => c.CotizacionSeleccionadaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasOne(c => c.OrdenCompra)
            .WithMany()
            .HasForeignKey(c => c.OrdenCompraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasOne(c => c.RealizadoPor)
            .WithMany()
            .HasForeignKey(c => c.RealizadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasOne(c => c.AprobadoPor)
            .WithMany()
            .HasForeignKey(c => c.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacion>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacion>()
            .Property(c => c.Estado)
            .HasDefaultValue("BOR");

        modelBuilder.Entity<ComparativoCotizacion>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- ComparativoCotizacionDetalle -----
        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .HasIndex(d => d.ComparativoCotizacionId)
            .HasDatabaseName("IX_ComparativoCotizacionDetalle_ComparativoId");

        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .HasOne(d => d.ComparativoCotizacion)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.ComparativoCotizacionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .HasOne(d => d.CotizacionProveedor)
            .WithMany()
            .HasForeignKey(d => d.CotizacionProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .Property(d => d.PuntuacionPrecio)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .Property(d => d.PuntuacionTiempoEntrega)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .Property(d => d.PuntuacionCalidad)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .Property(d => d.PuntuacionCondicionesPago)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ComparativoCotizacionDetalle>()
            .Property(d => d.PuntuacionTotal)
            .HasPrecision(5, 2);

        // ----- EvaluacionProveedor -----
        modelBuilder.Entity<EvaluacionProveedor>()
            .HasIndex(e => new { e.EmpresaId, e.ProveedorId, e.Anio, e.Trimestre, e.Mes })
            .IsUnique()
            .HasDatabaseName("IX_EvaluacionProveedor_Empresa_Proveedor_Periodo");

        modelBuilder.Entity<EvaluacionProveedor>()
            .HasIndex(e => e.ProveedorId)
            .HasDatabaseName("IX_EvaluacionProveedor_ProveedorId");

        modelBuilder.Entity<EvaluacionProveedor>()
            .HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EvaluacionProveedor>()
            .HasOne(e => e.Proveedor)
            .WithMany()
            .HasForeignKey(e => e.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EvaluacionProveedor>()
            .HasOne(e => e.EvaluadoPor)
            .WithMany()
            .HasForeignKey(e => e.EvaluadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EvaluacionProveedor>()
            .Property(e => e.PromedioGeneral)
            .HasPrecision(3, 2);

        modelBuilder.Entity<EvaluacionProveedor>()
            .Property(e => e.MontoTotalCompras)
            .HasPrecision(18, 2);

        modelBuilder.Entity<EvaluacionProveedor>()
            .Property(e => e.MontoDevoluciones)
            .HasPrecision(18, 2);

        modelBuilder.Entity<EvaluacionProveedor>()
            .Property(e => e.PorcentajeEntregasATiempo)
            .HasPrecision(5, 2);

        modelBuilder.Entity<EvaluacionProveedor>()
            .Property(e => e.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- OrdenCompra -----
        modelBuilder.Entity<OrdenCompra>()
            .HasIndex(o => new { o.EmpresaId, o.Numero })
            .IsUnique()
            .HasDatabaseName("IX_OrdenCompra_Empresa_Numero");

        modelBuilder.Entity<OrdenCompra>()
            .HasIndex(o => o.ProveedorId)
            .HasDatabaseName("IX_OrdenCompra_ProveedorId");

        modelBuilder.Entity<OrdenCompra>()
            .HasIndex(o => o.Estado)
            .HasDatabaseName("IX_OrdenCompra_Estado");

        modelBuilder.Entity<OrdenCompra>()
            .HasIndex(o => o.Fecha)
            .HasDatabaseName("IX_OrdenCompra_Fecha");

        modelBuilder.Entity<OrdenCompra>()
            .HasQueryFilter(o => !o.IsDeleted);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.Empresa)
            .WithMany()
            .HasForeignKey(o => o.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.Proveedor)
            .WithMany()
            .HasForeignKey(o => o.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.Sucursal)
            .WithMany()
            .HasForeignKey(o => o.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.BodegaDestino)
            .WithMany()
            .HasForeignKey(o => o.BodegaDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.CreadoPor)
            .WithMany()
            .HasForeignKey(o => o.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.ModificadoPor)
            .WithMany()
            .HasForeignKey(o => o.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.AprobadoPor)
            .WithMany()
            .HasForeignKey(o => o.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .HasOne(o => o.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(o => o.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.TipoCambio)
            .HasPrecision(18, 4);

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.Descuento)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.Impuesto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.Estado)
            .HasDefaultValue("BOR");

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<OrdenCompra>()
            .Property(o => o.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- OrdenCompraDetalle -----
        modelBuilder.Entity<OrdenCompraDetalle>()
            .HasIndex(d => d.OrdenCompraId)
            .HasDatabaseName("IX_OrdenCompraDetalle_OrdenCompraId");

        modelBuilder.Entity<OrdenCompraDetalle>()
            .HasOne(d => d.OrdenCompra)
            .WithMany(o => o.Detalles)
            .HasForeignKey(d => d.OrdenCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.Cantidad)
            .HasPrecision(18, 4);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.CantidadRecibida)
            .HasPrecision(18, 4);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.PrecioUnitario)
            .HasPrecision(18, 4);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.PorcentajeDescuento)
            .HasPrecision(5, 2);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.Descuento)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.PorcentajeIVA)
            .HasPrecision(5, 2);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.Impuesto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrdenCompraDetalle>()
            .Property(d => d.TotalLinea)
            .HasPrecision(18, 2);

        // ----- RecepcionCompra -----
        modelBuilder.Entity<RecepcionCompra>()
            .HasIndex(r => new { r.EmpresaId, r.Numero })
            .IsUnique()
            .HasDatabaseName("IX_RecepcionCompra_Empresa_Numero");

        modelBuilder.Entity<RecepcionCompra>()
            .HasIndex(r => r.OrdenCompraId)
            .HasDatabaseName("IX_RecepcionCompra_OrdenCompraId");

        modelBuilder.Entity<RecepcionCompra>()
            .HasIndex(r => r.Fecha)
            .HasDatabaseName("IX_RecepcionCompra_Fecha");

        modelBuilder.Entity<RecepcionCompra>()
            .HasQueryFilter(r => !r.IsDeleted);

        modelBuilder.Entity<RecepcionCompra>()
            .HasOne(r => r.Empresa)
            .WithMany()
            .HasForeignKey(r => r.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompra>()
            .HasOne(r => r.OrdenCompra)
            .WithMany(o => o.Recepciones)
            .HasForeignKey(r => r.OrdenCompraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompra>()
            .HasOne(r => r.Bodega)
            .WithMany()
            .HasForeignKey(r => r.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompra>()
            .HasOne(r => r.CreadoPor)
            .WithMany()
            .HasForeignKey(r => r.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompra>()
            .HasOne(r => r.ModificadoPor)
            .WithMany()
            .HasForeignKey(r => r.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompra>()
            .HasOne(r => r.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(r => r.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompra>()
            .Property(r => r.Estado)
            .HasDefaultValue("APL");

        modelBuilder.Entity<RecepcionCompra>()
            .Property(r => r.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<RecepcionCompra>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- RecepcionCompraDetalle -----
        modelBuilder.Entity<RecepcionCompraDetalle>()
            .HasIndex(d => d.RecepcionCompraId)
            .HasDatabaseName("IX_RecepcionCompraDetalle_RecepcionId");

        modelBuilder.Entity<RecepcionCompraDetalle>()
            .HasOne(d => d.RecepcionCompra)
            .WithMany(r => r.Detalles)
            .HasForeignKey(d => d.RecepcionCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecepcionCompraDetalle>()
            .HasOne(d => d.OrdenCompraDetalle)
            .WithMany()
            .HasForeignKey(d => d.OrdenCompraDetalleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompraDetalle>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompraDetalle>()
            .HasOne(d => d.Lote)
            .WithMany()
            .HasForeignKey(d => d.LoteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecepcionCompraDetalle>()
            .Property(d => d.CantidadRecibida)
            .HasPrecision(18, 4);

        modelBuilder.Entity<RecepcionCompraDetalle>()
            .Property(d => d.CostoUnitario)
            .HasPrecision(18, 4);

        modelBuilder.Entity<RecepcionCompraDetalle>()
            .Property(d => d.CostoTotal)
            .HasPrecision(18, 2);

        // =============================================
        // FASE 7: CONTABILIDAD
        // =============================================

        // ----- CuentaContable -----
        modelBuilder.Entity<CuentaContable>()
            .HasIndex(c => new { c.EmpresaId, c.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_CuentaContable_Empresa_Codigo");

        modelBuilder.Entity<CuentaContable>()
            .HasIndex(c => c.TipoCuenta)
            .HasDatabaseName("IX_CuentaContable_TipoCuenta");

        modelBuilder.Entity<CuentaContable>()
            .HasIndex(c => c.Nivel)
            .HasDatabaseName("IX_CuentaContable_Nivel");

        modelBuilder.Entity<CuentaContable>()
            .HasIndex(c => c.CuentaPadreId)
            .HasDatabaseName("IX_CuentaContable_CuentaPadreId");

        modelBuilder.Entity<CuentaContable>()
            .HasQueryFilter(c => !c.IsDeleted);

        modelBuilder.Entity<CuentaContable>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaContable>()
            .HasOne(c => c.CuentaPadre)
            .WithMany(c => c.CuentasHijas)
            .HasForeignKey(c => c.CuentaPadreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaContable>()
            .HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaContable>()
            .HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaContable>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaContable>()
            .Property(c => c.SaldoInicial)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CuentaContable>()
            .Property(c => c.SaldoActual)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CuentaContable>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CuentaContable>()
            .Property(c => c.AceptaMovimientos)
            .HasDefaultValue(true);

        modelBuilder.Entity<CuentaContable>()
            .Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<CuentaContable>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- PeriodoFiscal -----
        modelBuilder.Entity<PeriodoFiscal>()
            .HasIndex(p => new { p.EmpresaId, p.AnioFiscal })
            .IsUnique()
            .HasDatabaseName("IX_PeriodoFiscal_Empresa_Anio");

        modelBuilder.Entity<PeriodoFiscal>()
            .HasIndex(p => p.Estado)
            .HasDatabaseName("IX_PeriodoFiscal_Estado");

        modelBuilder.Entity<PeriodoFiscal>()
            .HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<PeriodoFiscal>()
            .HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoFiscal>()
            .HasOne(p => p.AsientoCierre)
            .WithMany()
            .HasForeignKey(p => p.AsientoCierreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoFiscal>()
            .HasOne(p => p.AsientoApertura)
            .WithMany()
            .HasForeignKey(p => p.AsientoAperturaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoFiscal>()
            .HasOne(p => p.CreadoPor)
            .WithMany()
            .HasForeignKey(p => p.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoFiscal>()
            .HasOne(p => p.ModificadoPor)
            .WithMany()
            .HasForeignKey(p => p.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoFiscal>()
            .HasOne(p => p.CerradoPor)
            .WithMany()
            .HasForeignKey(p => p.CerradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoFiscal>()
            .HasOne(p => p.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoFiscal>()
            .Property(p => p.ResultadoEjercicio)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PeriodoFiscal>()
            .Property(p => p.Estado)
            .HasDefaultValue("ABT");

        modelBuilder.Entity<PeriodoFiscal>()
            .Property(p => p.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<PeriodoFiscal>()
            .Property(p => p.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- PeriodoContable -----
        modelBuilder.Entity<PeriodoContable>()
            .HasIndex(p => new { p.EmpresaId, p.Anio, p.Mes })
            .IsUnique()
            .HasDatabaseName("IX_PeriodoContable_Empresa_AnioMes");

        modelBuilder.Entity<PeriodoContable>()
            .HasIndex(p => p.PeriodoFiscalId)
            .HasDatabaseName("IX_PeriodoContable_PeriodoFiscalId");

        modelBuilder.Entity<PeriodoContable>()
            .HasIndex(p => p.Estado)
            .HasDatabaseName("IX_PeriodoContable_Estado");

        modelBuilder.Entity<PeriodoContable>()
            .HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<PeriodoContable>()
            .HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoContable>()
            .HasOne(p => p.PeriodoFiscal)
            .WithMany(pf => pf.PeriodosContables)
            .HasForeignKey(p => p.PeriodoFiscalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoContable>()
            .HasOne(p => p.CreadoPor)
            .WithMany()
            .HasForeignKey(p => p.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoContable>()
            .HasOne(p => p.ModificadoPor)
            .WithMany()
            .HasForeignKey(p => p.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoContable>()
            .HasOne(p => p.CerradoPor)
            .WithMany()
            .HasForeignKey(p => p.CerradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoContable>()
            .HasOne(p => p.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PeriodoContable>()
            .Property(p => p.TotalDebe)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PeriodoContable>()
            .Property(p => p.TotalHaber)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PeriodoContable>()
            .Property(p => p.Estado)
            .HasDefaultValue("ABT");

        modelBuilder.Entity<PeriodoContable>()
            .Property(p => p.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<PeriodoContable>()
            .Property(p => p.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- AsientoContable -----
        modelBuilder.Entity<AsientoContable>()
            .HasIndex(a => new { a.EmpresaId, a.PeriodoContableId, a.Numero })
            .IsUnique()
            .HasDatabaseName("IX_AsientoContable_Empresa_Periodo_Numero");

        modelBuilder.Entity<AsientoContable>()
            .HasIndex(a => a.Fecha)
            .HasDatabaseName("IX_AsientoContable_Fecha");

        modelBuilder.Entity<AsientoContable>()
            .HasIndex(a => a.Estado)
            .HasDatabaseName("IX_AsientoContable_Estado");

        modelBuilder.Entity<AsientoContable>()
            .HasIndex(a => a.TipoAsiento)
            .HasDatabaseName("IX_AsientoContable_TipoAsiento");

        modelBuilder.Entity<AsientoContable>()
            .HasIndex(a => a.ModuloOrigen)
            .HasDatabaseName("IX_AsientoContable_ModuloOrigen");

        modelBuilder.Entity<AsientoContable>()
            .HasIndex(a => a.DocumentoOrigenId)
            .HasDatabaseName("IX_AsientoContable_DocumentoOrigenId");

        modelBuilder.Entity<AsientoContable>()
            .HasQueryFilter(a => !a.IsDeleted);

        modelBuilder.Entity<AsientoContable>()
            .HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsientoContable>()
            .HasOne(a => a.PeriodoContable)
            .WithMany(p => p.Asientos)
            .HasForeignKey(a => a.PeriodoContableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsientoContable>()
            .HasOne(a => a.CreadoPor)
            .WithMany()
            .HasForeignKey(a => a.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsientoContable>()
            .HasOne(a => a.ModificadoPor)
            .WithMany()
            .HasForeignKey(a => a.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsientoContable>()
            .HasOne(a => a.AprobadoPor)
            .WithMany()
            .HasForeignKey(a => a.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsientoContable>()
            .HasOne(a => a.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AsientoContable>()
            .Property(a => a.TotalDebe)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AsientoContable>()
            .Property(a => a.TotalHaber)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AsientoContable>()
            .Property(a => a.TipoAsiento)
            .HasDefaultValue("DIA");

        modelBuilder.Entity<AsientoContable>()
            .Property(a => a.Estado)
            .HasDefaultValue("BOR");

        modelBuilder.Entity<AsientoContable>()
            .Property(a => a.IsDeleted)
            .HasDefaultValue(false);

        modelBuilder.Entity<AsientoContable>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ----- MovimientoContable -----
        modelBuilder.Entity<MovimientoContable>()
            .HasIndex(m => m.AsientoContableId)
            .HasDatabaseName("IX_MovimientoContable_AsientoContableId");

        modelBuilder.Entity<MovimientoContable>()
            .HasIndex(m => m.CuentaContableId)
            .HasDatabaseName("IX_MovimientoContable_CuentaContableId");

        modelBuilder.Entity<MovimientoContable>()
            .HasIndex(m => m.ClienteId)
            .HasDatabaseName("IX_MovimientoContable_ClienteId");

        modelBuilder.Entity<MovimientoContable>()
            .HasIndex(m => m.ProveedorId)
            .HasDatabaseName("IX_MovimientoContable_ProveedorId");

        modelBuilder.Entity<MovimientoContable>()
            .HasOne(m => m.AsientoContable)
            .WithMany(a => a.Movimientos)
            .HasForeignKey(m => m.AsientoContableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MovimientoContable>()
            .HasOne(m => m.CuentaContable)
            .WithMany(c => c.Movimientos)
            .HasForeignKey(m => m.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoContable>()
            .HasOne(m => m.Cliente)
            .WithMany()
            .HasForeignKey(m => m.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoContable>()
            .HasOne(m => m.Proveedor)
            .WithMany()
            .HasForeignKey(m => m.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoContable>()
            .Property(m => m.Debe)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoContable>()
            .Property(m => m.Haber)
            .HasPrecision(18, 2);

        // ----- ConfiguracionContable -----
        modelBuilder.Entity<ConfiguracionContable>()
            .HasIndex(c => c.EmpresaId)
            .IsUnique()
            .HasDatabaseName("IX_ConfiguracionContable_Empresa");

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaVentasGravadas)
            .WithMany()
            .HasForeignKey(c => c.CuentaVentasGravadasId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaVentasExentas)
            .WithMany()
            .HasForeignKey(c => c.CuentaVentasExentasId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaIvaDebito)
            .WithMany()
            .HasForeignKey(c => c.CuentaIvaDebitoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaIvaCredito)
            .WithMany()
            .HasForeignKey(c => c.CuentaIvaCreditoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaClientes)
            .WithMany()
            .HasForeignKey(c => c.CuentaClientesId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaProveedores)
            .WithMany()
            .HasForeignKey(c => c.CuentaProveedoresId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaInventario)
            .WithMany()
            .HasForeignKey(c => c.CuentaInventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaCostoVentas)
            .WithMany()
            .HasForeignKey(c => c.CuentaCostoVentasId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaCajaGeneral)
            .WithMany()
            .HasForeignKey(c => c.CuentaCajaGeneralId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaBancosColones)
            .WithMany()
            .HasForeignKey(c => c.CuentaBancosColonesId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaBancosDolares)
            .WithMany()
            .HasForeignKey(c => c.CuentaBancosDolaresId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaDifCambiariaGanancia)
            .WithMany()
            .HasForeignKey(c => c.CuentaDifCambiariaGananciaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaDifCambiariaPerdida)
            .WithMany()
            .HasForeignKey(c => c.CuentaDifCambiariaPerdidaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaUtilidadEjercicio)
            .WithMany()
            .HasForeignKey(c => c.CuentaUtilidadEjercicioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaPerdidaEjercicio)
            .WithMany()
            .HasForeignKey(c => c.CuentaPerdidaEjercicioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .Property(c => c.ToleranciaDiferencias)
            .HasPrecision(10, 4);

        modelBuilder.Entity<ConfiguracionContable>()
            .Property(c => c.MontoLimiteSinAprobacion)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConfiguracionContable>()
            .Property(c => c.MonedaBase)
            .HasDefaultValue("CRC");

        modelBuilder.Entity<ConfiguracionContable>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // Cuentas de Planilla (Costa Rica) - GAP-004
        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaSalariosPorPagar)
            .WithMany()
            .HasForeignKey(c => c.CuentaSalariosPorPagarId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaAguinaldoPorPagar)
            .WithMany()
            .HasForeignKey(c => c.CuentaAguinaldoPorPagarId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaVacacionesPorPagar)
            .WithMany()
            .HasForeignKey(c => c.CuentaVacacionesPorPagarId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaCesantiaPorPagar)
            .WithMany()
            .HasForeignKey(c => c.CuentaCesantiaPorPagarId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaCCSSPatronal)
            .WithMany()
            .HasForeignKey(c => c.CuentaCCSSPatronalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaCCSSObrero)
            .WithMany()
            .HasForeignKey(c => c.CuentaCCSSObreroId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaINSPatronal)
            .WithMany()
            .HasForeignKey(c => c.CuentaINSPatronalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaRetencionISR)
            .WithMany()
            .HasForeignKey(c => c.CuentaRetencionISRId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaGastoSalarios)
            .WithMany()
            .HasForeignKey(c => c.CuentaGastoSalariosId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaGastoCargasSociales)
            .WithMany()
            .HasForeignKey(c => c.CuentaGastoCargasSocialesId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaGastoAguinaldo)
            .WithMany()
            .HasForeignKey(c => c.CuentaGastoAguinaldoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaGastoVacaciones)
            .WithMany()
            .HasForeignKey(c => c.CuentaGastoVacacionesId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConfiguracionContable>()
            .HasOne(c => c.CuentaGastoCesantia)
            .WithMany()
            .HasForeignKey(c => c.CuentaGastoCesantiaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ----- CuentaIntegracion -----
        modelBuilder.Entity<CuentaIntegracion>()
            .HasIndex(c => new { c.EmpresaId, c.Modulo, c.TipoOperacion, c.ConceptoContable })
            .IsUnique()
            .HasDatabaseName("IX_CuentaIntegracion_Empresa_Modulo_TipoOperacion_Concepto");

        modelBuilder.Entity<CuentaIntegracion>()
            .HasIndex(c => c.CuentaContableId)
            .HasDatabaseName("IX_CuentaIntegracion_CuentaContableId");

        modelBuilder.Entity<CuentaIntegracion>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaIntegracion>()
            .HasOne(c => c.CuentaContable)
            .WithMany()
            .HasForeignKey(c => c.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaIntegracion>()
            .HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaIntegracion>()
            .HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaIntegracion>()
            .Property(c => c.Porcentaje)
            .HasPrecision(5, 2);

        modelBuilder.Entity<CuentaIntegracion>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CuentaIntegracion>()
            .Property(c => c.TipoMovimiento)
            .HasDefaultValue("D");

        modelBuilder.Entity<CuentaIntegracion>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // FASE 8: WORKFLOW - CONFIGURACIONES
        // =============================================

        // ===== TipoWorkflow =====
        modelBuilder.Entity<TipoWorkflow>()
            .HasQueryFilter(t => !t.IsDeleted);

        modelBuilder.Entity<TipoWorkflow>()
            .HasIndex(t => new { t.EmpresaId, t.Codigo })
            .IsUnique();

        modelBuilder.Entity<TipoWorkflow>()
            .HasOne(t => t.Empresa)
            .WithMany()
            .HasForeignKey(t => t.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TipoWorkflow>()
            .HasOne(t => t.CreadoPor)
            .WithMany()
            .HasForeignKey(t => t.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TipoWorkflow>()
            .HasOne(t => t.ModificadoPor)
            .WithMany()
            .HasForeignKey(t => t.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TipoWorkflow>()
            .HasOne(t => t.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(t => t.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TipoWorkflow>()
            .Property(t => t.MontoMinimoAprobacion)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TipoWorkflow>()
            .Property(t => t.MontoMaximoSinAprobacion)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TipoWorkflow>()
            .Property(t => t.RequiereAprobacion)
            .HasDefaultValue(true);

        modelBuilder.Entity<TipoWorkflow>()
            .Property(t => t.NotificarEmail)
            .HasDefaultValue(true);

        modelBuilder.Entity<TipoWorkflow>()
            .Property(t => t.NotificarSistema)
            .HasDefaultValue(true);

        modelBuilder.Entity<TipoWorkflow>()
            .Property(t => t.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<TipoWorkflow>()
            .Property(t => t.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== NivelAprobacion =====
        modelBuilder.Entity<NivelAprobacion>()
            .HasQueryFilter(n => !n.IsDeleted);

        modelBuilder.Entity<NivelAprobacion>()
            .HasIndex(n => new { n.TipoWorkflowId, n.Orden })
            .IsUnique();

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.Empresa)
            .WithMany()
            .HasForeignKey(n => n.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.TipoWorkflow)
            .WithMany(t => t.Niveles)
            .HasForeignKey(n => n.TipoWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.UsuarioAprobador)
            .WithMany()
            .HasForeignKey(n => n.UsuarioAprobadorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.DepartamentoAprobador)
            .WithMany()
            .HasForeignKey(n => n.DepartamentoAprobadorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.NivelEscalamiento)
            .WithMany()
            .HasForeignKey(n => n.NivelEscalamientoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.CreadoPor)
            .WithMany()
            .HasForeignKey(n => n.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.ModificadoPor)
            .WithMany()
            .HasForeignKey(n => n.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NivelAprobacion>()
            .HasOne(n => n.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(n => n.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.MontoMinimo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.MontoMaximo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.TipoAprobador)
            .HasDefaultValue("USU");

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.AprobadoresMinimos)
            .HasDefaultValue(1);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.PuedeRechazar)
            .HasDefaultValue(true);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.PuedeDevolver)
            .HasDefaultValue(true);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.NotificarEmail)
            .HasDefaultValue(true);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.NotificarSistema)
            .HasDefaultValue(true);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<NivelAprobacion>()
            .Property(n => n.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== SolicitudAprobacion =====
        modelBuilder.Entity<SolicitudAprobacion>()
            .HasQueryFilter(s => !s.IsDeleted);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasIndex(s => new { s.EmpresaId, s.ModuloOrigen, s.DocumentoId });

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasIndex(s => new { s.EmpresaId, s.Estado });

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasIndex(s => s.NumeroDocumento);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.Empresa)
            .WithMany()
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.TipoWorkflow)
            .WithMany(t => t.Solicitudes)
            .HasForeignKey(s => s.TipoWorkflowId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.NivelActual)
            .WithMany(n => n.Solicitudes)
            .HasForeignKey(s => s.NivelActualId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.Solicitante)
            .WithMany()
            .HasForeignKey(s => s.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.DepartamentoSolicitante)
            .WithMany()
            .HasForeignKey(s => s.DepartamentoSolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.CreadoPor)
            .WithMany()
            .HasForeignKey(s => s.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.ModificadoPor)
            .WithMany()
            .HasForeignKey(s => s.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .HasOne(s => s.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(s => s.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.Monto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.MontoMonedaBase)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.Estado)
            .HasDefaultValue("PEN");

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.Prioridad)
            .HasDefaultValue("NOR");

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.NumeroNivel)
            .HasDefaultValue(1);

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.Moneda)
            .HasDefaultValue("CRC");

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<SolicitudAprobacion>()
            .Property(s => s.FechaSolicitud)
            .HasDefaultValueSql("GETDATE()");

        // ===== AccionAprobacion =====
        modelBuilder.Entity<AccionAprobacion>()
            .HasIndex(a => new { a.SolicitudAprobacionId, a.FechaAccion });

        modelBuilder.Entity<AccionAprobacion>()
            .HasIndex(a => new { a.UsuarioId, a.FechaAccion });

        modelBuilder.Entity<AccionAprobacion>()
            .HasOne(a => a.Empresa)
            .WithMany()
            .HasForeignKey(a => a.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionAprobacion>()
            .HasOne(a => a.SolicitudAprobacion)
            .WithMany(s => s.Acciones)
            .HasForeignKey(a => a.SolicitudAprobacionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AccionAprobacion>()
            .HasOne(a => a.NivelAprobacion)
            .WithMany(n => n.Acciones)
            .HasForeignKey(a => a.NivelAprobacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionAprobacion>()
            .HasOne(a => a.NivelSiguiente)
            .WithMany()
            .HasForeignKey(a => a.NivelSiguienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionAprobacion>()
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionAprobacion>()
            .HasOne(a => a.UsuarioOriginal)
            .WithMany()
            .HasForeignKey(a => a.UsuarioOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionAprobacion>()
            .HasOne(a => a.ReasignadoA)
            .WithMany()
            .HasForeignKey(a => a.ReasignadoAId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccionAprobacion>()
            .Property(a => a.TiempoRespuestaHoras)
            .HasPrecision(10, 2);

        modelBuilder.Entity<AccionAprobacion>()
            .Property(a => a.DentroDelLimite)
            .HasDefaultValue(true);

        modelBuilder.Entity<AccionAprobacion>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<AccionAprobacion>()
            .Property(a => a.FechaAccion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // FASE 9: PRESUPUESTOS Y CONCILIACIÓN - CONFIGURACIONES
        // =============================================

        // ===== CentroCosto =====
        modelBuilder.Entity<CentroCosto>()
            .HasQueryFilter(c => !c.IsDeleted);

        modelBuilder.Entity<CentroCosto>()
            .HasIndex(c => new { c.EmpresaId, c.Codigo })
            .IsUnique();

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.Padre)
            .WithMany(c => c.Hijos)
            .HasForeignKey(c => c.PadreId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.Sucursal)
            .WithMany()
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.Departamento)
            .WithMany()
            .HasForeignKey(c => c.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.Responsable)
            .WithMany()
            .HasForeignKey(c => c.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CentroCosto>()
            .Property(c => c.Tipo)
            .HasDefaultValue("COS");

        modelBuilder.Entity<CentroCosto>()
            .Property(c => c.Nivel)
            .HasDefaultValue(1);

        modelBuilder.Entity<CentroCosto>()
            .Property(c => c.AceptaMovimientos)
            .HasDefaultValue(true);

        modelBuilder.Entity<CentroCosto>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CentroCosto>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== Presupuesto =====
        modelBuilder.Entity<Presupuesto>()
            .HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Presupuesto>()
            .HasIndex(p => new { p.EmpresaId, p.Codigo })
            .IsUnique();

        modelBuilder.Entity<Presupuesto>()
            .HasIndex(p => new { p.EmpresaId, p.AnioFiscal });

        modelBuilder.Entity<Presupuesto>()
            .HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Presupuesto>()
            .HasOne(p => p.PresupuestoBase)
            .WithMany(p => p.Versiones)
            .HasForeignKey(p => p.PresupuestoBaseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Presupuesto>()
            .HasOne(p => p.AprobadoPor)
            .WithMany()
            .HasForeignKey(p => p.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Presupuesto>()
            .HasOne(p => p.CreadoPor)
            .WithMany()
            .HasForeignKey(p => p.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Presupuesto>()
            .HasOne(p => p.ModificadoPor)
            .WithMany()
            .HasForeignKey(p => p.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Presupuesto>()
            .HasOne(p => p.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(p => p.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Presupuesto>()
            .Property(p => p.MontoTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Presupuesto>()
            .Property(p => p.MontoEjecutado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Presupuesto>()
            .Property(p => p.TipoPresupuesto)
            .HasDefaultValue("ANU");

        modelBuilder.Entity<Presupuesto>()
            .Property(p => p.Estado)
            .HasDefaultValue("BOR");

        modelBuilder.Entity<Presupuesto>()
            .Property(p => p.Version)
            .HasDefaultValue(1);

        modelBuilder.Entity<Presupuesto>()
            .Property(p => p.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== LineaPresupuesto =====
        modelBuilder.Entity<LineaPresupuesto>()
            .HasIndex(l => new { l.PresupuestoId, l.CuentaContableId, l.CentroCostoId });

        modelBuilder.Entity<LineaPresupuesto>()
            .HasOne(l => l.Empresa)
            .WithMany()
            .HasForeignKey(l => l.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LineaPresupuesto>()
            .HasOne(l => l.Presupuesto)
            .WithMany(p => p.Lineas)
            .HasForeignKey(l => l.PresupuestoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LineaPresupuesto>()
            .HasOne(l => l.CuentaContable)
            .WithMany()
            .HasForeignKey(l => l.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LineaPresupuesto>()
            .HasOne(l => l.CentroCosto)
            .WithMany(c => c.LineasPresupuesto)
            .HasForeignKey(l => l.CentroCostoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LineaPresupuesto>()
            .HasOne(l => l.Sucursal)
            .WithMany()
            .HasForeignKey(l => l.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar precisión para todos los montos mensuales
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoEnero).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoFebrero).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoMarzo).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoAbril).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoMayo).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoJunio).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoJulio).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoAgosto).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoSeptiembre).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoOctubre).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoNoviembre).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.MontoDiciembre).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoEnero).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoFebrero).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoMarzo).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoAbril).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoMayo).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoJunio).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoJulio).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoAgosto).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoSeptiembre).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoOctubre).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoNoviembre).HasPrecision(18, 2);
        modelBuilder.Entity<LineaPresupuesto>().Property(l => l.EjecutadoDiciembre).HasPrecision(18, 2);

        // ===== CuentaBancaria =====
        modelBuilder.Entity<CuentaBancaria>()
            .HasQueryFilter(c => !c.IsDeleted);

        modelBuilder.Entity<CuentaBancaria>()
            .HasIndex(c => new { c.EmpresaId, c.NumeroCuenta })
            .IsUnique();

        modelBuilder.Entity<CuentaBancaria>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaBancaria>()
            .HasOne(c => c.Banco)
            .WithMany()
            .HasForeignKey(c => c.BancoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaBancaria>()
            .HasOne(c => c.Sucursal)
            .WithMany()
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaBancaria>()
            .HasOne(c => c.CuentaContable)
            .WithMany()
            .HasForeignKey(c => c.CuentaContableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaBancaria>()
            .HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaBancaria>()
            .HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaBancaria>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaBancaria>()
            .Property(c => c.SaldoInicial)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CuentaBancaria>()
            .Property(c => c.SaldoActual)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CuentaBancaria>()
            .Property(c => c.TipoCuenta)
            .HasDefaultValue("CTE");

        modelBuilder.Entity<CuentaBancaria>()
            .Property(c => c.Moneda)
            .HasDefaultValue("CRC");

        modelBuilder.Entity<CuentaBancaria>()
            .Property(c => c.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<CuentaBancaria>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== MovimientoBancario =====
        modelBuilder.Entity<MovimientoBancario>()
            .HasIndex(m => new { m.EmpresaId, m.Numero })
            .IsUnique();

        modelBuilder.Entity<MovimientoBancario>()
            .HasIndex(m => new { m.CuentaBancariaId, m.Fecha });

        modelBuilder.Entity<MovimientoBancario>()
            .HasIndex(m => new { m.CuentaBancariaId, m.Conciliado });

        modelBuilder.Entity<MovimientoBancario>()
            .HasOne(m => m.Empresa)
            .WithMany()
            .HasForeignKey(m => m.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoBancario>()
            .HasOne(m => m.CuentaBancaria)
            .WithMany(c => c.Movimientos)
            .HasForeignKey(m => m.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoBancario>()
            .HasOne(m => m.Conciliacion)
            .WithMany(c => c.Movimientos)
            .HasForeignKey(m => m.ConciliacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoBancario>()
            .HasOne(m => m.CreadoPor)
            .WithMany()
            .HasForeignKey(m => m.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoBancario>()
            .HasOne(m => m.ModificadoPor)
            .WithMany()
            .HasForeignKey(m => m.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.Monto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.SaldoAnterior)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.SaldoNuevo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.TipoMovimiento)
            .HasDefaultValue("DEP");

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.Naturaleza)
            .HasDefaultValue("CRE");

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.Estado)
            .HasDefaultValue("REG");

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<MovimientoBancario>()
            .Property(m => m.Fecha)
            .HasDefaultValueSql("GETDATE()");

        // GAP-003: Integración Contable
        modelBuilder.Entity<MovimientoBancario>()
            .HasOne(m => m.AsientoContable)
            .WithMany()
            .HasForeignKey(m => m.AsientoContableId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== ConciliacionBancaria =====
        modelBuilder.Entity<ConciliacionBancaria>()
            .HasIndex(c => new { c.EmpresaId, c.Numero })
            .IsUnique();

        modelBuilder.Entity<ConciliacionBancaria>()
            .HasIndex(c => new { c.CuentaBancariaId, c.Anio, c.Mes })
            .IsUnique();

        modelBuilder.Entity<ConciliacionBancaria>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConciliacionBancaria>()
            .HasOne(c => c.CuentaBancaria)
            .WithMany(cb => cb.Conciliaciones)
            .HasForeignKey(c => c.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConciliacionBancaria>()
            .HasOne(c => c.ConciliadoPor)
            .WithMany()
            .HasForeignKey(c => c.ConciliadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConciliacionBancaria>()
            .HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConciliacionBancaria>()
            .HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.SaldoInicialLibros)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.SaldoFinalLibros)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.SaldoEstadoCuenta)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.DepositosEnTransito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.ChequesEnTransito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.NotasCredito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.NotasDebito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.Diferencia)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.Estado)
            .HasDefaultValue("PEN");

        modelBuilder.Entity<ConciliacionBancaria>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== ExtractoBancario =====
        modelBuilder.Entity<ExtractoBancario>()
            .HasIndex(e => new { e.EmpresaId, e.Numero })
            .IsUnique();

        modelBuilder.Entity<ExtractoBancario>()
            .HasIndex(e => new { e.CuentaBancariaId, e.FechaInicio, e.FechaFin });

        modelBuilder.Entity<ExtractoBancario>()
            .HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExtractoBancario>()
            .HasOne(e => e.CuentaBancaria)
            .WithMany(c => c.Extractos)
            .HasForeignKey(e => e.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExtractoBancario>()
            .HasOne(e => e.ConciliacionBancaria)
            .WithMany(c => c.Extractos)
            .HasForeignKey(e => e.ConciliacionBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExtractoBancario>()
            .HasOne(e => e.CreadoPor)
            .WithMany()
            .HasForeignKey(e => e.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExtractoBancario>()
            .HasOne(e => e.ModificadoPor)
            .WithMany()
            .HasForeignKey(e => e.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExtractoBancario>()
            .Property(e => e.SaldoInicial)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ExtractoBancario>()
            .Property(e => e.SaldoFinal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ExtractoBancario>()
            .Property(e => e.TotalCreditos)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ExtractoBancario>()
            .Property(e => e.TotalDebitos)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ExtractoBancario>()
            .Property(e => e.Estado)
            .HasDefaultValue("PEN");

        modelBuilder.Entity<ExtractoBancario>()
            .Property(e => e.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<ExtractoBancario>()
            .Property(e => e.FechaImportacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== LineaExtractoBancario =====
        modelBuilder.Entity<LineaExtractoBancario>()
            .HasIndex(l => new { l.ExtractoBancarioId, l.NumeroLinea });

        modelBuilder.Entity<LineaExtractoBancario>()
            .HasIndex(l => new { l.ExtractoBancarioId, l.EstadoConciliacion });

        modelBuilder.Entity<LineaExtractoBancario>()
            .HasOne(l => l.Empresa)
            .WithMany()
            .HasForeignKey(l => l.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LineaExtractoBancario>()
            .HasOne(l => l.ExtractoBancario)
            .WithMany(e => e.Lineas)
            .HasForeignKey(l => l.ExtractoBancarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LineaExtractoBancario>()
            .HasOne(l => l.MovimientoBancario)
            .WithMany(m => m.LineasExtracto)
            .HasForeignKey(l => l.MovimientoBancarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LineaExtractoBancario>()
            .HasOne(l => l.ReglaConciliacion)
            .WithMany(r => r.LineasConciliadas)
            .HasForeignKey(l => l.ReglaConciliacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LineaExtractoBancario>()
            .HasOne(l => l.ConciliadoPor)
            .WithMany()
            .HasForeignKey(l => l.ConciliadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LineaExtractoBancario>()
            .Property(l => l.Debito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LineaExtractoBancario>()
            .Property(l => l.Credito)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LineaExtractoBancario>()
            .Property(l => l.SaldoAcumulado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LineaExtractoBancario>()
            .Property(l => l.ConfianzaMatch)
            .HasPrecision(5, 2);

        modelBuilder.Entity<LineaExtractoBancario>()
            .Property(l => l.EstadoConciliacion)
            .HasDefaultValue("PEN");

        // ===== ReglaConciliacion =====
        modelBuilder.Entity<ReglaConciliacion>()
            .HasIndex(r => new { r.EmpresaId, r.Nombre })
            .IsUnique();

        modelBuilder.Entity<ReglaConciliacion>()
            .HasIndex(r => new { r.EmpresaId, r.Prioridad });

        modelBuilder.Entity<ReglaConciliacion>()
            .HasOne(r => r.Empresa)
            .WithMany()
            .HasForeignKey(r => r.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaConciliacion>()
            .HasOne(r => r.CuentaBancaria)
            .WithMany(c => c.ReglasConciliacion)
            .HasForeignKey(r => r.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaConciliacion>()
            .HasOne(r => r.CuentaContableDefault)
            .WithMany()
            .HasForeignKey(r => r.CuentaContableDefaultId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaConciliacion>()
            .HasOne(r => r.CreadoPor)
            .WithMany()
            .HasForeignKey(r => r.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaConciliacion>()
            .HasOne(r => r.ModificadoPor)
            .WithMany()
            .HasForeignKey(r => r.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.ToleranciaMonto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.ToleranciaPorcentaje)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.ConfianzaMinima)
            .HasPrecision(5, 2);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.Prioridad)
            .HasDefaultValue(100);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.Activa)
            .HasDefaultValue(true);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.CompararMonto)
            .HasDefaultValue(true);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.CompararFecha)
            .HasDefaultValue(true);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.ToleranciaFechaDias)
            .HasDefaultValue(3);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.ConfianzaMinima)
            .HasDefaultValue(95m);

        modelBuilder.Entity<ReglaConciliacion>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // CUENTAS POR PAGAR - NAVIGATION PROPERTIES
        // =============================================

        // CuentaPorPagar - Empresa
        modelBuilder.Entity<CuentaPorPagar>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // CuentaPorPagar - Proveedor
        modelBuilder.Entity<CuentaPorPagar>()
            .HasOne(c => c.Proveedor)
            .WithMany()
            .HasForeignKey(c => c.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        // CuentaPorPagar - OrdenCompra (opcional)
        modelBuilder.Entity<CuentaPorPagar>()
            .HasOne(c => c.OrdenCompra)
            .WithMany()
            .HasForeignKey(c => c.OrdenCompraId)
            .OnDelete(DeleteBehavior.Restrict);

        // CuentaPorPagar - Usuario Creación
        modelBuilder.Entity<CuentaPorPagar>()
            .HasOne(c => c.CreadoPor)
            .WithMany()
            .HasForeignKey(c => c.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        // CuentaPorPagar - Usuario Modificación
        modelBuilder.Entity<CuentaPorPagar>()
            .HasOne(c => c.ModificadoPor)
            .WithMany()
            .HasForeignKey(c => c.ModificadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        // CuentaPorPagar - Usuario Eliminación
        modelBuilder.Entity<CuentaPorPagar>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // AbonoPago - CuentaPorPagar
        modelBuilder.Entity<AbonoPago>()
            .HasOne(a => a.CuentaPorPagar)
            .WithMany(c => c.Abonos)
            .HasForeignKey(a => a.CuentaPorPagarId)
            .OnDelete(DeleteBehavior.Restrict);

        // AbonoPago - CuentaBancaria (opcional)
        modelBuilder.Entity<AbonoPago>()
            .HasOne(a => a.CuentaBancaria)
            .WithMany()
            .HasForeignKey(a => a.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        // AbonoPago - Usuario Registro
        modelBuilder.Entity<AbonoPago>()
            .HasOne(a => a.RegistradoPor)
            .WithMany()
            .HasForeignKey(a => a.RegistradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique index: Número de factura por proveedor
        modelBuilder.Entity<CuentaPorPagar>()
            .HasIndex(c => new { c.EmpresaId, c.ProveedorId, c.NumeroFactura })
            .IsUnique();

        // Default values
        modelBuilder.Entity<CuentaPorPagar>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<CuentaPorPagar>()
            .Property(c => c.Estado)
            .HasDefaultValue("PEN");

        modelBuilder.Entity<CuentaPorPagar>()
            .Property(c => c.Moneda)
            .HasDefaultValue("CRC");

        modelBuilder.Entity<CuentaPorPagar>()
            .Property(c => c.TipoCambio)
            .HasDefaultValue(1m);

        modelBuilder.Entity<AbonoPago>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // =============================================
        // CUENTAS POR COBRAR - NAVIGATION PROPERTIES
        // =============================================

        // ===== CuentaPorCobrar =====
        modelBuilder.Entity<CuentaPorCobrar>()
            .HasIndex(c => new { c.EmpresaId, c.DocumentoId })
            .IsUnique();

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasIndex(c => new { c.ClienteId, c.Estado });

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasIndex(c => new { c.EmpresaId, c.FechaVencimiento });

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasIndex(c => new { c.EmpresaId, c.Estado, c.FechaVencimiento });

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasOne(c => c.Documento)
            .WithMany()
            .HasForeignKey(c => c.DocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasOne(c => c.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasOne(c => c.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaPorCobrar>()
            .HasOne(c => c.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(c => c.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CuentaPorCobrar>()
            .Property(c => c.MontoOriginal)
            .HasPrecision(18, 5);

        modelBuilder.Entity<CuentaPorCobrar>()
            .Property(c => c.MontoSaldo)
            .HasPrecision(18, 5);

        modelBuilder.Entity<CuentaPorCobrar>()
            .Property(c => c.TipoCambio)
            .HasPrecision(18, 5);

        modelBuilder.Entity<CuentaPorCobrar>()
            .Property(c => c.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");

        // ===== AbonoCobranza =====
        modelBuilder.Entity<AbonoCobranza>()
            .HasIndex(a => a.CuentaPorCobrarId);

        modelBuilder.Entity<AbonoCobranza>()
            .HasIndex(a => a.FechaPago);

        modelBuilder.Entity<AbonoCobranza>()
            .HasOne(a => a.CuentaPorCobrar)
            .WithMany(c => c.Abonos)
            .HasForeignKey(a => a.CuentaPorCobrarId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AbonoCobranza>()
            .HasOne(a => a.ReciboPago)
            .WithMany()
            .HasForeignKey(a => a.ReciboPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AbonoCobranza>()
            .HasOne(a => a.MovimientoBancario)
            .WithMany()
            .HasForeignKey(a => a.MovimientoBancarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AbonoCobranza>()
            .HasOne(a => a.UsuarioCreacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioCreacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AbonoCobranza>()
            .HasOne(a => a.UsuarioModificacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioModificacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AbonoCobranza>()
            .HasOne(a => a.UsuarioEliminacion)
            .WithMany()
            .HasForeignKey(a => a.UsuarioEliminacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AbonoCobranza>()
            .Property(a => a.Monto)
            .HasPrecision(18, 5);

        modelBuilder.Entity<AbonoCobranza>()
            .Property(a => a.TipoCambio)
            .HasPrecision(18, 5);

        modelBuilder.Entity<AbonoCobranza>()
            .Property(a => a.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");
    }
}
