using System.Net;

namespace Application.Exceptions;

public class ForbiddenException :Exception
{
    public List<string>ErrorMessage { get; set; }
    public HttpStatusCode  StatusCode { get; set; }

    public ForbiddenException(List<string> errorMessage=default, HttpStatusCode statusCode =HttpStatusCode.Forbidden)
    {
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }
}