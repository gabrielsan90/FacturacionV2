using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAmbienteToConsecutivoUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consecutivos_EmpresaId_SucursalId_TerminalId_TipoDocumento_ClaveNumeracion",
                table: "Consecutivos");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_EmpresaId_SucursalId_TerminalId_TipoDocumento_Ambiente_ClaveNumeracion",
                table: "Consecutivos",
                columns: new[] { "EmpresaId", "SucursalId", "TerminalId", "TipoDocumento", "Ambiente", "ClaveNumeracion" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consecutivos_EmpresaId_SucursalId_TerminalId_TipoDocumento_Ambiente_ClaveNumeracion",
                table: "Consecutivos");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_EmpresaId_SucursalId_TerminalId_TipoDocumento_ClaveNumeracion",
                table: "Consecutivos",
                columns: new[] { "EmpresaId", "SucursalId", "TerminalId", "TipoDocumento", "ClaveNumeracion" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
