using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EndpointMapper.OpenApi;

/// <summary>
/// Swashbuckle.AspNetCore Operation Filter to map <see cref="AuthorizeAttribute"/> from endpoints
/// </summary>
public sealed class AuthenticationRequirementOperationFilter : IOperationFilter
{
    private readonly string? _defaultSchema;

    /// <summary>
    /// Don't initialize this call directly, use OperationFilter&lt;TFilter&gt;() from the configure action in AddSwaggerGen
    /// </summary>
    /// <param name="schemeProvider">IAuthenticationSchemeProvider</param>
    public AuthenticationRequirementOperationFilter(IAuthenticationSchemeProvider schemeProvider)
    {
        var getDefaultAuthenticateSchemeAsyncTask = schemeProvider.GetDefaultAuthenticateSchemeAsync();

        _defaultSchema = getDefaultAuthenticateSchemeAsyncTask.GetAwaiter().GetResult()?.Name;
    }

    /// <summary>
    /// Called from Swashbuckle.AspNetCore when using OperationFilter&lt;TFilter&gt;()
    /// </summary>
    /// <param name="operation">OpenApiOperation</param>
    /// <param name="context">OperationFinderContext</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var auth = context
            .ApiDescription
            .ActionDescriptor
            .EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .ElementAtOrDefault(0);

        if (auth is null)
            return;

        var authenticationSchemes = auth.AuthenticationSchemes ?? _defaultSchema;

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = authenticationSchemes
            }
        };

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {scheme, Array.Empty<string>()}
        });
    }
}
