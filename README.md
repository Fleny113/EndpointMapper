# EndpointMapper

Build on top of Minimal APIs and easy to use

## Installation

```sh
dotnet add package EndpointMapper
```

## Requiments

- .NET 7

## Usage

Simple add 
- `builder.Services.AddEndpointMapper<T>();`
- `app.UseEndpointMapper();`
- A class extending IEndpoint (see [Samples](#sample) for an example)

> **Note**:
> If you want to use Swagger see [this section under Advanced](#swagger)

#### Sample

Program.cs: 
```cs
using EndpointMapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointMapper<Program>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseEndpointMapper();

app.Run();
```

ExampleEndpoint.cs:
```cs
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
> If you want to bind values from the request into params of the request see [this section under Advanced](#parameters-to-the-handler-method)

## Advanced

### Swagger
You can use `Swashbuckle.AspNetCore` (and `Microsoft.AspNetCore.OpenApi`) and it will work out of the box.

#### Authentication

If you have authentication you need to add the SecurityDefinition(s) for swagger and an OperationFilter for swagger
to get the requied values to see as secure each endpoint in the `AddSwaggerGen` configure action.

There is a OperationFilter into EndpointMapper to do add all the SecurityRequiments to the endpoints 
called `SecureEndpointAuthRequirementFilter`, you can use it by adding `OperationFilter<SecureEndpointAuthRequirementFilter>()`
to the `AddSwaggerGen()` call.

Example of AddSwaggerGen call[^1]:
```cs
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SecureEndpointAuthRequirementFilter>();

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Bearer JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
});
```

#### XML Documentation
You will need to add:

- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to your csproj
- (Optionally) `<NoWarn>$(NoWarn);1591</NoWarn>` to disable the warning [`Missing XML comment for publicly visible type or member 'Type_or_Member'`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591)
- The following 2 lines of code to the `AddSwaggerGen()` call (You may need to add `using System.Reflection;` to the top of the file)
```cs
var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
```

### Create your own `IEndpointConfigurationAttribute`

Simple create a class that extends `Attribute` and `IEndpointConfigurationAttribute`

`IEndpointConfigurationAttribute` requires you to implement a method called `Configure` where you have access to the `RouteHandlerBuilder`
from ASP.NET, the rest is handled automatically

### Create your own `HttpMapAttribute`

Simple create a class that extends `HttpMapAttribute` and use `: base()` on the constructor to pass the HttpVerb and the route(s),
the rest is handled automatically

### Parameters to the Handler Method

You can you use the method like it was an inline lambda from the `MapGet()` method on the `WebApplication`, so you can do:
- Dependency Injection (from the method params)
- Bind Query/Route/Body/Header values into params

### Method based configuration

If you prefer you can implement a `Configure` method into the class that implement IEndpoint witch give you the ability to use the `WebApplication`
to manually configure the routes and all the configuration to it

> **Warning**:
> Doing this **WILL** cause a few thing to don't be respected, for example, the `RoutePrefix` will NOT be respected as the code to map the endpoint
is not EndpointMapper but it's your implementation for the `Configure` method.

[^1]: See EndpointMapper.TestApplication/Program.cs for more.
