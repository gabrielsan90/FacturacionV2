using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSucursalAddressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barrio",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Canton",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Distrito",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "OtrasSenas",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Sucursales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Barrio",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Canton",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Distrito",
                table: "Sucursales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtrasSenas",
                table: "Sucursales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Provincia",
                table: "Sucursales",
                type: "int",
                nullable: true);
        }
    }
}
