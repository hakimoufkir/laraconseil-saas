using MultiTenantStripeAPI.Application.IRepositories;

namespace MultiTenantStripeAPI.Application.IUnitOfWork
{
    public interface IUnitOfWork
    {
        ITenantRepository TenantRepository { get; }

        IRoleRepository RoleRepository { get; }

        IUserRoleRepository UserRoleRepository { get; }

        IRolePermissionRepository RolePermissionRepository { get; }

        IUserRepository UserRepository { get; }

        string CurrentTenantId { get; }

        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
