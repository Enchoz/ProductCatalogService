using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs.Requests;
using ProductService.Domain.Entities;
using ProductService.Services.Interfaces;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IValidator<CreateProductDto> _createProductValidator;
        private readonly IValidator<UpdateProductDto> _updateProductValidator;

        public ProductController(IProductService productService
            , IValidator<CreateProductDto> productValidator
            , IValidator<UpdateProductDto> updateProductValidator
            )
        {
            _productService = productService;
            _createProductValidator = productValidator;
            _updateProductValidator = updateProductValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productService.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto careteProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdProduct = await _productService.AddProductAsync(careteProductDto);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {        
            await _productService.UpdateProductAsync(id, updateProductDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var existingProduct = await _productService.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}