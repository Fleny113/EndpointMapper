#pragma warning disable IDE0130 // Namespace does not match the folder structure

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EndpointMapper;

/// <summary>
/// Add a filter to the Endpoint
/// </summary>
/// <typeparam name="TFilter">Type of the <see cref="IEndpointFilter"/> to apply</typeparam>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class FilterAttribute<TFilter> : EndpointConfigurationAttribute where TFilter : IEndpointFilter
{
    /// <summary>
    /// Configure a route using the <see cref="RouteHandlerBuilder"/>
    /// </summary>
    /// <param name="builder">RouteHandlerBuilder for configuring the RouteHandler</param>
    public override void Configure(RouteHandlerBuilder builder) => builder.AddEndpointFilter<TFilter>();
}
