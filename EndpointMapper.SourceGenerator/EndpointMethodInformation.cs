using System.Collections.Immutable;

namespace EndpointMapper.SourceGenerator;

internal sealed record EndpointMethodInformation(string MethodName, string HttpVerb, ImmutableArray<string> Routes);