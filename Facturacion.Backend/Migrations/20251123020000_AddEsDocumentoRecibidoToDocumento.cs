using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEsDocumentoRecibidoToDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsDocumentoRecibido",
                table: "Documentos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_EsDocumentoRecibido",
                table: "Documentos",
                column: "EsDocumentoRecibido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documentos_EsDocumentoRecibido",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "EsDocumentoRecibido",
                table: "Documentos");
        }
    }
}
