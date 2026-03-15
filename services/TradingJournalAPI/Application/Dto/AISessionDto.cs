using Domain.AIConversation;

namespace Application.Dto;

public class AISessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AISessionContext Context { get; set; }
    public Guid LinkedEntityId { get; set; }
    public List<AIMessageDto> Messages { get; set; } = new();
}

public class AIMessageDto
{
    public Guid Id { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}
