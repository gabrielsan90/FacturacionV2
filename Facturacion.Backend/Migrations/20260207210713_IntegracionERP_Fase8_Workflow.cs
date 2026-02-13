using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracionERP_Fase8_Workflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposWorkflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiereAprobacion = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MontoMinimoAprobacion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoMaximoSinAprobacion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NotificarEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotificarSistema = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DiasEscalamiento = table.Column<int>(type: "int", nullable: true),
                    EscalamientoAutomatico = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_TiposWorkflow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposWorkflow_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TiposWorkflow_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TiposWorkflow_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TiposWorkflow_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NivelesAprobacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoWorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoAprobador = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "USU"),
                    UsuarioAprobadorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RolAprobador = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartamentoAprobadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MontoMinimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoMaximo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RequiereTodosAprobadores = table.Column<bool>(type: "bit", nullable: false),
                    AprobadoresMinimos = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PuedeRechazar = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PuedeDevolver = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DiasLimite = table.Column<int>(type: "int", nullable: true),
                    NivelEscalamientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotificarEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotificarSistema = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DiasRecordatorio = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_NivelesAprobacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_AspNetUsers_UsuarioAprobadorId",
                        column: x => x.UsuarioAprobadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_Departamentos_DepartamentoAprobadorId",
                        column: x => x.DepartamentoAprobadorId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_NivelesAprobacion_NivelEscalamientoId",
                        column: x => x.NivelEscalamientoId,
                        principalTable: "NivelesAprobacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NivelesAprobacion_TiposWorkflow_TipoWorkflowId",
                        column: x => x.TipoWorkflowId,
                        principalTable: "TiposWorkflow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesAprobacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoWorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuloOrigen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "CRC"),
                    MontoMonedaBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    NivelActualId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroNivel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TotalNiveles = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaUltimaAccion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SolicitanteId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DepartamentoSolicitanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Prioridad = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "NOR"),
                    EsUrgente = table.Column<bool>(type: "bit", nullable: false),
                    MotivoUrgencia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FueEscalada = table.Column<bool>(type: "bit", nullable: false),
                    FechaEscalamiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VecesEscalada = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SolicitudesAprobacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_AspNetUsers_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_AspNetUsers_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_Departamentos_DepartamentoSolicitanteId",
                        column: x => x.DepartamentoSolicitanteId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_NivelesAprobacion_NivelActualId",
                        column: x => x.NivelActualId,
                        principalTable: "NivelesAprobacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAprobacion_TiposWorkflow_TipoWorkflowId",
                        column: x => x.TipoWorkflowId,
                        principalTable: "TiposWorkflow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccionesAprobacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudAprobacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NivelAprobacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroNivel = table.Column<int>(type: "int", nullable: false),
                    TipoAccion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FechaAccion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EsDelegado = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioOriginalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    EstadoResultante = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    NivelSiguienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccionAutomatica = table.Column<bool>(type: "bit", nullable: false),
                    ReglaAplicada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DireccionIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReasignadoAId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MotivoReasignacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TiempoRespuestaHoras = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    DentroDelLimite = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NotificacionEnviada = table.Column<bool>(type: "bit", nullable: false),
                    FechaNotificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionesAprobacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccionesAprobacion_AspNetUsers_ReasignadoAId",
                        column: x => x.ReasignadoAId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccionesAprobacion_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccionesAprobacion_AspNetUsers_UsuarioOriginalId",
                        column: x => x.UsuarioOriginalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccionesAprobacion_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccionesAprobacion_NivelesAprobacion_NivelAprobacionId",
                        column: x => x.NivelAprobacionId,
                        principalTable: "NivelesAprobacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccionesAprobacion_NivelesAprobacion_NivelSiguienteId",
                        column: x => x.NivelSiguienteId,
                        principalTable: "NivelesAprobacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccionesAprobacion_SolicitudesAprobacion_SolicitudAprobacionId",
                        column: x => x.SolicitudAprobacionId,
                        principalTable: "SolicitudesAprobacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccionesAprobacion_EmpresaId",
                table: "AccionesAprobacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_AccionesAprobacion_NivelAprobacionId",
                table: "AccionesAprobacion",
                column: "NivelAprobacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccionesAprobacion_NivelSiguienteId",
                table: "AccionesAprobacion",
                column: "NivelSiguienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AccionesAprobacion_ReasignadoAId",
                table: "AccionesAprobacion",
                column: "ReasignadoAId");

            migrationBuilder.CreateIndex(
                name: "IX_AccionesAprobacion_SolicitudAprobacionId_FechaAccion",
                table: "AccionesAprobacion",
                columns: new[] { "SolicitudAprobacionId", "FechaAccion" });

            migrationBuilder.CreateIndex(
                name: "IX_AccionesAprobacion_UsuarioId_FechaAccion",
                table: "AccionesAprobacion",
                columns: new[] { "UsuarioId", "FechaAccion" });

            migrationBuilder.CreateIndex(
                name: "IX_AccionesAprobacion_UsuarioOriginalId",
                table: "AccionesAprobacion",
                column: "UsuarioOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_CreadoPorId",
                table: "NivelesAprobacion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_DepartamentoAprobadorId",
                table: "NivelesAprobacion",
                column: "DepartamentoAprobadorId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_EmpresaId",
                table: "NivelesAprobacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_ModificadoPorId",
                table: "NivelesAprobacion",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_NivelEscalamientoId",
                table: "NivelesAprobacion",
                column: "NivelEscalamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_TipoWorkflowId_Orden",
                table: "NivelesAprobacion",
                columns: new[] { "TipoWorkflowId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_UsuarioAprobadorId",
                table: "NivelesAprobacion",
                column: "UsuarioAprobadorId");

            migrationBuilder.CreateIndex(
                name: "IX_NivelesAprobacion_UsuarioEliminacionId",
                table: "NivelesAprobacion",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_CreadoPorId",
                table: "SolicitudesAprobacion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_DepartamentoSolicitanteId",
                table: "SolicitudesAprobacion",
                column: "DepartamentoSolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_EmpresaId_Estado",
                table: "SolicitudesAprobacion",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_EmpresaId_ModuloOrigen_DocumentoId",
                table: "SolicitudesAprobacion",
                columns: new[] { "EmpresaId", "ModuloOrigen", "DocumentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_ModificadoPorId",
                table: "SolicitudesAprobacion",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_NivelActualId",
                table: "SolicitudesAprobacion",
                column: "NivelActualId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_NumeroDocumento",
                table: "SolicitudesAprobacion",
                column: "NumeroDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_SolicitanteId",
                table: "SolicitudesAprobacion",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_TipoWorkflowId",
                table: "SolicitudesAprobacion",
                column: "TipoWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAprobacion_UsuarioEliminacionId",
                table: "SolicitudesAprobacion",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposWorkflow_CreadoPorId",
                table: "TiposWorkflow",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposWorkflow_EmpresaId_Codigo",
                table: "TiposWorkflow",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposWorkflow_ModificadoPorId",
                table: "TiposWorkflow",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposWorkflow_UsuarioEliminacionId",
                table: "TiposWorkflow",
                column: "UsuarioEliminacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccionesAprobacion");

            migrationBuilder.DropTable(
                name: "SolicitudesAprobacion");

            migrationBuilder.DropTable(
                name: "NivelesAprobacion");

            migrationBuilder.DropTable(
                name: "TiposWorkflow");
        }
    }
}
