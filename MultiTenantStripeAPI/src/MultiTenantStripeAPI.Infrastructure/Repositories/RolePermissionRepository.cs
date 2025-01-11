using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Domain.Entities;
using MultiTenantStripeAPI.Infrastructure.GenericRepo;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Infrastructure.Repositories
{
    public class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionRepository(ApplicationDbContext context): base(context)
        {
            _context = context;
        }

        public async Task<List<RolePermission>> GetAllAsNoTracking(Expression<Func<RolePermission, bool>> predicate)
        {
            return await _context.RolePermissions
                                 .AsNoTracking()
                                 .Include(rp => rp.Permission) // Ensure related permissions are loaded
                                 .Where(predicate)
                                 .ToListAsync();
        }
    }
}
