namespace sportsAPI.DTO.User
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string? Avatar { get; set; }
        public UserPreferencesDto Preferences { get; set; } = new();
        public UserStatsDto Stats { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserPreferencesDto
    {
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = false;
        public string Theme { get; set; } = "auto";
        public string Language { get; set; } = "en";
        public string Timezone { get; set; } = "UTC";
        public PrivacySettingsDto Privacy { get; set; } = new();
    }

    public class PrivacySettingsDto
    {
        public string ProfileVisibility { get; set; } = "public";
        public bool ShowEmail { get; set; } = false;
        public bool ShowPhone { get; set; } = false;
    }

    public class UserStatsDto
    {
        public int TotalVotes { get; set; }
        public int VotesUsed { get; set; }
        public int VotesRemaining { get; set; }
        public int OptionsParticipated { get; set; }
        public int OrganizationsJoined { get; set; }
        public string AccountLevel { get; set; } = "bronze";
        public DateTime JoinDate { get; set; }
    }

    public class UpdateProfileRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Bio { get; set; }
    }

    public class UpdatePreferencesRequestDto
    {
        public bool? EmailNotifications { get; set; }
        public bool? PushNotifications { get; set; }
        public string? Theme { get; set; }
        public string? Language { get; set; }
        public string? Timezone { get; set; }
        public PrivacySettingsDto? Privacy { get; set; }
    }

    public class UploadAvatarRequestDto
    {
        public IFormFile Avatar { get; set; } = null!;
    }
}
