using ProductService.Domain.Entities;

namespace ProductService.Services.Interfaces
{
    public interface IProductService
    {
        Task<Product> AddProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<Product>> GetAllProducts();
        Task<Product> GetProductByIdAsync(int id);
        Task UpdateProductAsync(Product product);
    }
}
