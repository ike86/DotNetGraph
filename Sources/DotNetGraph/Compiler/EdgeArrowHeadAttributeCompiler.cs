using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class EdgeArrowHeadAttributeCompiler : AttributeCompilerBase<DotEdgeArrowHeadAttribute>
    {
        protected override string OnAttributeTypeMatch(DotEdgeArrowHeadAttribute attribute)
        {
            return $"arrowhead={attribute.ArrowType.ToString().ToLowerInvariant()}";
        }
    }
}