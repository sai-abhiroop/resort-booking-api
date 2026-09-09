namespace ResortBooking.API.Dtos
{
    public class ApiResponse<TData>
    {
        public bool Success { get; init; }
        public int Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public TData? Data {  get; init; }
        public object? Errors {  get; init; }
        public DateTime TimeStamp { get; init; }= DateTime.UtcNow;

        public static ApiResponse<TData> Create(bool success,int status, string message, TData? data=default, object? errors = null)
        {
            return new ApiResponse<TData>()
            {
                Success= success,
                Status= status,
                Message= message,
                Data= data,
                Errors= errors
            };
        }
        public static ApiResponse<TData> Ok(TData? data=default,string message="Retrieved Successfully") => Create(true,200,message,data);

        public static ApiResponse<TData> CreatedAt(TData? data=default, string message = "Record Created Successfully") => Create(true, 201, message, data);

        public static ApiResponse<TData> NoContent(TData? data = default, string message = "No Content") => Create(true,204, message, data);

        public static ApiResponse<TData> BadRequest(string message = "Bad Request",object? errors=null) => Create(false, 400, message, errors:errors);

        public static ApiResponse<TData> NotFound(string message = "No Resource Found", object? errors = null) => Create(false, 404, message, errors: errors);

        public static ApiResponse<TData> Conflict(string message = "Conflicting Resource", object? errors = null) => Create(false, 409, message, errors: errors);

        public static ApiResponse<TData> Error(int status,string message = "Something Went Wrong!!", object? errors = null) => Create(false,status , message, errors: errors);
    }
}
