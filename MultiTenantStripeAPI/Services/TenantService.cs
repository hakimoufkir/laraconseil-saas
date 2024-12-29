using System;
using System.Linq;
using MultiTenantStripeAPI.Data;
using MultiTenantStripeAPI.Models;
using MultiTenantStripeAPI.Services.interfaces;

namespace MultiTenantStripeAPI.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantDbContext _context;

        public TenantService(TenantDbContext context)
        {
            _context = context;
        }

        public Tenant CreateTenant(string tenantName, string email)
        {
            if (string.IsNullOrWhiteSpace(tenantName) || string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Tenant name and email must be provided.");
            }

            var existingTenant = _context.Tenants.FirstOrDefault(t => t.Email == email);
            if (existingTenant != null)
            {
                throw new InvalidOperationException($"A tenant with email '{email}' already exists.");
            }

            var tenant = new Tenant
            {
                TenantId = Guid.NewGuid().ToString(),
                TenantName = tenantName,
                Email = email,
                SubscriptionStatus = "Pending"
            };

            _context.Tenants.Add(tenant);
            _context.SaveChanges();
            return tenant;
        }

        public Tenant GetTenantByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email must be provided.");
            }

            return _context.Tenants.FirstOrDefault(t => t.Email == email)
                   ?? throw new InvalidOperationException($"No tenant found with email '{email}'.");
        }

        public Tenant GetTenantById(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided.");
            }

            return _context.Tenants.FirstOrDefault(t => t.TenantId == tenantId)
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
            _context.SaveChanges();
        }

        public void DeleteTenant(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided.");
            }

            var tenant = GetTenantById(tenantId);
            _context.Tenants.Remove(tenant);
            _context.SaveChanges();
        }
    }
}
