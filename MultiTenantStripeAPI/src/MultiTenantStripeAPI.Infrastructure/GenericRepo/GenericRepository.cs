using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Application.IGenericRepo;
using System.Linq.Expressions;

namespace MultiTenantStripeAPI.Infrastructure.GenericRepo
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _entitySet;

        public GenericRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _entitySet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsNoTracking(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _entitySet.AsNoTracking();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        }

        public async Task<T?> GetAsNoTracking(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = _entitySet.AsNoTracking();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsTracking(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _entitySet.AsTracking();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        }

        public async Task<T?> GetAsTracking(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = _entitySet.AsTracking();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task CreateRangeAsync(ICollection<T> entities)
        {
            await _entitySet.AddRangeAsync(entities);
        }

        public async Task CreateAsync(T entity)
        {
            await _entitySet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            _entitySet.Update(entity);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(T entity)
        {
            _entitySet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
