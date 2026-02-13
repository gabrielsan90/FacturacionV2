using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addnewmodules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRechazo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConversion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteTipoIdentificacion = table.Column<int>(type: "int", nullable: true),
                    ClienteNumeroIdentificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ClienteNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClienteEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClienteTelefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CondicionVenta = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    PlazoCreditoDias = table.Column<int>(type: "int", nullable: true),
                    MedioPago = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalDescuentos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalImpuestos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ObservacionesInternas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DocumentoGeneradoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Documentos_DocumentoGeneradoId",
                        column: x => x.DocumentoGeneradoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Terminales_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuentasPorCobrar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MontoOriginal = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    MontoSaldo = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    MotivoCastigoBaja = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroConsecutivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiasCredito = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPorCobrar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuentasPorPagar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaFactura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoOriginal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoSaldo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    OrdenCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "CRC"),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 1m),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPorPagar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorPagar_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CotizacionDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    NaturalezaDescuento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CodigoTarifaIVA = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    TarifaIVA = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    MontoIVA = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalImpuestos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionDetalles_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CotizacionDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AbonosCobranza",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaPorCobrarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    NumeroReferencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReciboPagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovimientoBancarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonosCobranza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbonosCobranza_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonosCobranza_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonosCobranza_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonosCobranza_CuentasPorCobrar_CuentaPorCobrarId",
                        column: x => x.CuentaPorCobrarId,
                        principalTable: "CuentasPorCobrar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonosCobranza_MovimientosBancarios_MovimientoBancarioId",
                        column: x => x.MovimientoBancarioId,
                        principalTable: "MovimientosBancarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonosCobranza_RecibosPago_ReciboPagoId",
                        column: x => x.ReciboPagoId,
                        principalTable: "RecibosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AbonosPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaPorPagarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    NumeroReferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CuentaBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    RegistradoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbonosPago_AspNetUsers_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonosPago_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbonosPago_CuentasPorPagar_CuentaPorPagarId",
                        column: x => x.CuentaPorPagarId,
                        principalTable: "CuentasPorPagar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobranza_CuentaPorCobrarId",
                table: "AbonosCobranza",
                column: "CuentaPorCobrarId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobranza_FechaPago",
                table: "AbonosCobranza",
                column: "FechaPago");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobranza_MovimientoBancarioId",
                table: "AbonosCobranza",
                column: "MovimientoBancarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobranza_ReciboPagoId",
                table: "AbonosCobranza",
                column: "ReciboPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobranza_UsuarioCreacionId",
                table: "AbonosCobranza",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobranza_UsuarioEliminacionId",
                table: "AbonosCobranza",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCobranza_UsuarioModificacionId",
                table: "AbonosCobranza",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosPago_CuentaBancariaId",
                table: "AbonosPago",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosPago_CuentaPorPagarId",
                table: "AbonosPago",
                column: "CuentaPorPagarId");

            migrationBuilder.CreateIndex(
                name: "IX_AbonosPago_RegistradoPorId",
                table: "AbonosPago",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalle_CotizacionId",
                table: "CotizacionDetalles",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionDetalles_ProductoId",
                table: "CotizacionDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_Empresa_Estado",
                table: "Cotizaciones",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_EmpresaId",
                table: "Cotizaciones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_FechaEmision",
                table: "Cotizaciones",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_FechaVencimiento",
                table: "Cotizaciones",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_Numero",
                table: "Cotizaciones",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_ClienteId",
                table: "Cotizaciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_DocumentoGeneradoId",
                table: "Cotizaciones",
                column: "DocumentoGeneradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_SucursalId",
                table: "Cotizaciones",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_TerminalId",
                table: "Cotizaciones",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_UsuarioCreacionId",
                table: "Cotizaciones",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_UsuarioEliminacionId",
                table: "Cotizaciones",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_UsuarioModificacionId",
                table: "Cotizaciones",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_ClienteId_Estado",
                table: "CuentasPorCobrar",
                columns: new[] { "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_DocumentoId",
                table: "CuentasPorCobrar",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_DocumentoId",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "DocumentoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_Estado_FechaVencimiento",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "Estado", "FechaVencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_EmpresaId_FechaVencimiento",
                table: "CuentasPorCobrar",
                columns: new[] { "EmpresaId", "FechaVencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_UsuarioCreacionId",
                table: "CuentasPorCobrar",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_UsuarioEliminacionId",
                table: "CuentasPorCobrar",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_UsuarioModificacionId",
                table: "CuentasPorCobrar",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_CreadoPorId",
                table: "CuentasPorPagar",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_EmpresaId_ProveedorId_NumeroFactura",
                table: "CuentasPorPagar",
                columns: new[] { "EmpresaId", "ProveedorId", "NumeroFactura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_ModificadoPorId",
                table: "CuentasPorPagar",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_OrdenCompraId",
                table: "CuentasPorPagar",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_ProveedorId",
                table: "CuentasPorPagar",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorPagar_UsuarioEliminacionId",
                table: "CuentasPorPagar",
                column: "UsuarioEliminacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbonosCobranza");

            migrationBuilder.DropTable(
                name: "AbonosPago");

            migrationBuilder.DropTable(
                name: "CotizacionDetalles");

            migrationBuilder.DropTable(
                name: "CuentasPorCobrar");

            migrationBuilder.DropTable(
                name: "CuentasPorPagar");

            migrationBuilder.DropTable(
                name: "Cotizaciones");
        }
    }
}
