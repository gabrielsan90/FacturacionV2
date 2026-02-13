using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase1_CatalogosYMaestros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Banco",
                table: "Proveedores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bloqueado",
                table: "Proveedores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Proveedores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Celular",
                table: "Proveedores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoCompras",
                table: "Proveedores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CuentaBancaria",
                table: "Proveedores",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoGeneral",
                table: "Proveedores",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiasCredito",
                table: "Proveedores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmailContacto",
                table: "Proveedores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsExtranjero",
                table: "Proveedores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaCompra",
                table: "Proveedores",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoPago",
                table: "Proveedores",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Proveedores",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteCredito",
                table: "Proveedores",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBloqueo",
                table: "Proveedores",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Proveedores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                table: "Proveedores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PedidoMinimo",
                table: "Proveedores",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductosServicios",
                table: "Proveedores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RetencionIVA",
                table: "Proveedores",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetencionRenta",
                table: "Proveedores",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoPendiente",
                table: "Proveedores",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SitioWeb",
                table: "Proveedores",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoContacto",
                table: "Proveedores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TiempoEntrega",
                table: "Proveedores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TipoCuentaBancaria",
                table: "Proveedores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bloqueado",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Clientes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contacto",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoGeneral",
                table: "Clientes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiasCredito",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EnMora",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExentoIVA",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExoneracion",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaCompra",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoPago",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBloqueo",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreInstitucionExoneracion",
                table: "Clientes",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Clientes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroExoneracion",
                table: "Clientes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeExoneracion",
                table: "Clientes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereOrdenCompra",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoContacto",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoExoneracion",
                table: "Clientes",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Zona",
                table: "Clientes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Bancos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreCorto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodigoSINPE = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CodigoSWIFT = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    EsNacional = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EsEstatal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Pais = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_Bancos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bancos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bancos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bancos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bodegas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Responsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PermiteNegativos = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_Bodegas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bodegas_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bodegas_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bodegas_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bodegas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bodegas_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banco_Codigo",
                table: "Bancos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Banco_CodigoSINPE",
                table: "Bancos",
                column: "CodigoSINPE");

            migrationBuilder.CreateIndex(
                name: "IX_Bancos_UsuarioCreacionId",
                table: "Bancos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bancos_UsuarioEliminacionId",
                table: "Bancos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bancos_UsuarioModificacionId",
                table: "Bancos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bodega_Empresa_Sucursal_Codigo",
                table: "Bodegas",
                columns: new[] { "EmpresaId", "SucursalId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bodegas_SucursalId",
                table: "Bodegas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Bodegas_UsuarioCreacionId",
                table: "Bodegas",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bodegas_UsuarioEliminacionId",
                table: "Bodegas",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bodegas_UsuarioModificacionId",
                table: "Bodegas",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bancos");

            migrationBuilder.DropTable(
                name: "Bodegas");

            migrationBuilder.DropColumn(
                name: "Banco",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Bloqueado",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Celular",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "ContactoCompras",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "CuentaBancaria",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "DescuentoGeneral",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "DiasCredito",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "EmailContacto",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "EsExtranjero",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "FechaUltimaCompra",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "FechaUltimoPago",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "LimiteCredito",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "MotivoBloqueo",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Pais",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "PedidoMinimo",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "ProductosServicios",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "RetencionIVA",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "RetencionRenta",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "SaldoPendiente",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "SitioWeb",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "TelefonoContacto",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "TiempoEntrega",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "TipoCuentaBancaria",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Bloqueado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Contacto",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DescuentoGeneral",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DiasCredito",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnMora",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ExentoIVA",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaExoneracion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaUltimaCompra",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaUltimoPago",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "MotivoBloqueo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NombreInstitucionExoneracion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumeroExoneracion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "PorcentajeExoneracion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RequiereOrdenCompra",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TelefonoContacto",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoExoneracion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Zona",
                table: "Clientes");
        }
    }
}
