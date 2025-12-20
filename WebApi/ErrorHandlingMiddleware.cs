using System.Net;
using System.Text.Json;
using Application.Exceptions;
using Application.Wrappers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace WebApi;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception ex)
        {
            // Log the actual exception
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            
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
                    // In development, show the actual error message
                    if (_environment.IsDevelopment())
                    {
                        responseWrapper.Messages = new List<string> 
                        { 
                            ex.Message,
                            ex.InnerException?.Message ?? string.Empty,
                            ex.StackTrace ?? string.Empty
                        }.Where(m => !string.IsNullOrEmpty(m)).ToList();
                    }
                    else
                    {
                        responseWrapper.Messages = new List<string> { "Something went wrong . Contact Administrator." };
                    }
                    break;
            }
            
            var result = JsonSerializer.Serialize(responseWrapper);
            await response.WriteAsync(result);
        }
    }
}