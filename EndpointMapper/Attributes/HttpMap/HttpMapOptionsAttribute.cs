#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a OPTIONS Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapOptionsAttribute : HttpMapAttribute
{
    /// <summary>
    /// Map route(s) to the OPTIONS Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapOptionsAttribute([StringSyntax("Route")] params string[] routes) : base(HttpMethod.Options, routes) { }
}


