using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_CorreccionesCriticas_GAP002_003_004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AsientoContableId",
                table: "MovimientosBancarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AsientoContableId",
                table: "Documentos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaAguinaldoPorPagarId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaCCSSObreroId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaCCSSPatronalId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaCesantiaPorPagarId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaGastoAguinaldoId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaGastoCargasSocialesId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaGastoCesantiaId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaGastoSalariosId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaGastoVacacionesId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaINSPatronalId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaRetencionISRId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaSalariosPorPagarId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaVacacionesPorPagarId",
                table: "ConfiguracionesContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_AsientoContableId",
                table: "MovimientosBancarios",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_AsientoContableId",
                table: "Documentos",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaAguinaldoPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaAguinaldoPorPagarId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaCCSSObreroId",
                table: "ConfiguracionesContables",
                column: "CuentaCCSSObreroId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaCCSSPatronalId",
                table: "ConfiguracionesContables",
                column: "CuentaCCSSPatronalId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaCesantiaPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaCesantiaPorPagarId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoAguinaldoId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoAguinaldoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoCargasSocialesId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoCargasSocialesId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoCesantiaId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoCesantiaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoSalariosId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoSalariosId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoVacacionesId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoVacacionesId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaINSPatronalId",
                table: "ConfiguracionesContables",
                column: "CuentaINSPatronalId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaRetencionISRId",
                table: "ConfiguracionesContables",
                column: "CuentaRetencionISRId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaSalariosPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaSalariosPorPagarId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaVacacionesPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaVacacionesPorPagarId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaAguinaldoPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaAguinaldoPorPagarId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaCCSSObreroId",
                table: "ConfiguracionesContables",
                column: "CuentaCCSSObreroId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaCCSSPatronalId",
                table: "ConfiguracionesContables",
                column: "CuentaCCSSPatronalId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaCesantiaPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaCesantiaPorPagarId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoAguinaldoId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoAguinaldoId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoCargasSocialesId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoCargasSocialesId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoCesantiaId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoCesantiaId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoSalariosId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoSalariosId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoVacacionesId",
                table: "ConfiguracionesContables",
                column: "CuentaGastoVacacionesId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaINSPatronalId",
                table: "ConfiguracionesContables",
                column: "CuentaINSPatronalId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaRetencionISRId",
                table: "ConfiguracionesContables",
                column: "CuentaRetencionISRId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaSalariosPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaSalariosPorPagarId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaVacacionesPorPagarId",
                table: "ConfiguracionesContables",
                column: "CuentaVacacionesPorPagarId",
                principalTable: "CuentasContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_AsientosContables_AsientoContableId",
                table: "Documentos",
                column: "AsientoContableId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosBancarios_AsientosContables_AsientoContableId",
                table: "MovimientosBancarios",
                column: "AsientoContableId",
                principalTable: "AsientosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaAguinaldoPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaCCSSObreroId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaCCSSPatronalId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaCesantiaPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoAguinaldoId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoCargasSocialesId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoCesantiaId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoSalariosId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaGastoVacacionesId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaINSPatronalId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaRetencionISRId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaSalariosPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesContables_CuentasContables_CuentaVacacionesPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_AsientosContables_AsientoContableId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosBancarios_AsientosContables_AsientoContableId",
                table: "MovimientosBancarios");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosBancarios_AsientoContableId",
                table: "MovimientosBancarios");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_AsientoContableId",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaAguinaldoPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaCCSSObreroId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaCCSSPatronalId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaCesantiaPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoAguinaldoId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoCargasSocialesId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoCesantiaId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoSalariosId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaGastoVacacionesId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaINSPatronalId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaRetencionISRId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaSalariosPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionesContables_CuentaVacacionesPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "AsientoContableId",
                table: "MovimientosBancarios");

            migrationBuilder.DropColumn(
                name: "AsientoContableId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "CuentaAguinaldoPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaCCSSObreroId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaCCSSPatronalId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaCesantiaPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaGastoAguinaldoId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaGastoCargasSocialesId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaGastoCesantiaId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaGastoSalariosId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaGastoVacacionesId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaINSPatronalId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaRetencionISRId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaSalariosPorPagarId",
                table: "ConfiguracionesContables");

            migrationBuilder.DropColumn(
                name: "CuentaVacacionesPorPagarId",
                table: "ConfiguracionesContables");
        }
    }
}
