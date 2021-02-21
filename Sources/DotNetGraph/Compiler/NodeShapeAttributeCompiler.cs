using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class NodeShapeAttributeCompiler : AttributeCompilerBase<DotNodeShapeAttribute>
    {
        protected override string OnAttributeTypeMatch(DotNodeShapeAttribute attribute)
        {
            return $"shape={attribute.Shape.ToString().ToLowerInvariant()}";
        }
    }
}