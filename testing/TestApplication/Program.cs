using EndpointMapper;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer()
    .AddJwtBearer("AnotherJWT");

builder.Services.AddOutputCache();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // Add Security Definitions (Schemes)
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Name = "Bearer JWT",
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Scheme = "Bearer"
        };

        document.Components.SecuritySchemes["AnotherJWT"] = new OpenApiSecurityScheme
        {
            Name = "Another JWT",
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Scheme = "Bearer"
        };

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "v1"));

app.UseOutputCache();

var apiGroup = app.MapGroup("/api");

apiGroup.MapDelete("/helloWorld", () => "Hello World!");
apiGroup.MapEndpointMapperEndpoints();

app.Run();
