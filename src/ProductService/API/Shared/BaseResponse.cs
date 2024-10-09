namespace ProductService.API.Shared
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public BaseResponse()
        {
            Success = true;
            Errors = new List<string>();
        }

        public static BaseResponse<T> SuccessResult(T data, string message = null)
        {
            return new BaseResponse<T> { Data = data, Message = message };
        }

        public static BaseResponse<T> FailureResult(string message, List<string> errors = null)
        {
            return new BaseResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
