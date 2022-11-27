using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EndpointMapper;

public sealed class EndpointMapperMiddleware
{
    private readonly RequestDelegate _next;

    public EndpointMapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint is null)
            return _next(context);

        var endpointInstace = endpoint.Metadata.GetMetadata<IEndpoint>();

        // if can't get the instance of the endpoint then continue with the pipeline
        if (endpointInstace is null)
            return _next(context);

        var endpointType = endpointInstace.GetType();

        // if it's not an IEndpoint continue the pipeline
        if (endpointType is null || !endpointType.IsAssignableTo(typeof(IEndpoint)))
            return _next(context);

        // Update the services injected
        var constructor = endpointType.GetConstructors()[0];
        var constructorParams = constructor.GetParameters().AsSpan();

        var services = new object[constructorParams.Length];

        for (var i = 0; i < constructorParams.Length; i++)
            services[i] = context.RequestServices.GetRequiredService(constructorParams[i].ParameterType);

        var unused = constructor.Invoke(endpointInstace, services);

        // Continue with the pipeline
        return _next(context);
    }
}
