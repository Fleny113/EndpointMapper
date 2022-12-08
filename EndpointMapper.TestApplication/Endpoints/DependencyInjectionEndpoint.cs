using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace EndpointMapper.TestApplication;

public sealed class DependencyInjectionEndpoint : IEndpoint
{
    private readonly EndpointMapperOptions _options;

    public DependencyInjectionEndpoint(IOptions<EndpointMapperOptions> endpointMapperOptions)
    {
        _options = endpointMapperOptions.Value;
    }

    [HttpMapGet("/di")]
    public Ok<EndpointMapperOptions> Handle()
    {
        return TypedResults.Ok(_options);
    }
}