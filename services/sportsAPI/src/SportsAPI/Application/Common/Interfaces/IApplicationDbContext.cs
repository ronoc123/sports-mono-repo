using Domain.Leagues;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.SharedKernal;
using Domain.User.Entities;
using Domain.Users;
using Domain.VoteAccount;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<League> Leagues { get; }
    DbSet<Code> Codes { get; }
    DbSet<Player> Players { get; }
    DbSet<PlayerOption> PlayerOptions { get; }
    DbSet<Theme> Themes { get; }
    DbSet<VoteAccount> VoteAccounts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
