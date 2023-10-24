using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EndpointMapper.SourceGenerator;

internal class EndpointMethodInformation
{
    public ISymbol Method { get; set; } = null!;
    public string HttpVerb { get; set; } = null!;
    public IList<string> Routes { get; set; } = null!;
}