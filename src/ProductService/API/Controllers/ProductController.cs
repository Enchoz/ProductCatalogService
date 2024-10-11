using Azure;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs.Requests;
using ProductService.API.DTOs.Responses;
using ProductService.API.DTOs.Swagger;
using ProductService.API.Shared.Helpers;
using ProductService.API.Shared.Responses;
using ProductService.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductService.API.Controllers
{
    /// <summary>
    /// Controller for managing product-related operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Initializes a new instance of the ProductController.
        /// </summary>
        /// <param name="productService">The product service dependency.</param>
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// Retrieves a paginated list of products.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The page size for pagination (default is 10).</param>
        /// <returns>A paginated list of products.</returns>
        [HttpGet("products")]
        [SwaggerOperation(Summary = "Get all products", Description = "Retrieves a paginated list of all products.")]
        [SwaggerResponse(200, "Successfully retrieved the list of products", typeof(PagedResult<ProductDto>))]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] GetProductsRequest request)
        {
            var products = await _productService.GetAllProductsAsync(request);
            return Ok(products);
        }

        /// <summary>
        /// Retrieves a specific product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The requested product.</returns>
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Get a product by ID", Description = "Retrieves a specific product using its ID.")]
        [SwaggerResponse(200, "Successfully retrieved the product", typeof(Response<ProductDto>))]
        [SwaggerResponse(404, "Product not found")]
        public async Task<ActionResult<Response<ProductDto>>> GetProduct(int id)
        {
            return Ok(await _productService.GetProductByIdAsync(id));
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="createProductDto">The product data to create.</param>
        /// <returns>The created product.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Create a new product", Description = "Creates a new product with the provided data.")]
        [SwaggerResponse(201, "Product created successfully", typeof(Response<ProductDto>))]
        [SwaggerResponse(400, "Invalid input")]
        public async Task<ActionResult<Response<ProductDto>>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            var response = await _productService.AddProductAsync(createProductDto);
            return response.IsSuccess
                ? CreatedAtAction(nameof(GetProduct), new { id = response.Data.Id }, response)
                : BadRequest(response);
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="updateProductDto">The updated product data.</param>
        /// <returns>The updated product.</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update an existing product", Description = "Updates an existing product with the provided data.")]
        [SwaggerResponse(200, "Product updated successfully", typeof(Response<ProductDto>))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Product not found")]
        public async Task<ActionResult<Response<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            var response = await _productService.UpdateProductAsync(id, updateProductDto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Deletes a specific product.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>A success message if the product was deleted.</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete a product", Description = "Deletes a specific product using its ID.")]
        [SwaggerResponse(200, "Product deleted successfully", typeof(BaseResponse<bool>))]
        [SwaggerResponse(404, "Product not found", typeof(BaseResponse<bool>))]
        [SwaggerResponse(400, "Invalid request", typeof(BaseResponse<bool>))]


        [SwaggerResponse(200, "Product deleted successfully", typeof(ProductDeletedExample))]
        [SwaggerResponse(404, "Product not found", typeof(ProductNotFoundExample))]
        [SwaggerResponse(400, "Invalid request", typeof(InvalidRequestExample))]
        public async Task<ActionResult<Response<string>>> DeleteProduct(int id)
        {
            var response = await _productService.DeleteProductAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}