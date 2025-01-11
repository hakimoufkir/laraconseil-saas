using MultiTenantStripeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.IServices
{
    public interface IUserService
    {
        Task<User> CreateUser(string userName, string email);
        Task<User> GetUserByEmail(string email);
        Task<List<User>> GetUsersByTenant();
        Task DeleteUser(int userId);
    }
}
