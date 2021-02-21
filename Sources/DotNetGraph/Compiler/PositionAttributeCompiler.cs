using DotNetGraph.Attributes;

namespace DotNetGraph.Compiler
{
    public class PositionAttributeCompiler : AttributeCompilerBase<DotPositionAttribute>
    {
        protected override string OnAttributeTypeMatch(DotPositionAttribute attribute)
        {
            return $"pos=\"{attribute.Position.X},{attribute.Position.Y}!\"";
        }
    }
}