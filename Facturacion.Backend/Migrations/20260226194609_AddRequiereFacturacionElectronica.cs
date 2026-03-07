using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiereFacturacionElectronica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documento_Clave",
                table: "Documentos");

            migrationBuilder.AddColumn<bool>(
                name: "RequiereFacturacionElectronica",
                table: "Empresas",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Update existing empresas to true (they were created before this column existed)
            migrationBuilder.Sql("UPDATE Empresas SET RequiereFacturacionElectronica = 1");

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Documentos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ActividadEconomica",
                table: "Documentos",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6);

            migrationBuilder.CreateIndex(
                name: "IX_Documento_Clave",
                table: "Documentos",
                column: "Clave",
                unique: true,
                filter: "[Clave] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documento_Clave",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "RequiereFacturacionElectronica",
                table: "Empresas");

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Documentos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActividadEconomica",
                table: "Documentos",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documento_Clave",
                table: "Documentos",
                column: "Clave",
                unique: true);
        }
    }
}
