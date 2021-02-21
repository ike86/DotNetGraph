using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class LabelAttributeCompiler : FormattedAttributeCompilerBase<DotLabelAttribute>
    {
        protected override string OnAttributeTypeMatch(DotLabelAttribute attribute, bool shouldFormat)
        {
            return $"label={DotCompiler.SurroundStringWithQuotes(attribute.Text, shouldFormat)}";
        }
    }
}