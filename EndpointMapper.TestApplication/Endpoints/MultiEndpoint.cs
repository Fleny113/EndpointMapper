using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace EndpointMapper.TestApplication.Endpoints;

public class MultiEndpoint : IEndpoint, IConfigureEndpoint
{
    public static void Configure(RouteHandlerBuilder builder, string route, string method)
    {
        Debug.WriteLine($"Processing {route}, for verb {method}");

        switch (route)
        {
            case "/multi":
                builder.RequireAuthorization();
                break;
            case "/multi/3":
                var authAttribute = new AuthorizeAttribute
                {
                    AuthenticationSchemes = "AnotherJWT"
                };

                builder.WithMetadata(authAttribute);
                break;
        }
    }

    [HttpMap(HttpMapMethod.Get, "/multi", "/multi/2"), HttpMap(HttpMapMethod.Delete, "/multi/2")]
    public static Ok<string> Handle(HttpContext context) 
        => TypedResults.Ok($"{context.Request.Method} {context.Request.Path}; {nameof(Handle)}(HttpContext)");

    [HttpMap(HttpMapMethod.Post, "/multi/3")]
    public static Ok<string> HandleButDifferent(HttpContext context) 
        => TypedResults.Ok($"{context.Request.Method} {context.Request.Path}; {nameof(HandleButDifferent)}(HttpContext)");
}