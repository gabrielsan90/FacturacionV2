using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCabysIdToProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CabysId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CabysId",
                table: "Productos",
                column: "CabysId");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_CatalogosCAByS_CabysId",
                table: "Productos",
                column: "CabysId",
                principalTable: "CatalogosCAByS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_CatalogosCAByS_CabysId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_CabysId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CabysId",
                table: "Productos");
        }
    }
}
