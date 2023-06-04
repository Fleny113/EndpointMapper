using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Options for EndpointMapper
/// </summary>
public sealed record EndpointMapperConfiguration
{
    /// <summary>
    /// Prefix of all routes mapped from EndpointMapper, default: /
    /// </summary>
    [StringSyntax("Route")]
    public required string RoutePrefix { get; set; } = "/";

    /// <summary>
    /// Action used to configure the GroupBuilder used to register the routes
    /// </summary>
    public required Action<RouteGroupBuilder> ConfigureGroupBuilder { get; set; } = _ => { };

    /// <summary>
    /// Chose if print the time took to initialize EndpointMapper, default: false
    /// </summary>
    public required bool LogTimeTookToInitialize { get; set; } = false;
}
