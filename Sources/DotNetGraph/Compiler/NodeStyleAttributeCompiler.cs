using DotNetGraph.Attributes;
using DotNetGraph.Extensions;

namespace DotNetGraph.Compiler
{
    public class NodeStyleAttributeCompiler : FormattedAttributeCompilerBase<DotNodeStyleAttribute>
    {
        protected override string OnAttributeTypeMatch(DotNodeStyleAttribute attribute, bool shouldFormat)
        {
            return $"style={DotCompiler.SurroundStringWithQuotes(attribute.Style.FlagsToString(), shouldFormat)}";
        }
    }
}