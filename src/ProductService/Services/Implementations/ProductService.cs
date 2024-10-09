using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductService.API.DTOs.Requests;
using ProductService.API.DTOs.Responses;
using ProductService.API.Shared;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Configration;
using ProductService.Infrastructure.Interfaces;
using ProductService.Services.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ProductService.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _context;
        private readonly IValidator<CreateProductDto> _createProductValidator;
        private readonly IValidator<UpdateProductDto> _updateProductValidator;
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;


        public ProductService(
                ProductDbContext context
                , IValidator<CreateProductDto> productValidator
                , IValidator<UpdateProductDto> updateProductValidator
                , IUnitOfWork unitOfWork
            )
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _createProductValidator = productValidator;
            _updateProductValidator = updateProductValidator;
            _logger = Log.ForContext<ProductService>();
        }


        public async Task<BaseResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {

            try
            {            
                var sqlQuery = @"
                    SELECT p.Id, 
                           p.Name, 
                           p.Description, 
                           p.Price,
                           ISNULL((SELECT SUM(i.Quantity) FROM Inventories i WHERE i.ProductId = p.Id), 0) AS Quantity
                    FROM Products p";

                var productDtos = await _context.Products
                    .FromSqlRaw(sqlQuery)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Quantity = p.Inventories != null && p.Inventories.Any()
                                   ? p.Inventories.Sum(i => i.Quantity)
                                   : 0
                    })
                    .ToListAsync();


                _logger.Information("Retrieved {Count} products", productDtos.Count);
                return BaseResponse<IEnumerable<ProductDto>>.SuccessResult(productDtos);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while retrieving all products");
                return BaseResponse<IEnumerable<ProductDto>>.FailureResult("An error occurred while retrieving products");
            }
        }

        public async Task<BaseResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {     
                var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    _logger.Warning("Product with ID {ProductId} not found", id);
                    return BaseResponse<ProductDto>.FailureResult($"Product with id {id} not found");
                }

                var productDto = new ProductDto();
                productDto.MapToProductDto(product);

                _logger.Information("Retrieved product {@Product}", productDto);
                return BaseResponse<ProductDto>.SuccessResult(productDto);        
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while retrieving product with ID {ProductId}", id);
                return BaseResponse<ProductDto>.FailureResult("An error occurred while retrieving the product");
            }
        }

        public async Task<BaseResponse<ProductDto>> AddProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                var validationResult = await _createProductValidator.ValidateAsync(createProductDto);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("Product creation validation failed: {@ValidationErrors}", validationResult.Errors);
                    return BaseResponse<ProductDto>.FailureResult("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var product = new Product();
                createProductDto.MapToProduct(product);

                var createdProduct = await _unitOfWork.ProductRepository.AddAsync(product);
                await _unitOfWork.CommitAsync();

                var productDto = new ProductDto();
                productDto.MapToProductDto(createdProduct);

                _logger.Information("Created new product {@Product}", productDto);
                return BaseResponse<ProductDto>.SuccessResult(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while creating product {@ProductDto}", createProductDto);
                return BaseResponse<ProductDto>.FailureResult("An error occurred while creating the product");
            }
           
        }

        public async Task<BaseResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                if (id != updateProductDto.Id)
                {
                    _logger.Warning("Product ID mismatch: {@id}, {@updateProductDtoId}", id, updateProductDto.Id);
                    return BaseResponse<ProductDto>.FailureResult("Product ID mismatch", null);
                }

                var validationResult = await _updateProductValidator.ValidateAsync(updateProductDto);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("Product update validation failed: {@ValidationErrors}", validationResult.Errors);
                    return BaseResponse<ProductDto>.FailureResult("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var existingProduct = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == updateProductDto.Id);
                if (existingProduct == null)
                {
                    _logger.Warning("Attempted to update non-existent product with ID {ProductId}", id);
                    return BaseResponse<ProductDto>.FailureResult($"Product with id {id} not found");
                }

                var productDto = new ProductDto();
                productDto.MapToProductDto(existingProduct);

                await _unitOfWork.ProductRepository.UpdateAsync(existingProduct);
                await _unitOfWork.CommitAsync();

                _logger.Information("Updated product {@Product}", productDto);
                return BaseResponse<ProductDto>.SuccessResult(productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while updating product with ID {ProductId}", id);
                return BaseResponse<ProductDto>.FailureResult("An error occurred while updating the product");
            }
        }

        public async Task<BaseResponse<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    _logger.Warning("Attempted to delete non-existent product with ID {ProductId}", id);
                    return BaseResponse<bool>.FailureResult($"Product with id {id} not found");
                }

                await _unitOfWork.ProductRepository.DeleteAsync(product);
                await _unitOfWork.CommitAsync();

                _logger.Information("Deleted product with ID {ProductId}", id);
                return BaseResponse<bool>.SuccessResult(true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while deleting product with ID {ProductId}", id);
                return BaseResponse<bool>.FailureResult("An error occurred while deleting the product");
            }        
        }
    }
}
