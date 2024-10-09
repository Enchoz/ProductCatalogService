using Microsoft.EntityFrameworkCore;
using ProductService.API.DTOs.Requests;
using ProductService.API.DTOs.Responses;
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

        public ProductService(IUnitOfWork unitOfWork
                , ProductDbContext context
            )
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ProductDto> AddProductAsync(CreateProductDto createProductDto)
        {
            var product = new Product();
            createProductDto.MapToProduct(product);

            var createdProduct = await _unitOfWork.ProductRepository.AddAsync(product);
            await _unitOfWork.CommitAsync();

            var productDto = new ProductDto();
            productDto.MapToProductDto(createdProduct);

            return productDto;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProducts()
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

            return productDtos;
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }
            var productDto = new ProductDto();
            productDto.MapToProductDto(product);
            return productDto;
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            if (id != updateProductDto.Id)
            {
                throw new ArgumentException("Product ID mismatch");
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

            return productDto;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.SingleOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            await _unitOfWork.ProductRepository.DeleteAsync(product);
            await _unitOfWork.CommitAsync();
        }
    }
}
