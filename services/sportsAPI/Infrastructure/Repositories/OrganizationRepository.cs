using Domain.Organizations;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class OrganizationRepository : EfRepository<Organization, OrganizationId>, IOrganizationRepository
    {
    private readonly SportsDbAppContext _db;

    public OrganizationRepository(SportsDbAppContext db) : base(db) => _db = db;

    // If you want a named alias, just forward to base
    public Task<Organization?> GetOrganizationByIdAsync(OrganizationId id, CancellationToken ct = default)
      => GetByIdAsync(id, ct);

    // Optional convenience methods using the base Query/List/Exists API
    public Task<List<Organization>> GetAllOrganizationsAsync(CancellationToken ct = default)
      => Query().ToListAsync(ct);

    public Task AddOrganizationAsync(Organization organization, CancellationToken ct = default)
      => AddAsync(organization, ct);

    public Task UpdateOrganizationAsync(Organization organization, CancellationToken ct = default)
    {
      Update(organization);
      return Task.CompletedTask;
    }

    public async Task DeleteOrganizationAsync(OrganizationId id, CancellationToken ct = default)
    {
      var org = await GetByIdAsync(id, ct);
      if (org is null) return;
      Remove(org);
    }
  }
}
