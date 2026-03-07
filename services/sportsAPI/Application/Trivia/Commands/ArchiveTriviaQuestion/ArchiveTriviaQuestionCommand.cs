using Contracts.Contracts;
using MediatR;

namespace Application.Trivia.Commands.ArchiveTriviaQuestion;

public record ArchiveTriviaQuestionCommand(Guid QuestionId) : IRequest<ServiceResponse<bool>>;
