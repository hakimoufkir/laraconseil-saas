using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork.IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork.IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Creates a new user within a tenant.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="email">Email address of the user.</param>
        /// <returns>The created user.</returns>
        public async Task<User> CreateUser(string userName, string email)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("UserName and Email are required.");
            }

            // Validate tenant context
            string tenantId = _unitOfWork.CurrentTenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("Tenant ID is not set.");
            }

            // Check if the email is already associated with a user in the same tenant
            User existingUser = await _unitOfWork.UserRepository.GetAsNoTracking(u => u.Email == email && u.TenantId == tenantId);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"A user with the email '{email}' already exists in this tenant.");
            }

            // Create the user
            User newUser = new User
            {
                UserName = userName,
                Email = email,
                TenantId = tenantId // Ensure the user belongs to the current tenant
            };

            await _unitOfWork.UserRepository.CreateAsync(newUser);
            _unitOfWork.Commit();

            return newUser;
        }

        /// <summary>
        /// Retrieves a user by their email within the current tenant.
        /// </summary>
        /// <param name="email">Email address of the user.</param>
        /// <returns>The user if found.</returns>
        public async Task<User> GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.");
            }

            // Validate tenant context
            string tenantId = _unitOfWork.CurrentTenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("Tenant ID is not set.");
            }

            User user = await _unitOfWork.UserRepository.GetAsNoTracking(u => u.Email == email && u.TenantId == tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No user found with the email '{email}' in this tenant.");
            }

            return user;
        }

        /// <summary>
        /// Retrieves all users belonging to the current tenant.
        /// </summary>
        /// <returns>A list of users in the current tenant.</returns>
        public async Task<List<User>> GetUsersByTenant()
        {
            // Validate tenant context
            string tenantId = _unitOfWork.CurrentTenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("Tenant ID is not set.");
            }

            List<User> users = await _unitOfWork.UserRepository.GetAllAsNoTracking(u => u.TenantId == tenantId);
            return users;
        }

        /// <summary>
        /// Deletes a user by their ID within the current tenant.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        public async Task DeleteUser(int userId)
        {
            // Validate tenant context
            string tenantId = _unitOfWork.CurrentTenantId;
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("Tenant ID is not set.");
            }

            // Retrieve the user
            User user = await _unitOfWork.UserRepository.GetAsTracking(u => u.Id == userId && u.TenantId == tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} does not exist in this tenant.");
            }

            // Delete the user
            await _unitOfWork.UserRepository.RemoveAsync(user);
            _unitOfWork.Commit();
        }
    }
}
