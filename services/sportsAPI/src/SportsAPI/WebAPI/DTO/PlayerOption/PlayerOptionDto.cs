namespace sportsAPI.DTO.PlayerOption
{
    public class PlayerOptionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // trade, draft, lineup, strategy
        public string Status { get; set; } = string.Empty; // active, expired, completed, cancelled
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Organization and Player info
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public Guid? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string? PlayerPosition { get; set; }
        
        // Voting info
        public int TotalVotes { get; set; }
        public int VotesRequired { get; set; }
        public bool HasUserVoted { get; set; }
        public string? UserVoteChoice { get; set; }
        
        // Options/Choices
        public List<PlayerOptionChoiceDto> Choices { get; set; } = new();
        
        // Metadata
        public string CreatedBy { get; set; } = string.Empty;
        public int Priority { get; set; } = 1; // 1-5, 5 being highest
        public List<string> Tags { get; set; } = new();
    }

    public class PlayerOptionChoiceDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public decimal VotePercentage { get; set; }
        public bool IsSelected { get; set; }
        public string? ImpactDescription { get; set; }
    }

    public class CreatePlayerOptionRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid OrganizationId { get; set; }
        public Guid? PlayerId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int VotesRequired { get; set; } = 1;
        public int Priority { get; set; } = 1;
        public List<string> Tags { get; set; } = new();
        public List<CreatePlayerOptionChoiceDto> Choices { get; set; } = new();
    }

    public class CreatePlayerOptionChoiceDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImpactDescription { get; set; }
    }

    public class UpdatePlayerOptionRequestDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? VotesRequired { get; set; }
        public int? Priority { get; set; }
        public List<string>? Tags { get; set; }
        public string? Status { get; set; }
    }

    public class VoteOnPlayerOptionRequestDto
    {
        public Guid PlayerOptionId { get; set; }
        public Guid ChoiceId { get; set; }
        public Guid UserId { get; set; }
    }

    public class VoteOnPlayerOptionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PlayerOptionDto? UpdatedPlayerOption { get; set; }
        public int RemainingVotes { get; set; }
    }

    public class PlayerOptionStatsDto
    {
        public int TotalOptions { get; set; }
        public int ActiveOptions { get; set; }
        public int CompletedOptions { get; set; }
        public int ExpiredOptions { get; set; }
        public int UserParticipatedOptions { get; set; }
        public int UserVotesUsed { get; set; }
        public int UserVotesRemaining { get; set; }
        public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    }

    public class CategoryStatsDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public int UserParticipated { get; set; }
    }

    public class PlayerOptionFiltersDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? PlayerId { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public bool? HasUserVoted { get; set; }
        public int? Priority { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}
