using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase9_PresupuestosConciliacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CentroCostoId",
                table: "MovimientosContables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CentrosCosto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "COS"),
                    PadreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nivel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsableId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AceptaMovimientos = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentrosCosto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_AspNetUsers_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_CentrosCosto_PadreId",
                        column: x => x.PadreId,
                        principalTable: "CentrosCosto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CentrosCosto_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuentasBancarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BancoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreBanco = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TipoCuenta = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "CTE"),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "CRC"),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumeroClabe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SwiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Contacto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_Bancos_BancoId",
                        column: x => x.BancoId,
                        principalTable: "Bancos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AnioFiscal = table.Column<int>(type: "int", nullable: false),
                    TipoPresupuesto = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "ANU"),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "BOR"),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AprobadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PresupuestoBaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoEjecutado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Presupuestos_AspNetUsers_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Presupuestos_PresupuestoBaseId",
                        column: x => x.PresupuestoBaseId,
                        principalTable: "Presupuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConciliacionesBancarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CuentaBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaldoInicialLibros = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoFinalLibros = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoEstadoCuenta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepositosEnTransito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChequesEnTransito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NotasCredito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NotasDebito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    FechaConciliacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConciliadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConciliacionesBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConciliacionesBancarias_AspNetUsers_ConciliadoPorId",
                        column: x => x.ConciliadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConciliacionesBancarias_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConciliacionesBancarias_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConciliacionesBancarias_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConciliacionesBancarias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReglasConciliacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Prioridad = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CuentaBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompararMonto = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ToleranciaMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ToleranciaPorcentaje = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CompararFecha = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ToleranciaFechaDias = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    CompararReferencia = table.Column<bool>(type: "bit", nullable: false),
                    PatronReferencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompararDescripcion = table.Column<bool>(type: "bit", nullable: false),
                    PalabrasClaveDescripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AutoConciliar = table.Column<bool>(type: "bit", nullable: false),
                    CrearMovimientoFaltante = table.Column<bool>(type: "bit", nullable: false),
                    CuentaContableDefaultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoMovimientoDefault = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ConfianzaMinima = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 95m),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasConciliacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasConciliacion_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasConciliacion_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasConciliacion_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasConciliacion_CuentasContables_CuentaContableDefaultId",
                        column: x => x.CuentaContableDefaultId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReglasConciliacion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LineasPresupuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PresupuestoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CentroCostoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MontoEnero = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoFebrero = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoMarzo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoAbril = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoMayo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoJunio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoJulio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoAgosto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoSeptiembre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoOctubre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoNoviembre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoDiciembre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoEnero = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoFebrero = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoMarzo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoAbril = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoMayo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoJunio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoJulio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoAgosto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoSeptiembre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoOctubre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoNoviembre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EjecutadoDiciembre = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasPresupuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasPresupuesto_CentrosCosto_CentroCostoId",
                        column: x => x.CentroCostoId,
                        principalTable: "CentrosCosto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasPresupuesto_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasPresupuesto_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasPresupuesto_Presupuestos_PresupuestoId",
                        column: x => x.PresupuestoId,
                        principalTable: "Presupuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineasPresupuesto_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtractosBancarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoFinal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCreditos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDebitos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadTransacciones = table.Column<int>(type: "int", nullable: false),
                    FechaImportacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ArchivoOrigen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FormatoArchivo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    ConciliacionBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractosBancarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractosBancarios_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExtractosBancarios_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExtractosBancarios_ConciliacionesBancarias_ConciliacionBancariaId",
                        column: x => x.ConciliacionBancariaId,
                        principalTable: "ConciliacionesBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExtractosBancarios_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExtractosBancarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosBancarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CuentaBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "DEP"),
                    Naturaleza = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "CRE"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoNuevo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Beneficiario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumeroReferencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoDocumentoOrigen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentoOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Conciliado = table.Column<bool>(type: "bit", nullable: false),
                    FechaConciliacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConciliacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "REG"),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosBancarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosBancarios_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosBancarios_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosBancarios_ConciliacionesBancarias_ConciliacionId",
                        column: x => x.ConciliacionId,
                        principalTable: "ConciliacionesBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosBancarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LineasExtractoBancario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtractoBancarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaValor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenciaExterna = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Debito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoAcumulado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TipoTransaccion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    MovimientoBancarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstadoConciliacion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    FechaConciliacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConciliadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NotaConciliacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfianzaMatch = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ConciliacionAutomatica = table.Column<bool>(type: "bit", nullable: false),
                    ReglaConciliacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasExtractoBancario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasExtractoBancario_AspNetUsers_ConciliadoPorId",
                        column: x => x.ConciliadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasExtractoBancario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasExtractoBancario_ExtractosBancarios_ExtractoBancarioId",
                        column: x => x.ExtractoBancarioId,
                        principalTable: "ExtractosBancarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineasExtractoBancario_MovimientosBancarios_MovimientoBancarioId",
                        column: x => x.MovimientoBancarioId,
                        principalTable: "MovimientosBancarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasExtractoBancario_ReglasConciliacion_ReglaConciliacionId",
                        column: x => x.ReglaConciliacionId,
                        principalTable: "ReglasConciliacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosContables_CentroCostoId",
                table: "MovimientosContables",
                column: "CentroCostoId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_CreadoPorId",
                table: "CentrosCosto",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_DepartamentoId",
                table: "CentrosCosto",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_EmpresaId_Codigo",
                table: "CentrosCosto",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_ModificadoPorId",
                table: "CentrosCosto",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_PadreId",
                table: "CentrosCosto",
                column: "PadreId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_ResponsableId",
                table: "CentrosCosto",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_SucursalId",
                table: "CentrosCosto",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosCosto_UsuarioEliminacionId",
                table: "CentrosCosto",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_ConciliadoPorId",
                table: "ConciliacionesBancarias",
                column: "ConciliadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_CreadoPorId",
                table: "ConciliacionesBancarias",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_CuentaBancariaId_Anio_Mes",
                table: "ConciliacionesBancarias",
                columns: new[] { "CuentaBancariaId", "Anio", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_EmpresaId_Numero",
                table: "ConciliacionesBancarias",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionesBancarias_ModificadoPorId",
                table: "ConciliacionesBancarias",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_BancoId",
                table: "CuentasBancarias",
                column: "BancoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_CreadoPorId",
                table: "CuentasBancarias",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_CuentaContableId",
                table: "CuentasBancarias",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_EmpresaId_NumeroCuenta",
                table: "CuentasBancarias",
                columns: new[] { "EmpresaId", "NumeroCuenta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_ModificadoPorId",
                table: "CuentasBancarias",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_SucursalId",
                table: "CuentasBancarias",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_UsuarioEliminacionId",
                table: "CuentasBancarias",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_ConciliacionBancariaId",
                table: "ExtractosBancarios",
                column: "ConciliacionBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_CreadoPorId",
                table: "ExtractosBancarios",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_CuentaBancariaId_FechaInicio_FechaFin",
                table: "ExtractosBancarios",
                columns: new[] { "CuentaBancariaId", "FechaInicio", "FechaFin" });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_EmpresaId_Numero",
                table: "ExtractosBancarios",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtractosBancarios_ModificadoPorId",
                table: "ExtractosBancarios",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasExtractoBancario_ConciliadoPorId",
                table: "LineasExtractoBancario",
                column: "ConciliadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasExtractoBancario_EmpresaId",
                table: "LineasExtractoBancario",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasExtractoBancario_ExtractoBancarioId_EstadoConciliacion",
                table: "LineasExtractoBancario",
                columns: new[] { "ExtractoBancarioId", "EstadoConciliacion" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasExtractoBancario_ExtractoBancarioId_NumeroLinea",
                table: "LineasExtractoBancario",
                columns: new[] { "ExtractoBancarioId", "NumeroLinea" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasExtractoBancario_MovimientoBancarioId",
                table: "LineasExtractoBancario",
                column: "MovimientoBancarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasExtractoBancario_ReglaConciliacionId",
                table: "LineasExtractoBancario",
                column: "ReglaConciliacionId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasPresupuesto_CentroCostoId",
                table: "LineasPresupuesto",
                column: "CentroCostoId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasPresupuesto_CuentaContableId",
                table: "LineasPresupuesto",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasPresupuesto_EmpresaId",
                table: "LineasPresupuesto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasPresupuesto_PresupuestoId_CuentaContableId_CentroCostoId",
                table: "LineasPresupuesto",
                columns: new[] { "PresupuestoId", "CuentaContableId", "CentroCostoId" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasPresupuesto_SucursalId",
                table: "LineasPresupuesto",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_ConciliacionId",
                table: "MovimientosBancarios",
                column: "ConciliacionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_CreadoPorId",
                table: "MovimientosBancarios",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_CuentaBancariaId_Conciliado",
                table: "MovimientosBancarios",
                columns: new[] { "CuentaBancariaId", "Conciliado" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_CuentaBancariaId_Fecha",
                table: "MovimientosBancarios",
                columns: new[] { "CuentaBancariaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_EmpresaId_Numero",
                table: "MovimientosBancarios",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_ModificadoPorId",
                table: "MovimientosBancarios",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_AprobadoPorId",
                table: "Presupuestos",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_CreadoPorId",
                table: "Presupuestos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_EmpresaId_AnioFiscal",
                table: "Presupuestos",
                columns: new[] { "EmpresaId", "AnioFiscal" });

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_EmpresaId_Codigo",
                table: "Presupuestos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_ModificadoPorId",
                table: "Presupuestos",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_PresupuestoBaseId",
                table: "Presupuestos",
                column: "PresupuestoBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_UsuarioEliminacionId",
                table: "Presupuestos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasConciliacion_CreadoPorId",
                table: "ReglasConciliacion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasConciliacion_CuentaBancariaId",
                table: "ReglasConciliacion",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasConciliacion_CuentaContableDefaultId",
                table: "ReglasConciliacion",
                column: "CuentaContableDefaultId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasConciliacion_EmpresaId_Nombre",
                table: "ReglasConciliacion",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReglasConciliacion_EmpresaId_Prioridad",
                table: "ReglasConciliacion",
                columns: new[] { "EmpresaId", "Prioridad" });

            migrationBuilder.CreateIndex(
                name: "IX_ReglasConciliacion_ModificadoPorId",
                table: "ReglasConciliacion",
                column: "ModificadoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosContables_CentrosCosto_CentroCostoId",
                table: "MovimientosContables",
                column: "CentroCostoId",
                principalTable: "CentrosCosto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosContables_CentrosCosto_CentroCostoId",
                table: "MovimientosContables");

            migrationBuilder.DropTable(
                name: "LineasExtractoBancario");

            migrationBuilder.DropTable(
                name: "LineasPresupuesto");

            migrationBuilder.DropTable(
                name: "ExtractosBancarios");

            migrationBuilder.DropTable(
                name: "MovimientosBancarios");

            migrationBuilder.DropTable(
                name: "ReglasConciliacion");

            migrationBuilder.DropTable(
                name: "CentrosCosto");

            migrationBuilder.DropTable(
                name: "Presupuestos");

            migrationBuilder.DropTable(
                name: "ConciliacionesBancarias");

            migrationBuilder.DropTable(
                name: "CuentasBancarias");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosContables_CentroCostoId",
                table: "MovimientosContables");

            migrationBuilder.DropColumn(
                name: "CentroCostoId",
                table: "MovimientosContables");
        }
    }
}
