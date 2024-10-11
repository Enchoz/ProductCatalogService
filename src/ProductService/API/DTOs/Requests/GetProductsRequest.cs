using ProductService.Contracts.Interfaces;

namespace ProductService.API.DTOs.Requests
{
    public class GetProductsRequest : IPaginationRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }


        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
