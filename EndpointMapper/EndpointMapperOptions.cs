namespace EndpointMapper;

public sealed record EndpointMapperOptions
{
    public required string RoutePrefix { get; set; } = "";
};
