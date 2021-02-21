using DotNetGraph.Core;

namespace DotNetGraph.Compiler
{
    public interface IAttributeCompiler : IAttributeCompilerUnion
    {
        string Compile(IDotAttribute attribute);
    }
}