#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a PUT Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapPutAttribute : HttpMapAttribute
{
    /// <inheritdoc />
    public override IEnumerable<string> Methods => new[] { HttpMethod.Put.Method };

    /// <summary>
    /// Map route(s) to the PUT Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapPutAttribute([StringSyntax("Route")] params string[] routes) : base(routes)
    {
    }
}
