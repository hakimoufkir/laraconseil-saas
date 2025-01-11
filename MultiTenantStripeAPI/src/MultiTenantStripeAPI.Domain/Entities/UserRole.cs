namespace MultiTenantStripeAPI.Domain.Entities
{
    public class UserRole : BaseEntity
    {
        public int UserId { get; set; } // Links to Tenant
        public User User { get; set; } // Navigation property
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}