using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;

namespace EndpointMapper.TestApplication.Endpoints;

public abstract class DependencyInjectionEndpoint : IEndpoint
{
    public static void Configure(RouteHandlerBuilder builder, string route, string method)
    {
        builder.CacheOutput(x => x.Expire(TimeSpan.FromSeconds(10)));
    }

    [HttpMap(HttpMapMethod.Get, "/di")]
    public static Ok<long> Handle([FromServices] IOptions<OutputCacheOptions> outputCachingOptions)
        => TypedResults.Ok(outputCachingOptions.Value.MaximumBodySize);
}