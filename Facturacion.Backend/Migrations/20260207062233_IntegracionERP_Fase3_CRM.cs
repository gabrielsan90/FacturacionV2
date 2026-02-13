using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase3_CRM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competidores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Fortalezas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Debilidades = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competidores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Competidores_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Competidores_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Competidores_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Competidores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EtapasPipeline",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    ProbabilidadSugerida = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    EsFinal = table.Column<bool>(type: "bit", nullable: false),
                    EsGanada = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtapasPipeline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtapasPipeline_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EtapasPipeline_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EtapasPipeline_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EtapasPipeline_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Oportunidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreContacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmailContacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MontoEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProbabilidadCierre = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 50m),
                    EtapaPipelineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CodigoEtapa = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PRO"),
                    FechaCierreEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCierreReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VendedorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    OrigenDetalle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MotivoGanoPerdido = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompetidorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Prioridad = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "MED"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oportunidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oportunidades_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oportunidades_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oportunidades_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oportunidades_AspNetUsers_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oportunidades_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oportunidades_Competidores_CompetidorId",
                        column: x => x.CompetidorId,
                        principalTable: "Competidores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oportunidades_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oportunidades_EtapasPipeline_EtapaPipelineId",
                        column: x => x.EtapaPipelineId,
                        principalTable: "EtapasPipeline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActividadesCRM",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoActividad = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OportunidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    FechaRealizada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AsignadoAId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    Prioridad = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "MED"),
                    Resultado = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recordatorio = table.Column<bool>(type: "bit", nullable: false),
                    MinutosAntes = table.Column<int>(type: "int", nullable: true),
                    ReminderEnviado = table.Column<bool>(type: "bit", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadesCRM", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActividadesCRM_AspNetUsers_AsignadoAId",
                        column: x => x.AsignadoAId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActividadesCRM_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActividadesCRM_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActividadesCRM_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActividadesCRM_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActividadesCRM_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActividadesCRM_Oportunidades_OportunidadId",
                        column: x => x.OportunidadId,
                        principalTable: "Oportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialEtapasOportunidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OportunidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EtapaAnterior = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    EtapaNueva = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    EtapaAnteriorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EtapaNuevaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MontoAlCambio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProbabilidadAlCambio = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiasEnEtapaAnterior = table.Column<int>(type: "int", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CambiadoPorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEtapasOportunidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEtapasOportunidad_AspNetUsers_CambiadoPorId",
                        column: x => x.CambiadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEtapasOportunidad_EtapasPipeline_EtapaAnteriorId",
                        column: x => x.EtapaAnteriorId,
                        principalTable: "EtapasPipeline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEtapasOportunidad_EtapasPipeline_EtapaNuevaId",
                        column: x => x.EtapaNuevaId,
                        principalTable: "EtapasPipeline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialEtapasOportunidad_Oportunidades_OportunidadId",
                        column: x => x.OportunidadId,
                        principalTable: "Oportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotasOportunidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OportunidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    EsImportante = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasOportunidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasOportunidad_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasOportunidad_Oportunidades_OportunidadId",
                        column: x => x.OportunidadId,
                        principalTable: "Oportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadCRM_AsignadoAId",
                table: "ActividadesCRM",
                column: "AsignadoAId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadCRM_ClienteId",
                table: "ActividadesCRM",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadCRM_Empresa_FechaProgramada",
                table: "ActividadesCRM",
                columns: new[] { "EmpresaId", "FechaProgramada" });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadCRM_Estado",
                table: "ActividadesCRM",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadCRM_OportunidadId",
                table: "ActividadesCRM",
                column: "OportunidadId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesCRM_UsuarioCreacionId",
                table: "ActividadesCRM",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesCRM_UsuarioEliminacionId",
                table: "ActividadesCRM",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesCRM_UsuarioModificacionId",
                table: "ActividadesCRM",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Competidor_Empresa_Nombre",
                table: "Competidores",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Competidores_UsuarioCreacionId",
                table: "Competidores",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Competidores_UsuarioEliminacionId",
                table: "Competidores",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Competidores_UsuarioModificacionId",
                table: "Competidores",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EtapaPipeline_Empresa_Codigo",
                table: "EtapasPipeline",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtapasPipeline_UsuarioCreacionId",
                table: "EtapasPipeline",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EtapasPipeline_UsuarioEliminacionId",
                table: "EtapasPipeline",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EtapasPipeline_UsuarioModificacionId",
                table: "EtapasPipeline",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEtapaOportunidad_FechaCambio",
                table: "HistorialEtapasOportunidad",
                column: "FechaCambio");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEtapaOportunidad_OportunidadId",
                table: "HistorialEtapasOportunidad",
                column: "OportunidadId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEtapasOportunidad_CambiadoPorId",
                table: "HistorialEtapasOportunidad",
                column: "CambiadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEtapasOportunidad_EtapaAnteriorId",
                table: "HistorialEtapasOportunidad",
                column: "EtapaAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEtapasOportunidad_EtapaNuevaId",
                table: "HistorialEtapasOportunidad",
                column: "EtapaNuevaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotaOportunidad_OportunidadId",
                table: "NotasOportunidad",
                column: "OportunidadId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasOportunidad_UsuarioCreacionId",
                table: "NotasOportunidad",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidad_ClienteId",
                table: "Oportunidades",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidad_CodigoEtapa",
                table: "Oportunidades",
                column: "CodigoEtapa");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidad_Empresa_Numero",
                table: "Oportunidades",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidad_VendedorId",
                table: "Oportunidades",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidades_CompetidorId",
                table: "Oportunidades",
                column: "CompetidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidades_EtapaPipelineId",
                table: "Oportunidades",
                column: "EtapaPipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidades_UsuarioCreacionId",
                table: "Oportunidades",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidades_UsuarioEliminacionId",
                table: "Oportunidades",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Oportunidades_UsuarioModificacionId",
                table: "Oportunidades",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActividadesCRM");

            migrationBuilder.DropTable(
                name: "HistorialEtapasOportunidad");

            migrationBuilder.DropTable(
                name: "NotasOportunidad");

            migrationBuilder.DropTable(
                name: "Oportunidades");

            migrationBuilder.DropTable(
                name: "Competidores");

            migrationBuilder.DropTable(
                name: "EtapasPipeline");
        }
    }
}
