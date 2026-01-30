using Facturacion.Shared.DTOs;
using Facturacion.Shared.Responses;

namespace Facturacion.Frontend.Services;

public interface IAuthService
{
    Task<ActionResponse<bool>> LoginAsync(LoginDto model);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    bool IsAuthenticated();
    Task UpdateAuthenticationAsync(LoginResponseDto loginResponse);
}
