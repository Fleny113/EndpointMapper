﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace EndpointMapper;

/// <summary>
/// Interface to implement for the AssemblyScanner to detect and add your endpoint(s)
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Use the <see cref="IEndpointRouteBuilder"/> to map (and optionally configure) the routes into the endpoint
    /// </summary>
    /// <param name="builder"><see cref="IEndpointRouteBuilder"/>  to register and configure the route(s)</param>
    public void Register(IEndpointRouteBuilder builder)
    {
    }

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
    /// <param name="methodInfo">Method information about the method currently under configure</param>
    public void Configure(RouteHandlerBuilder builder, string route, string method, MethodInfo methodInfo) => Configure(builder, route, method);

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
    public void Configure(RouteHandlerBuilder builder, string route, string method) => Configure(builder, route);

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
    public void Configure(RouteHandlerBuilder builder, string route) => Configure(builder);

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
    public void Configure(RouteHandlerBuilder builder)
    {
    }
}
