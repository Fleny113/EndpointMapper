using Microsoft.AspNetCore.Http.HttpResults;

namespace EndpointMapper.TestApplication.Endpoints;

internal abstract class RegisterEndpoints : IEndpoint
{
    public static void Register(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/something", SomethingHandler);
    }

    public static Ok<string> SomethingHandler(HttpContext context)
    {
        return TypedResults.Ok($"{context.Request.Method} {context.Request.Path}; {nameof(SomethingHandler)}(HttpContext)");
    }
}