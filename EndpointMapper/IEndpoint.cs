using Microsoft.AspNetCore.Builder;

namespace EndpointMapper;

public interface IEndpoint
{
    public virtual void Configure(WebApplication builder)
    {
    }
}
