using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class EdgeArrowTailAttributeCompiler : AttributeCompilerBase<DotEdgeArrowTailAttribute>
    {
        protected override string OnAttributeTypeMatch(DotEdgeArrowTailAttribute attribute)
        {
            return $"arrowtail={attribute.ArrowType.ToString().ToLowerInvariant()}";
        }
    }
}