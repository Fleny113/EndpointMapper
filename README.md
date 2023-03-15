# EndpointMapper

Build on top of Minimal APIs and designed to be as easy to use as possible

## Installation

Add the package to your ASP.NET Core project

```sh
dotnet add package EndpointMapper
```

## Requirements

- .NET 7
- ASP.NET Core 7

## Usage

Add into the `Program.cs`
- `builder.Services.AddEndpointMapper<T>();`
- `app.UseEndpointMapper();`

Then create a public class that implement `IEndpoint` and add a method with attribute `HttpMapGet`
or one of the variants for each HTTP verb

> **Note**:
> see [Samples](#sample) for an example

> **Note**:
> If you want to use bind parameters to the method function [see this section](#parameters-binding-and-function-return)

> **Note**:
> If you want to use dependency injection [see this section](#dependency-injection)

> **Note**:
> If you want to use Swagger [see this section](#openapi-support-swagger)

### Sample

Program.cs: 
```csharp
using EndpointMapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointMapper<Program>();

var app = builder.Build();

app.UseEndpointMapper();

app.Run();
```

Then create a file, in this case it's the root of the project but could be any folder

ExampleEndpoint.cs:
```csharp
using EndpointMapper;

public class ExampleEndpoint : IEndpoint
{
    [HttpMapGet("/example")]
    public Ok<string> Handle()
    {
        return TypedResults.Ok("Hello world from EndpointMapper");
    }
}
```
> **Note**:
> If you want to bind values from the request into params of the request see [this](#parameters-binding-and-function-return)

---

## Parameters Binding and function return

Since EndpointMapper uses the native ASP.NET delegate system to map your endpoint and make them work you can threat your
method just like it was an inline delegate to the `app.MapGet(...)` method.

So you can do
- Http Body, Query, Route, Headers binding into the function arguments
- Dependency Injection (for more details [see this section](#dependency-injection))
- Attributes like `[FromBody]` or `[FromQuery]` to explicitly map the required values into arguments
- Return using the `Results` or `TypedResults` methods or directly a `string` or any other values that ASP.NET
automatically can translate into a valid HTTP response body

An example of this can be seen in the [example](#sample) where it's used TypedResults to send an 200 Status code
response back with attached a body that contains a string saying `Hello world from EndpointMapper`

---

## Dependency Injection

While you can use the method parameters to inject your services to be used, if you prefer you still can use the 
constructor of the class you created to resolve your services and map them to a private readonly field for example.

Since this is done by EndpointMapper it's requires a middleware, the middleware is register by-default when using the
`.UseEndpointMapper()` on the `WebApplication`, but if you don't want to add it to the middleware pipeline and you
don't want to use the constructor based DI then you can pass a `false` to the method like this: 
`.UseEndpointMapper(addMiddleware: false)`

> **Warning**
> As sayed above setting this to false **will** skip the DI resolution using the middleware, and since EndpointMapper
> when create the instance of the class in the `.UseEndpointMapper()` method, the class is created uninitialized all the 
> fields will be null (unless the field has a default value) and the _non static_ constructor **wont** be called, so if
> you need to use a constructor to initialize something and for some reason you can't do it using a default value
> then setting this setting _addMiddleware_ to false may cause unexpected behaviour, keep in mind that static
> constructor will be called and will function as normal, as them are called by the .NET runtime

> **Note**
> You could just pass false and don't add the "addMiddleware: " part, but for readability purposes it's kept in here
> to make it easier to understand what this false actually means

---

## OpenAPI support (swagger)

EndpointMapper only supports `Swashbuckle.AspNetCore`[^1], but it's support is out-of-the-box and don't require
any additional package to work (except `Swashbuckle.AspNetCore` obviously)

> **Warning**
> For authentication or XML documentation you may need to add some code to your `.AddSwaggerGen(...)` call, 
> see [this section for more information about the authentication support](#authentication-requirements) and
> [this section for the XML documentation](#xml-documentation)

### XML documentation

You will need to add:

- To your `.csproj`
  - `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to generate the XML file to use
  - Optionally, `<NoWarn>$(NoWarn);CS1591</NoWarn>` to disable the warning [`Missing XML comment for publicly visible type or member 'Type_or_Member'`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591)

- To the `.AddSwaggerGen()` call
```csharp
// Get the XML file path from the Assembly
var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

// Add the comments into the generation for the OpenApi scheme
config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);
```

> **Note**
> You may need to add `using System.Reflection;` because we use the `Assembly` class that lives in the 
> `System.Reflection` namespace

> **Note**
> In the code you need to add the action passed as first argument to `.AddSwaggerGen()`, `config` it's the name
> of the variable to witch the `SwaggerGenOption` instance is bound, if you named it in another way you need to change
> the code accordingly, if you are unsure there is an example of the complete call to `.AddSwaggerGen()` to the [end of
> the swagger section](#addswaggergen-example-call)

[^1]: That in .NET 7 the ASP.NET template also uses `Microsoft.AspNetCore.OpenApi`

### Authentication requirements

If you have authentication in your application you need to let swagger known how to authenticate against it, so you
need to add to the `.AddSwaggerGen()` call the registration of Security Definition and if you use the ASP.NET
`[Authorize]` attribute you may also need to add some code to detect the attribute and add the requirement

EndpointMapper provides you an operation filter to add those requirements automatically when it detects the
`[Authorize]` attribute, if you manually check for authentication using a Filter or something else then this is not
needed.

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
config.OperationFilter<SecureEndpointAuthRequirementFilter>();
```

> **Note**
> In the code you need to add the action passed as first argument to `.AddSwaggerGen()`, `config` it's the name
> of the variable to witch the `SwaggerGenOption` instance is bound, if you named it in another way you need to change
> the code accordingly, if you are unsure there is an example of the complete call to `.AddSwaggerGen()` to the [end of
> the swagger section](#addswaggergen-example-call)

### AddSwaggerGen example call

This is an example call to the `.AddSwaggerGen()` method witch has both XML comments integration and user authentication

This call is here to help understand the code snippet(s) in the context of the whole call, you may not copy-paste all
the function as (especially the authentication stuff) requires edits based on your application needs, if you want more
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
    config.OperationFilter<SecureEndpointAuthRequirementFilter>();
    
    // Get the XML file path from the Assembly
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    // Add the comments into the generation for the OpenApi scheme
    config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);
});
```

---

## Advanced

### Method based configuration

You may want to add a OutputCaching policy, but there is a problem: since parameters to attributes can only be 
compile-time constant value, so we can't pass an action to configure and for this reason we would need to define a
policy for each and every endpoint that has even one small need

That is not practical at all. It will bloat your application with policies used in 1 or maybe 2 places, that in some
context can be totally fine, as it gives a central access to all the policies, but you may not want this

For this reason EndpointMapper when maps an endpoint first checks for attributes extending 
`EndpointConfigurationAttribute` to add some attribute that may be missing in ASP.NET, like `[Filter]` and then runs
the `Configure(RouteHandlerBuilder)` that is in your class, where you have the `RouteHandlerBuilder` and you can do
whatever you want with it, for example, setting a policy for OutputCaching, the only thing you need to do is overriding
the `Configure(RouteHandlerBuilder)` method and add your own logic for configuring the endpoint

> **Warning**
> If you registered your endpoint with the [`Register(IEndpointRouteBuilder)` method](#method-based-registration)
> EndpointMapper won't call the `Configure(RouteHandlerBuilder)` and you will need to do your configuration in the
> `Register` method

> **Note**
> Since this method is for class and with EndpointMapper you can add more then one handler function to the class
> because nothing it's stopping you from doing that, the `Configure` method will be called for each endpoint mapped 
> independently from if it was a different function[^2], if it is for the second / third / ... route of the attribute[^3]
> or if it the same method with 2 different HTTP verb, from 2 different attributes[^4], all of there 3 cases mixes,
> so let's assume you have 2 method, each with 3 routes and 2 attribute, then the `Configure` will be called 12 times
> in total[^5]

[^2]: example, you have 2 methods, `Handle` and `HandleButDifferent` and you map both with an `[HttpMapGet]`,
EndpointMapper will call the `Configure` first for `Handle` and it's Get mapping, then for `HandleButDifferent` and it's
Get mapping

[^3]: example, you have a `Handle` method with `[HttpMapGet("/a", "/b")]`,
EndpointMapper will call the `Configure` first for /a, then for /b again

[^4]: example, you have a `Handle` method with `[HttpMapGet("/"), HttpMapPost("/")]`, Endpoint will call the `Configure`
first for the Get mapping, then for the Post mapping, BUT if the attribute that you use for mapping implies multiple
HTTP verb, like an attribute named `[HttpMapGetAndPost("/")]` that bind your endpoint to both Get and Post then
EndpointMapper will call `Configure` only once for this method because it's in fact a single attribute

[^5]: explanation of why 12, first we get the first method, it has 2 attributes, we take the first, we get the 3 routes
and as stated in the second rule[^3] we call it 3 times total, then we get the second attributes and call the `Configure`
other 3 times for the second rule[^3], and this was the third rule[^4] (the 2 attributes) and we have already called
the `Configure` 6 times, then to respect the first rule[^2] we need to this again, and we get another 6 calls, up to a
total of 12

### Method based registration

If you don't like the fact that EndpointMapper uses attributes to map your endpoints or you need to map to a HTTP verb
that does not have an attribute then you can override the `Register(IEndpointRouteBuilder)` method on the class
implementing IEndpoint that gives you the `IEndpointRouteBuilder` witch has access to methods like `.MapGet()`

> **Warning**
> Don't use `Register(IEndpointRouteBuilder)` if you need to configure thing like OutputCaching that on can accept an
> action to configure it's behaviour without creating a policy, for this there is the `Configure(RouteHandlerBuilder)`
> method instead, you can [see more about this here](#method-based-configuration)

> **Note**
> EndpointMapper checks for both Attributes and `Register` to register your endpoints

> **Note**
> If you use a debugger, you will see that the actual type that EndpointMapper uses to call the method is 
> `RouteGroupBuilder`, this is because to respect the configuration of `RoutePrefix` EndpointMapper uses a minimal api
> group to map all your endpoint, you could cast the type back to `RouteGroupBuilder` using an hard cast and use the
> builder to, for example, configure the group with some behaviour settings, for example setting a default output cache 
> policy, although this is NOT encouraged because it can be pretty easy to forget about this code
> and since it's in a endpoint class you are configuring the group from and endpoint witch is not that great, since
> you may still want to configure some behaviour in the group, in the `.AddEndpointMapper()` call you can pass an 
> action witch gives you the ability to configure EndpointMapper configuration and in there you can set the 
> `ConfigureGroupBuilder` action to configure the group, it gives you the full `RouteGroupBuilder` without having to
> cast it and it wont "belong" to any Endpoint that may be deleted later for any reason

### Create your own `EndpointConfigurationAttribute`

Simple create a class that extends `EndpointConfigurationAttribute`

`EndpointConfigurationAttribute` requires you to implement a method called `Configure` where you have access to the 
`RouteHandlerBuilder` from ASP.NET to configure the route, the rest is handled automatically

### Create your own `HttpMapAttribute`

Simple create a class that extends `HttpMapAttribute` and use `: base()` on the constructor to pass the route(s)
and overload the `Methods` propriety to set the list of HTTP Verb that the attribute correspond to, the verb MUST be 
supported by ASP.NET or else it wont work, the rest is handled automatically
