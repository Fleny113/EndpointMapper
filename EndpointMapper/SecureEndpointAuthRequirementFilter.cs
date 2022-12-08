using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EndpointMapper;

/// <summary>
/// Swashbuckle.AspNetCore Operation Filder to map <see cref="AuthorizeAttribute"/> from endpoints
/// </summary>
public sealed class SecureEndpointAuthRequirementFilter : IOperationFilter
{
    private readonly string? _defaultSchema;

    /// <summary>
    /// Don't inizializate this call directly, use OperationFilter&lt;TFilter&gt;() from the configure action in AddSwaggerGen
    /// </summary>
    /// <param name="schemeProvider">IAuthenticationSchemeProvider</param>
    public SecureEndpointAuthRequirementFilter(IAuthenticationSchemeProvider schemeProvider)
    {
        var task = schemeProvider.GetDefaultAuthenticateSchemeAsync();

        task.Wait();

        _defaultSchema = task.Result?.Name;
    }

    /// <summary>
    /// Called from Swashbuckle.AspNetCore when using OperationFilter&lt;TFilter&gt;()
    /// </summary>
    /// <param name="operation">OpenApiOperation</param>
    /// <param name="context">OperationFilderContext</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
            return;

        var authAttribute = context.MethodInfo.CustomAttributes.FirstOrDefault(attr => attr.AttributeType == typeof(AuthorizeAttribute));

        if (authAttribute is null)
            return;

        var auth = authAttribute.NamedArguments.FirstOrDefault(x => x.MemberName == "AuthenticationSchemes").TypedValue.Value as string;

        auth ??= _defaultSchema;

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = auth } }] = new List<string>()
        });
    }
}
