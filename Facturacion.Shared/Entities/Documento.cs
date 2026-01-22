using Facturacion.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Entidad principal para documentos electrónicos según Hacienda v4.4
/// Soporta todos los tipos: FE, ND, NC, TE, FEC, FEE, CCE
/// Resolución MH-DGT-RES-0027-2024
/// </summary>
public class Documento
{
    [Key]
    public Guid Id { get; set; }

    // ========================================
    // RELACIONES CON ARQUITECTURA MULTI-TENANT
    // ========================================

    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    [Display(Name = "Sucursal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid SucursalId { get; set; }

    [Display(Name = "Terminal")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid TerminalId { get; set; }

    // ========================================
    // CLAVE Y CONSECUTIVO (HACIENDA)
    // ========================================

    /// <summary>
    /// Clave numérica de 50 dígitos según formato Hacienda
    /// Formato: CCPPPPDDDDDDDDDDSSSTTTNNNNNNNNNNNNNNNNNNNNSSSSSSSSC
    /// </summary>
    [Display(Name = "Clave")]
    [MaxLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [MinLength(50, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Clave { get; set; } = null!;

    /// <summary>
    /// Número consecutivo de 20 dígitos
    /// Formato: XXX-YYYYY-ZZ-AAAAAAAAAA
    /// XXX = Código de sucursal (3 dígitos)
    /// YYYYY = Código de terminal (5 dígitos)
    /// ZZ = Tipo de documento (2 dígitos)
    /// AAAAAAAAAA = Consecutivo (10 dígitos)
    /// </summary>
    [Display(Name = "Número Consecutivo")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string NumeroConsecutivo { get; set; } = null!;

    // ========================================
    // TIPO DE DOCUMENTO Y ESTADO
    // ========================================

    [Display(Name = "Tipo de Documento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DocumentoTipo TipoDocumento { get; set; }

    [Display(Name = "Estado")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public EstadoDocumento Estado { get; set; }

    /// <summary>
    /// Indica si es un documento recibido (true) o emitido (false)
    /// Documentos emitidos: generados por la empresa (ventas)
    /// Documentos recibidos: recibidos de proveedores (compras)
    /// </summary>
    [Display(Name = "Documento Recibido")]
    public bool EsDocumentoRecibido { get; set; } = false;

    /// <summary>
    /// Ambiente de Hacienda en el que se emitió el documento
    /// stag = Pruebas/Sandbox, prod = Producción
    /// </summary>
    [Display(Name = "Ambiente")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Ambiente Ambiente { get; set; }

    // ========================================
    // ACTIVIDAD ECONÓMICA (v4.4)
    // ========================================

    /// <summary>
    /// Código de actividad económica del emisor (6 dígitos CIIU4 desde 31/08/2025)
    /// Obligatorio en v4.4
    /// </summary>
    [Display(Name = "Actividad Económica Emisor")]
    [MaxLength(6, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ActividadEconomica { get; set; } = null!;

    // ========================================
    // FECHAS
    // ========================================

    [Display(Name = "Fecha de Emisión")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaEmision { get; set; }

    // ========================================
    // EMISOR (Datos de Empresa + Sucursal + Terminal)
    // ========================================
    // Los datos del emisor se obtienen de las relaciones Empresa, Sucursal, Terminal
    // No se duplican aquí para evitar redundancia

    // ========================================
    // RECEPTOR (Cliente o Proveedor)
    // ========================================

    /// <summary>
    /// Para documentos de venta: ClienteId
    /// Para documentos de compra: ProveedorId
    /// </summary>
    [Display(Name = "Cliente")]
    public Guid? ClienteId { get; set; }

    [Display(Name = "Proveedor")]
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Tipo de identificación del receptor (duplicado para histórico)
    /// </summary>
    [Display(Name = "Receptor - Tipo de Identificación")]
    public TipoIdentificacion? ReceptorTipoIdentificacion { get; set; }

    /// <summary>
    /// Número de identificación del receptor (duplicado para histórico)
    /// </summary>
    [Display(Name = "Receptor - Número de Identificación")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReceptorNumeroIdentificacion { get; set; }

    /// <summary>
    /// Nombre del receptor (duplicado para histórico)
    /// </summary>
    [Display(Name = "Receptor - Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReceptorNombre { get; set; }

    /// <summary>
    /// Nombre comercial del receptor (duplicado para histórico)
    /// </summary>
    [Display(Name = "Receptor - Nombre Comercial")]
    [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReceptorNombreComercial { get; set; }

    /// <summary>
    /// NUEVO en v4.4: Actividad económica del receptor (OBLIGATORIO en facturas desde 01/04/2025)
    /// </summary>
    [Display(Name = "Receptor - Actividad Económica")]
    [MaxLength(6, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReceptorActividadEconomica { get; set; }

    // Ubicación del receptor (duplicado para histórico)
    [Display(Name = "Receptor - Provincia")]
    public int? ReceptorProvincia { get; set; }

    [Display(Name = "Receptor - Cantón")]
    public int? ReceptorCanton { get; set; }

    [Display(Name = "Receptor - Distrito")]
    public int? ReceptorDistrito { get; set; }

    [Display(Name = "Receptor - Barrio")]
    public int? ReceptorBarrio { get; set; }

    [Display(Name = "Receptor - Otras Señas")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReceptorOtrasSenas { get; set; }

    /// <summary>
    /// NUEVO en v4.4: Hasta 4 emails permitidos (separados por coma)
    /// </summary>
    [Display(Name = "Receptor - Emails")]
    [MaxLength(400, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReceptorEmails { get; set; }

    [Display(Name = "Receptor - Teléfono")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? ReceptorTelefono { get; set; }

    // ========================================
    // CONDICIÓN DE VENTA
    // ========================================

    /// <summary>
    /// Código de condición de venta según catálogo Hacienda
    /// 01-Contado, 02-Crédito, 03-Consignación, 04-Apartado, 05-Arrendamiento con opción de compra,
    /// 06-Arrendamiento en función financiera, 07-Cobro a favor de un tercero,
    /// 08-Servicios prestados al Estado a crédito, 09-Pago del servicios prestado al Estado,
    /// 10-Mercancía no nacionalizada (NUEVO v4.4), 99-Otros
    /// </summary>
    [Display(Name = "Condición de Venta")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string CondicionVenta { get; set; } = null!;

    /// <summary>
    /// Plazo de crédito en días (si CondicionVenta = 02)
    /// </summary>
    [Display(Name = "Plazo de Crédito (días)")]
    public int? PlazoCreditoDias { get; set; }

    /// <summary>
    /// Número de orden de compra del receptor (opcional)
    /// Usado para relacionar la factura con una orden de compra del cliente
    /// </summary>
    [Display(Name = "Número Orden de Compra")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroOrdenCompra { get; set; }

    // ========================================
    // MEDIO DE PAGO
    // ========================================

    /// <summary>
    /// Código de medio de pago según catálogo Hacienda
    /// 01-Efectivo, 02-Tarjeta, 03-Cheque, 04-Transferencia, 05-Recaudado por terceros,
    /// 06-SINPE Móvil (NUEVO v4.4), 99-Otros
    /// </summary>
    [Display(Name = "Medio de Pago Principal")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string MedioPago { get; set; } = null!;

    // ========================================
    // MONEDA Y TIPO DE CAMBIO
    // ========================================

    [Display(Name = "Moneda")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public TipoMoneda Moneda { get; set; }

    /// <summary>
    /// Tipo de cambio (obligatorio si Moneda != CRC)
    /// Hasta 5 decimales según Hacienda
    /// </summary>
    [Display(Name = "Tipo de Cambio")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? TipoCambio { get; set; }

    // ========================================
    // TOTALES DEL DOCUMENTO
    // ========================================

    /// <summary>
    /// Total de servicios gravados (con impuesto)
    /// </summary>
    [Display(Name = "Total Servicios Gravados")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalServiciosGravados { get; set; }

    /// <summary>
    /// Total de servicios exentos (sin impuesto)
    /// </summary>
    [Display(Name = "Total Servicios Exentos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalServiciosExentos { get; set; }

    /// <summary>
    /// Total de servicios exonerados
    /// </summary>
    [Display(Name = "Total Servicios Exonerados")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalServiciosExonerados { get; set; }

    /// <summary>
    /// Total de mercancías gravadas (con impuesto)
    /// </summary>
    [Display(Name = "Total Mercancías Gravadas")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalMercanciasGravadas { get; set; }

    /// <summary>
    /// Total de mercancías exentas (sin impuesto)
    /// </summary>
    [Display(Name = "Total Mercancías Exentas")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalMercanciasExentas { get; set; }

    /// <summary>
    /// Total de mercancías exoneradas
    /// </summary>
    [Display(Name = "Total Mercancías Exoneradas")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalMercanciasExoneradas { get; set; }

    /// <summary>
    /// Total gravado (suma de servicios y mercancías gravadas)
    /// </summary>
    [Display(Name = "Total Gravado")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalGravado { get; set; }

    /// <summary>
    /// Total exento (suma de servicios y mercancías exentas)
    /// </summary>
    [Display(Name = "Total Exento")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalExento { get; set; }

    /// <summary>
    /// Total exonerado (suma de servicios y mercancías exoneradas)
    /// </summary>
    [Display(Name = "Total Exonerado")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalExonerado { get; set; }

    /// <summary>
    /// Subtotal (antes de impuestos y descuentos)
    /// </summary>
    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Total de descuentos aplicados al documento
    /// </summary>
    [Display(Name = "Total Descuentos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalDescuentos { get; set; }

    /// <summary>
    /// Total de impuestos (suma de todos los impuestos)
    /// </summary>
    [Display(Name = "Total Impuestos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalImpuestos { get; set; }

    /// <summary>
    /// Monto total de IVA devuelto (para casos específicos)
    /// </summary>
    [Display(Name = "IVA Devuelto")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal? IVADevuelto { get; set; }

    /// <summary>
    /// Total de otros cargos
    /// </summary>
    [Display(Name = "Total Otros Cargos")]
    [Column(TypeName = "decimal(18, 5)")]
    public decimal TotalOtrosCargos { get; set; }

    /// <summary>
    /// Total de venta (Subtotal - Descuentos + Impuestos + Otros Cargos)
    /// </summary>
    [Display(Name = "Total Venta")]
    [Column(TypeName = "decimal(18, 5)")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public decimal TotalVenta { get; set; }

    // ========================================
    // INFORMACIÓN ADICIONAL
    // ========================================

    /// <summary>
    /// Observaciones o notas del documento
    /// </summary>
    [Display(Name = "Observaciones")]
    [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    // ========================================
    // XML Y FIRMA DIGITAL
    // ========================================

    /// <summary>
    /// XML generado del documento (sin firma)
    /// </summary>
    [Display(Name = "XML Generado")]
    public string? XmlGenerado { get; set; }

    /// <summary>
    /// XML firmado digitalmente (XAdES-BES)
    /// </summary>
    [Display(Name = "XML Firmado")]
    public string? XmlFirmado { get; set; }

    /// <summary>
    /// Fecha de firma digital
    /// </summary>
    [Display(Name = "Fecha de Firma")]
    public DateTime? FechaFirma { get; set; }

    // ========================================
    // COMUNICACIÓN CON HACIENDA
    // ========================================

    /// <summary>
    /// Fecha de envío a Hacienda
    /// </summary>
    [Display(Name = "Fecha de Envío a Hacienda")]
    public DateTime? FechaEnvioHacienda { get; set; }

    /// <summary>
    /// Fecha de respuesta de Hacienda
    /// </summary>
    [Display(Name = "Fecha de Respuesta Hacienda")]
    public DateTime? FechaRespuestaHacienda { get; set; }

    /// <summary>
    /// Mensaje de respuesta de Hacienda (resumen de texto)
    /// </summary>
    [Display(Name = "Mensaje Hacienda")]
    public string? MensajeHacienda { get; set; }

    /// <summary>
    /// Lista de mensajes estructurados de Hacienda (errores, advertencias, info)
    /// Almacenado como JSON para preservar la estructura completa
    /// Contiene: Codigo, Mensaje, Detalle, Tipo
    /// </summary>
    [Display(Name = "Mensajes Hacienda (JSON)")]
    public string? MensajesHaciendaJson { get; set; }

    /// <summary>
    /// XML de respuesta de Hacienda
    /// </summary>
    [Display(Name = "XML Respuesta Hacienda")]
    public string? XmlRespuestaHacienda { get; set; }

    // ========================================
    // CONTINGENCIA
    // ========================================

    /// <summary>
    /// Indica si el documento fue generado en modo contingencia
    /// </summary>
    [Display(Name = "Es Contingencia")]
    public bool EsContingencia { get; set; }

    /// <summary>
    /// Fecha de resolución de contingencia (cuando se envió a Hacienda)
    /// </summary>
    [Display(Name = "Fecha Resolución Contingencia")]
    public DateTime? FechaResolucionContingencia { get; set; }

    // ========================================
    // PDF
    // ========================================

    /// <summary>
    /// Ruta o URL del PDF generado
    /// </summary>
    [Display(Name = "PDF")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PDF { get; set; }

    // ========================================
    // RETENCIÓN LEGAL (v4.4 - M9)
    // ========================================

    /// <summary>
    /// NUEVO v4.4 - M9: Fecha de vencimiento de retención legal de documentos
    /// Según normativa costarricense, los comprobantes deben conservarse 5 años
    /// Calculada automáticamente: FechaEmision + 5 años
    /// </summary>
    [Display(Name = "Fecha Vencimiento Retención")]
    public DateTime? FechaVencimientoRetencion { get; set; }

    // ========================================
    // AUDIT TRAIL
    // ========================================

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // ========================================
    // SOFT DELETE
    // ========================================

    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================

    public Empresa? Empresa { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Terminal? Terminal { get; set; }
    public Cliente? Cliente { get; set; }
    public Proveedor? Proveedor { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    /// <summary>
    /// Líneas de detalle del documento
    /// </summary>
    public ICollection<DocumentoDetalle> Detalles { get; set; } = new List<DocumentoDetalle>();

    /// <summary>
    /// Descuentos aplicados al documento (nivel documento)
    /// </summary>
    public ICollection<DocumentoDescuento> Descuentos { get; set; } = new List<DocumentoDescuento>();

    /// <summary>
    /// Referencias a otros documentos (para ND/NC)
    /// </summary>
    public ICollection<DocumentoReferencia> Referencias { get; set; } = new List<DocumentoReferencia>();

    /// <summary>
    /// Medios de pago utilizados (para pagos mixtos)
    /// </summary>
    public ICollection<DocumentoMedioPago> MediosPago { get; set; } = new List<DocumentoMedioPago>();

    /// <summary>
    /// Otra información (key-value pairs)
    /// </summary>
    public ICollection<DocumentoOtraInformacion> OtraInformacion { get; set; } = new List<DocumentoOtraInformacion>();

    /// <summary>
    /// Otros cargos del documento (v4.4)
    /// </summary>
    public ICollection<DocumentoOtroCargo> OtrosCargos { get; set; } = new List<DocumentoOtroCargo>();

    /// <summary>
    /// Información específica de exportación (solo para FEE - tipo 09)
    /// </summary>
    public DocumentoExportacion? Exportacion { get; set; }
}
