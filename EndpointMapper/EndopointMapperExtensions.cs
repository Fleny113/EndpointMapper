using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace EndpointMapper;

/// <summary>
/// Extensions from EndpointMapper
/// </summary>
public static class EndopointMapperExtensions
{
    private static readonly List<Type> s_endpointTypes = new();

    /// <summary>
    /// Map all endpoints into the assembly where the type <typeparamref name="T"/> lives
    /// </summary>
    /// <typeparam name="T">AssemblyScanner Type, you can use types like Program or one of your ones</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <returns>The <see cref="IServiceCollection"/> instace for chaning methods</returns>
    public static IServiceCollection AddEndpointMapper<T>(this IServiceCollection services, Action<EndpointMapperOptions>? configure = null)
        => services.AddEndpointMapper(configure, typeof(T));

    /// <summary>
    /// Map all endpoints into the assemblies where the types into the markes array lives
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <param name="markers">Array of Type for AssemblyScanning</param>
    /// <returns>The <see cref="IServiceCollection"/> instace for chaning methods</returns>
    public static IServiceCollection AddEndpointMapper(this IServiceCollection services, Action<EndpointMapperOptions>? configure = null, params Type[] markers)
        => services.AddEndpointMapper(configure, markers.Select(x => x.Assembly).ToArray());

    /// <summary>
    /// Map all endpoints into the assemblies that are in the assemblies array
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <param name="assemblies">Array of Assembly for AssemblyScanning</param>
    /// <returns>The <see cref="IServiceCollection"/> instace for chaning methods</returns>
    public static IServiceCollection AddEndpointMapper(this IServiceCollection services, Action<EndpointMapperOptions>? configure = null, params Assembly[] assemblies)
    {
        // don't modify the default values
        configure ??= _ => { };

        services.Configure(configure);

        if (assemblies.Length is 0)
            throw new ArgumentException("You must provide at least one assembly to scan", nameof(assemblies));

        foreach (var assembly in assemblies)
        {
            var types = assembly.ExportedTypes.Where(cls => cls.IsClass && !cls.IsAbstract && cls.IsAssignableTo(typeof(IEndpoint)));
            s_endpointTypes.AddRange(types);
        }

        return services;
    }

    /// <summary>
    /// Register middleware into the Request Pipeline and map all endpoints as ASP.NET Core Minimal Apis
    /// </summary>
    /// <param name="app"><see cref="WebApplication"/> instance</param>
    /// <returns>The <see cref="WebApplication"/> instance for chaning methods</returns>
    public static WebApplication UseEndpointMapper(this WebApplication app)
    {
        app.UseMiddleware<EndpointMapperMiddleware>();

        using var scope = app.Services.CreateScope();

        var endpoints = s_endpointTypes
            .Select(t => ActivatorUtilities.CreateInstance(scope.ServiceProvider, t))
            .Cast<IEndpoint>()
            .ToArray();

        foreach (var endpoint in endpoints)
        {
            endpoint.Configure(app);

            MapEndpoints(app, endpoint);
        }

        return app;
    }

    private static void MapEndpoints(WebApplication app, IEndpoint endpoint)
    {
        var options = app.Services.GetRequiredService<IOptions<EndpointMapperOptions>>();

        foreach (var method in endpoint.GetType().GetMethods())
            MapAttributes(app, endpoint, method, options.Value);
    }

    private static void MapAttributes(WebApplication app, IEndpoint endpoint, MethodInfo method, EndpointMapperOptions options)
    {
        foreach (var customAttribute in method.CustomAttributes)
        {
            if (!customAttribute.AttributeType.IsAssignableTo(typeof(HttpMapAttribute)))
                continue;

            var attribute = method.GetCustomAttribute(customAttribute.AttributeType);

            if (attribute is not HttpMapAttribute mapAttribute)
                continue;

            MapMethods(app, endpoint, method, mapAttribute, options);
        }
    }

    private static void MapMethods(WebApplication app, IEndpoint endpoint, MethodInfo method, HttpMapAttribute attribute, EndpointMapperOptions options)
    {
        foreach (var route in attribute.Routes)
        {
            var builder = app.MapMethods($"{options.RoutePrefix}{route}", new[] { attribute.Verb.Method }, method.CreateDelegate(endpoint))
                .WithMetadata(endpoint);

            ConfigurationAttributes(builder, method);
        }
    }

    private static void ConfigurationAttributes(RouteHandlerBuilder builder, MethodInfo method)
    {
        foreach (var customAttribute in method.CustomAttributes)
        {
            if (!customAttribute.AttributeType.IsAssignableTo(typeof(IEndpointConfigurationAttribute)))
                continue;

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
