using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain.Entities
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; } 

        public int Quantity { get; set; }

        public virtual Product Product { get; set; }
    }
}
