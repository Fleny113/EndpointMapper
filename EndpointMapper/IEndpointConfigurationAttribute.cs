using Microsoft.AspNetCore.Builder;

namespace EndpointMapper;

public interface IEndpointConfigurationAttribute
{
    void Configure(RouteHandlerBuilder builder);
}
