using System.Globalization;
using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class NodeHeightAttributeCompiler : AttributeCompilerBase<DotNodeHeightAttribute>
    {
        protected override string OnAttributeTypeMatch(DotNodeHeightAttribute attribute)
        {
            return string.Format(CultureInfo.InvariantCulture, "height={0:F2}", attribute.Value);
        }
    }
}