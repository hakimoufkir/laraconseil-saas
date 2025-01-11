using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Roles.Commands.AssignRole
{
    public class AssignRoleCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
