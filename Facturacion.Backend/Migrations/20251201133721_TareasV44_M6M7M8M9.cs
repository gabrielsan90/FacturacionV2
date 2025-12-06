using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class TareasV44_M6M7M8M9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoRetencion",
                table: "Documentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormaFarmaceuticaId",
                table: "DocumentoDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentoDetalleVINs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroVIN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroOrden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoDetalleVINs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleVINs_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleVINs_DocumentoDetalles_DocumentoDetalleId",
                        column: x => x.DocumentoDetalleId,
                        principalTable: "DocumentoDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormasFarmaceuticas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasFarmaceuticas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalles_FormaFarmaceuticaId",
                table: "DocumentoDetalles",
                column: "FormaFarmaceuticaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleVIN_Detalle_Orden",
                table: "DocumentoDetalleVINs",
                columns: new[] { "DocumentoDetalleId", "NumeroOrden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleVIN_DetalleId",
                table: "DocumentoDetalleVINs",
                column: "DocumentoDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleVINs_UsuarioCreacionId",
                table: "DocumentoDetalleVINs",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormaFarmaceutica_Codigo",
                table: "FormasFarmaceuticas",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentoDetalles_FormasFarmaceuticas_FormaFarmaceuticaId",
                table: "DocumentoDetalles",
                column: "FormaFarmaceuticaId",
                principalTable: "FormasFarmaceuticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentoDetalles_FormasFarmaceuticas_FormaFarmaceuticaId",
                table: "DocumentoDetalles");

            migrationBuilder.DropTable(
                name: "DocumentoDetalleVINs");

            migrationBuilder.DropTable(
                name: "FormasFarmaceuticas");

            migrationBuilder.DropIndex(
                name: "IX_DocumentoDetalles_FormaFarmaceuticaId",
                table: "DocumentoDetalles");

            migrationBuilder.DropColumn(
                name: "FechaVencimientoRetencion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "FormaFarmaceuticaId",
                table: "DocumentoDetalles");
        }
    }
}
