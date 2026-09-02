namespace EKeyAuthenticationService.ViewModel
{
    public class ResultDto
    {
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
    }


    public class ApiResponse
    {
        public const int SUCCESS = 200;
        public const string SUCCESS_MESSAGE = "Success";

        public const int ERROR = 500;
        public const string ERROR_MESSAGE = "Error";


    }
}
