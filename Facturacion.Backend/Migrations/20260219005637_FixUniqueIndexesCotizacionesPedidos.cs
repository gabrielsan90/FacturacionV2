using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueIndexesCotizacionesPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PedidoVenta_Numero",
                table: "PedidosVenta");

            migrationBuilder.DropIndex(
                name: "IX_Cotizacion_Numero",
                table: "Cotizaciones");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVenta_Empresa_Numero",
                table: "PedidosVenta",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_Empresa_Numero",
                table: "Cotizaciones",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PedidoVenta_Empresa_Numero",
                table: "PedidosVenta");

            migrationBuilder.DropIndex(
                name: "IX_Cotizacion_Empresa_Numero",
                table: "Cotizaciones");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoVenta_Numero",
                table: "PedidosVenta",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_Numero",
                table: "Cotizaciones",
                column: "Numero",
                unique: true);
        }
    }
}
