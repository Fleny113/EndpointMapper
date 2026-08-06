# EndpointMapper

Built on top of Minimal APIs and easy to use

> [!NOTE]
> If you are updating your project to use EndpointMapper v3 [see the update guide](#updating-to-v3)

## Installation

Add the package to your ASP.NET Core project

```sh
dotnet add package EndpointMapper
```

## Requirements

- [.NET 10][getDotnet]
- [ASP.NET Core 10][getDotnet]

[getDotnet]: https://get.dot.net/10

## Usage

Call `MapEndpointMapperEndpoints` in your `Program.cs` with the `WebApplication` or a route group:
```cs
app.MapEndpointMapperEndpoints();
```

Then create a public class that implements `IEndpoint`, then pick one of 2 methods for mapping the endpoint(s):

### Attribute based

Add a static method with attribute `HttpMap(HttpMapMethod.Get, "<route>")` where you can change `HttpMapMethod.Get`[^HttpMapMethods] to any other options for
different HTTP verbs and `"<route>"` to one, or more, routes to map the endpoint to.

[^HttpMapMethods]: The values in `HttpMapMethods` are simply const strings, you can use any const string and the source generator will accept it.
The `HttpMapMethod` class is a convenience, as `HttpMethods` uses `static readonly` strings which are not allowed in attibutes.

If you need to edit some property of the mapped method and you can't use the provided attributes, you can override the virtual method
`static void Configure(RouteHandlerBuilder builder, string route, string method)`: this gives you access to the `RouteHandlerBuilder` returned by
ASP.NET's mapping methods to customize the endpoint. `route` and `method` can be useful if you map multiple endpoints in the same class to distinguish them.

The method you write is mapped directly with ASP.NET's `MapGet`/`MapPost`/... so you can use it as if you were writing the function passed to it.
This mean you can use `[AsRoute]`/`[AsBody]`/... attributes or the implicit mappings.

`Configure` is never called with methods mapped with [the method based](#method-based) mapping.

### Method based

Override the virtual method `static void Register(IEndpointRouteBuilder builder)` and use `IEndpointRouteBuilder`[^IEndpointRouteBuilder] to call the ASP.NET's mapping methods
and use the return value to customize the endpoint.

[^IEndpointRouteBuilder]: This is the interface used for the `MapGet`/`MapPost`/... methods. A `WebApplication` and the return of `MapGroup` both implement this.
The `IEndpointRouteBuilder` instance is the one you used to call `MapEndpointMapperEndpoints`.

> [!NOTE]
> This the only supported way to get NativeAOT/Trimming support, as while EndpointMapper itself doesn't use any reflection and instead uses a source generator, since source generator don't see another generators outputs, the ASP.NET RequestDelegate source generator can't generate the NativeAOT/Trimmimg compatible code for the Map method making it incompatible for NativeAOT/Trimming.

You can mix the 2 things if you want to, the source generator will always call `Register` and, if any, call `Configure` on all attribute mapped methods in the class.

## Example

Program.cs:
```csharp
using EndpointMapper;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapEndpointMapperEndpoints();

app.Run();
```

Then create a class that implements `IEndpoint`

ExampleEndpoint.cs:
```csharp
using EndpointMapper;

namespace YourProject;

public class ExampleEndpoint : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/example")]
    public static Ok<string> Handle()
    {
        return TypedResults.Ok("Hello world from EndpointMapper");
    }
}
```

You can see more examples in the `EndpointMapper.TestApplication` and `EndpointMapper.TestApplication.NativeAOT` projects.

## Updating to v3

In v3 there have been some breaking changes:

- `EndpointMapper.OpenApi` has been removed. This packed used to provide a operation filter for the `Autorize` attribute,
the new OpenAPI packages deal with that by themself.
- `IConfigureEndpoint` and `IRegisterEndpoint` no longer exist in favor of virtual methods on `IEndpoint`.
- The library is now built against .NET 10
- `HttpMapAttribute` no longer has attributes: there used to be a public method string and an internal string array for the routes,
however these have been removed. Constructor parameters are not stored as the source generator doesn't rely on them.
- `EndpointMapperExtensions` used to be generated as a public class, it's now an internal embedded class, meaning that it will only be accessible by the assembly
that generates it even with `InternalsVisibileTo`. If you need to expose this method to another assembly, wrap it with a custom public api.

To see all the changes that have been made to the EndpointMapper since v2 code you can check the [Github commits](https://github.com/Fleny113/EndpointMapper/compare/v2.0.0..main)

## Licence

EndpointMapper is under the [MIT](https://github.com/Fleny113/EndpointMapper/blob/main/LICENSE.txt) license.
