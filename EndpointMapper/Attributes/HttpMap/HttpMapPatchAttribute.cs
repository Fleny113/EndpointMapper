#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a PATCH Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapPatchAttribute : HttpMapAttribute
{
    /// <summary>
    /// Map route(s) to the PATCH Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapPatchAttribute([StringSyntax("Route")] params string[] routes) : base(HttpMethod.Patch, routes) { }
}
