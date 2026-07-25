using System.Diagnostics.CodeAnalysis;

namespace EndpointMapper;

/// <summary>
/// Map an endpoint to a specific HTTP Method and Route
/// </summary>
/// <param name="method">ASP.NET HTTP methods</param>
/// <param name="routes">ASP.NET route strings</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
#pragma warning disable CS9113 // Parameter is unread. - We only need these to pass the values to the source generator, so we can ignore the warning about them not being used
public sealed class HttpMapAttribute(string method, [StringSyntax("Route")] params string[] routes) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.
