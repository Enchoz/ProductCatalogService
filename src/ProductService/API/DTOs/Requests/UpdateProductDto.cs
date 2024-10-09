using ProductService.Domain.Entities;

namespace ProductService.API.DTOs.Requests
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        internal void MapToProduct(Product existingProduct)
        {
            existingProduct.Name = Name;
            existingProduct.Price = Price;
            existingProduct.Description = Description;
        }
    }
}
