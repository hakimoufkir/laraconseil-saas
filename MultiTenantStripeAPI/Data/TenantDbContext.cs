using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Models;

namespace MultiTenantStripeAPI.Data
{
    public class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
    {
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>().HasIndex(t => t.TenantId).IsUnique();
            base.OnModelCreating(modelBuilder);
        }
    }
}
