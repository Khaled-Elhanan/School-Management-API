using System.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors.Contexts;

namespace Infrastructure.OpenApi;
using NSwag.Generation.Processors;

public class SwaggerHeaderAttributeProcessor:IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (context.MethodInfo.GetCustomAttribute(typeof(SwaggerHeaderAttribute))is SwaggerHeaderAttribute swaggerHeader)
        {
            var parameters = context.OperationDescription.Operation.Parameters;
            var existingParam  = parameters
                .FirstOrDefault(x => x.Kind == NSwag.OpenApiParameterKind.Header && x.Name == swaggerHeader.HeaderName);
            if (existingParam  is not null)
            {
                parameters.Remove(existingParam );
            }
            parameters.Add(new OpenApiParameter
            {
                Name = swaggerHeader.HeaderName,
                Kind = OpenApiParameterKind.Header,
                Description = swaggerHeader.Description,
                IsRequired = true,
                Schema = new JsonSchema
                {
                    Type = JsonObjectType.String,
                    Default = swaggerHeader.DefaultValue,
                }
            });
        }
        return true; 
    }
} 