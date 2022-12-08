#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific HTTP Method and Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public abstract class HttpMapAttribute : Attribute
{
    internal HttpMethod Verb { get; set; }
    internal IEnumerable<string> Routes { get; set; }

    /// <summary>
    /// HttpAttribute Constuctor
    /// </summary>
    /// <param name="verb">HTTP Method</param>
    /// <param name="route">ASP.NET route string</param>
    protected HttpMapAttribute(HttpMethod verb, [StringSyntax("Route")] string route)
    {
        Verb = verb;
        Routes = new List<string> { route };
    }

    /// <summary>
    /// HttpAttribute Constuctor
    /// </summary>
    /// <param name="verb">HTTP Method</param>
    /// <param name="routes">ASP.NET route strings</param>
    protected HttpMapAttribute(HttpMethod verb, [StringSyntax("Route")] params string[] routes)
    {
        Verb = verb;
        Routes = routes;
    }
}
