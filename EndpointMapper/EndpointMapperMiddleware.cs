using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointMapper;

/// <summary>
/// Middleware to resolve Dependency Injection into the endpoints
/// </summary>
public sealed class EndpointMapperMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Constructor for the EndpointMapper DI Middleware
    /// </summary>
    /// <param name="next">ASP.NET continuation pipeline delegate</param>
    public EndpointMapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    /// <summary>
    /// Middleware code where we try to get the instance from the metadata and re-populate it's services
    /// </summary>
    /// <param name="context">HttpContext for the request</param>
    /// <returns>The <see cref="RequestDelegate"/> invoked with the given <see cref="HttpContext"/></returns>
    [UsedImplicitly]
    public Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is null)
            return _next(context);
        
        var instance = endpoint.Metadata.GetMetadata<IEndpoint>();

        // if can't get the instance of the endpoint then continue with the pipeline
        if (instance is null)
            return _next(context);

        var endpointType = instance.GetType();

        // Update the services injected
        var constructors = endpointType.GetConstructors();

        // There isn't any constructor
        if (constructors.Length <= 0)
            return _next(context);

        var constructor = constructors[0];
        
        var constructorParams = constructor.GetParameters().AsSpan();
        
        var services = constructorParams.IsEmpty
            ? Array.Empty<object>()
            : new object[constructorParams.Length];

        if (!constructorParams.IsEmpty)
        {
            for (var i = 0; i < constructorParams.Length; i++)
            {
                services[i] = context.RequestServices.GetRequiredService(constructorParams[i].ParameterType);   
            }
        }
        
        // Call the constructor with the new services
        constructor.Invoke(instance, services);

        // Continue with the pipeline
        return _next(context);
    }
}
