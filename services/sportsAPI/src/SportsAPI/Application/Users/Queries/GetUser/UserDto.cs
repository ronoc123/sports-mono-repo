namespace Application.Users.Queries.GetUser;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public List<UserVoteDto> VotesAvailable { get; set; } = new();
    public List<VoteDto> Votes { get; set; } = new();
    public List<CodeDto> RedeemedCodes { get; set; } = new();
}

public class UserVoteDto
{
    public Guid OrganizationId { get; set; }
    public int VotesRemaining { get; set; }
}

public class VoteDto
{
    public Guid Id { get; set; }
    public int VotesSpent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CodeDto
{
    public Guid Id { get; set; }
    public int VotesAwarded { get; set; }
    public DateTime RedeemedAt { get; set; }
}
