using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
  public sealed record VoteAccountId(LeagueId LeagueId, UserId UserId)
  {
    // EF-friendly ctor (scalar params)
    public VoteAccountId(Guid leagueId, Guid userId)
        : this(LeagueId.Of(leagueId), UserId.Of(userId)) { }

    // Parameterless ctor is optional if you use the scalar-parameter one
    private VoteAccountId() : this(LeagueId.Of(Guid.NewGuid()), UserId.Of(Guid.Empty)) { }

    public static VoteAccountId Of(LeagueId leagueId, UserId userId)
        => new(leagueId, userId);
  }
}
