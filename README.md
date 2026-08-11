# EndpointMapper

An easy to use library for Minimal API endpoint autodiscovery and mapping.

> [!NOTE]
> If you are updating your project to use EndpointMapper v3, see the [update guide](#updating-to-v3) below.

## Installation

Simply add the package to your ASP.NET Core project:

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

Then create a public class that implements `IEndpoint`, then choose one of the following approaches for mapping the endpoint(s):

### Attribute based

Add a static method with attribute `HttpMap(HttpMapMethod.Get, "<route>")` where `HttpMapMethod.Get`[^HttpMapMethods] can be changed to any other options for
different HTTP verbs and `"<route>"` to one or more routes to map the endpoint to.

[^HttpMapMethods]: The values in `HttpMapMethods` are simply `const` strings, any `const` string can be used.
The `HttpMapMethod` class is a convenience, as `HttpMethods` uses `static readonly` strings which are not allowed in attibutes.

Any mapped method's properties can be edited without the provided attributes by overriding the virtual method
`static void Configure(RouteHandlerBuilder builder, string route, string method)`: this grants access to the `RouteHandlerBuilder` returned by
ASP.NET's mapping methods which can be used to customize the endpoint. The `route` and `method` parameters can be useful to distinguish multiple endpoints mapped in the same class.

Any written method is mapped directly via ASP.NET's `MapGet`/`MapPost`/... so it can be used as if you were writing the function passed to it.
As such, either `[AsRoute]`/`[AsBody]`/... or implicit mappings can be used.

`Configure` is never called with methods mapped with the [method based mapping](#method-based).

### Method based

Override the virtual method `static void Register(IEndpointRouteBuilder builder)` and use `IEndpointRouteBuilder`[^IEndpointRouteBuilder] to call ASP.NET's mapping methods, then use the return value to customize the endpoint.

[^IEndpointRouteBuilder]: This is the interface used for the `MapGet`/`MapPost`/... methods. Both `WebApplication` and `MapGroup`'s return value implement it.
The `IEndpointRouteBuilder` instance is used to call `MapEndpointMapperEndpoints`.

> [!NOTE]
> This is the only way to get NativeAOT/Trimming support. Although EndpointMapper uses a source generator instead of reflection, source generators can't see other generators' outputs, so ASP.NET's RequestDelegate source generator can't generate NativeAOT/Trimming-compatible code for the Map methods.

The two approaches can be mixed: the source generator will always call `Register`, and if the class contains any attribute mapped methods, it will call `Configure` on all of them.

## Example

Program.cs:
```csharp
using EndpointMapper;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapEndpointMapperEndpoints();

app.Run();
```

Then create a class that implements `IEndpoint`:

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

In v3, there have been some breaking changes:

- `EndpointMapper.OpenApi` has been removed. This package provided an operation filter for the `Authorize` attribute, no longer needed as the new OpenAPI packages deal with that by themselves;
- `IConfigureEndpoint` and `IRegisterEndpoint` have been removed in favor of virtual methods on `IEndpoint`;
- The library is now built against .NET 10;
- `HttpMapAttribute` no longer has properties: the public method string and the internal string array for the routes have been removed. Constructor parameters are not stored as the source generator doesn't rely on them.
- `EndpointMapperExtensions` is now generated as an internal embedded class (was previously a public class), and as such is now only accessible by its generating assembly, even with `InternalsVisibileTo`.
To expose the method to another assembly, wrap it with a custom API.

To see all the changes that have been made to the EndpointMapper since v2, check the [GitHub commits](https://github.com/Fleny113/EndpointMapper/compare/v2.0.0..main).

## Licence

EndpointMapper is licensed under the [MIT license](https://github.com/Fleny113/EndpointMapper/blob/main/LICENSE.txt).
