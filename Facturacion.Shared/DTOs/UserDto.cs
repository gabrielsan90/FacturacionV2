namespace Facturacion.Shared.DTOs;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Document { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public Guid? EmpresaId { get; set; }
    public string? EmpresaNombre { get; set; }
}
