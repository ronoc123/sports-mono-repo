using IdentityService.DTOs;

namespace IdentityService.Services;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> GetGoogleUserInfoAsync(string googleToken);
    Task<AuthenticationResponse> AuthenticateGoogleUserAsync(string googleToken);
}
