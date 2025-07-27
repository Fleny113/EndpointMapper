using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EndpointMapper.SourceGenerator;

internal sealed record EndpointClassInformation(
    List<EndpointMethodInformation> Endpoints,
    INamedTypeSymbol ClassSymbol,
    bool RegisterImplemented,
    bool ConfigureImplemented);