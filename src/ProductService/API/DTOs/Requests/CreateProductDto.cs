using ProductService.Domain.Entities;

namespace ProductService.API.DTOs.Requests
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        internal void MapToProduct(Product product)
        {
            product.Name = Name;
            product.Price = Price;
            product.Description = Description;
        }
    }
}
