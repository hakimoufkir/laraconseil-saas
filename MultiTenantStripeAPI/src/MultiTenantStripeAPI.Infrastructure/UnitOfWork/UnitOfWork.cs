using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Application.IUnitOfWork;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;

namespace MultiTenantStripeAPI.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        public ITenantRepository TenantRepository { get; }

        public UnitOfWork(ApplicationDbContext dbContext, ITenantRepository tenantRepository)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            TenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        }
      


        public UnitOfWork(ApplicationDbContext context)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));          
        }

        public void Commit()
            => _dbContext.SaveChanges();

        public async Task CommitAsync()
            => await _dbContext.SaveChangesAsync();

        public void Rollback()
            => _dbContext.Dispose();

        public async Task RollbackAsync()
            => await _dbContext.DisposeAsync();
    }
}
