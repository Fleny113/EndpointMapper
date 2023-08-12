using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;

namespace EndpointMapper.SourceGenerator;

[Generator(LanguageNames.CSharp)]
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
                        public static partial global::Microsoft.AspNetCore.Builder.WebApplication HelloFrom(global::Microsoft.AspNetCore.Builder.WebApplication app, bool addMiddleware)
                        {
                            if (addMiddleware)
                                global::Microsoft.AspNetCore.Builder.UseMiddlewareExtensions.UseMiddleware<global::EndpointMapper.EndpointMapperMiddleware>(app);

                """);

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

                    // TODO: proper support multiple routes
                    // TODO: map to the proper handler
                    strBuilder.AppendLine($$"""            global::Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapMethods(app, "{{routes[0]}}", new[] { "{{httpMethod}}" }, () => "Hello from the IIncrementalGenerator");""");
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
}
