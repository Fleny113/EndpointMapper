using ConsoleApp;
using EndpointMapper.TestApplication.NativeAOT;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton("Hi World");

var app = builder.Build();

var todosApi = app.MapGroup("/todos");

todosApi.MapEndpointMapperEndpoints();

app.Run();

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }

namespace ConsoleApp
{
    public static partial class Program
    {
        public static partial IEndpointRouteBuilder MapEndpointMapperEndpoints(this IEndpointRouteBuilder builder);
    }
}
