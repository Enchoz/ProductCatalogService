using ProductService.API.Shared.Responses;

namespace ProductService.API.DTOs.Swagger
{
    public class ProductDeletedExample
    {
        public static BaseResponse<bool> GetExample()
        {
            return new BaseResponse<bool>
            {
                IsSuccess = true,
                Message = "Product deleted successfully",
                Data = true,
                Errors = new List<string>()
            };
        }
    }

    public class ProductNotFoundExample
    {
        public static BaseResponse<bool> GetExample()
        {
            return new BaseResponse<bool>
            {
                IsSuccess = false,
                Message = "Product with id 20 not found",
                Data = false,
                Errors = new List<string>()
            };
        }
    }

    public class InvalidRequestExample
    {
        public static BaseResponse<bool> GetExample()
        {
            return new BaseResponse<bool>
            {
                IsSuccess = false,
                Message = "Invalid request",
                Data = false,
                Errors = new List<string> { "Invalid product ID" }
            };
        }
    }

}
