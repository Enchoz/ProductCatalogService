using ProductService.API.DTOs.Requests;
using ProductService.API.DTOs.Responses;
using ProductService.Domain.Entities;

namespace ProductService.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> AddProductAsync(CreateProductDto product);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllProducts();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto product);
    }
}
