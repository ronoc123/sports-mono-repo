using Contracts.Contracts;
using MediatR;

namespace Application.Trivia.Queries.GetTriviaSeries;

public record GetTriviaSeriesQuery(Guid OrganizationId)
    : IRequest<ServiceResponse<List<TriviaSeriesDto>>>;
