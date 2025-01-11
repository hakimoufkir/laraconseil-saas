using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Roles.Queries.CheckPermission
{
    public class CheckPermissionQuery : IRequest<bool>
    {
        public int UserId { get; set; }
        public string PermissionName { get; set; }
    }
}
