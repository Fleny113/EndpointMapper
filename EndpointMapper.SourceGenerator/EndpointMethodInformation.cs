using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EndpointMapper.SourceGenerator;

internal sealed record EndpointMethodInformation(ISymbol Method, string HttpVerb, List<string> Routes);