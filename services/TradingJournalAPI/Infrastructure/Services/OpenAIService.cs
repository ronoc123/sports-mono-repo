using Application.Common.Interfaces;
using Domain.AIConversation;
using Domain.TradeJournal;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services;

public sealed class OpenAIService : IAIService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private const string Model = "gpt-4o";
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _apiKey = ""
            ?? throw new InvalidOperationException("OpenAI:ApiKey not configured.");
    }

    public async Task<string> GenerateJournalResponseAsync(
        IReadOnlyList<AIMessage> history,
        AIJournalContext context,
        CancellationToken ct = default)
        => await SendAsync(BuildJournalSystemPrompt(context), history, ct);

    public async Task<string> GenerateIntegrityResponseAsync(
        IReadOnlyList<AIMessage> history,
        AIIntegrityContext context,
        CancellationToken ct = default)
        => await SendAsync(BuildIntegritySystemPrompt(context), history, ct);

    public async Task<string> GenerateReviewResponseAsync(
        IReadOnlyList<AIMessage> history,
        AIReviewContext context,
        CancellationToken ct = default)
        => await SendAsync(BuildReviewSystemPrompt(context), history, ct);

    private async Task<string> SendAsync(
        string systemPrompt,
        IReadOnlyList<AIMessage> history,
        CancellationToken ct)
    {
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        messages.AddRange(history.Select(m => (object)new
        {
            role = m.Role == MessageRole.User ? "user" : "assistant",
            content = m.Content
        }));

        var body = new
        {
            model = Model,
            max_tokens = 1024,
            messages
        };

        var json = JsonSerializer.Serialize(body);
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    private static string BuildJournalSystemPrompt(AIJournalContext ctx)
    {
        var effortlessNote = ctx.IsEffortless
            ? "The trader said this phase felt effortless."
            : "The trader said this phase did NOT feel effortless — explore why.";

        return $"""
            You are an emotionally intelligent trading journal coach.
            Current phase: {ctx.Phase}
            Emotional state: {ctx.EmotionalState}
            {effortlessNote}
            Trader notes: {ctx.Notes ?? "(none)"}
            {(ctx.RecentPatternSummary is not null ? $"Recent patterns: {ctx.RecentPatternSummary}" : "")}

            Your role: Reflect on what the trader has shared, draw a clear conclusion about their mindset,
            make a specific suggestion, then ask ONE probing question to deepen self-awareness.
            Be direct, warm, and concise. Do not use bullet points.
            """;
    }

    private static string BuildIntegritySystemPrompt(AIIntegrityContext ctx)
    {
        var answerNote = ctx.Answer
            ? "The trader answered YES to this integrity question."
            : "The trader answered NO to this integrity question.";

        return $"""
            You are a trading psychology coach reviewing backtest integrity.
            Question {ctx.QuestionIndex}: {ctx.QuestionText}
            {answerNote}
            Trader's reason: {ctx.Reason ?? "(none provided)"}

            Your role: Acknowledge their answer honestly. If NO, help them understand the implication
            without judgment. Ask ONE clarifying question about their trading process.
            Be direct and concise.
            """;
    }

    private static string BuildReviewSystemPrompt(AIReviewContext ctx)
    {
        return $"""
            You are an emotionally intelligent trading coach conducting a biweekly performance review.

            Trade journal summary for this period:
            {ctx.TradesSummary}

            Key patterns observed:
            {ctx.TopPatterns}

            Your role: Guide the trader through a reflective review session. Identify emotional patterns,
            acknowledge progress, name one key growth area, and ask ONE meaningful question to deepen insight.
            Be direct, warm, and concise. Draw conclusions from the data — don't just ask questions.
            """;
    }
}
