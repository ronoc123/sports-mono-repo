namespace Application.RenderJobs.Dto;

public class RenderJobResponse
{
    public string Id { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>Pending | Processing | Completed | Failed</summary>
    public string Status { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string AspectRatio { get; set; } = string.Empty;

    /// <summary>Populated once the worker uploads the result. Null until Completed.</summary>
    public string? OutputVideoKey { get; set; }

    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateRenderJobResponse
{
    public string JobId { get; set; } = string.Empty;
}

public class UploadAssetResponse
{
    public string ObjectKey { get; set; } = string.Empty;
}
