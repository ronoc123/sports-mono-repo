using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.BacktestIntegrity.Commands.SubmitIntegrityAnswers;

public record IntegrityAnswerInput(int QuestionIndex, bool Answer, string? Reason);

public record SubmitIntegrityAnswersCommand(
    Guid UserId,
    List<IntegrityAnswerInput> Answers
) : IRequest<ServiceResponse<BacktestStatusDto>>;
