using Azure;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs.Requests;
using ProductService.API.Shared.DTOs;
using ProductService.Domain.Entities;
using ProductService.Services.Interfaces;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService
            )
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Product>>> GetProducts(int pageNumber = 1)
        {
            var products = await _productService.GetAllProductsAsync(pageNumber);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            return Ok(await _productService.GetProductByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto careteProductDto)
        {           

            var response = await _productService.AddProductAsync(careteProductDto);
            return response.IsSuccess ? CreatedAtAction(nameof(GetProduct), new { id = response.Data.Id }, response) : BadRequest(response);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            var response = await _productService.UpdateProductAsync(id, updateProductDto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {         
            var response = await _productService.DeleteProductAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}