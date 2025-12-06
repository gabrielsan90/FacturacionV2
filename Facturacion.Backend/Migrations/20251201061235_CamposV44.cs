using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class CamposV44 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProveedorSistemasIdentificacion",
                table: "Empresas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProveedorSistemasNombre",
                table: "Empresas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProveedorSistemasTipoIdentificacion",
                table: "Empresas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPago",
                table: "DocumentoMediosPago",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoTransaccion",
                table: "DocumentoDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FactorIVA",
                table: "DocumentoDetalleImpuestos",
                type: "decimal(5,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProveedorSistemasIdentificacion",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ProveedorSistemasNombre",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ProveedorSistemasTipoIdentificacion",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "FechaPago",
                table: "DocumentoMediosPago");

            migrationBuilder.DropColumn(
                name: "TipoTransaccion",
                table: "DocumentoDetalles");

            migrationBuilder.DropColumn(
                name: "FactorIVA",
                table: "DocumentoDetalleImpuestos");
        }
    }
}
