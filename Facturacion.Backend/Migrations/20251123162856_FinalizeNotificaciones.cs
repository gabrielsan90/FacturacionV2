using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TipoNotificacion = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaLeida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntidadRelacionadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoEntidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UrlAccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Importante = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_EmpresaId",
                table: "Notificaciones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_FechaCreacion",
                table: "Notificaciones",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_FechaExpiracion",
                table: "Notificaciones",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_Tipo_Leida",
                table: "Notificaciones",
                columns: new[] { "TipoNotificacion", "Leida" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_Usuario_Empresa_Leida",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "EmpresaId", "Leida" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificaciones");
        }
    }
}
