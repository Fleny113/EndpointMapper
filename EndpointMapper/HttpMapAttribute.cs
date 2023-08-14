using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific HTTP Method and Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class HttpMapAttribute : Attribute
{
    /// <summary>
    /// HTTP Verbs to be used when mapping the endpoint
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// HTTP Routes that the endpoint will map to
    /// </summary>
    internal string[] Routes { get; }

    /// <summary>
    /// HttpAttribute Constructor with multiple routes
    /// </summary>
    /// <param name="method">ASP.NET HTTP methods</param>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapAttribute(string method, [StringSyntax("Route")] params string[] routes)
    {
        Routes = routes;
        Method = method;
    }
}
