using Microsoft.EntityFrameworkCore;
using ProductService.API.DTOs.Requests;
using ProductService.Infrastructure.Configration;
using ProductService.API.Validators;
using ProductService.Infrastructure.Repositories;

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

            _productService = new Services.Implementations.ProductService(
                _context,
                createProductValidator,
                updateProductValidator,
                unitOfWork
            );
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsAllProducts()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count());
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