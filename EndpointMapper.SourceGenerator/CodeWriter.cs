// File copied from https://github.com/dotnet/aspnetcore/blob/2196cfd2cbac60dab2147073c261d237c6c9829c/src/Shared/RoslynUtils/CodeWriter.cs

using System.CodeDom.Compiler;
using System.IO;

namespace EndpointMapper.SourceGenerator;

internal sealed class CodeWriter : IndentedTextWriter
{
    public CodeWriter(StringWriter stringWriter, int baseIndent) : base(stringWriter)
    {
        Indent = baseIndent;
    }

    public void StartBlock()
    {
        WriteLine("{");
        Indent++;
    }

    public void EndBlock()
    {
        Indent--;
        WriteLine("}");
    }

    public void EndBlockWithComma()
    {
        Indent--;
        WriteLine("},");
    }

    public void EndBlockWithSemicolon()
    {
        Indent--;
        WriteLine("};");
    }

    // The IndentedTextWriter adds the indentation
    // _after_ writing the first line of text. This
    // method can be used ot initialize indentation
    // when an emit method might only emit one line
    // of code or when the code writer is emitting
    // indented code as part of a larger string.
    public void InitializeIndent()
    {
        for (var i = 0; i < Indent; i++)
        {
            Write(DefaultTabString);
        }
    }
}