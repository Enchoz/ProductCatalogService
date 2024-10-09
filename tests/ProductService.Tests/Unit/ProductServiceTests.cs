using Moq;
using FluentValidation;
using FluentValidation.Results;
using ProductService.API.DTOs.Requests;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using ProductService.Infrastructure.Configration;
using ProductService.API.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;

namespace ProductService.Tests.Unit
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IValidator<CreateProductDto>> _mockCreateProductValidator;
        private readonly Mock<IValidator<UpdateProductDto>> _mockUpdateProductValidator;
        //private readonly Mock<ProductDbContext> _mockDbContext;
        private readonly Services.Implementations.ProductService _productService;
        private readonly ProductDbContext _context;


        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCreateProductValidator = new Mock<IValidator<CreateProductDto>>();
            _mockUpdateProductValidator = new Mock<IValidator<UpdateProductDto>>();
            ////_mockDbContext = new Mock<ProductDbContext>();

            //_productService = new Services.Implementations.ProductService(
            //    //_mockDbContext.Object,
            //    _mockCreateProductValidator.Object,
            //    _mockUpdateProductValidator.Object,
            //    _mockUnitOfWork.Object
            //);



            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: "TestProductDb")
                .Options;
            _context = new ProductDbContext(options);
            SeedDatabase();

            // Initialize the service with the context
            _productService = new Services.Implementations.ProductService(
                _context,
                _mockCreateProductValidator.Object,
                _mockUpdateProductValidator.Object,
                _mockUnitOfWork.Object
                );
        }

        private void SeedDatabase()
        {
            var products = new List<Product>
            {
                new Product { /*Id = 1,*/ Name = "Product 1", Price = 10, Description = "Description 1" },
                new Product {/* Id = 2,*/ Name = "Product 2", Price = 20, Description = "Description 2" }
            };

            _context.Products.AddRange(products);
            _context.SaveChanges();
        }



        //[Fact]
        //public async Task GetAllProducts_ShouldReturnListOfProducts()
        //{
        //    // Arrange
        //    var products = new List<ProductDto>
        //    {
        //        new ProductDto { Id = 1, Name = "Product 1", Price = 10, Quantity = 100 },
        //        new ProductDto { Id = 2, Name = "Product 2", Price = 20, Quantity = 200 }
        //    };
        //    _mockUnitOfWork.Setup(repo => repo.ProductRepository.GetAllAsync())
        //        .ReturnsAsync(products);

        //    // Act
        //    var result = await _productService.GetAllProductsAsync();

        //    // Assert
        //    Assert.True(result.IsSuccess);
        //    Assert.Equal(2, result.Data.Count());
        //    Assert.Equal("Product 1", result.Data.First().Name);
        //}

        [Fact]
        public async Task GetProductByIdAsync_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product" };
            _mockUnitOfWork.Setup(uow => uow.ProductRepository.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(productId, result.Data.Id);
            Assert.Equal("Test Product", result.Data.Name);
        }

        [Fact]
        public async Task AddProductAsync_ValidProduct_ReturnsSuccess()
        {
            // Arrange
            var createProductDto = new CreateProductDto { Name = "New Product", Price = 10.99m };
            _mockCreateProductValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateProductDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _mockUnitOfWork.Setup(uow => uow.ProductRepository.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync(new Product { Id = 1, Name = "New Product", Price = 10.99m });

            // Act
            var result = await _productService.AddProductAsync(createProductDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("New Product", result.Data.Name);
            Assert.Equal(10.99m, result.Data.Price);
        }


    }
}