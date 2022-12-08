using Microsoft.AspNetCore.Builder;

namespace EndpointMapper;

/// <summary>
/// Interface to implement for the AssemblyScanner to detect and registrate your endpoint
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Use the <see cref="WebApplication"/> to map (and optionally configure) the routes into the endpoint
    /// </summary>
    /// <param name="builder">WebApplication to registrare and configure the routes</param>
    public virtual void Configure(WebApplication builder)
    {
    }
}
