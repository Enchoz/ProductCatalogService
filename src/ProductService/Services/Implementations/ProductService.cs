using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductService.API.DTOs.Requests;
using ProductService.API.DTOs.Responses;
using ProductService.API.Shared;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Configration;
using ProductService.Infrastructure.Interfaces;
using ProductService.Services.Interfaces;

namespace ProductService.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProductDbContext _context;
        private readonly IValidator<CreateProductDto> _createProductValidator;
        private readonly IValidator<UpdateProductDto> _updateProductValidator;

        public ProductService(IUnitOfWork unitOfWork
                , ProductDbContext context
                , IValidator<CreateProductDto> productValidator
                , IValidator<UpdateProductDto> updateProductValidator
            )
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _createProductValidator = productValidator;
            _updateProductValidator = updateProductValidator;
        }


        public async Task<BaseResponse<ProductDto>> AddProductAsync(CreateProductDto createProductDto)
        {
            var validationResult = await _createProductValidator.ValidateAsync(createProductDto);
            if (!validationResult.IsValid)
            {
                return BaseResponse<ProductDto>.FailureResult("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var product = new Product();
            createProductDto.MapToProduct(product);

            var createdProduct = await _unitOfWork.ProductRepository.AddAsync(product);
            await _unitOfWork.CommitAsync();

            var productDto = new ProductDto();
            productDto.MapToProductDto(createdProduct);

            return BaseResponse<ProductDto>.SuccessResult(productDto, "Product created successfully");
        }

        public async Task<BaseResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            var sqlQuery = @"
                SELECT p.Id, 
                       p.Name, 
                       p.Description, 
                       p.Price,
                       ISNULL((SELECT SUM(i.Quantity) FROM Inventories i WHERE i.ProductId = p.Id), 0) AS Quantity
                FROM Products p";

            var productDtos = new List<ProductDto>();

            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var productDto = new ProductDto
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                            };
                            productDtos.Add(productDto);
                        }
                    }
                }
            }

            return BaseResponse<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }

        public async Task<BaseResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return BaseResponse<ProductDto>.FailureResult($"Product with id {id} not found");
            }

            var productDto = new ProductDto();
            productDto.MapToProductDto(product);
            return BaseResponse<ProductDto>.SuccessResult(productDto);
        }

        public async Task<BaseResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            if (id != updateProductDto.Id)
            {
                return BaseResponse<ProductDto>.FailureResult("Product ID mismatch", null);
            }

            var validationResult = await _updateProductValidator.ValidateAsync(updateProductDto);
            if (!validationResult.IsValid)
            {
                return BaseResponse<ProductDto>.FailureResult("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var existingProduct = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == updateProductDto.Id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }
            var productDto = new ProductDto();
            productDto.MapToProductDto(existingProduct); 

            await _unitOfWork.ProductRepository.UpdateAsync(existingProduct);
            await _unitOfWork.CommitAsync();

            return BaseResponse<ProductDto>.SuccessResult(productDto, "Product updated successfully");
        }

        public async Task<BaseResponse<bool>> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return BaseResponse<bool>.FailureResult($"Product with id {id} not found");
            }

            await _unitOfWork.ProductRepository.DeleteAsync(product);
            await _unitOfWork.CommitAsync();

            return BaseResponse<bool>.SuccessResult(true, "Product deleted successfully");

        }
    }
}
