using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureDocumentoOtrosCargoUserRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioCreacionId",
                table: "DocumentoOtrosCargos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioEliminacionId",
                table: "DocumentoOtrosCargos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioModificacionId",
                table: "DocumentoOtrosCargos");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioCreacionId",
                table: "DocumentoOtrosCargos",
                column: "UsuarioCreacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioEliminacionId",
                table: "DocumentoOtrosCargos",
                column: "UsuarioEliminacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioModificacionId",
                table: "DocumentoOtrosCargos",
                column: "UsuarioModificacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioCreacionId",
                table: "DocumentoOtrosCargos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioEliminacionId",
                table: "DocumentoOtrosCargos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioModificacionId",
                table: "DocumentoOtrosCargos");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioCreacionId",
                table: "DocumentoOtrosCargos",
                column: "UsuarioCreacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioEliminacionId",
                table: "DocumentoOtrosCargos",
                column: "UsuarioEliminacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoOtrosCargos_AspNetUsers_UsuarioModificacionId",
                table: "DocumentoOtrosCargos",
                column: "UsuarioModificacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
