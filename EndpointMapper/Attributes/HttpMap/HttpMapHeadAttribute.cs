#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a HEAD Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapHeadAttribute : HttpMapAttribute
{
    /// <inheritdoc />
    public override IEnumerable<string> Methods => new[] { HttpMethod.Head.Method };

    /// <summary>
    /// Map route(s) to the HEAD Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapHeadAttribute([StringSyntax("Route")] params string[] routes) : base(routes)
    {
    }
}
