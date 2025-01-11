using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Roles.Queries.CheckPermission
{
    public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, bool>
    {
        private readonly IRoleService _roleService;

        public CheckPermissionQueryHandler(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        public async Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
        {
            return await _roleService.CheckUserPermission(request.UserId, request.PermissionName);
        }
    }
}
