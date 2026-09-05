namespace ClasificadorIA.Services;

public sealed class AiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }

    public AiException(int statusCode, string? errorCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}