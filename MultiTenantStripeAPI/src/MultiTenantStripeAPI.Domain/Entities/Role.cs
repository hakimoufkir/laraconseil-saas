namespace MultiTenantStripeAPI.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TenantId { get; set; } = string.Empty; // Scoped to a tenant
        public ICollection<UserRole> UserRoles { get; set; } // Navigation property
    }
}