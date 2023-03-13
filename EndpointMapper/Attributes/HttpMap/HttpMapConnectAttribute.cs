#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a CONNECT Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapConnectAttribute : HttpMapAttribute
{
    /// <inheritdoc />
    public override IEnumerable<string> Methods => new[] { HttpMethod.Connect.Method };

    /// <summary>
    /// Map route(s) to the CONNECT Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapConnectAttribute([StringSyntax("Route")] params string[] routes) : base(routes)
    {
    }
}
