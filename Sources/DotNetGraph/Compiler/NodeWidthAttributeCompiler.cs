using System.Globalization;
using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class NodeWidthAttributeCompiler : AttributeCompilerBase<DotNodeWidthAttribute>
    {
        protected override string OnAttributeTypeMatch(DotNodeWidthAttribute attribute)
        {
            return string.Format(CultureInfo.InvariantCulture, "width={0:F2}", attribute.Value);
        }
    }
}