using EndpointMapper;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer()
    .AddJwtBearer("AnotherJWT");

builder.Services.AddOutputCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SecureEndpointAuthRequirementFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Bearer JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });

    c.AddSecurityDefinition("AnotherJWT", new OpenApiSecurityScheme
    {
        Name = "Another JWT",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
});

builder.Services.AddEndpointMapper<Program>(cfg =>
{
    cfg.RoutePrefix = "/api";
    cfg.ConfigureGroupBuilder = groupBuilder =>
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
