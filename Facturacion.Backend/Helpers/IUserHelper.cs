using Facturacion.Shared.DTOs;
using Facturacion.Shared.Entities;
using Microsoft.AspNetCore.Identity;

namespace Facturacion.Backend.Helpers;

public interface IUserHelper
{
    Task<User> GetUserAsync(string email);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IdentityResult> AddUserAsync(User user, string password);
    Task CheckRoleAsync(string roleName);
    Task AddUserToRoleAsync(User user, string roleName);
    Task<bool> IsUserInRoleAsync(User user, string roleName);
    Task<SignInResult> LoginAsync(LoginDto model);
    Task LogoutAsync();
    Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<IdentityResult> UpdateUserAsync(User user);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    Task<bool> IsLockedOutAsync(User user);
    Task<int> GetAccessFailedCountAsync(User user);
    Task<IdentityResult> ResetAccessFailedCountAsync(User user);
    Task<DateTimeOffset?> GetLockoutEndDateAsync(User user);
}
