using Facturacion.Shared.DTOs;

namespace Facturacion.Frontend.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginDto model);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    bool IsAuthenticated();
    Task UpdateAuthenticationAsync(LoginResponseDto loginResponse);
}
