using ProductService.Domain.Entities;
using System.Text.Encodings.Web;

namespace ProductService.API.DTOs.Responses
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public void MapToProductDto(Product product)
        {
            if (product == null)
            {
                return;
            }

            Id = product.Id;
            Name = HtmlEncoder.Default.Encode(product.Name);
            Description = HtmlEncoder.Default.Encode(product.Description);
            Price = product.Price;
            Quantity = product.Inventories?.Sum(i => i.Quantity) ?? 0;
        }

        internal void MapToProduct(Product existingProduct)
        {
            existingProduct.Name = Name;
            existingProduct.Price = Price;
            existingProduct.Description = Description;
        }
    }
}
