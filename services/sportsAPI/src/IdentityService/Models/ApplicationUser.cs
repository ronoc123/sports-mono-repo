using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models;

public class ApplicationUser : IdentityUser
{
    // Extended properties for Google authentication
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? GoogleId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Locale { get; set; }
    
    // Additional user properties
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Business properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsGoogleUser => !string.IsNullOrEmpty(GoogleId);
    public bool HasValidRefreshToken => RefreshToken != null && RefreshTokenExpiryTime > DateTime.UtcNow;
}
