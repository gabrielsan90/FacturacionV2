using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHaciendaTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OtrasSenas",
                table: "Clientes",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "HaciendaTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    FechaExpiracionToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracionRefreshToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaciendaTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaciendaTokens_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HaciendaToken_Empresa_Ambiente_Activo",
                table: "HaciendaTokens",
                columns: new[] { "EmpresaId", "Ambiente", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_HaciendaToken_FechaExpiracion",
                table: "HaciendaTokens",
                column: "FechaExpiracionToken");

            migrationBuilder.CreateIndex(
                name: "IX_HaciendaToken_FechaExpiracionRefresh",
                table: "HaciendaTokens",
                column: "FechaExpiracionRefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HaciendaTokens");

            migrationBuilder.AlterColumn<string>(
                name: "OtrasSenas",
                table: "Clientes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}
