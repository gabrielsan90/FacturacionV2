using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoIdentificacion = table.Column<int>(type: "int", nullable: false),
                    NumeroIdentificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreComercial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmailPrincipal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelefonoPrincipal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Provincia = table.Column<int>(type: "int", nullable: false),
                    Canton = table.Column<int>(type: "int", nullable: false),
                    Distrito = table.Column<int>(type: "int", nullable: false),
                    OtrasSenas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proveedores_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proveedores_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proveedores_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proveedores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Telefonos_ProveedorId",
                table: "Telefonos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_ProveedorId",
                table: "Emails",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_EmpresaId_NumeroIdentificacion",
                table: "Proveedores",
                columns: new[] { "EmpresaId", "NumeroIdentificacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_UsuarioCreacionId",
                table: "Proveedores",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_UsuarioEliminacionId",
                table: "Proveedores",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_UsuarioModificacionId",
                table: "Proveedores",
                column: "UsuarioModificacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Proveedores_ProveedorId",
                table: "Emails",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Telefonos_Proveedores_ProveedorId",
                table: "Telefonos",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Proveedores_ProveedorId",
                table: "Emails");

            migrationBuilder.DropForeignKey(
                name: "FK_Telefonos_Proveedores_ProveedorId",
                table: "Telefonos");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Telefonos_ProveedorId",
                table: "Telefonos");

            migrationBuilder.DropIndex(
                name: "IX_Emails_ProveedorId",
                table: "Emails");
        }
    }
}
