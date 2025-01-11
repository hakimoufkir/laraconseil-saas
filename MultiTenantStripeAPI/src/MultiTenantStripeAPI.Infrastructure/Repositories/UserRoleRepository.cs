using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Domain.Entities;
using MultiTenantStripeAPI.Infrastructure.GenericRepo;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;

namespace MultiTenantStripeAPI.Infrastructure.Repositories
{
    public class UserRoleRepository : GenericRepository<UserRole>, IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }   
    }
}
