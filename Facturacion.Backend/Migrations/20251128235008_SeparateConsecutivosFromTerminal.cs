using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeparateConsecutivosFromTerminal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Terminales_ClaveNumeracionConsecutiva",
                table: "Terminales");

            migrationBuilder.DropColumn(
                name: "ClaveNumeracionConsecutiva",
                table: "Terminales");

            migrationBuilder.DropColumn(
                name: "NumeroConsecutivoActual",
                table: "Terminales");

            migrationBuilder.DropColumn(
                name: "NumeroFin",
                table: "Terminales");

            migrationBuilder.DropColumn(
                name: "NumeroInicio",
                table: "Terminales");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Terminales");

            migrationBuilder.CreateTable(
                name: "Consecutivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClaveNumeracion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroInicio = table.Column<long>(type: "bigint", nullable: false),
                    NumeroFin = table.Column<long>(type: "bigint", nullable: false),
                    NumeroActual = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consecutivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consecutivos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Consecutivos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Consecutivos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Consecutivos_Terminales_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_ClaveNumeracion",
                table: "Consecutivos",
                column: "ClaveNumeracion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_TerminalId_TipoDocumento_Activo",
                table: "Consecutivos",
                columns: new[] { "TerminalId", "TipoDocumento", "Activo" },
                filter: "[IsDeleted] = 0 AND [Activo] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_UsuarioCreacionId",
                table: "Consecutivos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_UsuarioEliminacionId",
                table: "Consecutivos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_UsuarioModificacionId",
                table: "Consecutivos",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consecutivos");

            migrationBuilder.AddColumn<string>(
                name: "ClaveNumeracionConsecutiva",
                table: "Terminales",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumeroConsecutivoActual",
                table: "Terminales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumeroFin",
                table: "Terminales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumeroInicio",
                table: "Terminales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "Terminales",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Terminales_ClaveNumeracionConsecutiva",
                table: "Terminales",
                column: "ClaveNumeracionConsecutiva",
                unique: true);
        }
    }
}
