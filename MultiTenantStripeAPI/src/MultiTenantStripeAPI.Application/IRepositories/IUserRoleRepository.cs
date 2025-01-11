using MultiTenantStripeAPI.Application.IGenericRepo;
using MultiTenantStripeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.IRepositories
{
    public interface IUserRoleRepository : IGenericRepository<UserRole>
    {
    }
}
