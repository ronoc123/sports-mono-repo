using System.Text.Json.Serialization;

namespace Contracts.Contracts
{
  public class ServiceResponse<T>
  {
    public T? Data { get; init; }
    public bool Success { get; init; } = true;
    public string Message { get; init; } = string.Empty;

    // --- error metadata (null on success) ---
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? ValidationErrors { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Details { get; init; }
  }

}
