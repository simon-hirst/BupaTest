namespace BupaTest.Exceptions
{
    // Custom exception class for MOT API errors.
    // Includes an error code and status code for more detailed information.
    public class MotApiException : Exception
    {
        public MotApiException(string message, int statusCode, string? errorCode = null) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public int StatusCode { get; }
        public string? ErrorCode { get; }
    }
}