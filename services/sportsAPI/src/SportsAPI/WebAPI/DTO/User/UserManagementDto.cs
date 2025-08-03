namespace sportsAPI.DTO.User
{
    public class UserManagementDto
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
        public string AccountLevel { get; set; } = "bronze";
        public string Status { get; set; } = "active"; // active, suspended, inactive
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        // Statistics
        public UserActivityStatsDto ActivityStats { get; set; } = new();
        
        // Organizations
        public List<UserOrganizationDto> Organizations { get; set; } = new();
        
        // Roles and Permissions
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    public class UserActivityStatsDto
    {
        public int TotalVotes { get; set; }
        public int VotesUsed { get; set; }
        public int VotesRemaining { get; set; }
        public int OptionsParticipated { get; set; }
        public int OrganizationsJoined { get; set; }
        public int CodesRedeemed { get; set; }
        public DateTime? LastActivity { get; set; }
        public int LoginCount { get; set; }
        public double EngagementScore { get; set; } // 0-100
    }

    public class UserOrganizationDto
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // member, admin, gm, csp
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUserRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string Password { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<Guid> OrganizationIds { get; set; } = new();
    }

    public class UpdateUserStatusRequestDto
    {
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty; // active, suspended, inactive
        public string? Reason { get; set; }
    }

    public class AssignUserRoleRequestDto
    {
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class UserSearchFiltersDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? AccountLevel { get; set; }
        public Guid? OrganizationId { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? LastLoginAfter { get; set; }
        public DateTime? LastLoginBefore { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class UserStatsOverviewDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersThisWeek { get; set; }
        public double AverageEngagementScore { get; set; }
        public List<UserLevelStatsDto> LevelStats { get; set; } = new();
        public List<UserActivityTrendDto> ActivityTrends { get; set; } = new();
    }

    public class UserLevelStatsDto
    {
        public string Level { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class UserActivityTrendDto
    {
        public DateTime Date { get; set; }
        public int ActiveUsers { get; set; }
        public int NewRegistrations { get; set; }
        public int VotesCast { get; set; }
        public int CodesRedeemed { get; set; }
    }

    public class BulkUserActionRequestDto
    {
        public List<Guid> UserIds { get; set; } = new();
        public string Action { get; set; } = string.Empty; // suspend, activate, delete, assign_role
        public string? Reason { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class BulkUserActionResponseDto
    {
        public int TotalRequested { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public List<BulkActionResultDto> Results { get; set; } = new();
    }

    public class BulkActionResultDto
    {
        public Guid UserId { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
