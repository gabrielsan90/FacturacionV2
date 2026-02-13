using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase7_Contabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuentasContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoCuenta = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Naturaleza = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    CuentaPadreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AceptaMovimientos = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequiereCentroCosto = table.Column<bool>(type: "bit", nullable: false),
                    RequiereTercero = table.Column<bool>(type: "bit", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasContables_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasContables_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasContables_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasContables_CuentasContables_CuentaPadreId",
                        column: x => x.CuentaPadreId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasContables_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MesInicioPeriodoFiscal = table.Column<int>(type: "int", nullable: false),
                    DiaInicioPeriodoFiscal = table.Column<int>(type: "int", nullable: false),
                    NumeroPeriodosPorAnio = table.Column<int>(type: "int", nullable: false),
                    MonedaBase = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "CRC"),
                    DecimalesMoneda = table.Column<int>(type: "int", nullable: false),
                    DecimalesTipoCambio = table.Column<int>(type: "int", nullable: false),
                    TipoNumeracion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    UsarPrefijoNumero = table.Column<bool>(type: "bit", nullable: false),
                    FormatoPrefijo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LongitudNumero = table.Column<int>(type: "int", nullable: false),
                    PeriodosAbiertosSimultaneos = table.Column<int>(type: "int", nullable: false),
                    PermitirMovimientosFuturos = table.Column<bool>(type: "bit", nullable: false),
                    BloquearPeriodosCerrados = table.Column<bool>(type: "bit", nullable: false),
                    GenerarAsientosAutomaticos = table.Column<bool>(type: "bit", nullable: false),
                    FrecuenciaGeneracion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AprobarAsientosAutomaticos = table.Column<bool>(type: "bit", nullable: false),
                    RequiereAprobacionCierre = table.Column<bool>(type: "bit", nullable: false),
                    ValidarBalanceAntesCierre = table.Column<bool>(type: "bit", nullable: false),
                    GenerarAsientoCierreAutomatico = table.Column<bool>(type: "bit", nullable: false),
                    ToleranciaDiferencias = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    RegistrarDiferenciasCambiarias = table.Column<bool>(type: "bit", nullable: false),
                    RequiereAprobacionAsientos = table.Column<bool>(type: "bit", nullable: false),
                    MontoLimiteSinAprobacion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NivelesAprobacion = table.Column<int>(type: "int", nullable: false),
                    CuentaVentasGravadasId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaVentasExentasId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaIvaDebitoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaIvaCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaClientesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaProveedoresId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaInventarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaCostoVentasId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaCajaGeneralId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaBancosColonesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaBancosDolaresId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaDifCambiariaGananciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaDifCambiariaPerdidaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaUtilidadEjercicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaPerdidaEjercicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaBancosColonesId",
                        column: x => x.CuentaBancosColonesId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaBancosDolaresId",
                        column: x => x.CuentaBancosDolaresId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaCajaGeneralId",
                        column: x => x.CuentaCajaGeneralId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaClientesId",
                        column: x => x.CuentaClientesId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaCostoVentasId",
                        column: x => x.CuentaCostoVentasId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaDifCambiariaGananciaId",
                        column: x => x.CuentaDifCambiariaGananciaId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaDifCambiariaPerdidaId",
                        column: x => x.CuentaDifCambiariaPerdidaId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaInventarioId",
                        column: x => x.CuentaInventarioId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaIvaCreditoId",
                        column: x => x.CuentaIvaCreditoId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaIvaDebitoId",
                        column: x => x.CuentaIvaDebitoId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaPerdidaEjercicioId",
                        column: x => x.CuentaPerdidaEjercicioId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaProveedoresId",
                        column: x => x.CuentaProveedoresId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaUtilidadEjercicioId",
                        column: x => x.CuentaUtilidadEjercicioId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaVentasExentasId",
                        column: x => x.CuentaVentasExentasId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_CuentasContables_CuentaVentasGravadasId",
                        column: x => x.CuentaVentasGravadasId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesContables_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuentasIntegracion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TipoOperacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConceptoContable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false, defaultValue: "D"),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CondicionAplicacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasIntegracion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasIntegracion_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasIntegracion_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasIntegracion_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasIntegracion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsientosContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodoContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoAsiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "DIA"),
                    Concepto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModuloOrigen = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DocumentoOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalDebe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalHaber = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "BOR"),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AprobadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientosContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsientosContables_AspNetUsers_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsientosContables_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsientosContables_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsientosContables_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsientosContables_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsientoContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Debe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Haber = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CentroCosto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tercero = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentoReferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosContables_AsientosContables_AsientoContableId",
                        column: x => x.AsientoContableId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosContables_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosContables_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosContables_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosFiscales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnioFiscal = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "ABT"),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CerradoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AsientoCierreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AsientoAperturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResultadoEjercicio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosFiscales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosFiscales_AsientosContables_AsientoAperturaId",
                        column: x => x.AsientoAperturaId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosFiscales_AsientosContables_AsientoCierreId",
                        column: x => x.AsientoCierreId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosFiscales_AspNetUsers_CerradoPorId",
                        column: x => x.CerradoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosFiscales_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosFiscales_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosFiscales_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosFiscales_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodoFiscalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "ABT"),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CerradoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UltimoNumeroAsiento = table.Column<int>(type: "int", nullable: false),
                    TotalDebe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalHaber = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadAsientos = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosContables_AspNetUsers_CerradoPorId",
                        column: x => x.CerradoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosContables_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosContables_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosContables_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosContables_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosContables_PeriodosFiscales_PeriodoFiscalId",
                        column: x => x.PeriodoFiscalId,
                        principalTable: "PeriodosFiscales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContable_DocumentoOrigenId",
                table: "AsientosContables",
                column: "DocumentoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContable_Empresa_Periodo_Numero",
                table: "AsientosContables",
                columns: new[] { "EmpresaId", "PeriodoContableId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContable_Estado",
                table: "AsientosContables",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContable_Fecha",
                table: "AsientosContables",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContable_ModuloOrigen",
                table: "AsientosContables",
                column: "ModuloOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_AsientoContable_TipoAsiento",
                table: "AsientosContables",
                column: "TipoAsiento");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_AprobadoPorId",
                table: "AsientosContables",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_CreadoPorId",
                table: "AsientosContables",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_ModificadoPorId",
                table: "AsientosContables",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_PeriodoContableId",
                table: "AsientosContables",
                column: "PeriodoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_UsuarioEliminacionId",
                table: "AsientosContables",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionContable_Empresa",
                table: "ConfiguracionesContables",
                column: "EmpresaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CreadoPorId",
                table: "ConfiguracionesContables",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaBancosColonesId",
                table: "ConfiguracionesContables",
                column: "CuentaBancosColonesId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaBancosDolaresId",
                table: "ConfiguracionesContables",
                column: "CuentaBancosDolaresId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaCajaGeneralId",
                table: "ConfiguracionesContables",
                column: "CuentaCajaGeneralId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaClientesId",
                table: "ConfiguracionesContables",
                column: "CuentaClientesId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaCostoVentasId",
                table: "ConfiguracionesContables",
                column: "CuentaCostoVentasId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaDifCambiariaGananciaId",
                table: "ConfiguracionesContables",
                column: "CuentaDifCambiariaGananciaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaDifCambiariaPerdidaId",
                table: "ConfiguracionesContables",
                column: "CuentaDifCambiariaPerdidaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaInventarioId",
                table: "ConfiguracionesContables",
                column: "CuentaInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaIvaCreditoId",
                table: "ConfiguracionesContables",
                column: "CuentaIvaCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaIvaDebitoId",
                table: "ConfiguracionesContables",
                column: "CuentaIvaDebitoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaPerdidaEjercicioId",
                table: "ConfiguracionesContables",
                column: "CuentaPerdidaEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaProveedoresId",
                table: "ConfiguracionesContables",
                column: "CuentaProveedoresId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaUtilidadEjercicioId",
                table: "ConfiguracionesContables",
                column: "CuentaUtilidadEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaVentasExentasId",
                table: "ConfiguracionesContables",
                column: "CuentaVentasExentasId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_CuentaVentasGravadasId",
                table: "ConfiguracionesContables",
                column: "CuentaVentasGravadasId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesContables_ModificadoPorId",
                table: "ConfiguracionesContables",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaContable_CuentaPadreId",
                table: "CuentasContables",
                column: "CuentaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaContable_Empresa_Codigo",
                table: "CuentasContables",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentaContable_Nivel",
                table: "CuentasContables",
                column: "Nivel");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaContable_TipoCuenta",
                table: "CuentasContables",
                column: "TipoCuenta");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_CreadoPorId",
                table: "CuentasContables",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_ModificadoPorId",
                table: "CuentasContables",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_UsuarioEliminacionId",
                table: "CuentasContables",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaIntegracion_CuentaContableId",
                table: "CuentasIntegracion",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaIntegracion_Empresa_Modulo_TipoOperacion_Concepto",
                table: "CuentasIntegracion",
                columns: new[] { "EmpresaId", "Modulo", "TipoOperacion", "ConceptoContable" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasIntegracion_CreadoPorId",
                table: "CuentasIntegracion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasIntegracion_ModificadoPorId",
                table: "CuentasIntegracion",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoContable_AsientoContableId",
                table: "MovimientosContables",
                column: "AsientoContableId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoContable_ClienteId",
                table: "MovimientosContables",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoContable_CuentaContableId",
                table: "MovimientosContables",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoContable_ProveedorId",
                table: "MovimientosContables",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodoContable_Empresa_AnioMes",
                table: "PeriodosContables",
                columns: new[] { "EmpresaId", "Anio", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodoContable_Estado",
                table: "PeriodosContables",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodoContable_PeriodoFiscalId",
                table: "PeriodosContables",
                column: "PeriodoFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosContables_CerradoPorId",
                table: "PeriodosContables",
                column: "CerradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosContables_CreadoPorId",
                table: "PeriodosContables",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosContables_ModificadoPorId",
                table: "PeriodosContables",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosContables_UsuarioEliminacionId",
                table: "PeriodosContables",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodoFiscal_Empresa_Anio",
                table: "PeriodosFiscales",
                columns: new[] { "EmpresaId", "AnioFiscal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodoFiscal_Estado",
                table: "PeriodosFiscales",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFiscales_AsientoAperturaId",
                table: "PeriodosFiscales",
                column: "AsientoAperturaId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFiscales_AsientoCierreId",
                table: "PeriodosFiscales",
                column: "AsientoCierreId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFiscales_CerradoPorId",
                table: "PeriodosFiscales",
                column: "CerradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFiscales_CreadoPorId",
                table: "PeriodosFiscales",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFiscales_ModificadoPorId",
                table: "PeriodosFiscales",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosFiscales_UsuarioEliminacionId",
                table: "PeriodosFiscales",
                column: "UsuarioEliminacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsientosContables_PeriodosContables_PeriodoContableId",
                table: "AsientosContables",
                column: "PeriodoContableId",
                principalTable: "PeriodosContables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsientosContables_PeriodosContables_PeriodoContableId",
                table: "AsientosContables");

            migrationBuilder.DropTable(
                name: "ConfiguracionesContables");

            migrationBuilder.DropTable(
                name: "CuentasIntegracion");

            migrationBuilder.DropTable(
                name: "MovimientosContables");

            migrationBuilder.DropTable(
                name: "CuentasContables");

            migrationBuilder.DropTable(
                name: "PeriodosContables");

            migrationBuilder.DropTable(
                name: "PeriodosFiscales");

            migrationBuilder.DropTable(
                name: "AsientosContables");
        }
    }
}
