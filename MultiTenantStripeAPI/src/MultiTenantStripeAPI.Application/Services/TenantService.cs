using System;
using Npgsql;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork.IUnitOfWork _unitOfWork;

        public TenantService(IUnitOfWork.IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Tenant> CreateTenantAsync(string tenantId, string tenantName, string email, string planType)
        {
            // Ensure all required fields are provided
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(tenantName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(planType))
            {
                throw new ArgumentException("TenantId, TenantName, email, and plan type must be provided.");
            }

            Console.WriteLine($"Attempting to create tenant with TenantId: {tenantId}, TenantName: {tenantName}, Email: {email}, PlanType: {planType}");

            // Check if a tenant with the same email already exists
            var existingTenant = _unitOfWork.TenantRepository.GetByEmail(email);
            if (existingTenant != null)
            {
                Console.WriteLine($"Tenant with email {email} already exists.");
                throw new InvalidOperationException($"A tenant with email '{email}' already exists.");
            }

            // Create a new tenant with the provided details
            var tenant = new Tenant
            {
                TenantId = tenantId, // Use the provided TenantId
                TenantName = tenantName, // Set the TenantName
                Email = email,
                PlanType = planType,  // Ensure PlanType is set here
                SubscriptionStatus = "Pending"  // Default subscription status
            };

            // Dynamically create a database for the tenant
            var databaseName = $"Tenant_{tenant.TenantId}";
            var masterConnection = "Host=my-lc-postgres-server.postgres.database.azure.com;Database=postgres;Username=adminLaraconseil;Password=StrongPassword@123;";

            try
            {
                using (var connection = new NpgsqlConnection(masterConnection))
                {
                    connection.Open();
                    var createDbCommand = connection.CreateCommand();
                    createDbCommand.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                    createDbCommand.ExecuteNonQuery();
                    Console.WriteLine($"Database '{databaseName}' created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database for tenant {tenant.TenantName}: {ex.Message}");
                throw; // Propagate the error
            }

            // Assign the database connection string to the tenant
            tenant.DatabaseConnectionString = $"Host=my-lc-postgres-server.postgres.database.azure.com;Database={databaseName};Username=adminLaraconseil;Password=StrongPassword@123;";

            // Save the tenant to the database
            await _unitOfWork.TenantRepository.CreateAsync(tenant); // Ensure async is awaited
            await _unitOfWork.CommitAsync(); // Ensure commit is also awaited

            Console.WriteLine($"Tenant created successfully with database: {tenant.TenantId}");
            return tenant;
        }

        public async Task<Tenant> GetTenantByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email must be provided.");
            }

            var tenant = _unitOfWork.TenantRepository.GetByEmail(email);
            if (tenant == null)
            {
                Console.WriteLine($"No tenant found with email '{email}'.");
            }
            return tenant;
        }

        public async Task<Tenant> GetTenantByIdAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided.");
            }

            return _unitOfWork.TenantRepository.GetByTenantId(tenantId)
                   ?? throw new InvalidOperationException($"No tenant found with ID '{tenantId}'.");
        }

        public async Task UpdateTenantStatusAsync(Tenant tenant, string status)
        {
            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant), "Tenant must be provided.");
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Status must be provided.");
            }

            tenant.SubscriptionStatus = status;
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteTenantAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided.");
            }

            var tenant = await GetTenantByIdAsync(tenantId);
            await _unitOfWork.TenantRepository.RemoveAsync(tenant);
            await _unitOfWork.CommitAsync();
        }


        public async Task<Tenant> GetTenantByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email must be provided.");
            }

            var tenant = _unitOfWork.TenantRepository.GetByEmail(email);
            if (tenant == null)
            {
                Console.WriteLine($"No tenant found with email '{email}'.");
            }
            return tenant;
        }

        public async Task<Tenant> GetTenantById(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided.");
            }

            return _unitOfWork.TenantRepository.GetByTenantId(tenantId)
                   ?? throw new InvalidOperationException($"No tenant found with ID '{tenantId}'.");
        }
    }
}
