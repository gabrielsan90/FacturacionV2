using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.Shared.Entities;

/// <summary>
/// Empleados de la empresa.
/// Incluye datos personales, laborales, de pago y seguro social.
/// </summary>
public class Empleado
{
    [Key]
    public Guid Id { get; set; }

    // =====================================================
    // Multi-Tenancy
    // =====================================================
    [Display(Name = "Empresa")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid EmpresaId { get; set; }

    // =====================================================
    // Identificación
    // =====================================================
    [Display(Name = "Código")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Codigo { get; set; } = null!;

    /// <summary>
    /// Tipo: 01=Cédula Física, 02=Cédula Jurídica, 03=DIMEX, 04=NITE
    /// </summary>
    [Display(Name = "Tipo Identificación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(2, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoIdentificacion { get; set; } = "01";

    [Display(Name = "Identificación")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Identificacion { get; set; } = null!;

    // =====================================================
    // Datos personales
    // =====================================================
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Display(Name = "Primer Apellido")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string PrimerApellido { get; set; } = null!;

    [Display(Name = "Segundo Apellido")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? SegundoApellido { get; set; }

    [Display(Name = "Fecha de Nacimiento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaNacimiento { get; set; }

    /// <summary>
    /// Género: M=Masculino, F=Femenino
    /// </summary>
    [Display(Name = "Género")]
    [MaxLength(1, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Genero { get; set; }

    [Display(Name = "Estado Civil")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? EstadoCivil { get; set; }

    [Display(Name = "Nacionalidad")]
    [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Nacionalidad { get; set; } = "Costarricense";

    // =====================================================
    // Contacto
    // =====================================================
    [Display(Name = "Dirección")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Direccion { get; set; }

    [Display(Name = "Teléfono")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Telefono { get; set; }

    [Display(Name = "Celular")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Celular { get; set; }

    [Display(Name = "Email Personal")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [EmailAddress(ErrorMessage = "El campo {0} debe ser un email válido.")]
    public string? Email { get; set; }

    [Display(Name = "Email Corporativo")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    [EmailAddress(ErrorMessage = "El campo {0} debe ser un email válido.")]
    public string? EmailCorporativo { get; set; }

    // =====================================================
    // Datos laborales
    // =====================================================
    [Display(Name = "Departamento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public Guid DepartamentoId { get; set; }

    [Display(Name = "Puesto")]
    public Guid? PuestoId { get; set; }

    [Display(Name = "Nombre del Puesto")]
    [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? PuestoNombre { get; set; }

    [Display(Name = "Fecha de Ingreso")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public DateTime FechaIngreso { get; set; }

    [Display(Name = "Fecha de Salida")]
    public DateTime? FechaSalida { get; set; }

    /// <summary>
    /// Estado: ACT=Activo, INA=Inactivo, VAC=Vacaciones, INC=Incapacidad, LIC=Licencia
    /// </summary>
    [Display(Name = "Estado")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string Estado { get; set; } = "ACT";

    // =====================================================
    // Datos de contrato y pago
    // =====================================================
    /// <summary>
    /// Tipo: IND=Indefinido, TMP=Temporal, SER=Servicios Profesionales
    /// </summary>
    [Display(Name = "Tipo de Contrato")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string TipoContrato { get; set; } = "IND";

    /// <summary>
    /// Forma de Pago: QUI=Quincenal, MEN=Mensual, SEM=Semanal
    /// </summary>
    [Display(Name = "Forma de Pago")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string FormaPago { get; set; } = "QUI";

    [Display(Name = "Salario Base")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalarioBase { get; set; }

    [Display(Name = "Moneda Salario")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string MonedaSalario { get; set; } = "CRC";

    // =====================================================
    // Datos bancarios
    // =====================================================
    [Display(Name = "Banco")]
    public Guid? BancoId { get; set; }

    [Display(Name = "Número de Cuenta")]
    [MaxLength(30, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroCuenta { get; set; }

    /// <summary>
    /// Tipo: AHO=Ahorro, COR=Corriente
    /// </summary>
    [Display(Name = "Tipo de Cuenta")]
    [MaxLength(3, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? TipoCuenta { get; set; }

    [Display(Name = "IBAN")]
    [MaxLength(30, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? IBAN { get; set; }

    // =====================================================
    // Seguro Social
    // =====================================================
    [Display(Name = "Número Seguro Social")]
    [MaxLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? NumeroSeguroSocial { get; set; }

    [Display(Name = "Aporta CCSS")]
    public bool AportaCCSS { get; set; } = true;

    // =====================================================
    // Jerarquía
    // =====================================================
    [Display(Name = "Jefe Directo")]
    public Guid? JefeDirectoId { get; set; }

    // =====================================================
    // Datos adicionales
    // =====================================================
    [Display(Name = "Observaciones")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? Observaciones { get; set; }

    [Display(Name = "Foto")]
    [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
    public string? FotoUrl { get; set; }

    /// <summary>
    /// Relación opcional con usuario del sistema
    /// </summary>
    [Display(Name = "Usuario del Sistema")]
    public string? UserId { get; set; }

    // =====================================================
    // Audit Trail
    // =====================================================
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; }

    [Display(Name = "Usuario Creación")]
    public string? UsuarioCreacionId { get; set; }

    [Display(Name = "Fecha de Modificación")]
    public DateTime? FechaModificacion { get; set; }

    [Display(Name = "Usuario Modificación")]
    public string? UsuarioModificacionId { get; set; }

    // =====================================================
    // Soft Delete
    // =====================================================
    [Display(Name = "Eliminado")]
    public bool IsDeleted { get; set; }

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [Display(Name = "Usuario Eliminación")]
    public string? UsuarioEliminacionId { get; set; }

    // =====================================================
    // Propiedades calculadas
    // =====================================================
    [NotMapped]
    public string NombreCompleto => $"{Nombre} {PrimerApellido} {SegundoApellido}".Trim();

    [NotMapped]
    public int Edad => DateTime.Today.Year - FechaNacimiento.Year -
        (DateTime.Today.DayOfYear < FechaNacimiento.DayOfYear ? 1 : 0);

    [NotMapped]
    public int AntiguedadAnios => DateTime.Today.Year - FechaIngreso.Year -
        (DateTime.Today.DayOfYear < FechaIngreso.DayOfYear ? 1 : 0);

    [NotMapped]
    public string EstadoDescripcion => Estado switch
    {
        "ACT" => "Activo",
        "INA" => "Inactivo",
        "VAC" => "Vacaciones",
        "INC" => "Incapacidad",
        "LIC" => "Licencia",
        _ => Estado
    };

    [NotMapped]
    public string TipoContratoDescripcion => TipoContrato switch
    {
        "IND" => "Indefinido",
        "TMP" => "Temporal",
        "SER" => "Servicios Profesionales",
        _ => TipoContrato
    };

    [NotMapped]
    public string FormaPagoDescripcion => FormaPago switch
    {
        "QUI" => "Quincenal",
        "MEN" => "Mensual",
        "SEM" => "Semanal",
        _ => FormaPago
    };

    // =====================================================
    // Navigation Properties
    // =====================================================
    public Empresa? Empresa { get; set; }
    public Departamento? Departamento { get; set; }
    public Puesto? Puesto { get; set; }
    public Banco? Banco { get; set; }
    public Empleado? JefeDirecto { get; set; }
    public User? User { get; set; }
    public User? UsuarioCreacion { get; set; }
    public User? UsuarioModificacion { get; set; }
    public User? UsuarioEliminacion { get; set; }

    public ICollection<Empleado>? Subordinados { get; set; }
    public ICollection<ContactoEmergencia>? ContactosEmergencia { get; set; }
    public ICollection<ExpedienteDigital>? ExpedientesDigitales { get; set; }
    public ICollection<Vacacion>? Vacaciones { get; set; }
    public ICollection<Incapacidad>? Incapacidades { get; set; }
    public ICollection<AccionPersonal>? AccionesPersonal { get; set; }
    public ICollection<DetallePlanilla>? DetallesPlanilla { get; set; }
}
