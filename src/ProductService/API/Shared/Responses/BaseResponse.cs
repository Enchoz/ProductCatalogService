namespace ProductService.API.Shared.Responses
{
    public class BaseResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public BaseResponse()
        {
            IsSuccess = true;
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
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
