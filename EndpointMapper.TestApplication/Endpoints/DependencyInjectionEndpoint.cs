using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace EndpointMapper.TestApplication.Endpoints;

public sealed class DependencyInjectionEndpoint : IEndpoint
{
    private readonly EndpointMapperConfiguration _configuration;

    public DependencyInjectionEndpoint(IOptions<EndpointMapperConfiguration> endpointMapperOptions)
    {
        _configuration = endpointMapperOptions.Value;
    }

    public void Configure(RouteHandlerBuilder builder) => builder.CacheOutput(x => x.Expire(TimeSpan.FromSeconds(10)));

    [HttpMapGet("/di")]
    public Ok<string> Handle() => TypedResults.Ok(_configuration.RoutePrefix);
}