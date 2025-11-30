namespace Facturacion.Shared.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
