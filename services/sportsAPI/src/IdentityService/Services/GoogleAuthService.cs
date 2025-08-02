using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using IdentityService.DTOs;
using IdentityService.Models;

namespace IdentityService.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        HttpClient httpClient,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<GoogleAuthService> logger)
    {
        _httpClient = httpClient;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> GetGoogleUserInfoAsync(string googleToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={googleToken}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Google user info. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var googleUser = JsonSerializer.Deserialize<GoogleUserInfo>(json, options);
            return googleUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user info");
            return null;
        }
    }

    public async Task<AuthenticationResponse> AuthenticateGoogleUserAsync(string googleToken)
    {
        try
        {
            // Get user info from Google
            var googleUserInfo = await GetGoogleUserInfoAsync(googleToken);
            if (googleUserInfo == null)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = "Failed to get user information from Google"
                };
            }

            // Check if user exists by Google ID
            var existingUser = await _userManager.FindByEmailAsync(googleUserInfo.Email);
            
            ApplicationUser user;
            bool isNewUser = false;

            if (existingUser != null)
            {
                // Update existing user with Google info if not already set
                if (string.IsNullOrEmpty(existingUser.GoogleId))
                {
                    existingUser.GoogleId = googleUserInfo.Id;
                    existingUser.FirstName = googleUserInfo.GivenName;
                    existingUser.LastName = googleUserInfo.FamilyName;
                    existingUser.ProfilePictureUrl = googleUserInfo.Picture;
                    existingUser.Locale = googleUserInfo.Locale;
                    existingUser.EmailConfirmed = googleUserInfo.VerifiedEmail;
                    
                    await _userManager.UpdateAsync(existingUser);
                }
                
                user = existingUser;
            }
            else
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = googleUserInfo.Email,
                    Email = googleUserInfo.Email,
                    EmailConfirmed = googleUserInfo.VerifiedEmail,
                    GoogleId = googleUserInfo.Id,
                    FirstName = googleUserInfo.GivenName,
                    LastName = googleUserInfo.FamilyName,
                    ProfilePictureUrl = googleUserInfo.Picture,
                    Locale = googleUserInfo.Locale,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new AuthenticationResponse
                    {
                        Success = false,
                        Message = $"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}"
                    };
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");
                isNewUser = true;
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _tokenService.SaveRefreshTokenAsync(user, refreshToken);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthenticationResponse
            {
                Success = true,
                Message = isNewUser ? "User created and authenticated successfully" : "User authenticated successfully",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Should match token expiration
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsGoogleUser = user.IsGoogleUser,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating Google user");
            return new AuthenticationResponse
            {
                Success = false,
                Message = "An error occurred during authentication"
            };
        }
    }
}
