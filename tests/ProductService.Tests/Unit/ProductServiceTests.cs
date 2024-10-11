using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using ProductService.API.DTOs.Requests;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Configration;
using ProductService.Infrastructure.Interfaces;

namespace ProductService.Tests.Unit
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IValidator<CreateProductDto>> _mockCreateProductValidator;
        private readonly Mock<IValidator<UpdateProductDto>> _mockUpdateProductValidator;
        private readonly Mock<IDistributedCache> _mockCache;

        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCreateProductValidator = new Mock<IValidator<CreateProductDto>>();
            _mockUpdateProductValidator = new Mock<IValidator<UpdateProductDto>>();
            _mockCache = new Mock<IDistributedCache>();
        }

        private ProductDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Unique DB for each test
                .Options;
            var context = new ProductDbContext(options);
            SeedDatabase(context);
            return context;
        }

        private void SeedDatabase(ProductDbContext context)
        {
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10, Description = "Description 1" },
            new Product { Id = 2, Name = "Product 2", Price = 20, Description = "Description 2" }
        };

            context.Products.AddRange(products);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetProductByIdAsync_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            var context = CreateDbContext();
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product", Description = "Test Desciption" };
            var productService = new Services.Implementations.ProductService(
                context, _mockCreateProductValidator.Object, _mockUpdateProductValidator.Object, _mockUnitOfWork.Object, _mockCache.Object
            );
            _mockUnitOfWork.Setup(uow => uow.ProductRepository.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
                .ReturnsAsync(product);

            // Act
            var result = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(productId, result.Data.Id);
            Assert.Equal("Test Product", result.Data.Name);
        }

        [Fact]
        public async Task UpdateProductAsync_ValidUpdate_ReturnsSuccess()
        {
            // Arrange
            var context = CreateDbContext();
            var productId = 1;
            var updateProductDto = new UpdateProductDto { Id = productId, Name = "Updated Product", Price = 15.99m, Description = "Test Desciption" };
            var productService = new Services.Implementations.ProductService(
               context, _mockCreateProductValidator.Object, _mockUpdateProductValidator.Object, _mockUnitOfWork.Object, _mockCache.Object
           );
            _mockUnitOfWork.Setup(uow => uow.ProductRepository.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
                .ReturnsAsync(new Product { Id = productId, Name = "Old Product", Price = 10 });

            _mockUpdateProductValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateProductDto>(), default))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await productService.UpdateProductAsync(productId, updateProductDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Updated Product", result.Data.Name);
            Assert.Equal(15.99m, result.Data.Price);
        }

        [Fact]
        public async Task AddProductAsync_ValidProduct_ReturnsSuccess()
        {
            // Arrange
            var context = CreateDbContext();
            var createProductDto = new CreateProductDto { Name = "New Product", Price = 10.99m, Description = "New Product Description" };
            var productService = new Services.Implementations.ProductService(
               context, _mockCreateProductValidator.Object, _mockUpdateProductValidator.Object, _mockUnitOfWork.Object, _mockCache.Object
           );
            _mockCreateProductValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateProductDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _mockUnitOfWork.Setup(uow => uow.ProductRepository.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync(new Product { Id = 1, Name = "New Product", Price = 10.99m, Description = "New Product Description" });

            // Act
            var result = await productService.AddProductAsync(createProductDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("New Product", result.Data.Name);
            Assert.Equal(10.99m, result.Data.Price);
        }

        [Fact]
        public async Task GetAllProductsAsync_ValidRequest_ReturnsPagedResult()
        {
            // Arrange
            var context = CreateDbContext();
            var productService = new Services.Implementations.ProductService(
                context, _mockCreateProductValidator.Object, _mockUpdateProductValidator.Object, _mockUnitOfWork.Object, _mockCache.Object
            );

            var request = new GetProductsRequest
            {
                PageNumber = 1,
                PageSize = 10 
            };

            // Act
            var result = await productService.GetAllProductsAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Items.Count());
            Assert.Equal(1, result.Data.PageNumber);
            Assert.Equal(10, result.Data.PageSize);
            Assert.Equal(1, result.Data.TotalPages);
        }
        [Fact]
        public async Task DeleteProductAsync_ExistingProduct_ReturnsSuccess()
        {
            // Arrange
            var context = CreateDbContext();
            var productService = new Services.Implementations.ProductService(
                context, _mockCreateProductValidator.Object, _mockUpdateProductValidator.Object, _mockUnitOfWork.Object, _mockCache.Object
            );

            var productId = 1;
            var product = new Product { Id = productId, Name = "Product to Delete" };

            _mockUnitOfWork.Setup(uow => uow.ProductRepository.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
                .ReturnsAsync(product);

            // Act
            var result = await productService.DeleteProductAsync(productId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductDoesNotExist_ReturnsFailure()
        {
            // Arrange
            var context = CreateDbContext();
            var productService = new Services.Implementations.ProductService(
                context, _mockCreateProductValidator.Object, _mockUpdateProductValidator.Object, _mockUnitOfWork.Object, _mockCache.Object
            );

            var productId = 99; // Non-existent product
            _mockUnitOfWork.Setup(uow => uow.ProductRepository.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await productService.DeleteProductAsync(productId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"Product with id {productId} not found", result.Message);
        }
    }
}