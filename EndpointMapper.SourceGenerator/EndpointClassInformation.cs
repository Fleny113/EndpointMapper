using System.Collections.Immutable;

namespace EndpointMapper.SourceGenerator;

internal sealed record EndpointClassInformation(ImmutableArray<EndpointMethodInformation> Endpoints, string ClassName);