#pragma warning disable IDE0130 // Namespace does not match the folder structure

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EndpointMapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class FilterAttribute<T> : Attribute, IEndpointConfigurationAttribute where T : IEndpointFilter
{
    public void Configure(RouteHandlerBuilder builder) => builder.AddEndpointFilter<T>();
}
