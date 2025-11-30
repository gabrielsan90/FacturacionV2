# GUÍA DE PATRONES DEL PROYECTO SHARED

## PROPÓSITO
Este documento define TODOS los patrones que DEBEN usarse en el proyecto Shared para Entities, DTOs, Enums y Responses.

## ESTRUCTURA DE CARPETAS

```
[NOMBREPROYECTO].Shared/
├── Entities/
│   ├── User.cs
│   ├── [Entidad].cs
│   └── [OtraEntidad].cs
├── DTOs/
│   ├── [Entidad]DTO.cs
│   └── LoginDTO.cs
├── Enums/
│   ├── UserType.cs
│   └── [OtroEnum].cs
├── Responses/
│   └── ActionResponse.cs
└── [NOMBREPROYECTO].Shared.csproj
```

## 1. ENTITIES PATTERN

### Entidad Base (sin herencia)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MJL.Shared.Entities
{
    public class [Entidad]
    {
        // PATRÓN: ID siempre es int, siempre se llama "Id", siempre con [Key]
        [Key]
        public int Id { get; set; }

        // PATRÓN: Campos obligatorios con [Required] y [MaxLength]
        [Display(Name = "Nombre del Campo")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Campo { get; set; } = null!;

        // PATRÓN: Campos opcionales sin [Required]
        [Display(Name = "Descripción")]
        [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string? Descripcion { get; set; }

        // PATRÓN: Decimales con precisión definida
        [Display(Name = "Precio")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Precio { get; set; }

        // PATRÓN: Fechas DateTime nullable
        [Display(Name = "Fecha de Creación")]
        public DateTime? FechaCreacion { get; set; }

        // PATRÓN: Relaciones con Foreign Key
        [Display(Name = "Usuario")]
        public string UserId { get; set; } = null!;

        // PATRÓN: Propiedades de navegación siempre nullable
        public User? User { get; set; }

        // PATRÓN: Colecciones con inicialización
        public ICollection<[Relacion]>? Relaciones { get; set; } = new List<[Relacion]>();

        // PATRÓN: Propiedades calculadas (no mapeadas en BD)
        [NotMapped]
        public bool TieneRelaciones => Relaciones != null && Relaciones.Any();
    }
}
```

### EJEMPLO COMPLETO: Paquete.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Column;

namespace MJL.Shared.Entities
{
    public class Paquete
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Proveniente")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Proveniente { get; set; } = null!;

        [Display(Name = "Número de Tracking")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string NumeroTracking { get; set; } = null!;

        [Display(Name = "Descripción")]
        [MaxLength(500, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string? Descripcion { get; set; }

        [Display(Name = "Peso (kilos)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Peso { get; set; }

        [Display(Name = "Valor Declarado")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Valor { get; set; }

        [Display(Name = "Estado")]
        public int EstadoId { get; set; }

        [Display(Name = "Usuario")]
        public string UserId { get; set; } = null!;

        [Display(Name = "Fecha de Prealerta")]
        public DateTime? FechaPrealerta { get; set; }

        [Display(Name = "Fecha en transito")]
        public DateTime? FechaTransito { get; set; }

        [Display(Name = "Fecha Disponible Retiro Inicio")]
        public DateTime? FechaInicioDisponible { get; set; }

        [Display(Name = "Fecha Disponible Retiro Fin")]
        public DateTime? FechaFinDisponible { get; set; }

        [Display(Name = "Fecha en Ruta")]
        public DateTime? FechaRuta { get; set; }

        [Display(Name = "Fecha de Entrega")]
        public DateTime? FechaEntrega { get; set; }

        [Display(Name = "Observaciones")]
        [MaxLength(1000, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string? Observaciones { get; set; }

        [Display(Name = "Warehouse")]
        [MaxLength(200, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        public string? Warehouse { get; set; }

        // Propiedades de navegación
        public EstadoPaquete? Estado { get; set; }
        public User? User { get; set; }
        public ICollection<DetalleTicketPago>? DetallesTicket { get; set; } = new List<DetalleTicketPago>();
        public ICollection<AdjuntoPaquete>? Adjuntos { get; set; } = new List<AdjuntoPaquete>();

        // Propiedades no mapeadas
        [NotMapped]
        public bool TieneAdjuntos { get; set; }
    }
}
```

### User Entity (Identity)

```csharp
using Microsoft.AspNetCore.Identity;
using MJL.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace MJL.Shared.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "Nombres")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Apellidos")]
        [MaxLength(50, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Tipo de Usuario")]
        public UserType UserType { get; set; }

        [Display(Name = "Número de Casillero")]
        [MaxLength(20)]
        public string? Casillero { get; set; }

        [Display(Name = "Número de Identidad")]
        [MaxLength(20)]
        public string? NumeroIdentidad { get; set; }

        // Propiedad calculada
        [Display(Name = "Nombre Completo")]
        public string FullName => $"{FirstName} {LastName}";

        // Navegación
        public ICollection<Paquete>? Paquetes { get; set; }
        public ICollection<DireccionUsuario>? Direcciones { get; set; }
    }
}
```

## 2. DTOs PATTERN

### DTO Básico

```csharp
using MJL.Shared.Entities;

namespace MJL.Shared.DTO
{
    public class [Entidad]DTO
    {
        // PATRÓN: Incluir SOLO propiedades necesarias para transferencia
        public int Id { get; set; }
        public string Campo1 { get; set; } = null!;
        public string? Campo2 { get; set; }
        public decimal? Precio { get; set; }

        // PATRÓN: Incluir propiedades de navegación cuando se necesiten
        public User? User { get; set; }
        public Estado? Estado { get; set; }

        // PATRÓN: Incluir propiedades calculadas o formateadas
        public string FechaFormateada { get; set; } = null!;
        public int CantidadRelaciones { get; set; }
    }
}
```

### EJEMPLO: PaqueteDTO.cs

```csharp
using MJL.Shared.Entities;

namespace MJL.Shared.DTO
{
    public class PaqueteDto
    {
        public int Id { get; set; }
        public string NumeroTracking { get; set; } = null!;
        public string? Descripcion { get; set; }
        public decimal? Peso { get; set; }
        public decimal? Valor { get; set; }
        public string Proveniente { get; set; } = null!;
        public string? Warehouse { get; set; }

        // Foreign Keys
        public int EstadoId { get; set; }
        public string UserId { get; set; } = null!;

        // Navegación
        public EstadoPaquete? Estado { get; set; }
        public User? User { get; set; }
        public ICollection<DetalleTicketPago>? DetallesTicket { get; set; }

        // Fechas
        public DateTime? FechaPrealerta { get; set; }
        public DateTime? FechaTransito { get; set; }
        public DateTime? FechaInicioDisponible { get; set; }
        public DateTime? FechaFinDisponible { get; set; }
        public DateTime? FechaRuta { get; set; }
        public DateTime? FechaEntrega { get; set; }
    }
}
```

### DTOs para Autenticación

```csharp
using System.ComponentModel.DataAnnotations;

namespace MJL.Shared.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = null!;
    }

    public class RegisterDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Teléfono no válido")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class TokenDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El token es obligatorio")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
```

## 3. ENUMS PATTERN

### Enum Básico

```csharp
namespace MJL.Shared.Enums
{
    public enum [NombreEnum]
    {
        Opcion1 = 0,
        Opcion2 = 1,
        Opcion3 = 2
    }
}
```

### EJEMPLO: UserType.cs

```csharp
namespace MJL.Shared.Enums
{
    public enum UserType
    {
        Admin = 0,
        User = 1,
        Employee = 2
    }
}
```

### CUÁNDO USAR ENUM vs ENTITY

**USAR ENUM cuando:**
- Los valores son fijos y NO cambian (Admin, User, Employee)
- Son propiedades del sistema, no del negocio
- Máximo 5-10 opciones

**USAR ENTITY cuando:**
- Los valores pueden cambiar dinámicamente
- Los administradores necesitan agregar/editar opciones
- Se necesitan más propiedades además del nombre (color, orden, descripción)
- **EJEMPLO**: EstadoPaquete es una entidad porque:
  - Se agregan/editan estados desde la aplicación
  - Puede tener propiedades adicionales (color, icono, orden)

```csharp
// ✅ BIEN: Enum para tipos fijos del sistema
public enum UserType
{
    Admin = 0,
    User = 1,
    Employee = 2
}

// ✅ BIEN: Entity para estados dinámicos del negocio
public class EstadoPaquete
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Color { get; set; }
    public int Orden { get; set; }
}
```

## 4. ACTIONRESPONSE PATTERN

### ActionResponse.cs (OBLIGATORIO)

```csharp
namespace MJL.Shared.Responses
{
    public class ActionResponse<T>
    {
        // PATRÓN: Indica si la operación fue exitosa
        public bool WasSuccess { get; set; }

        // PATRÓN: Mensaje de error o información
        public string? Message { get; set; }

        // PATRÓN: Resultado de la operación (puede ser null)
        public T? Result { get; set; }
    }
}
```

### USO DE ActionResponse

```csharp
// En Repository (SIEMPRE devolver ActionResponse)
public async Task<ActionResponse<Paquete>> GetPaqueteAsync(int id)
{
    try
    {
        var paquete = await _context.Paquetes.FindAsync(id);
        if (paquete == null)
        {
            return new ActionResponse<Paquete>
            {
                WasSuccess = false,
                Message = "Paquete no encontrado"
            };
        }

        return new ActionResponse<Paquete>
        {
            WasSuccess = true,
            Result = paquete
        };
    }
    catch (Exception ex)
    {
        return new ActionResponse<Paquete>
        {
            WasSuccess = false,
            Message = ex.Message
        };
    }
}

// En Controller (SIEMPRE verificar WasSuccess)
var action = await _unitOfWork.GetPaqueteAsync(id);
if (action.WasSuccess)
{
    return Ok(action.Result);
}
return NotFound(action.Message);
```

## 5. VALIDATIONS

### Data Annotations (en Entities y DTOs)

```csharp
// Obligatorio
[Required(ErrorMessage = "El campo {0} es obligatorio.")]

// Longitud máxima
[MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]

// Longitud mínima
[MinLength(6, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]

// Email
[EmailAddress(ErrorMessage = "Email no válido")]

// Teléfono
[Phone(ErrorMessage = "Teléfono no válido")]

// Rango
[Range(1, 100, ErrorMessage = "El valor debe estar entre {1} y {2}")]

// Comparación (para confirmación de contraseña)
[Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]

// Expresión regular
[RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El formato no es válido")]
```

## 6. RELACIONES ENTRE ENTIDADES

### One-to-Many

```csharp
// Entidad Padre (One)
public class Categoria
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = null!;

    // Colección de hijos
    public ICollection<Producto>? Productos { get; set; } = new List<Producto>();
}

// Entidad Hija (Many)
public class Producto
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = null!;

    // Foreign Key
    public int CategoriaId { get; set; }

    // Navegación al padre
    public Categoria? Categoria { get; set; }
}
```

### Many-to-Many (con tabla intermedia)

```csharp
// Entidad 1
public class Paquete
{
    [Key]
    public int Id { get; set; }

    public ICollection<DetalleTicketPago>? DetallesTicket { get; set; }
}

// Entidad 2
public class TicketPago
{
    [Key]
    public int Id { get; set; }

    public ICollection<DetalleTicketPago>? Detalles { get; set; }
}

// Tabla intermedia
public class DetalleTicketPago
{
    [Key]
    public int Id { get; set; }

    public int TicketId { get; set; }
    public TicketPago? Ticket { get; set; }

    public int PaqueteId { get; set; }
    public Paquete? Paquete { get; set; }

    // Propiedades adicionales del detalle
    public decimal Subtotal { get; set; }
}
```

## REGLAS OBLIGATORIAS

### Entities
1. **SIEMPRE** usar `[Key]` para la propiedad Id
2. **SIEMPRE** usar `int` para IDs (NUNCA Guid, NUNCA string para IDs)
3. **SIEMPRE** usar `= null!;` para campos requeridos no nullable
4. **SIEMPRE** usar `?` para campos opcionales
5. **SIEMPRE** usar `[Display(Name = "...")]` para etiquetas
6. **SIEMPRE** usar `[MaxLength]` en strings
7. **SIEMPRE** usar `[Column(TypeName = "decimal(18,2)")]` para decimales
8. **SIEMPRE** usar colecciones con inicialización: `= new List<>()`
9. **SIEMPRE** hacer nullable las propiedades de navegación
10. **SIEMPRE** usar `[NotMapped]` para propiedades calculadas

### DTOs
11. **USAR** DTOs cuando se necesite:
    - Filtrar propiedades (no exponer toda la entidad)
    - Agregar propiedades calculadas
    - Combinar datos de múltiples entidades
12. **NUNCA** usar Data Annotations de validación en DTOs que solo leen
13. **SIEMPRE** usar Data Annotations en DTOs de input (LoginDTO, RegisterDTO)

### Enums
14. **USAR** Enum para valores fijos del sistema
15. **USAR** Entity para valores dinámicos del negocio
16. **SIEMPRE** asignar valores explícitos: `Admin = 0, User = 1`

### ActionResponse
17. **SIEMPRE** usar `ActionResponse<T>` en Repositories
18. **SIEMPRE** verificar `WasSuccess` antes de usar `Result`
19. **SIEMPRE** incluir mensaje descriptivo en caso de error
