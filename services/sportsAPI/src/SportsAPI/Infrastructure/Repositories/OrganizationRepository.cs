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

    // Bridge your Guid-based method to the strongly-typed ID
    public Task<Organization?> GetOrganizationByIdAsync(OrganizationId organizationId)
        => FindAsync(organizationId);

    public Task<List<Organization>> GetAllOrganizationsAsync()
        => _db.Set<Organization>().AsNoTracking().ToListAsync();

    public Task AddOrganizationAsync(Organization organization)
        => _db.Set<Organization>().AddAsync(organization).AsTask();

    public Task UpdateOrganizationAsync(Organization organization)
    {
        _db.Set<Organization>().Update(organization);
        return Task.CompletedTask;
    }

    public async Task DeleteOrganizationAsync(OrganizationId organizationId)
    {
        var org = await FindAsync(organizationId);
        if (org is null) return;
        _db.Set<Organization>().Remove(org);
    }

    public Task DeleteOrganizationAsync(Guid organizationId)
    {
      throw new NotImplementedException();
    }
  }
}
