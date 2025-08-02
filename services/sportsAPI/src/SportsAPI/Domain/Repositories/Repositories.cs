using Domain.Leagues;
using Domain.Organizations;
using Domain.User.Entities;
using Domain.Users;

namespace Domain.Repositories
{
    public interface IVoteRepository
    {
        Task<Vote?> GetVoteByIdAsync(Guid voteId);
        Task<List<Vote>> GetVotesByUserIdAsync(Guid userId);
        Task AddVoteAsync(Vote vote);
        Task UpdateVoteAsync(Vote vote);
    }
    public interface IUserRepository
    {
        Task<Domain.Users.User?> GetUserByIdAsync(Guid userId);
        Task<List<Domain.Users.User>> GetAllUsersAsync();
        Task AddUserAsync(Domain.Users.User user);
        Task UpdateUserAsync(Domain.Users.User user);
        Task DeleteUserAsync(Guid userId);
    }
    public interface IOrganizationRepository
    {
        Task<Organization?> GetOrganizationByIdAsync(Guid organizationId);
        Task<List<Organization>> GetAllOrganizationsAsync();
        Task AddOrganizationAsync(Organization organization);
        Task UpdateOrganizationAsync(Organization organization);
        Task DeleteOrganizationAsync(Guid organizationId);
    }

    public interface ILeagueRepository
    {
        Task<League?> GetByIdAsync(Guid leagueId);
        Task<List<League>> GetLeaguesByOrganizationIdAsync(Guid organizationId);
        Task AddLeagueAsync(League league);
        Task UpdateLeagueAsync(League league);
        Task DeleteLeagueAsync(Guid leagueId);
    }
}
