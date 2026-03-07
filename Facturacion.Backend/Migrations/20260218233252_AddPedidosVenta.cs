using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidosVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidosVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalDescuentos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalImpuestos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UsuarioAprobacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.PrimaryKey("PK_PedidosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_AspNetUsers_UsuarioAprobacionId",
                        column: x => x.UsuarioAprobacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Documentos_DocumentoGeneradoId",
                        column: x => x.DocumentoGeneradoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoVentaDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PedidoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MontoIVA = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoVentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoVentaDetalles_PedidosVenta_PedidoVentaId",
                        column: x => x.PedidoVentaId,
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoVentaDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_ClienteId",
                table: "PedidosVenta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_DocumentoGeneradoId",
                table: "PedidosVenta",
                column: "DocumentoGeneradoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_UsuarioAprobacionId",
                table: "PedidosVenta",
                column: "UsuarioAprobacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_UsuarioCreacionId",
                table: "PedidosVenta",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_UsuarioEliminacionId",
                table: "PedidosVenta",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_UsuarioModificacionId",
                table: "PedidosVenta",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVenta_Empresa_Estado",
                table: "PedidosVenta",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVenta_EmpresaId",
                table: "PedidosVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVenta_Fecha",
                table: "PedidosVenta",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVenta_Numero",
                table: "PedidosVenta",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVentaDetalle_PedidoVentaId",
                table: "PedidoVentaDetalles",
                column: "PedidoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVentaDetalles_ProductoId",
                table: "PedidoVentaDetalles",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidoVentaDetalles");

            migrationBuilder.DropTable(
                name: "PedidosVenta");
        }
    }
}
