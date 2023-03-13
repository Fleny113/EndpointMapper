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
    /// Do not initialize this class manually. Use UseMiddleware on an <see cref="Microsoft.AspNetCore.Builder.WebApplication"/> instance
    /// </summary>
    /// <param name="next">ASP.NET Request Delegate</param>
    public EndpointMapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Middleware code.
    /// </summary>
    /// <param name="context">HttpContext for the incoming request</param>
    /// <returns>A <see cref="Task"/></returns>
    public Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is null)
            return _next(context);

        var endpointInstance = endpoint.Metadata.GetMetadata<IEndpoint>();

        // if can't get the instance of the endpoint then continue with the pipeline
        if (endpointInstance is null)
            return _next(context);

        var endpointType = endpointInstance.GetType();

        // if it's not an IEndpoint continue the pipeline
        if (!endpointType.IsAssignableTo(typeof(IEndpoint)))
            return _next(context);

        // Update the services injected
        var constructor = endpointType.GetConstructors()[0];
        var constructorParams = constructor.GetParameters().AsSpan();

        var services = new object[constructorParams.Length];

        for (var i = 0; i < constructorParams.Length; i++)
            services[i] = context.RequestServices.GetRequiredService(constructorParams[i].ParameterType);

        // Call the constructor with the new services
        constructor.Invoke(endpointInstance, services);

        // Continue with the pipeline
        return _next(context);
    }
}
