using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Namotion.Reflection;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Infrastructure.OpenApi;

public class SwaggerGlobalAuthProcessor(string scheme):IOperationProcessor
{
    private readonly string _scheme = scheme;

    public SwaggerGlobalAuthProcessor():this(JwtBearerDefaults.AuthenticationScheme)
    {
        
    }
    public bool Process(OperationProcessorContext context)
    {
        if (context is not AspNetCoreOperationProcessorContext aspNetContext)
            return true;

        var metadata = aspNetContext.ApiDescription
            .ActionDescriptor
            .TryGetPropertyValue<IList<object>>("EndpointMetadata");

        if (metadata is null)
            return true;

        // لو endpoint عليه AllowAnonymous → مفيش Security
        if (metadata.OfType<AllowAnonymousAttribute>().Any())
            return true;

        // لو مفيش security متضافة قبل كده
        if (context.OperationDescription.Operation.Security == null ||
            context.OperationDescription.Operation.Security.Count == 0)
        {
            context.OperationDescription.Operation.Security ??=
                new List<OpenApiSecurityRequirement>();

            context.OperationDescription.Operation.Security.Add(
                new OpenApiSecurityRequirement
                {
                    {
                        _scheme,
                        Array.Empty<string>()
                    }
                });
        }

        return true;
    }

}

public static class ObjectExtenstions
{
    public static T TryGetPropertyValue<T>(
        this object obj,
        string propertyName,
        T defaultValue = default)
    {
        if (obj == null)
            return defaultValue;

        var property = obj.GetType().GetRuntimeProperty(propertyName);

        if (property == null)
            return defaultValue;

        return property.GetValue(obj) is T value
            ? value
            : defaultValue;
    }

}

