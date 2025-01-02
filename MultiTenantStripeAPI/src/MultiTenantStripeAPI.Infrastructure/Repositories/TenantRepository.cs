using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Domain.Entities;
using MultiTenantStripeAPI.Infrastructure.GenericRepo;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;
using System.Linq;

namespace MultiTenantStripeAPI.Infrastructure.Repositories
{
    public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
    {
        private readonly ApplicationDbContext _context;

        public TenantRepository(ApplicationDbContext context) : base(context)
        {
            _context = context; // Assign the ApplicationDbContext to the local context variable
        }

        public Tenant? GetByEmail(string email)
        {
            // Use the _context to access the Tenants DbSet
            return _context.Tenants.FirstOrDefault(t => t.Email == email);
        }

        public Tenant? GetByTenantId(string tenantId)
        {
            // Use the _context to access the Tenants DbSet
            return _context.Tenants.FirstOrDefault(t => t.TenantId == tenantId);
        }
    }
}
