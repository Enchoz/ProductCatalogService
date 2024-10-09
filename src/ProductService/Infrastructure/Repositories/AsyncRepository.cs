using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure.Configration;
using ProductService.Infrastructure.Interfaces;
using System.Linq.Expressions;

namespace ProductService.Infrastructure.Repositories
{
    public class AsyncRepository<TEntity> : IAsyncRepository<TEntity> where TEntity : class
    {
        protected readonly ProductDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public AsyncRepository(ProductDbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public Task<List<TEntity>> GetAllAsync(bool asNoTracking = true)
        {
            return asNoTracking ? DbSet.AsNoTracking().ToListAsync() : DbSet.ToListAsync();
        }

        public Task<TEntity> GetByIdAsync(decimal id)
        {
            return DbSet.FindAsync(id).AsTask();
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
            //await CommitChangesAsync();
            return entity;
        }

        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            DbSet.Update(entity);
            return CommitChangesAsync().ContinueWith(_ => entity);
            //return entity;
        }

        public Task<TEntity> DeleteAsync(TEntity entity)
        {
            DbSet.Remove(entity);
            return CommitChangesAsync().ContinueWith(_ => entity);
        }

        public Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true)
        {
            return asNoTracking ? DbSet.Where(predicate).AsNoTracking().ToListAsync() : DbSet.Where(predicate).ToListAsync();
        }

        public IQueryable<TEntity> WhereQueryable(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true)
        {
            return asNoTracking ? DbSet.Where(predicate).AsNoTracking() : DbSet.Where(predicate);
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.SingleOrDefaultAsync(predicate);
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await DbSet.AddRangeAsync(entities);
            //await CommitChangesAsync();
        }

        public Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            DbSet.UpdateRange(entities);
            //return CommitChangesAsync();
            return default;
        }

        public Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            DbSet.RemoveRange(entities);
            //return CommitChangesAsync();
            return default;
        }

        public Task<int> CommitChangesAsync()
        {
            return Context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Context?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
