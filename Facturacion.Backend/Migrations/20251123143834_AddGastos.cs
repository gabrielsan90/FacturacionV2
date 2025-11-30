using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGastos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasGasto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasGasto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gastos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoriaGastoId = table.Column<int>(type: "int", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaGasto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MontoSubtotal = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    MontoImpuesto = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false, defaultValue: 0m),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    FormaPago = table.Column<int>(type: "int", nullable: false),
                    EstadoPago = table.Column<int>(type: "int", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false, defaultValue: 0m),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false, defaultValue: 0m),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aprobado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsuarioAprobacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotasAprobacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Comprobante = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Gastos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gastos_AspNetUsers_UsuarioAprobacionId",
                        column: x => x.UsuarioAprobacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_CategoriasGasto_CategoriaGastoId",
                        column: x => x.CategoriaGastoId,
                        principalTable: "CategoriasGasto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasGasto_Nombre",
                table: "CategoriasGasto",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_CategoriaGastoId",
                table: "Gastos",
                column: "CategoriaGastoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_Empresa_NumeroDocumento",
                table: "Gastos",
                columns: new[] { "EmpresaId", "NumeroDocumento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_EmpresaId",
                table: "Gastos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_EstadoPago",
                table: "Gastos",
                column: "EstadoPago");

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_FechaGasto",
                table: "Gastos",
                column: "FechaGasto");

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_ProveedorId",
                table: "Gastos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_UsuarioAprobacionId",
                table: "Gastos",
                column: "UsuarioAprobacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_UsuarioCreacionId",
                table: "Gastos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_UsuarioEliminacionId",
                table: "Gastos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_UsuarioModificacionId",
                table: "Gastos",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gastos");

            migrationBuilder.DropTable(
                name: "CategoriasGasto");
        }
    }
}
