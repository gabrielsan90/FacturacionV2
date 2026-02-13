using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase5_ActivosFijos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasActivo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CuentaActivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CuentaDepreciacionAcumulada = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CuentaGastoDepreciacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VidaUtilAnios = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    PorcentajeDepreciacionAnual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_CategoriasActivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriasActivo_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoriasActivo_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoriasActivo_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoriasActivo_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivosFijos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoriaActivoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Proveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ValorOriginal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorResidual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepreciacionAcumulada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MetodoDepreciacion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "LR"),
                    VidaUtilAnios = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    PorcentajeDepreciacionAnual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FrecuenciaDepreciacion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "MEN"),
                    UltimaDepreciacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaInicioDepreciacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "ACT"),
                    FechaBaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoBaja = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValorVenta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumeroSerie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AnioFabricacion = table.Column<int>(type: "int", nullable: true),
                    GarantiaHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ActivosFijos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_CategoriasActivo_CategoriaActivoId",
                        column: x => x.CategoriaActivoId,
                        principalTable: "CategoriasActivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_Empleados_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepreciacionesActivo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivoFijoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    MontoDepreciacion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepreciacionAcumuladaAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepreciacionAcumuladaNueva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorLibrosAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorLibrosNuevo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroAsiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Contabilizado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaContabilizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "CAL"),
                    Observaciones = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepreciacionesActivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepreciacionesActivo_ActivosFijos_ActivoFijoId",
                        column: x => x.ActivoFijoId,
                        principalTable: "ActivosFijos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepreciacionesActivo_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrasladosActivo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActivoFijoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SucursalOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SucursalDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AprobadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecibidoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrasladosActivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_ActivosFijos_ActivoFijoId",
                        column: x => x.ActivoFijoId,
                        principalTable: "ActivosFijos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_AspNetUsers_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_AspNetUsers_RecibidoPorId",
                        column: x => x.RecibidoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_Empleados_ResponsableDestinoId",
                        column: x => x.ResponsableDestinoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_Empleados_ResponsableOrigenId",
                        column: x => x.ResponsableOrigenId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_Sucursales_SucursalDestinoId",
                        column: x => x.SucursalDestinoId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrasladosActivo_Sucursales_SucursalOrigenId",
                        column: x => x.SucursalOrigenId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivoFijo_CategoriaId",
                table: "ActivosFijos",
                column: "CategoriaActivoId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivoFijo_Empresa_Codigo",
                table: "ActivosFijos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivoFijo_Estado",
                table: "ActivosFijos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ActivoFijo_NumeroSerie",
                table: "ActivosFijos",
                column: "NumeroSerie");

            migrationBuilder.CreateIndex(
                name: "IX_ActivoFijo_ResponsableId",
                table: "ActivosFijos",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivoFijo_SucursalId",
                table: "ActivosFijos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_ProveedorId",
                table: "ActivosFijos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_UsuarioCreacionId",
                table: "ActivosFijos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_UsuarioEliminacionId",
                table: "ActivosFijos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_UsuarioModificacionId",
                table: "ActivosFijos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaActivo_Empresa_Codigo",
                table: "CategoriasActivo",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasActivo_UsuarioCreacionId",
                table: "CategoriasActivo",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasActivo_UsuarioEliminacionId",
                table: "CategoriasActivo",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasActivo_UsuarioModificacionId",
                table: "CategoriasActivo",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionActivo_Activo_Periodo",
                table: "DepreciacionesActivo",
                columns: new[] { "ActivoFijoId", "Anio", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionActivo_ActivoFijoId",
                table: "DepreciacionesActivo",
                column: "ActivoFijoId");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionActivo_Fecha",
                table: "DepreciacionesActivo",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciacionesActivo_UsuarioCreacionId",
                table: "DepreciacionesActivo",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladoActivo_ActivoFijoId",
                table: "TrasladosActivo",
                column: "ActivoFijoId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladoActivo_Empresa_Numero",
                table: "TrasladosActivo",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrasladoActivo_Estado",
                table: "TrasladosActivo",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladoActivo_Fecha",
                table: "TrasladosActivo",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_AprobadoPorId",
                table: "TrasladosActivo",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_RecibidoPorId",
                table: "TrasladosActivo",
                column: "RecibidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_ResponsableDestinoId",
                table: "TrasladosActivo",
                column: "ResponsableDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_ResponsableOrigenId",
                table: "TrasladosActivo",
                column: "ResponsableOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_SucursalDestinoId",
                table: "TrasladosActivo",
                column: "SucursalDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_SucursalOrigenId",
                table: "TrasladosActivo",
                column: "SucursalOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_UsuarioCreacionId",
                table: "TrasladosActivo",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrasladosActivo_UsuarioModificacionId",
                table: "TrasladosActivo",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepreciacionesActivo");

            migrationBuilder.DropTable(
                name: "TrasladosActivo");

            migrationBuilder.DropTable(
                name: "ActivosFijos");

            migrationBuilder.DropTable(
                name: "CategoriasActivo");
        }
    }
}
