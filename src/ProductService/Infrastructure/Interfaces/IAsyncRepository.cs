using System.Linq.Expressions;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IAsyncRepository<TEntity> where TEntity : class
    {
        Task<List<TEntity>> GetAllAsync(bool asNoTracking = true);
        Task<TEntity> GetByIdAsync(decimal id);
        Task<TEntity> AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> T);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task UpdateRangeAsync(IEnumerable<TEntity> T);
        Task DeleteRangeAsync(IEnumerable<TEntity> T);
        Task<TEntity> DeleteAsync(TEntity entity);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true);
        IQueryable<TEntity> WhereQueryable(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = true);
        Task<int> CommitChangesAsync();
    }
}
