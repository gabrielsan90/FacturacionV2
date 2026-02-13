using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addnewmodules1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantillasAsiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoAsiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ConceptoTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UltimaUtilizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VecesUtilizada = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasAsiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillasAsiento_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlantillasAsiento_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlantillasAsiento_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasAsientoLineas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantillaAsientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoMonto = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MontoDebeFijo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoHaberFijo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NaturalezaVariable = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasAsientoLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillasAsientoLineas_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlantillasAsientoLineas_PlantillasAsiento_PlantillaAsientoId",
                        column: x => x.PlantillaAsientoId,
                        principalTable: "PlantillasAsiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasAsiento_CreadoPorId",
                table: "PlantillasAsiento",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasAsiento_EmpresaId",
                table: "PlantillasAsiento",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasAsiento_ModificadoPorId",
                table: "PlantillasAsiento",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasAsientoLineas_CuentaContableId",
                table: "PlantillasAsientoLineas",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasAsientoLineas_PlantillaAsientoId",
                table: "PlantillasAsientoLineas",
                column: "PlantillaAsientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillasAsientoLineas");

            migrationBuilder.DropTable(
                name: "PlantillasAsiento");
        }
    }
}
