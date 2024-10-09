using ProductService.Domain.Entities;
using ProductService.Infrastructure.Configration;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductDbContext _context;
        private IAsyncRepository<Product> _productRepository;

        public UnitOfWork(ProductDbContext context)
        {
            _context = context;
        }

        public IAsyncRepository<Product> ProductRepository =>
            _productRepository ??= new AsyncRepository<Product>(_context);

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
