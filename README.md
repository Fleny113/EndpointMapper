# EndpointMapper

Built on top of Minimal APIs and easy to use

## Installation

Add the package to your ASP.NET Core project

```sh
dotnet add package EndpointMapper
```

## Requirements

- .NET 7
- ASP.NET Core 7

## Usage

Add this into the `Program.cs`
- `builder.Services.AddEndpointMapper<T>();`
- `app.UseEndpointMapper();`

Then create a public class that implements `IEndpoint` and add a method with attribute `HttpMapGet`
or one of the variants for each HTTP verb

> **Note**:
> see [Samples](#sample) for an example

> **Note**:
>
> If you want to use bind parameters to the method function [see this section](#parameters-binding-and-function-return)
>
> If you want to use dependency injection [see this section](#dependency-injection)
>
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

Then create a file, in this case it's in the root of the project but it could be in any folder

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
> If you want to bind values from the request into the params of the request see [this](#parameters-binding-and-function-return)

---

## Parameters Binding and function return

Since EndpointMapper uses the native ASP.NET delegate system to map your endpoint and make them work, you can threat
your method like the inline delegate to the `app.MapGet(...)` method.

So you can do:
- Http Body, Query, Route, Headers binding into the function arguments
- Dependency Injection (for more details, [see this section](#dependency-injection))
- Attributes like `[FromBody]` or `[FromQuery]` to explicitly map the required values into arguments
- Return using the `Results` or `TypedResults` methods or directly a `string` or any other values that ASP.NET
automatically can translate into a valid HTTP response body

An example of this can be seen in the [example](#sample) where TypedResults is used to send an 200 Status code
response back with a body attached that contains a string saying `Hello world from EndpointMapper`

---

## Dependency Injection

While you can use the method parameters to inject and use your services, if you want you can still use the 
constructor of the class you created to resolve your services and map them to a private readonly field, for example.

This is done by EndpointMapper using a middleware that's registered by-default when using the
`.UseEndpointMapper()` on the `WebApplication`, but if you don't want to add it to the middleware pipeline and you
don't want to use the constructor based DI then you can pass a `false` to the method like this: 
`.UseEndpointMapper(addMiddleware: false)`

> **Warning**
> As stated above, setting this to false **will** skip the DI resolution using the middleware, and since EndpointMapper
> creates the endpoint classes during the `.UseEndpointMapper()` method, and the created classes are uninitialized, all 
> the fields will be null (unless the field has a default value) and the _non static_ constructor **won't** be called.
> So if you need to use a constructor to initialize something and for some reason you can't do it using a default value
> then setting addMiddleware to false may cause unexpected behaviour, keep in mind that static constructor will be 
> called and will function as normal, as them are called by the .NET runtime

> **Note**
> You could just pass false and don't add the "addMiddleware: " part, but for readability purposes it's kept in here
> to make it easier to understand what this false actually means

---

## OpenAPI support (swagger)

EndpointMapper only supports `Swashbuckle.AspNetCore`[^template], and you will need to add the EndpointMapper.OpenApi package

> **Warning**
> For [authentication](#authentication-requirements) or [XML documentation](#xml-documentation) you may need to add 
> some code to your `.AddSwaggerGen(...)` call

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
> You may need to add `using System.Reflection;` because we use the `Assembly` class available in the 
> `System.Reflection` namespace

> **Note**
> In the code you need to add the action passed as first argument to `.AddSwaggerGen()`, `config` is the name
> of the variable to which the `SwaggerGenOption` instance is bound, if you named it in another way you'll need to change
> the code accordingly, if you're not sure about what you're doing, there is an example of the complete call to `.AddSwaggerGen()` to the [end of
> the swagger section](#addswaggergen-example-call)

[^template]: The .NET 7 the ASP.NET template also uses `Microsoft.AspNetCore.OpenApi`

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

## Advanced

### Method based configuration

You may want to add a OutputCaching policy, but there is a problem: since parameters to attributes can only be 
compile-time constant value, we can't pass an action to configure the caching and for this reason we would need to 
define a policy for each and every endpoint

That is not practical at all. It will bloat your application with policies used in 1 or maybe 2 places, but in some
contexts it can be totally fine, as it gives a central access to all the policies, but you may not want this

For this reason when EndpointMapper maps an endpoint, it firstly checks for attributes extending 
`EndpointConfigurationAttribute` to add some attribute that may be missing in ASP.NET, like `[Filter<T>]`, and then runs
the `Configure` method that is in your class, where you have the `RouteHandlerBuilder` and you can do
whatever you want with it, for example, setting a policy for OutputCaching, the only thing you need to do is creating
the `Configure(RouteHandlerBuilder)` method and add your own logic for configuring the endpoint

> **Warning**
> If you registered your endpoint with the [`Register(IEndpointRouteBuilder)` method](#method-based-registration)
> EndpointMapper won't call the `Configure(RouteHandlerBuilder)` and you will need to do your configuration in the
> `Register` method

> **Note**
> Since this method is for class and with EndpointMapper you can add more then one handler function to the class, even 
> though it is not recommended, the `Configure` method will be called for each endpoint mapped independently from if
> it's a different function[^different-function], if it's is for the second / third / ... route of the 
> attribute[^multiple-routes] or if it's the same method with 2 different mapping attributes that map to different HTTP 
> methods[^different-attributes], all of there 3 cases mixes, so let's assume you have 2 methods, each with 3 
> routes and 2 attributes, then the `Configure` will be called 12 times in total[^explanation-for-12-call].
>
> To get more control on what configuration is applied where, you could split the endpoints in multiple files 
> or using one of the `Configure` overloads that gives information about the current endpoint that is being configured, 
> there are 3 overloads in total, ignoring the base one.
> Each one adds one information on top of the builder, the route, 
> the HTTP methods and finally the MethodInfo

[^different-function]: example, you have 2 methods, `Handle` and `HandleButDifferent` and you map both with an 
`[HttpMapGet]`, EndpointMapper will call the `Configure` first for `Handle` and its Get mapping, then for 
`HandleButDifferent` and its Get mapping

[^multiple-routes]: example, you have a `Handle` method with `[HttpMapGet("/a", "/b")]`,
EndpointMapper will call the `Configure` first for /a, then for /b again

[^different-attributes]: example, you have a `Handle` method with `[HttpMapGet("/"), HttpMapPost("/")]`, EndpointMapper
will call the `Configure` first for the Get mapping, then for the Post mapping, BUT if the attribute that you use for 
mapping implies multiple HTTP verb, like an attribute named `[HttpMapGetAndPost("/")]` that bind your endpoint to 
both Get and Post then EndpointMapper will call `Configure` only once for this method because it's in fact a single 
attribute

[^explanation-for-12-call]: explanation of why 12, first we get the first method, it has 2 attributes, we take the
first, we get the 3 routes and as stated in the second rule[^multiple-routes] we call it 3 times total, then we get the 
second attributes and call the `Configure` other 3 times for the second rule[^multiple-routes], and this was the third 
rule[^different-attributes] (the 2 attributes) and we have already called the `Configure` 6 times, then to respect the 
first rule[^different-function] we need to this again, and we get another 6 calls, up to a total of 12

### Method based registration

If you don't like the fact that EndpointMapper uses attributes to map your endpoints or you need to map to a HTTP verb
that does not have an attribute, you can override the `Register` method on the class implementing IEndpoint 
that gives you the `IEndpointRouteBuilder` which has access to methods like `.MapGet()`

> **Warning**
> Don't use `Register(IEndpointRouteBuilder)` if you need to configure stuff like OutputCaching that can accept an
> action to configure its behaviour without creating a policy, for this there is the `Configure(RouteHandlerBuilder)`
> method instead, you can [see more about this here](#method-based-configuration)

> **Note**
> EndpointMapper checks for both Attributes and `Register` to register your endpoints

> **Note**
> If you use a debugger, you will see that the actual type that EndpointMapper uses to call the method is 
> `RouteGroupBuilder`, this is because to respect the configuration of `RoutePrefix` EndpointMapper uses a minimal api
> group to map all your endpoint, you could cast the type back to `RouteGroupBuilder` using an hard cast and use the
> builder to, for example, configure the group with some behaviour settings, for example setting a default output cache 
> policy, although this is NOT encouraged because you can easily forget about it after setting it up
> and since it's in an endpoint class you are configuring the group from an endpoint, which is not that great, since
> you may still want to configure some behaviour in the group, in the `.AddEndpointMapper()` call you can pass an 
> action witch gives you the ability to configure the EndpointMapper configuration and in there you can set the 
> `ConfigureGroupBuilder` action to configure the group, it gives you the full `RouteGroupBuilder` without having to
> cast it and it won't "belong" to any Endpoint that may be deleted later for any reason

### Create your own `EndpointConfigurationAttribute`

Simple create a class that extends `EndpointConfigurationAttribute`

`EndpointConfigurationAttribute` requires you to implement a method called `Configure` where you have access to the 
`RouteHandlerBuilder` from ASP.NET to configure the route, the rest is handled automatically

### Create your own `HttpMapAttribute`

Simple create a class that extends `HttpMapAttribute` and use `: base()` on the constructor to pass the route(s)
and overload the `Methods` properties to set the list of HTTP Verb that the attributes correspond to, the verb MUST be 
supported by ASP.NET or else it wont work, the rest is handled automatically
