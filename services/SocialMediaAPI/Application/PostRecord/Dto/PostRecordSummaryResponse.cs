namespace Application.PostRecord.Dto;

public class PostRecordSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DescriptionSnippet { get; set; } = string.Empty;
    public string VideoReference { get; set; } = string.Empty;
    public DateTime? PostedAt { get; set; }
    public List<PlatformResultSummary> PlatformResults { get; set; } = new();
}

public class PlatformResultSummary
{
    public string Platform { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PostHistoryPageResponse
{
    public List<PostRecordSummaryResponse> Records { get; set; } = new();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
