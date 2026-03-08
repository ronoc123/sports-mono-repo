using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace IdentityService.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenKeyProvider _keys;

    public TokenService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        TokenKeyProvider keys)
    {
        _configuration = configuration;
        _userManager = userManager;
        _keys = keys;
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name,           user.UserName ?? string.Empty),
            new(ClaimTypes.Email,          user.Email    ?? string.Empty),
            new("firstName",       user.FirstName        ?? string.Empty),
            new("lastName",        user.LastName         ?? string.Empty),
            new("fullName",        user.FullName),
            new("profilePicture",  user.ProfilePictureUrl ?? string.Empty),
            new("isGoogleUser",    user.IsGoogleUser.ToString()),
            new("jti",             Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var signingKey = new RsaSecurityKey(_keys.PrivateKey) { KeyId = _keys.KeyId };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(_keys.PublicKey) { KeyId = _keys.KeyId },
            ValidateLifetime = false
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParams, out var securityToken);

        if (securityToken is not JwtSecurityToken jwt ||
            !jwt.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public Task<bool> ValidateRefreshTokenAsync(ApplicationUser user, string refreshToken)
        => Task.FromResult(user.RefreshToken == refreshToken && user.RefreshTokenExpiryTime > DateTime.UtcNow);

    public async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
        await _userManager.UpdateAsync(user);
    }

    public async Task RevokeRefreshTokenAsync(ApplicationUser user)
    {
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);
    }
}
