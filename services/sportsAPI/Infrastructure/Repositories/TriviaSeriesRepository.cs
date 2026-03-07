using Domain.Repositories;
using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public sealed class TriviaSeriesRepository : EfRepository<TriviaSeries, TriviaSeriesId>, ITriviaSeriesRepository
{
    public TriviaSeriesRepository(SportsDbAppContext db) : base(db) { }
}
