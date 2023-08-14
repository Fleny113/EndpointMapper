using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using EndpointClassInformation = (Microsoft.CodeAnalysis.IMethodSymbol[] methods, bool implementsRegisterMethod, bool implementsConfigureMethod);

namespace EndpointMapper.SourceGenerator;

[Generator]
public class MapMethodsGenerator : IIncrementalGenerator
{
#if false
    public MapMethodsGenerator()
    {
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
    }
#endif

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var endpoints = context.SyntaxProvider
            .CreateSyntaxProvider(predicate: (node, _) => node is ClassDeclarationSyntax, transform: SyntaxProviderEndpointTransformer)
            .Where(x => x is { methods.Length: > 0 })
            .Collect();

        var mapEndpointsLocations = context.SyntaxProvider
            .CreateSyntaxProvider(predicate: (node, _) => node is InvocationExpressionSyntax, transform: SyntaxProviderLocationTransformer)
            .Where(x => x is not null)
            .Collect();

        var endpointsAndMapLocation = endpoints.Combine(mapEndpointsLocations);

        context.RegisterSourceOutput(source: endpointsAndMapLocation, action: SourceOutputAction);
    }

    private static EndpointClassInformation SyntaxProviderEndpointTransformer(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classSymbol = context.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax) context.Node);

        var endpointInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("EndpointMapper.IEndpoint");
        var endpointRegisterInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("EndpointMapper.IRegisterEndpoint");
        var endpointConfigureInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("EndpointMapper.IConfigureEndpoint");

        if (
            classSymbol is null || 
            endpointInterface is null || 
            endpointRegisterInterface is null || 
            endpointConfigureInterface is null || 
            !classSymbol.Interfaces.Contains(endpointInterface)
        )
            return (Array.Empty<IMethodSymbol>(), false, false);

        var methods = classSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(x => x.IsStatic)
            .ToArray();

        return (
            methods,
            classSymbol.Interfaces.Contains(endpointRegisterInterface),
            classSymbol.Interfaces.Contains(endpointConfigureInterface)
        );
    }

    private static Location SyntaxProviderLocationTransformer(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var invocationExpression = (InvocationExpressionSyntax) context.Node;

        var invocationSymbol = context.SemanticModel.GetSymbolInfo(invocationExpression);

        var extensionMethod = context.SemanticModel.Compilation
            .GetTypeByMetadataName("EndpointMapper.EndpointMapperExtensions")
            ?.GetMembers("MapEndpointMapperEndpoints")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.IsExtensionMethod);

        if (
            invocationSymbol.Symbol is not IMethodSymbol methodSymbol ||
            methodSymbol.ReducedFrom is null ||
            extensionMethod is null ||
            !methodSymbol.ReducedFrom.Equals(extensionMethod, SymbolEqualityComparer.Default)
        )
            return null!;

        var memberAccessExpression = (MemberAccessExpressionSyntax) invocationExpression.Expression;

        return memberAccessExpression.Name.GetLocation();
    }

    private static void SourceOutputAction(SourceProductionContext context, (ImmutableArray<EndpointClassInformation> left, ImmutableArray<Location> rigth) endpointsAndMapLocation)
    {
        var strBuilderMapping = new StringBuilder();
        var strBuilderLocations = new StringBuilder();

        var (endpoints, locations) = endpointsAndMapLocation;

        foreach (var location in locations)
        {
            if (location.SourceTree is null)
                continue;

            var lineSpan = location.GetLineSpan();

            strBuilderLocations.AppendLine("");
            strBuilderLocations.Append($"""
                    [global::System.Runtime.CompilerServices.InterceptsLocation(@"{location.SourceTree.FilePath}", {lineSpan.StartLinePosition.Line + 1}, {lineSpan.StartLinePosition.Character + 1})]
            """);
        }

        foreach (var (methods, implementsRegisterMethod, implementsConfigureMethod) in endpoints)
        {
            var containingType = methods[0].ContainingType.ToDisplayString();

            strBuilderMapping.AppendLine($"            // Mapping endpoints in {containingType}");

            if (implementsRegisterMethod)
                strBuilderMapping.AppendLine($"            global::{containingType}.Register(builder);");

            MapMethods(strBuilderMapping, methods, implementsConfigureMethod, containingType);

            strBuilderMapping.AppendLine("");
        }

        context.AddSource("Endpoints.g.cs", $$"""
            // <auto-generated/>

            #nullable enable
            #pragma warning disable CS9113

            namespace EndpointMapper.SourceGenerator
            {
                public static class EndpointMapperExtensions
                {
                    private static readonly string[] ConnectVerb = new[] { "CONNECT" };
                    private static readonly string[] HeadVerb = new[] { "HEAD" };
                    private static readonly string[] OptionsVerb = new[] { "OPTIONS" };
                    private static readonly string[] TraceVerb = new[] { "TRACE" };
            {{strBuilderLocations}}
                    public static global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder MapEndpointMapperEndpoints(this global::Microsoft.AspNetCore.Routing.IEndpointRouteBuilder builder)
                    {
            {{strBuilderMapping}}
                        return builder;
                    }
                }
            }

            namespace System.Runtime.CompilerServices
            {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute
                {
                }
            }
            """);
    }

    private static void MapMethods(StringBuilder strBuilder, IMethodSymbol[] methods, bool implementsConfigureMethod, string containingType)
    {
        foreach (var method in methods)
        {
            var attributes = method
                .GetAttributes()
                .Where(x => x.AttributeClass is { Name: "HttpMapAttribute", ContainingNamespace.Name: "EndpointMapper" })
                .ToArray();

            MapAttributes(strBuilder, implementsConfigureMethod, containingType, method, attributes);
        }
    }

    private static void MapAttributes(StringBuilder strBuilder, bool implementsConfigureMethod, string containingType, IMethodSymbol method, AttributeData[] attributes)
    {
        foreach (var attribute in attributes)
        {
            if (!IsValidAttribute(attribute.ConstructorArguments[0], attribute.ConstructorArguments[1], out var httpMethod, out var routes))
                continue;

            MapRoutes(strBuilder, implementsConfigureMethod, containingType, method, httpMethod, routes);
        }
    }

    private static void MapRoutes(StringBuilder strBuilder, bool implementsConfigureMethod, string containingType, IMethodSymbol method, string httpMethod, string[] routes)
    {
        foreach (var route in routes)
        {
            // Get, Post, Put, Delete, Patch have a built-in method so we use that.
            var endpointRouteBuilderMethod = httpMethod switch
            {
                "GET" => $@"MapGet(builder, ""{route}"",",
                "POST" => $@"MapPost(builder, ""{route}"",",
                "PUT" => $@"MapPut(builder, ""{route}"",",
                "DELETE" => $@"MapDelete(builder, ""{route}"",",
                "PATCH" => $@"MapPatch(builder, ""{route}"",",
                _ => $@"MapMethods(builder, ""{route}"", {GetHttpMethods(httpMethod)},",
            };

            var endpointRouterBuilder = $$"""global::Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.{{endpointRouteBuilderMethod}} global::{{containingType}}.{{method.Name}})""";

            if (!implementsConfigureMethod)
            {
                strBuilder.AppendLine($"            {endpointRouterBuilder};");
                continue;
            }

            strBuilder.AppendLine($$"""            global::{{containingType}}.Configure({{endpointRouterBuilder}}, "{{route}}", "{{httpMethod}}");""");
        }
    }

    private static bool IsValidAttribute(TypedConstant httpMethodArgument, TypedConstant routesArgument, out string httpMethod, out string[] routes)
    {
        httpMethod = null!;
        routes = null!;

        if (httpMethodArgument is not { Kind: TypedConstantKind.Primitive, Value: string httpMethodOut })
            return false;

        httpMethod = httpMethodOut;

        if (routesArgument is not { Kind: TypedConstantKind.Array, Values: ImmutableArray<TypedConstant> routesArray })
            return false;

        if (routesArray.Select(x => x.Value).Cast<string>().ToArray() is not string[] routesOut)
            return false;

        routes = routesOut;

        return true;
    }

    private static string GetHttpMethods(object? httpMethod)
    {
        return httpMethod switch
        {
            "CONNECT" => "ConnectVerb",
            "HEAD" => "HeadVerb",
            "OPTIONS" => "OptionsVerb",
            "TRACE" => "TraceVerb",
            _ => throw new NotSupportedException($"HTTPMethod {httpMethod} is not supported!"),
        };
    }
}
