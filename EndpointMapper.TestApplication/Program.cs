using System.Reflection;
using EndpointMapper;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer()
    .AddJwtBearer("AnotherJWT");

builder.Services.AddOutputCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Bearer JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });

    config.AddSecurityDefinition("AnotherJWT", new OpenApiSecurityScheme
    {
        Name = "Another JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    
    config.OperationFilter<SecureEndpointAuthRequirementFilter>();
    
    // Get the XML file path from the Assembly
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    // Add the comments into the generation for the OpenApi scheme
    config.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), includeControllerXmlComments: true);
});

builder.Services.AddEndpointMapper<Program>(config =>
{
    config.RoutePrefix = "/api";
    config.ConfigureGroupBuilder = groupBuilder =>
    {
        groupBuilder.MapDelete("/helloWorld", () => "Hello World!");
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOutputCache();

app.UseEndpointMapper();

app.Run();
