using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific HTTP Method and Route
/// </summary>
/// <param name="method">ASP.NET HTTP methods</param>
/// <param name="routes">ASP.NET route strings</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
// The parameters are read directly by the source generator as constructor params, so we don't need to store them
#pragma warning disable CS9113 // Parameter is unread.
public sealed class HttpMapAttribute(string method, [StringSyntax("Route")] params string[] routes) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.
