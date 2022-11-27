#pragma warning disable IDE0130 // Namespace does not match the folder structure

namespace EndpointMapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class HttpMapPutAttribute : HttpMapAttribute
{
    public HttpMapPutAttribute(params string[] routes) : base(HttpMethod.Put, routes) { }
}
