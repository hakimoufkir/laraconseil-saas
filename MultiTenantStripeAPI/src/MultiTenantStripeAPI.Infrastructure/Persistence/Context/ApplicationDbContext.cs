using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Tenant entity
            modelBuilder.Entity<Tenant>().HasIndex(t => t.TenantId).IsUnique();

            // Seed data for Tenants
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant
                {
                    Id = 1,
                    TenantId = "tenant1-id",
                    TenantName = "Tenant One",
                    Email = "tenant1@example.com",
                    SubscriptionStatus = "Active"
                },
                new Tenant
                {
                    Id = 2,
                    TenantId = "tenant2-id",
                    TenantName = "Tenant Two",
                    Email = "tenant2@example.com",
                    SubscriptionStatus = "Pending"
                },
                new Tenant
                {
                    Id = 3,
                    TenantId = "tenant3-id",
                    TenantName = "Tenant Three",
                    Email = "tenant3@example.com",
                    SubscriptionStatus = "Expired"
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
