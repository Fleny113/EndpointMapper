using Microsoft.CodeAnalysis;

namespace EndpointMapper.SourceGenerator;

internal class EndpointMethodAttributes
{
    public ISymbol Method { get; set; } = null!;
    public AttributeData Attribute { get; set; } = null!;
}