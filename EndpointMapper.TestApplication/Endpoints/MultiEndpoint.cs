using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EndpointMapper.TestApplication;

public class MultiEndpoint : IEndpoint
{
    public void Configure(RouteHandlerBuilder builder, string route, IEnumerable<string> methods, MethodInfo method)
    {
        Console.WriteLine($"Processing {route}, for verbs {JsonSerializer.Serialize(methods)} for method {method.Name}");
    }

    [HttpMapGet("/multi", "/multi/2"), HttpMapDelete("/multi", "/multi/2")]
    public Ok<string> Handle(HttpContext context)
    {
        return TypedResults.Ok($"{context.Request.Method} {context.Request.Path}; {nameof(Handle)}(HttpContext)");
    }
    
    [HttpMapPost("/multi/3")]
    public Ok<string> HandleButDifferent(HttpContext context)
    {
        return TypedResults.Ok($"{context.Request.Method} {context.Request.Path}; {nameof(HandleButDifferent)}(HttpContext)");
    }
}