using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAsyncRepository<Product> ProductRepository { get; }

        Task<int> CommitAsync();
    }
}
