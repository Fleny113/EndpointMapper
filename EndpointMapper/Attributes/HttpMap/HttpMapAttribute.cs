#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific HTTP Method and Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class HttpMapAttribute : Attribute
{
    /// <summary>
    /// HTTP Verbs to be used when mapping the endpoint
    /// </summary>
    public abstract IEnumerable<string> Methods { get; }
    
    /// <summary>
    /// HTTP Routes that the endpoint will map to
    /// </summary>
    internal IEnumerable<string> Routes { get; }

    /// <summary>
    /// HttpAttribute Constructor with multiple routes
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    protected HttpMapAttribute([StringSyntax("Route")] params string[] routes)
    {
        Routes = routes;
    }
}
