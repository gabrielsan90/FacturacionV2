using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class EmpresaUniqueIndexNumeroIdentificacionAmbiente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Empresas_NumeroIdentificacion",
                table: "Empresas");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_NumeroIdentificacion_Ambiente",
                table: "Empresas",
                columns: new[] { "NumeroIdentificacion", "Ambiente" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Empresas_NumeroIdentificacion_Ambiente",
                table: "Empresas");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_NumeroIdentificacion",
                table: "Empresas",
                column: "NumeroIdentificacion",
                unique: true);
        }
    }
}
