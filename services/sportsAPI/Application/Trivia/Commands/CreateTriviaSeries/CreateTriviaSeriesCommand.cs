using Contracts.Contracts;
using MediatR;

namespace Application.Trivia.Commands.CreateTriviaSeries;

public record CreateTriviaSeriesCommand(
    Guid OrganizationId,
    string SeriesName
) : IRequest<ServiceResponse<CreateTriviaSeriesResult>>;

public record CreateTriviaSeriesResult(Guid SeriesId, string SeriesName);
