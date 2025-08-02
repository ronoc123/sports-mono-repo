using IdentityService.Models;
using System.Security.Claims;

namespace IdentityService.Services;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<bool> ValidateRefreshTokenAsync(ApplicationUser user, string refreshToken);
    Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken);
    Task RevokeRefreshTokenAsync(ApplicationUser user);
}
