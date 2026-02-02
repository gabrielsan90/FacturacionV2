using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class faltantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SmtpEnableSsl",
                table: "Empresas",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpDisplayName",
                table: "Empresas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpCopiaEmail",
                table: "Empresas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpEnviarPDF",
                table: "Empresas",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpEnviarXML",
                table: "Empresas",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpEnableSsl",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "SmtpDisplayName",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "SmtpCopiaEmail",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "SmtpEnviarPDF",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "SmtpEnviarXML",
                table: "Empresas");
        }
    }
}
