using DotNetGraph.Attributes;
using DotNetGraph.Extensions;

namespace DotNetGraph.Compiler
{
    public class EdgeStyleAttributeCompiler : FormattedAttributeCompilerBase<DotEdgeStyleAttribute>
    {
        protected override string OnAttributeTypeMatch(DotEdgeStyleAttribute attribute, bool shouldFormat)
        {
            return $"style={DotCompiler.SurroundStringWithQuotes(attribute.Style.FlagsToString(), shouldFormat)}";
        }
    }
}