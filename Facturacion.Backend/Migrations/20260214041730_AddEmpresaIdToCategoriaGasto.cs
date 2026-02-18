using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaIdToCategoriaGasto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CategoriasGasto_Nombre",
                table: "CategoriasGasto");

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "CategoriasGasto",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaGasto_Empresa_Nombre",
                table: "CategoriasGasto",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasGasto_Empresas_EmpresaId",
                table: "CategoriasGasto",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasGasto_Empresas_EmpresaId",
                table: "CategoriasGasto");

            migrationBuilder.DropIndex(
                name: "IX_CategoriaGasto_Empresa_Nombre",
                table: "CategoriasGasto");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "CategoriasGasto");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasGasto_Nombre",
                table: "CategoriasGasto",
                column: "Nombre",
                unique: true);
        }
    }
}
