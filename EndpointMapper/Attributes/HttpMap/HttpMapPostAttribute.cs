#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a POST Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapPostAttribute : HttpMapAttribute
{
    /// <summary>
    /// Map route(s) to the POST Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapPostAttribute([StringSyntax("Route")] params string[] routes) : base(HttpMethod.Post, routes) { }
}
