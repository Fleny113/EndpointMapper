using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EndpointMapper.SourceGenerator;

internal class EndpointClassInformation
{
    public IList<ISymbol> Methods { get; set; } = null!;
    public bool RegisterImplemented { get; set; }
    public bool ConfigureImplemented { get; set; }

    public void Deconstruct(out IList<ISymbol> methods, out bool registerImplemented, out bool configureImplemented)
    {
        methods = Methods;
        registerImplemented = RegisterImplemented;
        configureImplemented = ConfigureImplemented;
    }
}