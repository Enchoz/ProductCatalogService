namespace ProductService.Contracts.Interfaces
{
    public interface IPaginationRequest
    {
        int PageNumber { get; set; }
        int PageSize { get; set; }
    }

}
