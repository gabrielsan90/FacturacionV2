using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeReciboPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecibosPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoOriginalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaveDocumentoOriginal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroConsecutivoOriginal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoDocumentoOriginal = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RecibosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecibosPago_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecibosPago_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecibosPago_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecibosPago_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecibosPago_Documentos_DocumentoOriginalId",
                        column: x => x.DocumentoOriginalId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReciboPago_ClaveOriginal",
                table: "RecibosPago",
                column: "ClaveDocumentoOriginal");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboPago_Documento_Fecha",
                table: "RecibosPago",
                columns: new[] { "DocumentoOriginalId", "FechaPago" });

            migrationBuilder.CreateIndex(
                name: "IX_ReciboPago_DocumentoId",
                table: "RecibosPago",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReciboPago_DocumentoOriginal",
                table: "RecibosPago",
                column: "DocumentoOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_RecibosPago_UsuarioCreacionId",
                table: "RecibosPago",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecibosPago_UsuarioEliminacionId",
                table: "RecibosPago",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecibosPago_UsuarioModificacionId",
                table: "RecibosPago",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecibosPago");
        }
    }
}
