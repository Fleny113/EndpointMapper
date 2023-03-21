using Microsoft.AspNetCore.Routing;

namespace EndpointMapper;

/// <summary>
/// Options for EndpointMapper
/// </summary>
public sealed record EndpointMapperConfiguration
{
    /// <summary>
    /// Prefix of all routes mapped from EndpointMapper, default: /
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public required string RoutePrefix { get; set; } = "/";
    
    /// <summary>
    /// Action used to configure the GroupBuilder used to register the routes
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public required Action<RouteGroupBuilder> ConfigureGroupBuilder { get; set; } = _ => {};

    /// <summary>
    /// Chose if print the time took to initialize EndpointMapper, default: false
    /// </summary>
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public required bool LogTimeTookToInitialize { get; set; } = false;
}
