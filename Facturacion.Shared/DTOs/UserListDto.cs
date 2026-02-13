namespace Facturacion.Shared.DTOs;

public class UserListDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Document { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<EmpresaBasicDto> Empresas { get; set; } = new();
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
}

public class EmpresaBasicDto
{
    public Guid Id { get; set; }
    public string NombreComercial { get; set; } = null!;
}
