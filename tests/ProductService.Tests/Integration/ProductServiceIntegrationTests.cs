using Microsoft.EntityFrameworkCore;
using ProductService.API.DTOs.Requests;
using ProductService.Infrastructure.Configration;
using ProductService.API.Validators;
using ProductService.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ProductService.Tests.Integration
{
    public class ProductServiceIntegrationTests : IDisposable
    {
        private readonly ProductDbContext _context;
        private readonly Services.Implementations.ProductService _productService;

        public ProductServiceIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: "TestProductDb")
                .Options;

            _context = new ProductDbContext(options);
            var unitOfWork = new UnitOfWork(_context);
            var createProductValidator = new CreateProductDtoValidator();
            var updateProductValidator = new UpdateProductDtoValidator();
            var _mockCache = new Mock<IDistributedCache>();


            _productService = new Services.Implementations.ProductService(
                _context,
                createProductValidator,
                updateProductValidator,
                unitOfWork,
                _mockCache.Object
            );
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnEmptyListWhenNoProductsExist()
        {
            // Arrange
            var request = new GetProductsRequest
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetAllProductsAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0, result.Data.TotalCount);
            Assert.Equal(1, result.Data.PageNumber); 
            Assert.Equal(10, result.Data.PageSize);
            Assert.Equal(0, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsAllProducts()
        {
            // Arrange
            await SeedTestData();
            var request = new GetProductsRequest
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _productService.GetAllProductsAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Items.Count());
            Assert.Equal(1, result.Data.PageNumber);
            Assert.Equal(10, result.Data.PageSize);
            Assert.Equal(1, result.Data.TotalPages);
        }

        [Fact]
        public async Task AddProduct_ShouldAddProductToDatabase()
        {
            // Arrange
            var product = new CreateProductDto { Name = "Product 1", Price = 10, Description = "Description 1" };
            var pageNumber = 1;
            var pageSize = 10;

            // Act
            await _productService.AddProductAsync(product);

            var request = new GetProductsRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _productService.GetAllProductsAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Data.Items);
            Assert.Equal("Product 1", result.Data.Items.First().Name);
        }


        [Fact]
        public async Task AddProductAsync_AddsProductToDatabase()
        {
            // Arrange
            var createProductDto = new CreateProductDto
            {
                Name = "New Test Product",
                Description = "Test Description",
                Price = 15.99m
            };

            // Act
            var result = await _productService.AddProductAsync(createProductDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(await _context.Products.FirstOrDefaultAsync(p => p.Name == "New Test Product"));
        }

        private async Task SeedTestData()
        {
            await _context.Products.AddRangeAsync(
                new Domain.Entities.Product { Name = "Test Product 1", Price = 10.99m, Description = "Description 1" },
                new Domain.Entities.Product { Name = "Test Product 2", Price = 20.99m, Description = "Description 2" }
            );
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}