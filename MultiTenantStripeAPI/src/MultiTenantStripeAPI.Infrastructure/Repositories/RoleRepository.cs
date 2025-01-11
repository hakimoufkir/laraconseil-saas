using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Domain.Entities;
using MultiTenantStripeAPI.Infrastructure.GenericRepo;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;


namespace MultiTenantStripeAPI.Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
    }
}
