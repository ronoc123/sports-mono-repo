using Domain.Poll;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public sealed class PollRepository : EfRepository<Poll, PollId>, IPollRepository
{
    public PollRepository(SportsDbAppContext db) : base(db) { }
}
