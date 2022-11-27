#pragma warning disable IDE0130 // Namespace does not match the folder structure

namespace EndpointMapper;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public abstract class HttpMapAttribute : Attribute
{
    internal HttpMethod Verb { get; set; }
    internal IEnumerable<string> Routes { get; set; }

    protected HttpMapAttribute(HttpMethod verb, string route)
    {
        Verb = verb;
        Routes = new List<string> { route };
    }

    protected HttpMapAttribute(HttpMethod verb, params string[] routes)
    {
        Verb = verb;
        Routes = routes;
    }
}
