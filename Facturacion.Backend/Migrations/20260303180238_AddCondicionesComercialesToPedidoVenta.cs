using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCondicionesComercialesToPedidoVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoTarifaIVA",
                table: "PedidoVentaDetalles",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaIVA",
                table: "PedidoVentaDetalles",
                type: "decimal(18,5)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CondicionVenta",
                table: "PedidosVenta",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MedioPago",
                table: "PedidosVenta",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PlazoCreditoDias",
                table: "PedidosVenta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TipoCambio",
                table: "PedidosVenta",
                type: "decimal(18,5)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoTarifaIVA",
                table: "PedidoVentaDetalles");

            migrationBuilder.DropColumn(
                name: "TarifaIVA",
                table: "PedidoVentaDetalles");

            migrationBuilder.DropColumn(
                name: "CondicionVenta",
                table: "PedidosVenta");

            migrationBuilder.DropColumn(
                name: "MedioPago",
                table: "PedidosVenta");

            migrationBuilder.DropColumn(
                name: "PlazoCreditoDias",
                table: "PedidosVenta");

            migrationBuilder.DropColumn(
                name: "TipoCambio",
                table: "PedidosVenta");
        }
    }
}
