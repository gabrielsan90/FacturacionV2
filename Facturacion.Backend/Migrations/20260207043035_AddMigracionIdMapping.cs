using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMigracionIdMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MigracionesIdMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreEntidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdAnterior = table.Column<int>(type: "int", nullable: false),
                    IdNuevo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaMigracion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigracionesIdMapping", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MigracionIdMapping_Entidad_IdAnterior",
                table: "MigracionesIdMapping",
                columns: new[] { "NombreEntidad", "IdAnterior" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MigracionIdMapping_IdNuevo",
                table: "MigracionesIdMapping",
                column: "IdNuevo");

            migrationBuilder.CreateIndex(
                name: "IX_MigracionIdMapping_NombreEntidad",
                table: "MigracionesIdMapping",
                column: "NombreEntidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MigracionesIdMapping");
        }
    }
}
