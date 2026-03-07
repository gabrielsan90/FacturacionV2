using System.ComponentModel.DataAnnotations;

namespace Facturacion.Shared.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [EmailAddress(ErrorMessage = "El campo {0} no es válido.")]
    public string Email { get; set; } = null!;

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MinLength(6, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]
    public string Password { get; set; } = null!;

    [Display(Name = "Empresa")]
    public Guid? EmpresaId { get; set; }

    /// <summary>
    /// Token de pre-autenticación generado por ValidateCredentials.
    /// Evita re-hashear la contraseña en el login.
    /// </summary>
    public string? PreAuthToken { get; set; }
}

public class EmpresaLoginDto
{
    public Guid Id { get; set; }
    public string NombreComercial { get; set; } = null!;
    public string? CedulaJuridica { get; set; }
}

public class ValidateCredentialsResponseDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public List<EmpresaLoginDto> Empresas { get; set; } = new();
    public bool RequiresEmpresaSelection { get; set; }
    public string? PreAuthToken { get; set; }
    public bool IsSuperUser { get; set; }
}
