using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDetalleSurtidoYCamposV44 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetalleSurtido",
                table: "DocumentoDetalles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetalleSurtido",
                table: "DocumentoDetalles");
        }
    }
}
