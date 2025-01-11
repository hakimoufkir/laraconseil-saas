using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.IServices
{
    public interface IRoleService
    {
        Task<bool> AssignRoleToUser(int userId, int roleId);
        Task<bool> CheckUserPermission(int userId, string permissionName);
    }
}
