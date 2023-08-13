using Microsoft.AspNetCore.Mvc;

namespace EndpointMapper.TestApplication.NativeAOT.Endpoints;

public class Todos : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/todos", "/maybe"), HttpMap(HttpMapMethod.Trace, "/opts")]
    public static Todo[]? NoTodos()
    {
        return new Todo[] {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new(4, "Clean the bathroom"),
            new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        };
    }

    [HttpMap(HttpMapMethod.Post, "/idk")]
    public static string Hi([FromServices] string idkSomeValue)
    {
        return $"Hi! I'm an Static Method and the _idkSomeValue has value: {idkSomeValue} [ASP.NET Minimal Api DI]";
    }
}

public class Test : IEndpoint, IRegisterEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/hi/test")]
    public static string Hi()
    {
        return "Hi from the Test1 class!";
    }

    public static void Register(IEndpointRouteBuilder builder)
    {
    }
}

public class Test2 : IEndpoint, IConfigureEndpoint
{
    public static void Configure(RouteHandlerBuilder builder, string route, string method)
    {
    }

    [HttpMap(HttpMapMethod.Get, "/hi/test2")]
    public static string Hi()
    {
        return "Hi from the Test2 class!";
    }
}

public class Test3 : IEndpoint, IRegisterEndpoint, IConfigureEndpoint
{
    public static void Configure(RouteHandlerBuilder builder, string route, string method)
    {
    }

    public static void Register(IEndpointRouteBuilder builder)
    {
    }

    [HttpMap(HttpMapMethod.Get, "/hi/test3")]
    public static string Hi()
    {
        return "Hi from the Test3 class!";
    }
}
