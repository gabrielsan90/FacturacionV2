using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoAmbienteDocumentoYConsecutivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consecutivos_TerminalId_TipoDocumento_Activo",
                table: "Consecutivos");

            migrationBuilder.AddColumn<int>(
                name: "Ambiente",
                table: "Documentos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Ambiente",
                table: "Consecutivos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Documento_Empresa_Ambiente_Tipo",
                table: "Documentos",
                columns: new[] { "EmpresaId", "Ambiente", "TipoDocumento" });

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_TerminalId_TipoDocumento_Ambiente_Activo",
                table: "Consecutivos",
                columns: new[] { "TerminalId", "TipoDocumento", "Ambiente", "Activo" },
                filter: "[IsDeleted] = 0 AND [Activo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documento_Empresa_Ambiente_Tipo",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_Consecutivos_TerminalId_TipoDocumento_Ambiente_Activo",
                table: "Consecutivos");

            migrationBuilder.DropColumn(
                name: "Ambiente",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "Ambiente",
                table: "Consecutivos");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_TerminalId_TipoDocumento_Activo",
                table: "Consecutivos",
                columns: new[] { "TerminalId", "TipoDocumento", "Activo" },
                filter: "[IsDeleted] = 0 AND [Activo] = 1");
        }
    }
}
