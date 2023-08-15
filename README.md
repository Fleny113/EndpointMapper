# EndpointMapper

Built on top of Minimal APIs and easy to use

> **Note**
> If you are updating your project to use EndpointMapper v2 prerelease 3+ [see the update guide](#updating-to-v2-prerelease-3)

## Installation

Add the package to your ASP.NET Core project

```sh
dotnet add package EndpointMapper

# To add support for OpenAPI (See below for more instructions)
dotnet add package EndpointMapper.OpenApi
```

## Requirements

- [.NET 8][getDotnet]
- [ASP.NET Core 8][getDotnet]

## Usage

Add this into the `Program.cs`
```cs
app.MapEndpointMapperEndpoints();
```
And this line to your `.csproj` inside the `PropertyGroup`[^interceptors]
```xml
    <Features>InterceptorsPreview</Features>
```
Then create a public class that implements `IEndpoint` and add a static method with attribute `HttpMap(HttpMapMethod.Get, "<route>")`
where you can change `HttpMapMethod.Get` to any other options for different HTTP verbs and `"<route>"` to one, or more, routes to map the endpoint to

> **Note**
> see [Samples](#sample) for an example

> **Note**
>
> To bind parameters/inject dependencies to the method function [see more below](#parameters-binding-and-function-return)
>
> If you want to use Swagger [see this section](#openapi-support-swagger)

[^interceptors]: EndpointMapper uses a source generator and a interceptor to intercept the call to `MapEndpointMapperEndpoints` and map your endpoints.
The source generated code uses features from `C# 11`, so if you manually specify a lower `LangVersion` you need to bump it up at least to `11`

### Sample

Program.cs:
```csharp
using EndpointMapper;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapEndpointMapperEndpoints();

app.Run();
```

Then create a file, in this case it's in the root of the project but it could be in any folder

ExampleEndpoint.cs:
```csharp
using EndpointMapper;

public class ExampleEndpoint : IEndpoint
{
    [HttpMap(HttpMapMethod.Get, "/example")]
    public static Ok<string> Handle()
    {
        return TypedResults.Ok("Hello world from EndpointMapper");
    }
}
```
> **Note**
> To bind parameters/inject dependencies to the method function [see more below](#parameters-binding-and-function-return)

---

## Parameters Binding and function return

Since EndpointMapper uses the native ASP.NET mapping system to map your endpoint and make them work, you can threat
your method like the inline delegate to the `app.MapGet(...)` method.

So you can do:
- Http Body, Query, Route, Headers binding into the function arguments
- Dependency Injection from the method parameters
- Attributes like `[FromBody]` or `[FromQuery]` to explicitly map the required values into arguments
- Return using the `Results` or `TypedResults` methods or directly a `string` or any other values that ASP.NET
automatically can translate into a valid HTTP response body

An example of this can be seen in the [example](#sample) where `TypedResults` is used to send an 200 Status code
response back with a body attached that contains a string saying `Hello world from EndpointMapper`

---

## OpenAPI support (swagger)

EndpointMapper only supports `Swashbuckle.AspNetCore`, and you will need to add the `EndpointMapper.OpenApi` package

> **Warning**
> For [authentication](#authentication-requirements) or [XML documentation](#xml-documentation) you may need to add 
> some code to your `.AddSwaggerGen(...)` call

### XML documentation

You will need to add:

- To your `.csproj`
  - `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to generate the XML file to use
  - Optionally, `<NoWarn>$(NoWarn);CS1591</NoWarn>` to disable the warning [`Missing XML comment for publicly visible type or member 'Type_or_Member'`][CS1591]

- To the `.AddSwaggerGen()` call
```csharp
// Get the XML file path from the Assembly
var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

// Add the comments into the generation for the OpenApi scheme
config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);
```

> **Note**
> You may need to add `using System.Reflection;` because we use the `Assembly` class available in the 
> `System.Reflection` namespace

> **Note**
> In the code you need to add the action passed as first argument to `.AddSwaggerGen()`, `config` is the name
> of the variable to which the `SwaggerGenOption` instance is bound, if you named it in another way you'll need to change
> the code accordingly, if you're not sure about what you're doing, there is an example of the complete call to `.AddSwaggerGen()` to the [end of
> the swagger section](#addswaggergen-example-call)

### Authentication requirements

If you have authentication in your application you need to let swagger know how to authenticate against it, you will
need to add to the `.AddSwaggerGen()` call the registration of Security Definition and if you use the ASP.NET
`[Authorize]` attribute you may also need to add some code to detect the attribute and add the requirement

EndpointMapper.OpenApi provides you an operation filter to add those requirements automatically when it detects the
`[Authorize]` attribute, if you manually check for authentication using a Filter or something else then this is not
needed, and you will need something else to add the requirements

The only code you will need to add to your `.AddSwaggerGen()` call is the following
```csharp
// Add the security definition with name Bearer and the following options
config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Bearer JWT",
    Type = SecuritySchemeType.Http,
    In = ParameterLocation.Header,
    Scheme = "Bearer"
});

// Add the operationFilter to add the authentication requirements where needed
config.OperationFilter<AuthenticationRequirementOperationFilter>();
```

> **Note**
> In the code you'll need to add the action passed as first argument to `.AddSwaggerGen()`, `config` it's the name
> of the variable to witch the `SwaggerGenOption` instance is bound, if you named it in another way you need to change
> the code accordingly, if you're not sure about what you're doing there is an example of the complete call to `.AddSwaggerGen()` at the [end of
> the swagger section](#addswaggergen-example-call)

### AddSwaggerGen example call

This is an example call to the `.AddSwaggerGen()` method witch has both XML comments integration and user authentication

This call is here to help you understand the code snippet(s) in the context of the whole call, you may not copy-paste all
the function as (especially the authentication stuff) it requires edits based on your application needs, if you want more
context you can refer to the `EndpointMapper.TestApplication/Program.cs` file

```csharp
// builder is the variable assigned to the return value of "WebApplication.CreateBuilder()"
builder.Services.AddSwaggerGen(config =>
{
    // Add the security definition with name Bearer and the following options
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Bearer JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    
    // Add the operationFilter to add the authentication requirements where needed
    config.OperationFilter<AuthenticationRequirementOperationFilter>();
    
    // Get the XML file path from the Assembly
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    // Add the comments into the generation for the OpenApi scheme
    config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);
});
```

---

## Using a route group

In previous versions EndpointMapped used to have an option to prefix all the routes. Now this options doesn't exist anymore, [see the upgrade guide](#updating-to-v2-prerelease-3).

To still have the ability to prefix all your routes by using an ASP.NET Route Group, EndpointMapped used to use it under the hood
but now you have to use it yourself.

You can create a Route group by using the `MapGroup` method on the `WebApplication` or another route group, then
you can use the `MapEndpointMapperEndpoints` method on the Route group builder instance. If you wanted to configure the group
you can now simply use the Route group builder instance you just got.

This now allows to map multiple times your endpoints if you want, for example you could map all your endpoints to both `/api` and `/` if you wanted.

## Endpoint configurations

### Method based configuration

You may want to configure more some of your endpoints, but there is a problem. Since we are using `Attributes`
to map our endpoints and ASP.NET Core doesn't provide attribute to specify a filter attribute, for example, for a minimal API
it seems that we can't do much about this.

For this reason EndpointMapper allows you to specify a `Configure` method by implementing the `IConfigureEndpoint`,
this method gives you access to the `RouteHandlerBuilder`, the route the endpoint is being mapped to and the HTTP Method,
this way you can use the builder like if you were chaining methods to the result of `MapGet`, `MapPost`, ecc..

> **Warning**
> If you registered your endpoint with the [method based approach](#method-based-registration)
> EndpointMapper won't call the `Configure` method, as it is called for endpoints mapped by the `HttpMap` attribute,
> and you will need to do your configuration in the `Register` method

> **Note**
> Since the `Configure` method is implemented on the class and EndpointMapper doesn't enforce the 1 handler for class, this
> method will be called for each endpoint you map in the class you implement the `Configure` method. To differentiate a 
> route from another the function has 2 another `string` arguments, one it's the route the endpoint is being mapped to
> and the other one is the method that is being used to map the endpoint.
>
> There is one only thing to remember when having multiple endpoints in the class is that this method will be called for
> each route in each `HttpMap` attribute you have in the class. So if you have 2 methods, each with 2 attributes and 2 routes
> each you the `Configure` method will be called a total of 8 times, since in total you are mapping 8 different routes.

### Method based registration

If you don't like using attributes to map your endpoints you can implement the `IRegisterEndpoint` interface and the
`Register` method. In this method you have access to the `IEndpointRouteBuilder` you use to call the `MapEndpointMapperEndpoints`
method, using the builder you can use the extension methods that ASP.NET Core declares to map all your endpoints,
an example is the `MapGet` or `MapPost` method.

> **Warning**
> Don't use `Register` method if you need to configure your endpoints and you want to use the Attribute based mapping,
> for that you can use the [`Configure` method](#method-based-configuration)

> **Note**
> EndpointMapper checks for both the `HttpMap` attribute and the `Register` method to register your endpoints

## Updating to v2 prerelease 3+

In the prerelease 3 the public API of EndpointMapper changed quite a bit, so here are all the changes that have
been made and you have to do to update your project.

> **Note**
> If you are updating from v1 there is one extra thing to do.
>
> The swagger support is now optional, so you need to install the `EndpointMapper.OpenApi` nuget package 
> and add `using EndpointMapper.OpenApi;` for the `AuthenticationRequirementOperationFilter`

- `HttpMap<HttpVerb>(<routes>)` has now been replaced with `HttpMap(HttpMapMethod.<HttpVerb>, <routes>)`,
so a `HttpMapGet("/myRoute")` now is `HttpMap(HttpMapMethod.Get, "/myRoute")`
- All your methods that have an `HttpMap` attribute now needs to be `static`
- The constructor based DI is no longer supported. You now need to use the DI from the method parameters
- The `Configure` method now has only 1 overload, `Configure(RouteHandlerBuilder, string route, string method)`
- the `IEndpointConfigurationAttribute` interface and the `Filter<T>` attribute have been deleted. You now need to use the [method based configuration](#method-based-configuration)
- `AddEndpointMapper<T>(this IServiceCollection, Action<EndpointMapperConfiguration>)`,
`AddEndpointMapper(this IServiceCollection, Action<EndpointMapperConfiguration>, params Type[])` and
`AddEndpointMapper(this IServiceCollection, Action<EndpointMapperConfiguration>, params Assembly[])` have been removed.
- `UseEndpointMapper(this WebApplication, bool)` has been renamed to `MapEndpointMapperEndpoints(this IEndpointRouteBuilder)`
- You need [.NET 8 and ASP.NET Core 8][getDotnet]
- You need to enable `Interceptors`[^interceptors] by adding `<Features>InterceptorsPreview</Features>` to your `.csproj` inside a `PropertyGroup`
- You need at least C# 11, if you specify a `LangVersion` that is lower then 11 you need to bump it up
- The finding of your endpoints is now done at compile time via a source generator and not a runtime using reflection. Now EndpointMapper is NativeAOT friendly.
- The `LogTimeTookToInitialize` option doesn't exist anymore
- The `RoutePrefix` and `ConfigureGroupBuilder` options do not exist anymore. You can still configure EndpointMapper to use 
a [route prefix using a ASP.NET Route Group](#using-a-route-group)

To see all the changes that have been made to the EndpointMapper since v1 code you can check the [Github commits][gitCommits]

[getDotnet]: https://get.dot.net/8
[CS1591]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591
[gitCommits]: https://github.com/Fleny113/EndpointMapper/compare/08b4d3640586da116cff589b02cad5fab98e6cbb...main
