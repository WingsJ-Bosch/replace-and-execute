using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace replace_and_execute
{
    /// <summary>
    /// Upload File Operation Filter
    /// </summary>
    public class UploadFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile))
                .Select(p => new OpenApiParameter
                {
                    Name = p.Name,
                    In = ParameterLocation.Header,
                    Description = "Upload File",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                }).ToList();
            foreach (var parameter in fileParameters)
            {
                operation.Parameters.Add(parameter);
            }
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType(),
                }
            };

            var otherParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType != typeof(IFormFile))
                .Select(p => new OpenApiParameter
                {
                    Name = p.Name,
                    In = ParameterLocation.Query,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                }).ToList();
            foreach (var parameter in otherParameters)
            {
                operation.Parameters.Add(parameter);
            }
        }
    }
}
