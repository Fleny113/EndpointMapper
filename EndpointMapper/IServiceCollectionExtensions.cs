using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace EndpointMapper;

public static class IServiceCollectionExtensions
{
    private static readonly List<Type> s_endpointTypes = new();

    public static IServiceCollection AddEndpointMapper<T>(this IServiceCollection services, Action<EndpointMapperOptions>? configure = null)
        => services.AddEndpointMapper(configure, typeof(T));

    public static IServiceCollection AddEndpointMapper(this IServiceCollection services, Action<EndpointMapperOptions>? configure = null, params Type[] markers)
        => services.AddEndpointMapper(configure, markers.Select(x => x.Assembly).ToArray());

    public static IServiceCollection AddEndpointMapper(this IServiceCollection services, Action<EndpointMapperOptions>? configure = null, params Assembly[] assemblies)
    {
        // don't modify the default values
        configure ??= _ => { };

        var unused = services.Configure(configure);

        if (assemblies.Length is 0)
            throw new ArgumentException("You must provide at least one assembly to scan", nameof(assemblies));

        foreach (var assembly in assemblies)
            s_endpointTypes.AddRange(assembly.ExportedTypes.Where(cls => cls.IsAssignableTo(typeof(IEndpoint)) && cls.IsClass && !cls.IsAbstract));

        return services;
    }

    public static WebApplication UseEndpointMapper(this WebApplication app)
    {
        var unused = app.UseMiddleware<EndpointMapperMiddleware>();

        var endpoints = s_endpointTypes
            .Select(Activator.CreateInstance)
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
