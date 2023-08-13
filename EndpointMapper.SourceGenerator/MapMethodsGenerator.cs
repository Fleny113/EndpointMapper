using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System;

namespace EndpointMapper.SourceGenerator;

[Generator]
public class MapMethodsGenerator : IIncrementalGenerator
{
#if DEBUG
    public MapMethodsGenerator()
    {
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            // System.Diagnostics.Debugger.Launch();
        }
    }
#endif

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, ct) => node.IsKind(SyntaxKind.ClassDeclaration),
                (context, ct) =>
                {
                    var model = context.SemanticModel;
                    var classDeclaration = (ClassDeclarationSyntax)context.Node;
                    var classSymbol = model.GetDeclaredSymbol(classDeclaration);
                    var interfaceType = model.Compilation.GetTypeByMetadataName("EndpointMapper.IEndpoint");

                    if (classSymbol is null || interfaceType is null)
                        return null!;

                    if (classSymbol.Interfaces.Contains(interfaceType))
                        return classSymbol.GetMembers().OfType<IMethodSymbol>().ToArray();

                    return null!;
                }
            )
            .Where(x => x is not null);

        context.RegisterSourceOutput(methods, (context, methods) =>
        {
            var strBuilder = new StringBuilder();
            // TODO: don't rely on the ConsoleApp namespace
            strBuilder.Append("""
                namespace ConsoleApp
                {
                    public partial class Program
                    {
                        private static readonly string[] ConnectVerb = new[] { "CONNECT" };
                        private static readonly string[] HeadVerb = new[] { "HEAD" };
                        private static readonly string[] OptionsVerb = new[] { "OPTIONS" };
                        private static readonly string[] TraceVerb = new[] { "TRACE" };

                        public static partial global::Microsoft.AspNetCore.Builder.WebApplication HelloFrom(global::Microsoft.AspNetCore.Builder.WebApplication app, bool addMiddleware)
                        {
                            if (addMiddleware)
                                global::Microsoft.AspNetCore.Builder.UseMiddlewareExtensions.UseMiddleware<global::EndpointMapper.EndpointMapperMiddleware>(app);

                """);

            strBuilder.AppendLine("");

            foreach (var method in methods)
            {
                var methodAttributes = method.GetAttributes();
                var mapAttributes = methodAttributes
                    .Where(x => x.AttributeClass is { Name: "HttpMapAttribute", ContainingNamespace.Name: "EndpointMapper" });

                foreach (var attribute in mapAttributes)
                {
                    var httpMethodArgument = attribute.ConstructorArguments[0];
                    var routesArgument = attribute.ConstructorArguments[1];

                    if (httpMethodArgument.Kind is not TypedConstantKind.Primitive || routesArgument.Kind is not TypedConstantKind.Array)
                        continue;

                    var httpMethod = httpMethodArgument.Value;
                    var routes = routesArgument.Values.Select(x => x.Value).ToArray();


                    // Get, Post, Put, Delete, Patch have a built-in method so we use that.
                    var endpointRouteBuilderMethod = httpMethod switch
                    {
                        "GET" => "MapGet",
                        "POST" => "MapPost",
                        "PUT" => "MapPut",
                        "DELETE" => "MapDelete",
                        "PATCH" => "MapPatch",
                        _ => "MapMethods",
                    };

                    var MapMethodSecondParam = endpointRouteBuilderMethod is "MapMethods" ? $", {GetVerb(httpMethod)}" : "";

                    foreach (var route in routes)
                    {
                        // TODO: Add support for instance methods
                        strBuilder.AppendLine($$"""
                                     global::Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.{{endpointRouteBuilderMethod}}(app, "{{route}}"{{MapMethodSecondParam}}, global::{{method.ContainingType.ToDisplayString()}}.{{method.Name}});
                         """);
                    }
                }
            }

            strBuilder.Append("""

                            return app;
                        }
                    }
                }
                """);

            context.AddSource("Program.g.cs", strBuilder.ToString());
        });
    }

    private static string GetVerb(object? httpMethod)
    {
        return httpMethod switch
        {
            "CONNECT" => "ConnectVerb",
            "HEAD" => "HeadVerb",
            "OPTIONS" => "OptionsVerb",
            "TRACE" => "TraceVerb",
            _ => throw new NotSupportedException($"HttpMethod {httpMethod} is not supported!"),
        };
    }
}
