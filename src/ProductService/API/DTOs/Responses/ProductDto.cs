using ProductService.Domain.Entities;

namespace ProductService.API.DTOs.Responses
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public ProductDto MapToProductDto(Product product)
        {
            if (product == null)
            {
                return null;
            }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = (int)(product.Inventories?.Sum(inventory => inventory.Quantity))
            };
        }

        internal void MapToProduct(Product existingProduct)
        {
            existingProduct.Name = Name;
            existingProduct.Price = Price;
            existingProduct.Description = Description;
        }
    }
}
