using Microsoft.AspNetCore.Http;
using MultiTenantStripeAPI.Application.IRepositories;
using MultiTenantStripeAPI.Application.IUnitOfWork;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;

namespace MultiTenantStripeAPI.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IHttpContextAccessor HttpContextAccessor;

        public ITenantRepository TenantRepository { get; }
        public IRoleRepository RoleRepository { get; }

        public IUserRoleRepository UserRoleRepository { get; }

        public IRolePermissionRepository RolePermissionRepository { get; }

        public IUserRepository UserRepository { get; }
        public UnitOfWork(ApplicationDbContext dbContext, ITenantRepository tenantRepository, IRoleRepository roleRepository, IUserRoleRepository  userRoleRepository, IRolePermissionRepository rolePermissionRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor
            )
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            TenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            RoleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            UserRoleRepository = userRoleRepository ?? throw new ArgumentNullException( nameof(userRoleRepository));
            RolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        }

        public string CurrentTenantId
        {
            get
            {
                string? tenantId = HttpContextAccessor.HttpContext?.Items["TenantId"]?.ToString();
                Console.WriteLine($"UnitOfWork: Retrieved TenantId: {tenantId}");
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new InvalidOperationException("Tenant ID is not available in the current context.");
                }
                return tenantId;
            }
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
