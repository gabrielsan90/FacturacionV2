using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaIdToBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Banco_Codigo",
                table: "Bancos");

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Bancos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Banco_Empresa_Codigo",
                table: "Bancos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bancos_Empresas_EmpresaId",
                table: "Bancos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bancos_Empresas_EmpresaId",
                table: "Bancos");

            migrationBuilder.DropIndex(
                name: "IX_Banco_Empresa_Codigo",
                table: "Bancos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Bancos");

            migrationBuilder.CreateIndex(
                name: "IX_Banco_Codigo",
                table: "Bancos",
                column: "Codigo",
                unique: true);
        }
    }
}
