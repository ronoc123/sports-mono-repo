namespace Infrastructure.Settings;

public class VideoGenerationSettings
{
    public const string SectionName = "VideoGeneration";

    public string Provider { get; init; } = "Stub";
    public AnthropicSettings Anthropic { get; init; } = new();
    public HiggsFieldSettings Higgsfield { get; init; } = new();
}

public class AnthropicSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "claude-opus-4-5";
}

public class HiggsFieldSettings
{
    public string McpEndpoint { get; init; } = "https://mcp.higgsfield.ai/mcp";
    public string AuthToken { get; init; } = string.Empty;
    public int TargetDurationSeconds { get; init; } = 15;
}
