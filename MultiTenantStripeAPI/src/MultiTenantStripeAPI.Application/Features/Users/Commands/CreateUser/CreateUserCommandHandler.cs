using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public CreateUserCommandHandler(IUserService userService, IRoleService roleService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Create the user
            User user = await _userService.CreateUser(request.UserName, request.Email);

            // Assign roles
            foreach (int roleId in request.RoleIds)
            {
                await _roleService.AssignRoleToUser(user.Id, roleId);
            }

            return user.Id; // Return the created user's ID
        }
    }
}
