using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Application.IUnitOfWork;
using MultiTenantStripeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork.IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork.IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> AssignRoleToUser(int userId, int roleId)
        {
            // Get all roles
            List<Role> roles = await _unitOfWork.RoleRepository.GetAllAsNoTracking();

            // Check if the role exists
            Role? role = roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found.");
            }

            // Create and assign the role to the user
            UserRole userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };

            await _unitOfWork.UserRoleRepository.CreateAsync(userRole);
            _unitOfWork.Commit();
            return true;
        }

        public async Task<bool> CheckUserPermission(int userId, string permissionName)
        {
            // Retrieve all roles for the user
            List<UserRole> userRoles = await _unitOfWork.UserRoleRepository.GetAllAsNoTracking();
            List<UserRole> userRolesForUser = userRoles.Where(ur => ur.UserId == userId).ToList();

            if (!userRolesForUser.Any())
            {
                throw new InvalidOperationException("User does not have any assigned roles.");
            }

            // Iterate through each role to check permissions
            foreach (UserRole userRole in userRolesForUser)
            {
                if (userRole.RoleId == 0)
                {
                    throw new InvalidOperationException("RoleId for the user role is missing.");
                }

                // Get permissions for the role
                List<RolePermission> rolePermissions = await _unitOfWork.RolePermissionRepository
                    .GetAllAsNoTracking(rp => rp.RoleId == userRole.RoleId);

                if (rolePermissions == null)
                {
                    throw new InvalidOperationException($"No RolePermissions found for RoleId: {userRole.RoleId}");
                }

                // Check if any permission matches the given permission name
                if (rolePermissions.Any(rp => rp.Permission != null && rp.Permission.Name == permissionName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
