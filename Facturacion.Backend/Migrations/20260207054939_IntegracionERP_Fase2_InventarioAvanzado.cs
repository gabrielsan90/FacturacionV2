using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase2_InventarioAvanzado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BodegaId",
                table: "Inventarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoPromedio",
                table: "Inventarios",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "LoteId",
                table: "Inventarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AjustesInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BodegaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoAjuste = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AprobadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.PrimaryKey("PK_AjustesInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_AspNetUsers_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaFabricacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroFacturaCompra = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CantidadInicial = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadActual = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EnCuarentena = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MotivoCuarentena = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCuarentena = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrasladosInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BodegaOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BodegaDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnviadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RecibidoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.PrimaryKey("PK_TrasladosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_AspNetUsers_EnviadoPorId",
                        column: x => x.EnviadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_AspNetUsers_RecibidoPorId",
                        column: x => x.RecibidoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_Bodegas_BodegaDestinoId",
                        column: x => x.BodegaDestinoId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_Bodegas_BodegaOrigenId",
                        column: x => x.BodegaOrigenId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AjustesInventarioDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AjusteInventarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CantidadSistema = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadFisica = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Diferencia = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesInventarioDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AjustesInventarioDetalle_AjustesInventario_AjusteInventarioId",
                        column: x => x.AjusteInventarioId,
                        principalTable: "AjustesInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AjustesInventarioDetalle_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventarioDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrasladosInventarioDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrasladoInventarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CantidadSolicitada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadEnviada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrasladosInventarioDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrasladosInventarioDetalle_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventarioDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosInventarioDetalle_TrasladosInventario_TrasladoInventarioId",
                        column: x => x.TrasladoInventarioId,
                        principalTable: "TrasladosInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_BodegaId",
                table: "Inventarios",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_LoteId",
                table: "Inventarios",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_AjusteInventario_Empresa_Numero",
                table: "AjustesInventario",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_AprobadoPorId",
                table: "AjustesInventario",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_BodegaId",
                table: "AjustesInventario",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_UsuarioCreacionId",
                table: "AjustesInventario",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_UsuarioEliminacionId",
                table: "AjustesInventario",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_UsuarioModificacionId",
                table: "AjustesInventario",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventarioDetalle_AjusteInventarioId",
                table: "AjustesInventarioDetalle",
                column: "AjusteInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventarioDetalle_LoteId",
                table: "AjustesInventarioDetalle",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventarioDetalle_ProductoId",
                table: "AjustesInventarioDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lote_Empresa_NumeroLote",
                table: "Lotes",
                columns: new[] { "EmpresaId", "NumeroLote" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProductoId",
                table: "Lotes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProveedorId",
                table: "Lotes",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_UsuarioCreacionId",
                table: "Lotes",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_UsuarioEliminacionId",
                table: "Lotes",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_UsuarioModificacionId",
                table: "Lotes",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladoInventario_Empresa_Numero",
                table: "TrasladosInventario",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventario_BodegaDestinoId",
                table: "TrasladosInventario",
                column: "BodegaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventario_BodegaOrigenId",
                table: "TrasladosInventario",
                column: "BodegaOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventario_EnviadoPorId",
                table: "TrasladosInventario",
                column: "EnviadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventario_RecibidoPorId",
                table: "TrasladosInventario",
                column: "RecibidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventario_UsuarioCreacionId",
                table: "TrasladosInventario",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventario_UsuarioEliminacionId",
                table: "TrasladosInventario",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventario_UsuarioModificacionId",
                table: "TrasladosInventario",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventarioDetalle_LoteId",
                table: "TrasladosInventarioDetalle",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventarioDetalle_ProductoId",
                table: "TrasladosInventarioDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosInventarioDetalle_TrasladoInventarioId",
                table: "TrasladosInventarioDetalle",
                column: "TrasladoInventarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Bodegas_BodegaId",
                table: "Inventarios",
                column: "BodegaId",
                principalTable: "Bodegas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Lotes_LoteId",
                table: "Inventarios",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Bodegas_BodegaId",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Lotes_LoteId",
                table: "Inventarios");

            migrationBuilder.DropTable(
                name: "AjustesInventarioDetalle");

            migrationBuilder.DropTable(
                name: "TrasladosInventarioDetalle");

            migrationBuilder.DropTable(
                name: "AjustesInventario");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "TrasladosInventario");

            migrationBuilder.DropIndex(
                name: "IX_Inventario_BodegaId",
                table: "Inventarios");

            migrationBuilder.DropIndex(
                name: "IX_Inventarios_LoteId",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "BodegaId",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "CostoPromedio",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "LoteId",
                table: "Inventarios");
        }
    }
}
