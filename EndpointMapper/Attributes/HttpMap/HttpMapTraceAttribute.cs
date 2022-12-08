#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a TRACE Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapTraceAttribute : HttpMapAttribute
{
    /// <summary>
    /// Map route(s) to the TRACE Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapTraceAttribute([StringSyntax("Route")] params string[] routes) : base(HttpMethod.Trace, routes) { }
}
