using System.Net;
using System.Text.Json;
using Application.Exceptions;
using Application.Wrappers;

namespace WebApi;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception ex)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            var responseWrapper= ResponseWrapper.Fail();

            switch (ex)
            {
                case ConflictException ce:
                    response.StatusCode = (int)ce.StatusCode;
                    responseWrapper.Messages = ce.ErrorMessage;
                    break;
                case ForbiddenException fe:
                    response.StatusCode = (int)fe.StatusCode;
                    responseWrapper.Messages=fe.ErrorMessage;
                    break;
                case IdentityException ie :
                    response.StatusCode = (int)ie.StatusCode;
                    responseWrapper.Messages=ie.ErrorMessage;
                    break;
                case NotFoundException nfe:
                    response.StatusCode = (int)nfe.StatusCode;
                    responseWrapper.Messages = nfe.ErrorMessage;
                    break;
                case UnauthorizedException ue:
                    response.StatusCode = (int)ue.StatusCode;
                    responseWrapper.Messages = ue.ErrorMessages;
                    break;
                
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseWrapper.Messages=["Something went wrong . Contact Administrator."];
                    break;
            }
            
            var result = JsonSerializer.Serialize(responseWrapper);
            await response.WriteAsync(result);
        }
    }
}