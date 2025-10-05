namespace Application.Organizations.Queries.GetOrganizationDetails;

public class OrganizationDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamShortName { get; set; }
    public int? FormedYear { get; set; }
    public string? Sport { get; set; }
    public string? Stadium { get; set; }
    public string? Location { get; set; }
    public int? StadiumCapacity { get; set; }
    public string? Website { get; set; }
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    public string? Description { get; set; }
    public string? Color1 { get; set; }
    public string? Color2 { get; set; }
    public string? Color3 { get; set; }
    public string? BadgeUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? Fanart1Url { get; set; }
    public string? Fanart2Url { get; set; }
    public string? Fanart3Url { get; set; }
    public List<PlayerOptionDto> PlayerOptions { get; set; } = new();
    public ThemeDto? Theme { get; set; }
}

public class PlayerOptionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Votes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public PlayerDto Player { get; set; } = new();
}

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public int Age { get; set; }
}

public class ThemeDto
{
    public string Name { get; set; } = string.Empty;
    public string ColorPrimary { get; set; } = string.Empty;
    public string ColorSecondary { get; set; } = string.Empty;
    public string ColorTertiary { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
}
