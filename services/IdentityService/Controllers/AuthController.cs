using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<AuthController> _logger;
    private readonly TokenKeyProvider _keys;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IGoogleAuthService googleAuthService,
        ILogger<AuthController> logger,
        TokenKeyProvider keys)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _googleAuthService = googleAuthService;
        _logger = logger;
        _keys = keys;
    }

    /// <summary>
    /// JWKS endpoint — exposes the RSA public key so any service can verify our JWTs
    /// without needing a shared secret.
    /// </summary>
    [HttpGet(".well-known/jwks")]
    [AllowAnonymous]
    public IActionResult Jwks()
    {
        var rsaParams = _keys.PublicKey.ExportParameters(false);
        var jwk = new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = _keys.KeyId,
                    alg = "RS256",
                    n   = Base64UrlEncoder.Encode(rsaParams.Modulus),
                    e   = Base64UrlEncoder.Encode(rsaParams.Exponent)
                }
            }
        };
        return Ok(jwk);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthenticationResponse
                {
                    Success = false,
                    Message = "User with this email already exists"
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new AuthenticationResponse
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                });
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            // Generate tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _tokenService.SaveRefreshTokenAsync(user, refreshToken);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthenticationResponse
            {
                Success = true,
                Message = "User registered successfully",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    IsGoogleUser = user.IsGoogleUser,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new AuthenticationResponse
            {
                Success = false,
                Message = "An error occurred during registration"
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _tokenService.SaveRefreshTokenAsync(user, refreshToken);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthenticationResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
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
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return StatusCode(500, new AuthenticationResponse
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GoogleToken))
        {
            return BadRequest(new AuthenticationResponse { Success = false, Message = "Missing idToken" });
        }

        try
        {
            var result = await _googleAuthService.AuthenticateGoogleUserAsync(request.GoogleToken);
            if (!result.Success) return Unauthorized(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, new AuthenticationResponse { Success = false, Message = "Server error" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                });
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(request.RefreshToken);
            if (principal?.Identity?.Name == null)
            {
                return Unauthorized(new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }

            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null || !await _tokenService.ValidateRefreshTokenAsync(user, request.RefreshToken))
            {
                return Unauthorized(new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                });
            }

            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            await _tokenService.SaveRefreshTokenAsync(user, newRefreshToken);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthenticationResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
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
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new AuthenticationResponse
            {
                Success = false,
                Message = "An error occurred during token refresh"
            });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _tokenService.RevokeRefreshTokenAsync(user);
                }
            }

            return Ok(new { Success = true, Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { Success = false, Message = "An error occurred during logout" });
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            var result = new List<UserInfo>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserInfo
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
                });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            return StatusCode(500, new { Success = false, Message = "An error occurred while fetching users" });
        }
    }

    // ── OAuth 2.0 Authorization Code flow ──

    [HttpGet("google-login")]
    [AllowAnonymous]
    public IActionResult GoogleLogin([FromQuery] string returnUrl = "http://localhost:4200/auth/callback")
    {
        var callbackUrl = Url.Action("GoogleCallback", "Auth", null, Request.Scheme);
        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items = { ["returnUrl"] = returnUrl }
        };
        return Challenge(properties, "Google");
    }

    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback()
    {
        const string frontendBase = "http://localhost:4200/auth/callback";

        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded)
            return Redirect($"{frontendBase}?error=oauth_failed");

        var returnUrl = result.Properties?.Items["returnUrl"] ?? frontendBase;
        var principal  = result.Principal!;

        var email    = principal.FindFirstValue(ClaimTypes.Email);
        var googleId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
        var lastName  = principal.FindFirstValue(ClaimTypes.Surname) ?? "";
        var picture   = principal.FindFirstValue("urn:google:picture") ?? "";

        if (string.IsNullOrEmpty(email))
            return Redirect($"{returnUrl}?error=missing_email");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName      = email,
                Email         = email,
                EmailConfirmed = true,
                GoogleId      = googleId,
                FirstName     = firstName,
                LastName      = lastName,
                ProfilePictureUrl = picture,
                CreatedAt     = DateTime.UtcNow
            };
            var create = await _userManager.CreateAsync(user);
            if (!create.Succeeded)
                return Redirect($"{returnUrl}?error=user_creation_failed");
            await _userManager.AddToRoleAsync(user, "User");
        }
        else
        {
            if (string.IsNullOrEmpty(user.GoogleId))  user.GoogleId = googleId;
            user.FirstName        = string.IsNullOrEmpty(user.FirstName) ? firstName : user.FirstName;
            user.LastName         = string.IsNullOrEmpty(user.LastName)  ? lastName  : user.LastName;
            user.ProfilePictureUrl = string.IsNullOrEmpty(user.ProfilePictureUrl) ? picture : user.ProfilePictureUrl;
            user.EmailConfirmed   = true;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var accessToken  = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user, refreshToken);

        var roles     = await _userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60).ToString("o");

        await HttpContext.SignOutAsync("External");

        var redirect = new System.Text.StringBuilder(returnUrl)
            .Append("?accessToken=").Append(Uri.EscapeDataString(accessToken))
            .Append("&refreshToken=").Append(Uri.EscapeDataString(refreshToken))
            .Append("&expiresAt=").Append(Uri.EscapeDataString(expiresAt))
            .Append("&userId=").Append(Uri.EscapeDataString(user.Id))
            .Append("&email=").Append(Uri.EscapeDataString(email))
            .Append("&firstName=").Append(Uri.EscapeDataString(firstName))
            .Append("&lastName=").Append(Uri.EscapeDataString(lastName))
            .Append("&picture=").Append(Uri.EscapeDataString(picture))
            .Append("&roles=").Append(Uri.EscapeDataString(string.Join(",", roles)));

        return Redirect(redirect.ToString());
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetUser([FromQuery] string? email, [FromQuery] string? userId)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new
            {
                Message = "Either email or userId must be provided."
            });
        }

        ApplicationUser? user;

        if (!string.IsNullOrWhiteSpace(email))
        {
            user = await _userManager.FindByEmailAsync(email);
        }
        else
        {
            user = await _userManager.FindByIdAsync(userId!);
        }

        if (user == null)
        {
            return NotFound(new
            {
                Message = "User not found."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserInfo
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
        });
    }

}
