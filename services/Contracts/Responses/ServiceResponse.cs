using System.Text.Json.Serialization;

namespace Contracts.Contracts
{
  // Envelope for successful responses (errors come from middleware)
  public class ServiceResponse<T>
  {
    public T? Data { get; init; }
    public bool Success { get; init; } = true;
    public string Message { get; init; } = string.Empty;
    // --- error metadata (used only when you choose to return Fail envelopes) ---
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? ValidationErrors { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Details { get; init; }
  }

  public static class ServiceResponse
  {
    public static ServiceResponse<T> Ok<T>(T data, string? message = null) => new()
    {
      Data = data,
      Success = true,
      Message = message ?? string.Empty
    };

    public static ServiceResponse<T> Fail<T>(
      string message,
      string? code = null,
      string? traceId = null,
      object? details = null,
      IDictionary<string, string[]>? validation = null) => new()
      {
        Success = false,
        Message = message,
        ErrorCode = code,
        TraceId = traceId,
        Details = details,
        ValidationErrors = validation
      };

    public static ServiceResponse<T> FromException<T>(
      Exception ex,
      string message = "An unexpected error occurred.",
      string? code = null,
      string? traceId = null) =>
      Fail<T>(message, code ?? ex.GetType().Name, traceId, new { ex.Message, ex.StackTrace });
  }
}
