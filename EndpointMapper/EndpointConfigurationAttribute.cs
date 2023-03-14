using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace EndpointMapper;

/// <summary>
/// Interface to implement for the AssemblyScanner to detect the ConfigurationAttribute
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public abstract class EndpointConfigurationAttribute : Attribute
{
    /// <summary>
    /// Do extra configuration with the <see cref="RouteHandlerBuilder"/> of a route
    /// </summary>
    /// <param name="builder">Builder of the Route for calling configuration methods</param>
    public abstract void Configure(RouteHandlerBuilder builder);
}
