namespace EMedicineBE.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success")
            => new() { Success = true, StatusCode = 200, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message, int statusCode = 100)
            => new() { Success = false, StatusCode = statusCode, Message = message };
    }
}