using Domain.Leagues;
using Domain.Organizations;

namespace Domain.Repositories
{

    public interface IRepository<TAgg, TId>
        where TAgg : class, IAggregate<TId>
    {
      Task<TAgg?> FindAsync(TId id, CancellationToken ct = default);
    }

    public interface IPointsCodeRepository : IRepository<Domain.PointsCode.PointsCode, PointCodeId>
    {
      Task<Domain.PointsCode.PointsCode?> FindByCodeAsync(string code, CancellationToken ct = default);
      Task AddAsync(Domain.PointsCode.PointsCode code, CancellationToken ct = default);
      void Update(Domain.PointsCode.PointsCode code);
    }

    public interface IPointsWalletRepository : IRepository<Domain.PointsWallet.PointsWallet, PointsWalletId>
    {
      Task<Domain.PointsWallet.PointsWallet?> GetByUserAndOrgAsync(UserId userId, OrganizationId orgId, CancellationToken ct = default);
      Task AddAsync(Domain.PointsWallet.PointsWallet wallet, CancellationToken ct = default);
      void Update(Domain.PointsWallet.PointsWallet wallet);
    }

    public interface IUserRepository : IRepository<Domain.Users.User, UserId>
      {
          Task<Domain.Users.User?> GetUserByIdAsync(UserId userId);
          Task<List<Domain.Users.User>> GetAllUsersAsync();
          Task AddUserAsync(Domain.Users.User user);
          Task UpdateUserAsync(Domain.Users.User user);
          Task DeleteUserAsync(UserId userId);
      }
    public interface IOrganizationRepository : IRepository<Organization, OrganizationId>
    {
        Task<Organization?> GetOrganizationByIdAsync(OrganizationId organizationId);
        Task<List<Organization>> GetAllOrganizationsAsync();
        Task AddOrganizationAsync(Organization organization);
        Task UpdateOrganizationAsync(Organization organization);
        Task DeleteOrganizationAsync(OrganizationId organizationId);
    }

    public interface ILeagueRepository : IRepository<League, LeagueId>
    {
        Task<League?> GetByIdAsync(LeagueId leagueId);
        //Task<List<League>> GetLeaguesByOrganizationIdAsync(Guid organizationId);
        Task AddLeagueAsync(League league);
        Task UpdateLeagueAsync(League league);
        Task DeleteLeagueAsync(LeagueId leagueId);
    }
}
