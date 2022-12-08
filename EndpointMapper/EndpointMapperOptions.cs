namespace EndpointMapper;

/// <summary>
/// Options for EndpointMapper
/// </summary>
public sealed record EndpointMapperOptions
{
    /// <summary>
    /// Prefix of all routes mapped from EndpointMapper
    /// </summary>
    public required string RoutePrefix { get; set; } = "";
};
