using DotNetGraph.Attributes;
using DotNetGraph.Extensions;

namespace DotNetGraph.Compiler
{
    public class FontColorAttributeCompiler : AttributeCompilerBase<DotFontColorAttribute>
    {
        protected override string OnAttributeTypeMatch(DotFontColorAttribute attribute)
        {
            return $"fontcolor=\"{attribute.ToHex()}\"";
        }
    }
}