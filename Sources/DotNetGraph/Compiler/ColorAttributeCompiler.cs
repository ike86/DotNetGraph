using DotNetGraph.Attributes;
using DotNetGraph.Extensions;

namespace DotNetGraph.Compiler
{
    public class ColorAttributeCompiler : AttributeCompilerBase<DotColorAttribute>
    {
        protected override string OnAttributeTypeMatch(DotColorAttribute attribute)
        {
            return $"color=\"{attribute.ToHex()}\"";
        }
    }
}