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

        public Tenant CreateTenant(string tenantName, string email)
        {
            if (string.IsNullOrWhiteSpace(tenantName) || string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Tenant name and email must be provided.");
            }

            Console.WriteLine($"Attempting to create tenant: {tenantName}, {email}");

            // Check if a tenant with the same email already exists
            var existingTenant = _unitOfWork.TenantRepository.GetByEmail(email);
            if (existingTenant != null)
            {
                Console.WriteLine($"Tenant with email {email} already exists.");
                throw new InvalidOperationException($"A tenant with email '{email}' already exists.");
            }

            // Create a new tenant
            var tenant = new Tenant
            {
                TenantId = Guid.NewGuid().ToString(),
                TenantName = tenantName,
                Email = email,
                SubscriptionStatus = "Pending"
            };

            // Dynamically create a database for the tenant
            var databaseName = $"Tenant_{tenant.TenantId}";
            var masterConnection = "Host=my-lc-postgres-server.postgres.database.azure.com;Database=postgres;Username=adminLaraconseil;Password=StrongPassword@123;";

            using (var connection = new NpgsqlConnection(masterConnection))
            {
                connection.Open();
                var createDbCommand = connection.CreateCommand();
                createDbCommand.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                createDbCommand.ExecuteNonQuery();
            }

            // Assign the database connection string to the tenant
            tenant.DatabaseConnectionString = $"Host=my-lc-postgres-server.postgres.database.azure.com;Database={databaseName};Username=adminLaraconseil;Password=StrongPassword@123;";

            // Save the tenant to the database
            _unitOfWork.TenantRepository.CreateAsync(tenant);
            _unitOfWork.Commit();

            Console.WriteLine($"Tenant created successfully with database: {tenant.TenantId}");
            return tenant;
        }

        public Tenant GetTenantByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email must be provided.");
            }

            return _unitOfWork.TenantRepository.GetByEmail(email)
                   ?? throw new InvalidOperationException($"No tenant found with email '{email}'.");
        }

        public Tenant GetTenantById(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided.");
            }

            return _unitOfWork.TenantRepository.GetByTenantId(tenantId)
                   ?? throw new InvalidOperationException($"No tenant found with ID '{tenantId}'.");
        }

        public void UpdateTenantStatus(Tenant tenant, string status)
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
            _unitOfWork.Commit();
        }

        public void DeleteTenant(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided.");
            }

            var tenant = GetTenantById(tenantId);
            _unitOfWork.TenantRepository.RemoveAsync(tenant);
            _unitOfWork.Commit();
        }

        
    }
}
