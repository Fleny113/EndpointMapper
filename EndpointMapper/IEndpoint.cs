using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EndpointMapper;

/// <summary>
/// Interface to detect and add your endpoint(s)
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Use the <see cref="IEndpointRouteBuilder"/> to map and configure the routes into the endpoint
    /// </summary>
    /// <remarks>
    /// When using Trimming and NativeAOT, this is the preferred way to register your endpoints, as it allows the ASP.NET
    /// Request Delegate Generator to generate the code to allow for Trimming as the code won't need Runtime Type Information
    /// </remarks>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/> to register and configure the route(s)</param>
    public static virtual void Register(IEndpointRouteBuilder builder) { }

    /// <summary>
    /// Use the <see cref="RouteHandlerBuilder"/> to configure the endpoint created in the class
    /// </summary>
    /// <remarks>
    /// Useful when want to configure something like the OutputCaching that accept an Action to don't
    /// create a policy for each endpoint
    /// </remarks>
    /// <param name="builder">
    /// The <see cref="RouteHandlerBuilder"/> to configure the route, same as concatenating methods on the result of
    /// <see cref="EndpointRouteBuilderExtensions.MapGet(IEndpointRouteBuilder, string, RequestDelegate)"/>
    /// for example
    /// </param>
    /// <param name="route">Route of the handler currently being configured</param>
    /// <param name="method">HTTP Verbs of the handler currently being configured</param>
    public static virtual void Configure(RouteHandlerBuilder builder, string route, string method) { }
}

/// <summary>
/// Interface to declare you implement a manual registration for your endpoint(s)
/// </summary>
public interface IRegisterEndpoint
{
    /// <summary>
    /// Use the <see cref="IEndpointRouteBuilder"/> to map and configure the routes into the endpoint
    /// </summary>
    /// <remarks>
    /// When using Trimming and NativeAOT, this is the preferred way to register your endpoints, as it allows the ASP.NET
    /// Request Delegate Generator to generate the code to allow for Trimming as the code won't need Runtime Type Information
    /// </remarks>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/> to register and configure the route(s)</param>
    public static abstract void Register(IEndpointRouteBuilder builder);
}

/// <summary>
/// Interface to declare you have a manual configuration for your endpoint(s)
/// </summary>
public interface IConfigureEndpoint
{
    /// <summary>
    /// Use the <see cref="RouteHandlerBuilder"/> to configure the endpoint created in the class
    /// </summary>
    /// <remarks>
    /// Useful when want to configure something like the OutputCaching that accept an Action to don't
    /// create a policy for each endpoint
    /// </remarks>
    /// <param name="builder">
    /// The <see cref="RouteHandlerBuilder"/> to configure the route, same as concatenating methods on the result of
    /// <see cref="EndpointRouteBuilderExtensions.MapGet(IEndpointRouteBuilder, string, RequestDelegate)"/>
    /// for example
    /// </param>
    /// <param name="route">Route of the handler currently being configured</param>
    /// <param name="method">HTTP Verbs of the handler currently being configured</param>
    public static abstract void Configure(RouteHandlerBuilder builder, string route, string method);
}
