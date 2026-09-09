namespace Application.Common.Interfaces;

public record IdeationRequest(
    string ChannelName,
    string StyleToneContext,
    string? PromptTemplate,
    string? CharacterImagePath,
    string UserIdea);

public record IdeationResult(string Prompt, List<string> Scenes);

public interface IClaudeIdeationService
{
    Task<IdeationResult?> IdeateAsync(IdeationRequest request, CancellationToken cancellationToken = default);
}
