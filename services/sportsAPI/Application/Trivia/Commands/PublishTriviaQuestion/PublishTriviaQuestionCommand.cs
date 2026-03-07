using Contracts.Contracts;
using MediatR;

namespace Application.Trivia.Commands.PublishTriviaQuestion;

public record PublishTriviaQuestionCommand(Guid QuestionId) : IRequest<ServiceResponse<bool>>;
