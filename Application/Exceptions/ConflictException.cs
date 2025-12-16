using System.Net;

namespace Application.Exceptions;

public class ConflictException :Exception
{
    public List<string>ErrorMessage { get; set; }
    public HttpStatusCode  StatusCode { get; set; }

    public ConflictException(List<string> errorMessage=default, HttpStatusCode statusCode =HttpStatusCode.Conflict)
    {
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    } 
}