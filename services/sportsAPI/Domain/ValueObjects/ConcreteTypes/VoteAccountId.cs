using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.ConcreteTypes
{
  public sealed record VoteAccountId(OrganizationId OrgId, UserId UserId)
  {
    // EF-friendly ctor (scalar params)
    public VoteAccountId(Guid orgId, Guid userId)
        : this(OrganizationId.Of(orgId), UserId.Of(userId)) { }

    // Parameterless ctor is optional if you use the scalar-parameter one
    private VoteAccountId() : this(OrganizationId.Of(Guid.Empty), UserId.Of(Guid.Empty)) { }

    public static VoteAccountId Of(OrganizationId orgId, UserId userId)
        => new(orgId, userId);
  }
}
