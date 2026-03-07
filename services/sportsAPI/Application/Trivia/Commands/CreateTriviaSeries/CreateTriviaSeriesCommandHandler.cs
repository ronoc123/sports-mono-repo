using Contracts.Contracts;
using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Trivia.Commands.CreateTriviaSeries;

public sealed class CreateTriviaSeriesCommandHandler
    : IRequestHandler<CreateTriviaSeriesCommand, ServiceResponse<CreateTriviaSeriesResult>>
{
    private readonly ITriviaSeriesRepository _repo;

    public CreateTriviaSeriesCommandHandler(ITriviaSeriesRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<CreateTriviaSeriesResult>> Handle(
        CreateTriviaSeriesCommand request, CancellationToken ct)
    {
        var series = TriviaSeries.Create(
            OrganizationId.Of(request.OrganizationId),
            request.SeriesName);

        await _repo.AddAsync(series, ct);
        await _repo.SaveChangesAsync(ct);

        return ServiceResponse.Ok(
            new CreateTriviaSeriesResult(series.Id.Value, series.Name),
            "Trivia series created.");
    }
}
