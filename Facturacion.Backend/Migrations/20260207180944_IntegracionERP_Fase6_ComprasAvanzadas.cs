using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase6_ComprasAvanzadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluacionesProveedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Trimestre = table.Column<int>(type: "int", nullable: true),
                    Mes = table.Column<int>(type: "int", nullable: true),
                    CalidadProducto = table.Column<int>(type: "int", nullable: false),
                    TiempoEntrega = table.Column<int>(type: "int", nullable: false),
                    Precios = table.Column<int>(type: "int", nullable: false),
                    ServicioPostventa = table.Column<int>(type: "int", nullable: false),
                    DocumentacionCompleta = table.Column<int>(type: "int", nullable: false),
                    Comunicacion = table.Column<int>(type: "int", nullable: false),
                    Flexibilidad = table.Column<int>(type: "int", nullable: false),
                    CumplimientoEspecificaciones = table.Column<int>(type: "int", nullable: false),
                    PromedioGeneral = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    Clasificacion = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CantidadOrdenes = table.Column<int>(type: "int", nullable: false),
                    MontoTotalCompras = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadDevoluciones = table.Column<int>(type: "int", nullable: false),
                    MontoDevoluciones = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeEntregasATiempo = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CantidadNoConformidades = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AccionesCorrectivas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Recomendacion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    FechaEvaluacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluacionesProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluacionesProveedor_AspNetUsers_EvaluadoPorId",
                        column: x => x.EvaluadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluacionesProveedor_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluacionesProveedor_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BodegaDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Impuesto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaEntregaEsperada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CondicionPago = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DiasCredito = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "BOR"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AprobadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_AspNetUsers_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Bodegas_BodegaDestinoId",
                        column: x => x.BodegaDestinoId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requisiciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SolicitanteId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DepartamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Prioridad = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "MED"),
                    FechaRequerida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Justificacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MontoEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "BOR"),
                    AprobadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisiciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requisiciones_AspNetUsers_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requisiciones_AspNetUsers_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requisiciones_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requisiciones_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requisiciones_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requisiciones_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requisiciones_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requisiciones_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompraDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeIVA = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Impuesto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompraDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalle_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BodegaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroFacturaProveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaFacturaProveedor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "APL"),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CotizacionesProveedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequisicionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "ENV"),
                    ReferenciaProveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MontoSubtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoImpuestos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TiempoEntregaDias = table.Column<int>(type: "int", nullable: true),
                    ValidezDias = table.Column<int>(type: "int", nullable: true),
                    CondicionesPago = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LugarEntrega = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IncluyeFlete = table.Column<bool>(type: "bit", nullable: false),
                    MontoFlete = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Seleccionada = table.Column<bool>(type: "bit", nullable: false),
                    PuntuacionTotal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NombreArchivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegistradoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionesProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedor_AspNetUsers_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedor_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedor_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedor_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedor_Requisiciones_RequisicionId",
                        column: x => x.RequisicionId,
                        principalTable: "Requisiciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequisicionesDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisicionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Especificaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CantidadSolicitada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CantidadCotizada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadOrdenada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SubtotalEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProveedorSugeridoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicionesDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequisicionesDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesDetalle_Proveedores_ProveedorSugeridoId",
                        column: x => x.ProveedorSugeridoId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesDetalle_Requisiciones_RequisicionId",
                        column: x => x.RequisicionId,
                        principalTable: "Requisiciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesCompraDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecepcionCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenCompraDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesCompraDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_OrdenesCompraDetalle_OrdenCompraDetalleId",
                        column: x => x.OrdenCompraDetalleId,
                        principalTable: "OrdenesCompraDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_RecepcionesCompra_RecepcionCompraId",
                        column: x => x.RecepcionCompraId,
                        principalTable: "RecepcionesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComparativosCotizacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequisicionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "BOR"),
                    CotizacionSeleccionadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JustificacionSeleccion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CriteriosEvaluacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PesoPrecio = table.Column<int>(type: "int", nullable: false),
                    PesoTiempoEntrega = table.Column<int>(type: "int", nullable: false),
                    PesoCalidad = table.Column<int>(type: "int", nullable: false),
                    PesoCondicionesPago = table.Column<int>(type: "int", nullable: false),
                    RealizadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AprobadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrdenCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparativosCotizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacion_AspNetUsers_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacion_AspNetUsers_RealizadoPorId",
                        column: x => x.RealizadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacion_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacion_CotizacionesProveedor_CotizacionSeleccionadaId",
                        column: x => x.CotizacionSeleccionadaId,
                        principalTable: "CotizacionesProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacion_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacion_Requisiciones_RequisicionId",
                        column: x => x.RequisicionId,
                        principalTable: "Requisiciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CotizacionesProveedorDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisicionDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CodigoProveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeIVA = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MontoIVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TiempoEntregaDias = table.Column<int>(type: "int", nullable: true),
                    DisponibleInmediato = table.Column<bool>(type: "bit", nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaisOrigen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Garantia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionesProveedorDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedorDetalle_CotizacionesProveedor_CotizacionProveedorId",
                        column: x => x.CotizacionProveedorId,
                        principalTable: "CotizacionesProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedorDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedorDetalle_RequisicionesDetalle_RequisicionDetalleId",
                        column: x => x.RequisicionDetalleId,
                        principalTable: "RequisicionesDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComparativosCotizacionDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComparativoCotizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PuntuacionPrecio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PuntuacionTiempoEntrega = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PuntuacionCalidad = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PuntuacionCondicionesPago = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PuntuacionTotal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Ranking = table.Column<int>(type: "int", nullable: false),
                    CumpleRequisitos = table.Column<bool>(type: "bit", nullable: false),
                    MotivoNoCumple = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparativosCotizacionDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacionDetalle_ComparativosCotizacion_ComparativoCotizacionId",
                        column: x => x.ComparativoCotizacionId,
                        principalTable: "ComparativosCotizacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComparativosCotizacionDetalle_CotizacionesProveedor_CotizacionProveedorId",
                        column: x => x.CotizacionProveedorId,
                        principalTable: "CotizacionesProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparativoCotizacion_Empresa_Numero",
                table: "ComparativosCotizacion",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComparativoCotizacion_RequisicionId",
                table: "ComparativosCotizacion",
                column: "RequisicionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparativosCotizacion_AprobadoPorId",
                table: "ComparativosCotizacion",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparativosCotizacion_CotizacionSeleccionadaId",
                table: "ComparativosCotizacion",
                column: "CotizacionSeleccionadaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparativosCotizacion_OrdenCompraId",
                table: "ComparativosCotizacion",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparativosCotizacion_RealizadoPorId",
                table: "ComparativosCotizacion",
                column: "RealizadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparativosCotizacion_UsuarioModificacionId",
                table: "ComparativosCotizacion",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparativoCotizacionDetalle_ComparativoId",
                table: "ComparativosCotizacionDetalle",
                column: "ComparativoCotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparativosCotizacionDetalle_CotizacionProveedorId",
                table: "ComparativosCotizacionDetalle",
                column: "CotizacionProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesProveedor_RegistradoPorId",
                table: "CotizacionesProveedor",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesProveedor_UsuarioModificacionId",
                table: "CotizacionesProveedor",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionProveedor_Empresa_Numero",
                table: "CotizacionesProveedor",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionProveedor_Estado",
                table: "CotizacionesProveedor",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionProveedor_ProveedorId",
                table: "CotizacionesProveedor",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionProveedor_RequisicionId",
                table: "CotizacionesProveedor",
                column: "RequisicionId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesProveedorDetalle_ProductoId",
                table: "CotizacionesProveedorDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesProveedorDetalle_RequisicionDetalleId",
                table: "CotizacionesProveedorDetalle",
                column: "RequisicionDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionProveedorDetalle_CotizacionId",
                table: "CotizacionesProveedorDetalle",
                column: "CotizacionProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesProveedor_EvaluadoPorId",
                table: "EvaluacionesProveedor",
                column: "EvaluadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionProveedor_Empresa_Proveedor_Periodo",
                table: "EvaluacionesProveedor",
                columns: new[] { "EmpresaId", "ProveedorId", "Anio", "Trimestre", "Mes" },
                unique: true,
                filter: "[Trimestre] IS NOT NULL AND [Mes] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionProveedor_ProveedorId",
                table: "EvaluacionesProveedor",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_Empresa_Numero",
                table: "OrdenesCompra",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_Estado",
                table: "OrdenesCompra",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_Fecha",
                table: "OrdenesCompra",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_ProveedorId",
                table: "OrdenesCompra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_AprobadoPorId",
                table: "OrdenesCompra",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_BodegaDestinoId",
                table: "OrdenesCompra",
                column: "BodegaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_CreadoPorId",
                table: "OrdenesCompra",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_ModificadoPorId",
                table: "OrdenesCompra",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_SucursalId",
                table: "OrdenesCompra",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_UsuarioEliminacionId",
                table: "OrdenesCompra",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompraDetalle_OrdenCompraId",
                table: "OrdenesCompraDetalle",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalle_ProductoId",
                table: "OrdenesCompraDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionCompra_Empresa_Numero",
                table: "RecepcionesCompra",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionCompra_Fecha",
                table: "RecepcionesCompra",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionCompra_OrdenCompraId",
                table: "RecepcionesCompra",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_BodegaId",
                table: "RecepcionesCompra",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_CreadoPorId",
                table: "RecepcionesCompra",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_ModificadoPorId",
                table: "RecepcionesCompra",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_UsuarioEliminacionId",
                table: "RecepcionesCompra",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionCompraDetalle_RecepcionId",
                table: "RecepcionesCompraDetalle",
                column: "RecepcionCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompraDetalle_LoteId",
                table: "RecepcionesCompraDetalle",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompraDetalle_OrdenCompraDetalleId",
                table: "RecepcionesCompraDetalle",
                column: "OrdenCompraDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompraDetalle_ProductoId",
                table: "RecepcionesCompraDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisicion_Empresa_Numero",
                table: "Requisiciones",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requisicion_Estado",
                table: "Requisiciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Requisicion_Fecha",
                table: "Requisiciones",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Requisicion_SolicitanteId",
                table: "Requisiciones",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisiciones_AprobadoPorId",
                table: "Requisiciones",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisiciones_DepartamentoId",
                table: "Requisiciones",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisiciones_SucursalId",
                table: "Requisiciones",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisiciones_UsuarioCreacionId",
                table: "Requisiciones",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisiciones_UsuarioEliminacionId",
                table: "Requisiciones",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisiciones_UsuarioModificacionId",
                table: "Requisiciones",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionDetalle_RequisicionId",
                table: "RequisicionesDetalle",
                column: "RequisicionId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesDetalle_ProductoId",
                table: "RequisicionesDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesDetalle_ProveedorSugeridoId",
                table: "RequisicionesDetalle",
                column: "ProveedorSugeridoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparativosCotizacionDetalle");

            migrationBuilder.DropTable(
                name: "CotizacionesProveedorDetalle");

            migrationBuilder.DropTable(
                name: "EvaluacionesProveedor");

            migrationBuilder.DropTable(
                name: "RecepcionesCompraDetalle");

            migrationBuilder.DropTable(
                name: "ComparativosCotizacion");

            migrationBuilder.DropTable(
                name: "RequisicionesDetalle");

            migrationBuilder.DropTable(
                name: "OrdenesCompraDetalle");

            migrationBuilder.DropTable(
                name: "RecepcionesCompra");

            migrationBuilder.DropTable(
                name: "CotizacionesProveedor");

            migrationBuilder.DropTable(
                name: "OrdenesCompra");

            migrationBuilder.DropTable(
                name: "Requisiciones");
        }
    }
}
