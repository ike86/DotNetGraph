using System.Globalization;
using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class PenWidthAttributeCompiler : AttributeCompilerBase<DotPenWidthAttribute>
    {
        protected override string OnAttributeTypeMatch(DotPenWidthAttribute attribute)
        {
            return string.Format(CultureInfo.InvariantCulture, "penwidth={0:F2}", attribute.Value);
        }
    }
}