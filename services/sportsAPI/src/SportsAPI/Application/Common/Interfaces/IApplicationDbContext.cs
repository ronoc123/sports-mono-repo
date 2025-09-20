using Domain.Organizations;
using Domain.Leagues;
using Domain.Users;
using Domain.User.Entities;
using Domain.Organizations.Entities;
using Domain.SharedKernal;
using Microsoft.EntityFrameworkCore;
using Domain.PointsCode;
using Domain.PointsWallet;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<League> Leagues { get; }
    DbSet<Code> Codes { get; }
    DbSet<Player> Players { get; }
    DbSet<PlayerOption> PlayerOptions { get; }
    DbSet<Theme> Themes { get; }
    DbSet<PointsCode> PointsCodes { get; }
    DbSet<PointsWallet> PointsWallets { get; }

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
