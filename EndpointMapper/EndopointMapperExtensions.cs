using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace EndpointMapper;

/// <summary>
/// Extensions from EndpointMapper
/// </summary>
public static class EndpointMapperExtensions
{
    private static Type[] _endpointTypes = Array.Empty<Type>();

    /// <summary>
    /// Map all endpoints into the assembly where the type <typeparamref name="T"/> lives
    /// </summary>
    /// <typeparam name="T">AssemblyScanner Type, you can use types like Program or one of your ones</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <returns>The <see cref="IServiceCollection"/> instance for chaining methods</returns>
    public static IServiceCollection AddEndpointMapper<T>(this IServiceCollection services,
        Action<EndpointMapperOptions>? configure = null)
        => services.AddEndpointMapper(configure, typeof(T));

    /// <summary>
    /// Map all endpoints into the assemblies where the types into the markes array lives
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <param name="markers">Array of Type for AssemblyScanning</param>
    /// <returns>The <see cref="IServiceCollection"/> instance for chaining methods</returns>
    public static IServiceCollection AddEndpointMapper(this IServiceCollection services,
        Action<EndpointMapperOptions>? configure = null,
        params Type[] markers)
        => services.AddEndpointMapper(configure, markers.Select(x => x.Assembly).ToArray());

    /// <summary>
    /// Map all endpoints into the assemblies that are in the assemblies array
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <param name="assemblies">Array of Assembly for AssemblyScanning</param>
    /// <returns>The <see cref="IServiceCollection"/> instance for chaining methods</returns>
    public static IServiceCollection AddEndpointMapper(this IServiceCollection services,
        Action<EndpointMapperOptions>? configure = null,
        params Assembly[] assemblies)
    {
        // don't modify the default values
        configure ??= _ => { };

        services.Configure(configure);

        if (assemblies.Length is 0)
            throw new ArgumentException("You must provide at least one assembly to scan", nameof(assemblies));

        _endpointTypes = assemblies
            .SelectMany(a => a.ExportedTypes)
            .Where(c => c is { IsClass: true, IsAbstract: false } && c.IsAssignableTo(typeof(IEndpoint)))
            .ToArray();

        return services;
    }

    /// <summary>
    /// Register middleware into the Request Pipeline and map all endpoints as ASP.NET Core Minimal Apis
    /// </summary>
    /// <param name="app"><see cref="WebApplication"/> instance</param>
    /// <returns>The <see cref="WebApplication"/> instance for chaining methods</returns>
    public static WebApplication UseEndpointMapper(this WebApplication app)
    {
        app.UseMiddleware<EndpointMapperMiddleware>();
        var options = app.Services.GetRequiredService<IOptions<EndpointMapperOptions>>();

        using var scope = app.Services.CreateScope();

        var endpoints = _endpointTypes
            .Select(t => ActivatorUtilities.CreateInstance(scope.ServiceProvider, t))
            .Cast<IEndpoint>()
            .ToArray();

        foreach (var endpoint in endpoints)
        {
            var groupBuilder = app.MapGroup(options.Value.RoutePrefix);

            // Configure the groupBuilder
            options.Value.ConfigureGroupBuilder(groupBuilder);
            
            // Add the endpoint from a user-defined function given the WebApplication
            //  keeping in consideration the RoutePrefix
            endpoint.Register(groupBuilder);

            // Add the endpoint based on the attributes implemented on the methods
            foreach (var method in endpoint.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
                MapAttributes(groupBuilder, endpoint, method);
        }

        return app;
    }

    private static void MapAttributes(IEndpointRouteBuilder group, IEndpoint endpoint, MethodInfo method)
    {
        foreach (var customAttribute in method.CustomAttributes)
        {
            var attribute = method.GetCustomAttribute(customAttribute.AttributeType);

            if (attribute is HttpMapAttribute mapAttribute)
                MapEndpoints(group, endpoint, method, mapAttribute);
        }
    }

    private static void MapEndpoints(IEndpointRouteBuilder group, IEndpoint endpoint, MethodInfo method,
        HttpMapAttribute attribute)
    {
        foreach (var route in attribute.Routes)
        {
            var builder = group.MapMethods(route, attribute.Methods, method.CreateDelegate(endpoint))
                .WithMetadata(endpoint);

            // Configure the endpoint based on the attributes on it
            //  this only checks for the attribute implementing IEndpointConfigurationAttribute
            //  not the one from ASP.NET core
            ConfigurationAttributes(builder, method);
            
            // Configure the endpoint based on the configure method defined in the class
            endpoint.Configure(builder);
        }
    }

    private static void ConfigurationAttributes(RouteHandlerBuilder builder, MemberInfo method)
    {
        foreach (var customAttribute in method.CustomAttributes)
        {
            var custom = method.GetCustomAttribute(customAttribute.AttributeType);

            if (custom is IEndpointConfigurationAttribute attribute)
                attribute.Configure(builder);
        }
    }

    private static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
    {
        Func<Type[], Type> getType;
        var isAction = methodInfo.ReturnType == typeof(void);
        var paramsType = methodInfo.GetParameters().Select(p => p.ParameterType);

        if (isAction)
        {
            getType = Expression.GetActionType;
        }
        else
        {
            getType = Expression.GetFuncType;
            paramsType = paramsType.Concat(new[] { methodInfo.ReturnType });
        }

        return methodInfo.IsStatic
            ? Delegate.CreateDelegate(getType(paramsType.ToArray()), methodInfo)
            : Delegate.CreateDelegate(getType(paramsType.ToArray()), target, methodInfo.Name);
    }
}
