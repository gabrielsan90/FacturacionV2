using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoTotalesNoSujeto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalServiciosNoSujetos",
                table: "Documentos",
                type: "decimal(18,5)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMercanciasNoSujetas",
                table: "Documentos",
                type: "decimal(18,5)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNoSujeto",
                table: "Documentos",
                type: "decimal(18,5)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalServiciosNoSujetos",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "TotalMercanciasNoSujetas",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "TotalNoSujeto",
                table: "Documentos");
        }
    }
}
