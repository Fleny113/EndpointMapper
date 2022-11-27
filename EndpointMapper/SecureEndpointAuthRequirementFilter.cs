using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EndpointMapper;

public sealed class SecureEndpointAuthRequirementFilter : IOperationFilter
{
    private readonly string SecuritySchemeId;

    public SecureEndpointAuthRequirementFilter(string securitySchemeId)
    {
        SecuritySchemeId = securitySchemeId;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
            return;

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SecuritySchemeId } }] = new List<string>()
        });
    }
}
