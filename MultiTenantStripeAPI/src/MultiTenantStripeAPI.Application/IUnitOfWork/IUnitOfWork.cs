using MultiTenantStripeAPI.Application.IRepositories;

namespace MultiTenantStripeAPI.Application.IUnitOfWork
{
    public interface IUnitOfWork
    {
        ITenantRepository TenantRepository { get; }
        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
