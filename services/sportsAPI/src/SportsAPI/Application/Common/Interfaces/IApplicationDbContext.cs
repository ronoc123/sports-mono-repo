using Domain.Organizations;
using Domain.Leagues;
using Domain.Users;
using Domain.User.Entities;
using Domain.Organizations.Entities;
using Domain.SharedKernal;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<League> Leagues { get; }
    DbSet<User> Users { get; }
    DbSet<Vote> Votes { get; }
    DbSet<Code> Codes { get; }
    DbSet<Player> Players { get; }
    DbSet<PlayerOption> PlayerOptions { get; }
    DbSet<UserVotes> UserVotes { get; }
    DbSet<Theme> Themes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
