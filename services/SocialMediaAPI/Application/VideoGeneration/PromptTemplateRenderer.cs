using Application.Common.Models;

namespace Application.VideoGeneration;

public static class PromptTemplateRenderer
{
    private const string DefaultTemplate =
        "You are creating a short social media video for the channel \"{ChannelName}\".\n\n" +
        "Channel style: {StyleToneContext}\n\n" +
        "Recent post context (last 5 posts):\n{RecentPostHistory}\n\n" +
        "Video request: {UserPrompt}\n\n" +
        "Using the attached reference image and the Higgsfield video generation tool, " +
        "create a {TargetDurationSeconds}-second short-form video that matches this channel's style " +
        "and the request above. Select the best available Higgsfield model for the content type.";

    public static string Render(
        string? channelTemplate,
        string channelName,
        string styleToneContext,
        IEnumerable<PostHistoryItem> recentHistory,
        string userPrompt,
        int targetDurationSeconds = 15)
    {
        var template = string.IsNullOrWhiteSpace(channelTemplate)
            ? DefaultTemplate
            : channelTemplate;

        var historyBlock = BuildHistoryBlock(recentHistory);

        return template
            .Replace("{ChannelName}", channelName)
            .Replace("{StyleToneContext}", styleToneContext)
            .Replace("{RecentPostHistory}", historyBlock)
            .Replace("{UserPrompt}", userPrompt)
            .Replace("{TargetDurationSeconds}", targetDurationSeconds.ToString());
    }

    private static string BuildHistoryBlock(IEnumerable<PostHistoryItem> history)
    {
        var entries = history.Take(5).Select((p, i) =>
        {
            var snippet = p.Description.Length > 80
                ? p.Description[..80] + "..."
                : p.Description;
            return $"{i + 1}. \"{p.Title}\" — {snippet}";
        });

        var block = string.Join("\n", entries);
        return string.IsNullOrEmpty(block) ? "(no previous posts)" : block;
    }
}
