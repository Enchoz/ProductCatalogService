using ProductService.API.DTOs.Requests;
using ProductService.API.DTOs.Responses;
using ProductService.API.Shared.DTOs;
using ProductService.API.Shared.Responses;

namespace ProductService.Services.Interfaces
{
    public interface IProductService
    {
        Task<BaseResponse<PagedResult<ProductDto>>> GetAllProductsAsync(int page);
        Task<BaseResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<BaseResponse<ProductDto>> AddProductAsync(CreateProductDto createProductDto);
        Task<BaseResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<BaseResponse<bool>> DeleteProductAsync(int id);
    }
}
