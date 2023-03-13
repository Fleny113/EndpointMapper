#pragma warning disable IDE0130 // Namespace does not match the folder structure

using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific to a GET Http Verb and a Route
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class HttpMapGetAttribute : HttpMapAttribute
{
    /// <inheritdoc />
    public override IEnumerable<string> Methods => new[] { HttpMethod.Get.Method };

    /// <summary>
    /// Map route(s) to the GET Http Verb
    /// </summary>
    /// <param name="routes">ASP.NET route strings</param>
    public HttpMapGetAttribute([StringSyntax("Route")] params string[] routes) : base(routes)
    {
    }
}
