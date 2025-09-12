using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PointsWallet
{
  public sealed class PointsWallet : Aggregate<PointsWalletId>
  {
    internal PointsWallet() { }

    public UserId UserId { get; private set; } = null!;
    public OrganizationId OrganizationId { get; private set; } = null!;
    public int Balance { get; private set; }

    public static PointsWallet Open(PointsWalletId id, UserId userId, OrganizationId orgId)
        => new PointsWallet
        {
          Id = id,
          UserId = userId,
          OrganizationId = orgId,
          CreatedAt = DateTime.UtcNow,
          Balance = 0
        };

    public void Credit(int points, string reason)
    {
      if (points <= 0) throw new ArgumentException("Credit must be positive.", nameof(points));
      checked { Balance += points; } // overflow-safe
                                     // `reason` can be written to an audit log/projection by the app layer.
    }

    public void SpendForVote(PlayerOptionId optionId, int points)
    {
      if (points <= 0) throw new ArgumentException("Spend must be positive.", nameof(points));
      if (Balance < points) throw new InvalidOperationException("Insufficient points.");
      checked { Balance -= points; }
      // The app layer can record this spend (optionId, amount) in a read model/ledger.
    }
  }

}
