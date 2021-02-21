using DotNetGraph.Core;

namespace DotNetGraph.Compiler
{
    public interface IFormattedAttributeCompiler : IAttributeCompilerUnion
    {
        string Compile(IDotAttribute attribute, bool shouldFormat);
    }
}