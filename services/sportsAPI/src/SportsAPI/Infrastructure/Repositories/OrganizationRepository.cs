using Domain.Organizations;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly SportsDbAppContext _db;

        public OrganizationRepository(SportsDbAppContext db)
        {
            _db = db;
        }

        public async Task AddOrganizationAsync(Organization organization)
        {
            await _db.Organizations.AddAsync(organization);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteOrganizationAsync(Guid organizationId)
        {
            var organizationIdVO = OrganizationId.Of(organizationId);
            var organization = await _db.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationIdVO);

            if (organization != null)
            {
                _db.Organizations.Remove(organization);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            return await _db.Organizations.ToListAsync();
        }

        public async Task<Organization?> GetOrganizationByIdAsync(Guid organizationId)
        {
            var organizationIdVO = OrganizationId.Of(organizationId);
            return await _db.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationIdVO);
        }

        public async Task UpdateOrganizationAsync(Organization organization)
        {
            _db.Organizations.Update(organization);
            await _db.SaveChangesAsync();
        }
    }
}
