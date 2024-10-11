using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.API.DTOs.Requests;
using ProductService.API.DTOs.Responses;
using ProductService.API.Shared.Responses;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Configration;
using ProductService.Infrastructure.Interfaces;
using ProductService.Services.Interfaces;
using Serilog;
using System.Text.Json;
using System.Text.Encodings.Web;
using ILogger = Serilog.ILogger;
using ProductService.API.Validators;
using ProductService.API.Shared.Helpers;

namespace ProductService.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _context;
        private readonly IValidator<CreateProductDto> _createProductValidator;
        private readonly IValidator<UpdateProductDto> _updateProductValidator;
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;

        public ProductService(
            ProductDbContext context,
            IValidator<CreateProductDto> productValidator,
            IValidator<UpdateProductDto> updateProductValidator,
            IUnitOfWork unitOfWork,
            IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _createProductValidator = productValidator;
            _updateProductValidator = updateProductValidator;
            _logger = Log.ForContext<ProductService>();
            _cache = cache;
        }

        public async Task<BaseResponse<PagedResult<ProductDto>>> GetAllProductsAsync(GetProductsRequest request)
        {
            try
            {
                var validator = new GetProductsRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return BaseResponse<PagedResult<ProductDto>>.FailureResult(
                        string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                    );
                }

                string cacheKey = $"products_page_{request.PageNumber}_size_{request.PageSize}";
                string cachedResult = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    var cachedProducts = JsonSerializer.Deserialize<PagedResult<ProductDto>>(cachedResult);
                    return BaseResponse<PagedResult<ProductDto>>.SuccessResult(cachedProducts);
                }

                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    query = query.Where(p => p.Name.Contains(request.Name));
                }

                if (!string.IsNullOrWhiteSpace(request.Description))
                {
                    query = query.Where(p => p.Description.Contains(request.Description));
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

                var productDtos = await query
                    .Include(p => p.Inventories)
                    .OrderBy(p => p.Id)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = HtmlEncoder.Default.Encode(p.Name),
                        Description = HtmlEncoder.Default.Encode(p.Description),
                        Price = p.Price,
                        Quantity = p.Inventories != null && p.Inventories.Any()
                                   ? p.Inventories.Sum(i => i.Quantity)
                                   : 0
                    })
                    .ToListAsync();

                var pagedResult = new PagedResult<ProductDto>(productDtos.AsQueryable(), request.PageNumber, request.PageSize, totalItems);      

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(pagedResult), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                _logger.Information("Retrieved {Count} products for page {Page} and size {Size}", productDtos.Count, request.PageNumber, request.PageSize);
                return BaseResponse<PagedResult<ProductDto>>.SuccessResult(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while retrieving products for page {Page} and size {Size}", request.PageNumber, request.PageSize);
                return BaseResponse<PagedResult<ProductDto>>.FailureResult("An error occurred while retrieving products");
            }
        }

        public async Task<BaseResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BaseResponse<ProductDto>.FailureResult("Invalid product ID");
                }

                string cacheKey = $"product_{id}";
                string cachedResult = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedResult))
                {
                    var cachedProduct = JsonSerializer.Deserialize<ProductDto>(cachedResult);
                    return BaseResponse<ProductDto>.SuccessResult(cachedProduct);
                }

                var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    _logger.Warning("Product with ID {ProductId} not found", id);
                    return BaseResponse<ProductDto>.FailureResult($"Product with id {id} not found");
                }

                var productDto = new ProductDto();
                productDto.MapToProductDto(product);

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(productDto), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

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

                // Invalidate cache for product list
                await _cache.RemoveAsync("products_page_1");

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

                updateProductDto.MapToProduct(existingProduct);

                await _unitOfWork.ProductRepository.UpdateAsync(existingProduct);
                await _unitOfWork.CommitAsync();

                var productDto = new ProductDto();
                productDto.MapToProductDto(existingProduct);

                // Invalidate cache for this product and product list
                await _cache.RemoveAsync($"product_{id}");
                await _cache.RemoveAsync("products_page_1");

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
                if (id <= 0)
                {
                    return BaseResponse<bool>.FailureResult("Invalid product ID");
                }

                var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    _logger.Warning("Attempted to delete non-existent product with ID {ProductId}", id);
                    return BaseResponse<bool>.FailureResult($"Product with id {id} not found");
                }

                await _unitOfWork.ProductRepository.DeleteAsync(product);
                await _unitOfWork.CommitAsync();

                // Invalidate cache for this product and product list
                await _cache.RemoveAsync($"product_{id}");
                await _cache.RemoveAsync("products_page_1");

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