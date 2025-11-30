using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHaciendaDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroConsecutivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoDocumento = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ActividadEconomica = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceptorTipoIdentificacion = table.Column<int>(type: "int", nullable: true),
                    ReceptorNumeroIdentificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReceptorNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReceptorNombreComercial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReceptorActividadEconomica = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    ReceptorProvincia = table.Column<int>(type: "int", nullable: true),
                    ReceptorCanton = table.Column<int>(type: "int", nullable: true),
                    ReceptorDistrito = table.Column<int>(type: "int", nullable: true),
                    ReceptorBarrio = table.Column<int>(type: "int", nullable: true),
                    ReceptorOtrasSenas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceptorEmails = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ReceptorTelefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CondicionVenta = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    PlazoCreditoDias = table.Column<int>(type: "int", nullable: true),
                    MedioPago = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    TotalServiciosGravados = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalServiciosExentos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalServiciosExonerados = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalMercanciasGravadas = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalMercanciasExentas = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalMercanciasExoneradas = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalGravado = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalExento = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalExonerado = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalDescuentos = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalImpuestos = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    IVADevuelto = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    TotalOtrosCargos = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TotalVenta = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    XmlGenerado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlFirmado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEnvioHacienda = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRespuestaHacienda = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MensajeHacienda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlRespuestaHacienda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsContingencia = table.Column<bool>(type: "bit", nullable: false),
                    FechaResolucionContingencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PDF = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_Terminales_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Terminales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoDescuentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NaturalezaDescuento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoDescuentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoDescuentos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDescuentos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDescuentos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDescuentos_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoCodigo = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodigoCabys = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnidadMedidaId = table.Column<int>(type: "int", nullable: false),
                    UnidadMedidaComercial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    NaturalezaDescuento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    BaseImponible = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    MontoImpuesto = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    ImpuestoNeto = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    MontoTotalLinea = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    NumeroPartidaArancelaria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroRegistroMedicamento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FormaFarmaceutica = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    NumeroVIN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalles_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalles_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalles_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalles_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalles_UnidadesMedida_UnidadMedidaId",
                        column: x => x.UnidadMedidaId,
                        principalTable: "UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoExportaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreComprador = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdentificacionComprador = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Incoterm = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DescripcionIncoterm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroDUA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaDUA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MedioTransporte = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Transportista = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroTransporte = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PuertoSalida = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PuertoDestino = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroFacturaComercial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroPackingList = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroBL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalFOB = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    Flete = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    Seguro = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    TotalCIF = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoExportaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoExportaciones_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoExportaciones_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoExportaciones_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoExportaciones_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoMediosPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoMedioPago = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    MedioPagoId = table.Column<int>(type: "int", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroReferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoMediosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoMediosPago_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoMediosPago_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoMediosPago_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoMediosPago_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentoMediosPago_MediosPago_MedioPagoId",
                        column: x => x.MedioPagoId,
                        principalTable: "MediosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoOtraInformacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoOtraInformacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoOtraInformacion_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoOtraInformacion_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoOtraInformacion_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoOtraInformacion_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoReceptorMensajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoOriginalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaveMensaje = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroConsecutivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoMensaje = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodigoMensaje = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DetalleMensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MontoTotalImpuestoAceptado = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    MontoTotalAceptado = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    XmlGenerado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlFirmado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEnvioHacienda = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRespuestaHacienda = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MensajeHacienda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoReceptorMensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoReceptorMensajes_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoReceptorMensajes_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoReceptorMensajes_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoReceptorMensajes_Documentos_DocumentoOriginalId",
                        column: x => x.DocumentoOriginalId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoReferencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumentoReferenciado = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    NumeroDocumentoReferenciado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEmisionDocumentoReferenciado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClaveDocumentoReferenciado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodigoReferencia = table.Column<int>(type: "int", nullable: false),
                    RazonReferencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentoReferenciadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoReferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoReferencias_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoReferencias_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoReferencias_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoReferencias_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentoReferencias_Documentos_DocumentoReferenciadoId",
                        column: x => x.DocumentoReferenciadoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoDetalleDescuentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NaturalezaDescuento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoDetalleDescuentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleDescuentos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleDescuentos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleDescuentos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleDescuentos_DocumentoDetalles_DocumentoDetalleId",
                        column: x => x.DocumentoDetalleId,
                        principalTable: "DocumentoDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoDetalleImpuestos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImpuestoId = table.Column<int>(type: "int", nullable: false),
                    CodigoImpuesto = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CodigoTarifa = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Tarifa = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FactorIVADevuelto = table.Column<decimal>(type: "decimal(5,2)", precision: 18, scale: 5, nullable: true),
                    MontoBase = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    MontoImpuesto = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: false),
                    TieneExoneracion = table.Column<bool>(type: "bit", nullable: false),
                    TipoDocumentoExoneracion = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    NumeroDocumentoExoneracion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InstitucionExoneracion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaEmisionExoneracion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoExoneracion = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    PorcentajeExoneracion = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioEliminacionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoDetalleImpuestos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleImpuestos_AspNetUsers_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleImpuestos_AspNetUsers_UsuarioEliminacionId",
                        column: x => x.UsuarioEliminacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleImpuestos_AspNetUsers_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleImpuestos_DocumentoDetalles_DocumentoDetalleId",
                        column: x => x.DocumentoDetalleId,
                        principalTable: "DocumentoDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentoDetalleImpuestos_Impuestos_ImpuestoId",
                        column: x => x.ImpuestoId,
                        principalTable: "Impuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDescuento_DocumentoId",
                table: "DocumentoDescuentos",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDescuentos_UsuarioCreacionId",
                table: "DocumentoDescuentos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDescuentos_UsuarioEliminacionId",
                table: "DocumentoDescuentos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDescuentos_UsuarioModificacionId",
                table: "DocumentoDescuentos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleDescuento_DetalleId",
                table: "DocumentoDetalleDescuentos",
                column: "DocumentoDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleDescuentos_UsuarioCreacionId",
                table: "DocumentoDetalleDescuentos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleDescuentos_UsuarioEliminacionId",
                table: "DocumentoDetalleDescuentos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleDescuentos_UsuarioModificacionId",
                table: "DocumentoDetalleDescuentos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleImpuesto_DetalleId",
                table: "DocumentoDetalleImpuestos",
                column: "DocumentoDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleImpuestos_ImpuestoId",
                table: "DocumentoDetalleImpuestos",
                column: "ImpuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleImpuestos_UsuarioCreacionId",
                table: "DocumentoDetalleImpuestos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleImpuestos_UsuarioEliminacionId",
                table: "DocumentoDetalleImpuestos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalleImpuestos_UsuarioModificacionId",
                table: "DocumentoDetalleImpuestos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalle_Documento_Linea",
                table: "DocumentoDetalles",
                columns: new[] { "DocumentoId", "NumeroLinea" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalle_DocumentoId",
                table: "DocumentoDetalles",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalles_ProductoId",
                table: "DocumentoDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalles_UnidadMedidaId",
                table: "DocumentoDetalles",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalles_UsuarioCreacionId",
                table: "DocumentoDetalles",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalles_UsuarioEliminacionId",
                table: "DocumentoDetalles",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoDetalles_UsuarioModificacionId",
                table: "DocumentoDetalles",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoExportacion_DocumentoId",
                table: "DocumentoExportaciones",
                column: "DocumentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoExportaciones_UsuarioCreacionId",
                table: "DocumentoExportaciones",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoExportaciones_UsuarioEliminacionId",
                table: "DocumentoExportaciones",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoExportaciones_UsuarioModificacionId",
                table: "DocumentoExportaciones",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoMedioPago_DocumentoId",
                table: "DocumentoMediosPago",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoMediosPago_MedioPagoId",
                table: "DocumentoMediosPago",
                column: "MedioPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoMediosPago_UsuarioCreacionId",
                table: "DocumentoMediosPago",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoMediosPago_UsuarioEliminacionId",
                table: "DocumentoMediosPago",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoMediosPago_UsuarioModificacionId",
                table: "DocumentoMediosPago",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoOtraInformacion_DocumentoId",
                table: "DocumentoOtraInformacion",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoOtraInformacion_UsuarioCreacionId",
                table: "DocumentoOtraInformacion",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoOtraInformacion_UsuarioEliminacionId",
                table: "DocumentoOtraInformacion",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoOtraInformacion_UsuarioModificacionId",
                table: "DocumentoOtraInformacion",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReceptorMensaje_Clave",
                table: "DocumentoReceptorMensajes",
                column: "ClaveMensaje",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReceptorMensaje_DocumentoOriginal",
                table: "DocumentoReceptorMensajes",
                column: "DocumentoOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReceptorMensajes_UsuarioCreacionId",
                table: "DocumentoReceptorMensajes",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReceptorMensajes_UsuarioEliminacionId",
                table: "DocumentoReceptorMensajes",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReceptorMensajes_UsuarioModificacionId",
                table: "DocumentoReceptorMensajes",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReferencia_DocumentoId",
                table: "DocumentoReferencias",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReferencia_NumeroReferenciado",
                table: "DocumentoReferencias",
                column: "NumeroDocumentoReferenciado");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReferencias_DocumentoReferenciadoId",
                table: "DocumentoReferencias",
                column: "DocumentoReferenciadoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReferencias_UsuarioCreacionId",
                table: "DocumentoReferencias",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReferencias_UsuarioEliminacionId",
                table: "DocumentoReferencias",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoReferencias_UsuarioModificacionId",
                table: "DocumentoReferencias",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_Clave",
                table: "Documentos",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documento_Empresa_Tipo_Estado",
                table: "Documentos",
                columns: new[] { "EmpresaId", "TipoDocumento", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Documento_EmpresaId",
                table: "Documentos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_FechaEmision",
                table: "Documentos",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_NumeroConsecutivo",
                table: "Documentos",
                column: "NumeroConsecutivo");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_ClienteId",
                table: "Documentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_ProveedorId",
                table: "Documentos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_SucursalId",
                table: "Documentos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_TerminalId",
                table: "Documentos",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_UsuarioCreacionId",
                table: "Documentos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_UsuarioEliminacionId",
                table: "Documentos",
                column: "UsuarioEliminacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_UsuarioModificacionId",
                table: "Documentos",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentoDescuentos");

            migrationBuilder.DropTable(
                name: "DocumentoDetalleDescuentos");

            migrationBuilder.DropTable(
                name: "DocumentoDetalleImpuestos");

            migrationBuilder.DropTable(
                name: "DocumentoExportaciones");

            migrationBuilder.DropTable(
                name: "DocumentoMediosPago");

            migrationBuilder.DropTable(
                name: "DocumentoOtraInformacion");

            migrationBuilder.DropTable(
                name: "DocumentoReceptorMensajes");

            migrationBuilder.DropTable(
                name: "DocumentoReferencias");

            migrationBuilder.DropTable(
                name: "DocumentoDetalles");

            migrationBuilder.DropTable(
                name: "Documentos");
        }
    }
}
