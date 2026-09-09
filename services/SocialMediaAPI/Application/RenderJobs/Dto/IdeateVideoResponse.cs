namespace Application.RenderJobs.Dto;

public class IdeateVideoResponse
{
    public string Prompt { get; init; } = string.Empty;
    public List<string> Scenes { get; init; } = new();
}
