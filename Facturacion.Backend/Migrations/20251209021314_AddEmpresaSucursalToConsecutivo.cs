using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaSucursalToConsecutivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the old unique index on ClaveNumeracion
            migrationBuilder.DropIndex(
                name: "IX_Consecutivos_ClaveNumeracion",
                table: "Consecutivos");

            // 2. Add columns as nullable first
            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Consecutivos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SucursalId",
                table: "Consecutivos",
                type: "uniqueidentifier",
                nullable: true);

            // 3. Update existing records from Terminal -> Sucursal -> Empresa hierarchy
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.EmpresaId = s.EmpresaId,
                    c.SucursalId = t.SucursalId
                FROM Consecutivos c
                INNER JOIN Terminales t ON c.TerminalId = t.Id
                INNER JOIN Sucursales s ON t.SucursalId = s.Id
            ");

            // 4. Make columns required after data migration
            migrationBuilder.AlterColumn<Guid>(
                name: "EmpresaId",
                table: "Consecutivos",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SucursalId",
                table: "Consecutivos",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // 5. Create new composite unique index
            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_EmpresaId_SucursalId_TerminalId_TipoDocumento_ClaveNumeracion",
                table: "Consecutivos",
                columns: new[] { "EmpresaId", "SucursalId", "TerminalId", "TipoDocumento", "ClaveNumeracion" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_SucursalId",
                table: "Consecutivos",
                column: "SucursalId");

            // 6. Add foreign keys (NoAction to avoid cascade delete conflicts)
            migrationBuilder.AddForeignKey(
                name: "FK_Consecutivos_Empresas_EmpresaId",
                table: "Consecutivos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Consecutivos_Sucursales_SucursalId",
                table: "Consecutivos",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consecutivos_Empresas_EmpresaId",
                table: "Consecutivos");

            migrationBuilder.DropForeignKey(
                name: "FK_Consecutivos_Sucursales_SucursalId",
                table: "Consecutivos");

            migrationBuilder.DropIndex(
                name: "IX_Consecutivos_EmpresaId_SucursalId_TerminalId_TipoDocumento_ClaveNumeracion",
                table: "Consecutivos");

            migrationBuilder.DropIndex(
                name: "IX_Consecutivos_SucursalId",
                table: "Consecutivos");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Consecutivos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Consecutivos");

            migrationBuilder.CreateIndex(
                name: "IX_Consecutivos_ClaveNumeracion",
                table: "Consecutivos",
                column: "ClaveNumeracion",
                unique: true);
        }
    }
}
