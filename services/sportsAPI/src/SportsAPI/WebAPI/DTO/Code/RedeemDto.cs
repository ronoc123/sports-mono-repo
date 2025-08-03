namespace sportsAPI.DTO.Code
{
    public class RedeemCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // votes, premium, bonus, special
        public int Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RedemptionHistoryDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime RedeemedAt { get; set; }
        public string Status { get; set; } = string.Empty; // success, failed, expired
        public Guid UserId { get; set; }
    }

    public class UserBalanceDto
    {
        public int Votes { get; set; }
        public DateTime? PremiumUntil { get; set; }
        public decimal BonusMultiplier { get; set; } = 1.0m;
        public List<string> SpecialRewards { get; set; } = new();
    }

    public class RedeemRequestDto
    {
        public string Code { get; set; } = string.Empty;
    }

    public class RedeemResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RewardDto? Reward { get; set; }
        public UserBalanceDto? NewBalance { get; set; }
    }

    public class RewardDto
    {
        public string Type { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class DashboardStatsDto
    {
        public int ActiveOrganizations { get; set; }
        public int TotalUsers { get; set; }
        public int ActivePlayerOptions { get; set; }
        public decimal SystemHealth { get; set; }
        public string SystemStatus { get; set; } = "operational";
        public List<QuickActionDto> QuickActions { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    public class QuickActionDto
    {
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
    }

    public class RecentActivityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = "info"; // info, success, warning, error
    }
}
