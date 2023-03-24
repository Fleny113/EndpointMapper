using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace EndpointMapper;

/// <summary>
/// Extensions from EndpointMapper
/// </summary>
public static class EndpointMapperExtensions
{
    private static Type[] _endpointTypes = Array.Empty<Type>();
    private static int _endpointFound;
    private static TimeSpan _timeTookToFindEndpoints;

    /// <summary>
    /// Map all EndpointMapper endpoints into the assembly where the type <typeparamref name="T"/> lives
    /// </summary>
    /// <typeparam name="T">AssemblyScanner Type, you can use types like Program or one of your ones</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddEndpointMapper<T>(this IServiceCollection services,
        Action<EndpointMapperConfiguration>? configure = null)
        => services.AddEndpointMapper(configure, typeof(T).Assembly);

    /// <summary>
    /// Map all EndpointMapper endpoints into the assemblies where the types into <paramref name="markers"/> lives
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <param name="markers">Array of Type for AssemblyScanning</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddEndpointMapper(this IServiceCollection services,
        Action<EndpointMapperConfiguration>? configure = null,
        params Type[] markers)
        => services.AddEndpointMapper(configure, markers.Select(x => x.Assembly).ToArray());

    /// <summary>
    /// Map all EndpointMapper endpoints into the assemblies that are in the <paramref name="assemblies"/>  array
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">Configure the options for EndpointMapper</param>
    /// <param name="assemblies">Array of Assembly for AssemblyScanning</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    [UsedImplicitly]
    public static IServiceCollection AddEndpointMapper(this IServiceCollection services,
        Action<EndpointMapperConfiguration>? configure = null,
        params Assembly[] assemblies)
    {
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            // don't modify the default values
            configure ??= _ => { };

            services.Configure(configure);

            if (assemblies.Length is 0)
                throw new ArgumentException("You must provide at least one assembly to scan", nameof(assemblies));

            _endpointTypes = assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(IEndpoint)))
                .ToArray();

            return services;
        }
        finally
        {
            _timeTookToFindEndpoints = Stopwatch.GetElapsedTime(startTime);
        }
    }

    /// <summary>
    /// Register middleware into the Request Pipeline and map all endpoints as ASP.NET Core Minimal Apis
    /// </summary>
    /// <param name="app"><see cref="WebApplication"/> instance</param>
    /// <param name="addMiddleware">Add or not the middleware for the constructor based DI resolution, default: true</param>
    /// <returns>The <see cref="WebApplication"/> instance for chaining methods</returns>
    [UsedImplicitly]
    public static WebApplication UseEndpointMapper(this WebApplication app, bool addMiddleware = true)
    {
        var startTime = Stopwatch.GetTimestamp();
        
        var options = app.Services.GetRequiredService<IOptions<EndpointMapperConfiguration>>();
        var logger = app.Services.GetRequiredService<ILogger<StartupTimer>>();
        
        try 
        {
            if (addMiddleware)
                app.UseMiddleware<EndpointMapperMiddleware>();

            var endpoints = _endpointTypes
                .Select(RuntimeHelpers.GetUninitializedObject)
                .Cast<IEndpoint>()
                .ToArray();

            var groupBuilder = app.MapGroup(options.Value.RoutePrefix);

            options.Value.ConfigureGroupBuilder(groupBuilder);

            foreach (var endpoint in endpoints)
            {
                // Add the endpoint from a user-defined function given the RouteGroupBuilder
                //  keeping in consideration the RoutePrefix
                endpoint.Register(groupBuilder);

                // Add the endpoint based on the attributes implemented on the methods
                FindEndpoints(endpoint, groupBuilder);
            }

            return app;
        }
        finally
        {
            if (options.Value.LogTimeTookToInitialize)
            {
                var timeTook = Stopwatch.GetElapsedTime(startTime);
                var totalTimeTook = _timeTookToFindEndpoints + timeTook;
            
                logger.LogTrace("Finding {Count} endpoint classes took: {Time}ms",
                    _endpointTypes.Length,
                    _timeTookToFindEndpoints.Milliseconds);

                logger.LogTrace("Registering {Count} route took: {Time}ms", 
                    _endpointFound, 
                    timeTook.Milliseconds);

                logger.LogDebug("Registering {Count} endpoints took: {Time}ms",
                    _endpointFound,
                    totalTimeTook.Milliseconds);   
            }
        }
    }

    private static void FindEndpoints(IEndpoint endpoint, IEndpointRouteBuilder builder)
    {
        var methods = endpoint
            .GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public);

        foreach (var methodInfo in methods)
        {
            var httpMapAttributes = methodInfo.GetCustomAttributes<HttpMapAttribute>(false);
        
            foreach (var httpMapAttribute in httpMapAttributes)
            {
                MapEndpoints(builder, endpoint, methodInfo, httpMapAttribute);
            }
        }
    }

    private static void MapEndpoints(IEndpointRouteBuilder group,
        IEndpoint endpoint,
        MethodInfo method,
        HttpMapAttribute attribute)
    {
        foreach (var route in attribute.Routes)
        {
            var builder = group.MapMethods(route, attribute.Methods, method.CreateDelegate(endpoint))
                .WithMetadata(endpoint);

            _endpointFound++;
            
            // Configure the endpoint based on the attributes on it
            //  this only checks for the attribute implementing IEndpointConfigurationAttribute
            //  not the one from ASP.NET core
            var configurationAttributes = method.GetCustomAttributes<EndpointConfigurationAttribute>(false);

            foreach (var configurationAttribute in configurationAttributes)
                configurationAttribute.Configure(builder);
            
            // Configure the endpoint based on the configure method defined in the class
            endpoint.Configure(builder, route, attribute.Methods, method);
        }
    }

    private static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
    {
        var paramsType = methodInfo.GetParameters().Select(p => p.ParameterType);

        var delegateType = methodInfo.ReturnType switch
        {
            _ when methodInfo.ReturnType == typeof(void) => Expression.GetActionType(paramsType.ToArray()),
            _ => Expression.GetFuncType(paramsType.Concat(new [] { methodInfo.ReturnType }).ToArray())
        };

        return methodInfo.IsStatic
            ? Delegate.CreateDelegate(delegateType, methodInfo)
            : Delegate.CreateDelegate(delegateType, target, methodInfo);
    }
}
