using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set TenantId as the primary key for Tenant
            modelBuilder.Entity<Tenant>().HasKey(t => t.TenantId);

            // Configure relationship between User and Tenant
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure many-to-many relationship between User and Role using UserRole
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed tenants
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant
                {
                    TenantId = "tenant1-id",
                    TenantName = "Tenant One",
                    Email = "tenant1@example.com",
                    SubscriptionStatus = "Active",
                    PlanType = "Grower",
                    DatabaseConnectionString = "ConnectionStringForTenant1",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },
                new Tenant
                {
                    TenantId = "tenant2-id",
                    TenantName = "Tenant Two",
                    Email = "tenant2@example.com",
                    SubscriptionStatus = "Pending",
                    PlanType = "Station",
                    DatabaseConnectionString = "ConnectionStringForTenant2",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                }
            );

            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Grower", Description = "Grower role", TenantId = "tenant1-id" },
                new Role { Id = 2, Name = "StationAdmin", Description = "Station Admin role", TenantId = "tenant1-id" }
            );

            // Seed permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "ViewOwnData", Description = "View own farms and plots" },
                new Permission { Id = 2, Name = "SubmitTreatment", Description = "Submit treatments" },
                new Permission { Id = 3, Name = "RespondToAudit", Description = "Respond to audits" }
            );

            // Map roles to permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { Id = 1, RoleId = 1, PermissionId = 1 }, // Grower can ViewOwnData
                new RolePermission { Id = 2, RoleId = 1, PermissionId = 2 }, // Grower can SubmitTreatment
                new RolePermission { Id = 3, RoleId = 1, PermissionId = 3 }  // Grower can RespondToAudit
            );

            // Seed users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "John Doe",
                    Email = "john.doe@example.com",
                    TenantId = "tenant1-id"
                },
                new User
                {
                    Id = 2,
                    UserName = "Jane Smith",
                    Email = "jane.smith@example.com",
                    TenantId = "tenant1-id"
                },
                new User
                {
                    Id = 3,
                    UserName = "Emily Davis",
                    Email = "emily.davis@example.com",
                    TenantId = "tenant2-id"
                }
            );

            // Seed UserRoles (Assign roles to users)
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1 }, // John Doe as Grower
                new UserRole { Id = 2, UserId = 2, RoleId = 2 }, // Jane Smith as StationAdmin
                new UserRole { Id = 3, UserId = 3, RoleId = 1 }  // Emily Davis as Grower
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
