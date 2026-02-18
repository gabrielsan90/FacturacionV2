using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaImapFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarpetaIMAP",
                table: "Empresas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaveIMAP",
                table: "Empresas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ImapEnableSsl",
                table: "Empresas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PuertoIMAP",
                table: "Empresas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServidorIMAP",
                table: "Empresas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioIMAP",
                table: "Empresas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarpetaIMAP",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ClaveIMAP",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ImapEnableSsl",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "PuertoIMAP",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ServidorIMAP",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "UsuarioIMAP",
                table: "Empresas");
        }
    }
}
