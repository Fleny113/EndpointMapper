using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace EndpointMapper;

/// <summary>
/// Extension methods to map EndpointMapper endpoints
/// </summary>
public static class EndpointMapperExtensions
{
    /// <summary>
    /// Map your EndpointMapper endpoints to the specified group or <see cref="WebApplication"/>
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> given by <see cref="EndpointRouteBuilderExtensions.MapGroup(IEndpointRouteBuilder, string)" /> or <see cref="WebApplication"/></param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> to chain other methods</returns>
    public static IEndpointRouteBuilder MapEndpointMapperEndpoints(this IEndpointRouteBuilder builder)
        => throw new NotImplementedException("This method should get intercepted by the source generator!");
}
