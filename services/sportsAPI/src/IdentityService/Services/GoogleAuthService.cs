using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using IdentityService.DTOs;
using IdentityService.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;


namespace IdentityService.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<GoogleAuthService> _logger;
  private readonly GoogleAuthOptions _options;

  public GoogleAuthService(
        HttpClient httpClient,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<GoogleAuthService> logger,
        IOptions<GoogleAuthOptions> options)
    {
        _httpClient = httpClient;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _options = options.Value;
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

  public async Task<AuthenticationResponse> AuthenticateGoogleUserAsync(string idToken)
  {
    GoogleJsonWebSignature.Payload payload;
    try
    {
      var settings = new GoogleJsonWebSignature.ValidationSettings
      {
        // Ensure the token was meant for YOUR app
        Audience = _options.ClientIds
      };

      payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Invalid Google ID token");
      return new AuthenticationResponse { Success = false, Message = "Invalid Google token" };
    }

    // Map payload
    var email = payload.Email;
    var emailVerified = payload.EmailVerified;
    var googleId = payload.Subject;                // stable Google user id (sub)
    var firstName = payload.GivenName ?? "";
    var lastName = payload.FamilyName ?? "";
    var fullName = payload.Name ?? $"{firstName} {lastName}".Trim();
    var picture = payload.Picture;
    var locale = payload.Locale;

    if (string.IsNullOrWhiteSpace(email))
    {
      return new AuthenticationResponse { Success = false, Message = "Google token missing email" };
    }

    // Find or create Identity user
    var user = await _userManager.FindByEmailAsync(email);
    var isNew = false;

    if (user == null)
    {
      user = new ApplicationUser
      {
        UserName = email,
        Email = email,
        EmailConfirmed = emailVerified,
        GoogleId = googleId,
        FirstName = firstName,
        LastName = lastName,
        ProfilePictureUrl = picture,
        Locale = locale,
        CreatedAt = DateTime.UtcNow
      };

      var create = await _userManager.CreateAsync(user);
      if (!create.Succeeded)
      {
        return new AuthenticationResponse
        {
          Success = false,
          Message = string.Join("; ", create.Errors.Select(e => e.Description))
        };
      }

      // optional default role
      await _userManager.AddToRoleAsync(user, "User");
      isNew = true;
    }
    else
    {
      // Link fields if missing / update profile
      if (string.IsNullOrEmpty(user.GoogleId))
        user.GoogleId = googleId;

      user.FirstName = string.IsNullOrEmpty(user.FirstName) ? firstName : user.FirstName;
      user.LastName = string.IsNullOrEmpty(user.LastName) ? lastName : user.LastName;
      user.ProfilePictureUrl ??= picture;
      user.Locale ??= locale;
      if (emailVerified) user.EmailConfirmed = true;

      await _userManager.UpdateAsync(user);
    }

    user.LastLoginAt = DateTime.UtcNow;
    await _userManager.UpdateAsync(user);

    // Issue tokens
    var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
    var refreshToken = _tokenService.GenerateRefreshToken();
    await _tokenService.SaveRefreshTokenAsync(user, refreshToken);

    var roles = await _userManager.GetRolesAsync(user);

    return new AuthenticationResponse
    {
      Success = true,
      Message = isNew ? "User created and authenticated successfully" : "User authenticated successfully",
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      ExpiresAt = DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(Environment.GetEnvironmentVariable("Jwt__AccessTokenExpirationMinutes") ?? "60")
        ),
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
}
