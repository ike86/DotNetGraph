using DotNetGraph.Attributes;
using DotNetGraph.Extensions;

namespace DotNetGraph.Compiler
{
    public class FillColorAttributeCompiler : AttributeCompilerBase<DotFillColorAttribute>
    {
        protected override string OnAttributeTypeMatch(DotFillColorAttribute attribute)
        {
            return $"fillcolor=\"{attribute.ToHex()}\"";
        }
    }
}