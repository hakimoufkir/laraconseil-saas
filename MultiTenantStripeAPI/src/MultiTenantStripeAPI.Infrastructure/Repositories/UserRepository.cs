using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Domain.Entities;
using MultiTenantStripeAPI.Infrastructure.GenericRepo;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
