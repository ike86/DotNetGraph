using DotNetGraph.Core;

namespace DotNetGraph.Compiler
{
    public abstract class AttributeCompilerBase<T> : IAttributeCompiler
        where T : IDotAttribute
    {
        public string Compile(IDotAttribute attribute)
        {
            if (attribute is T matchingAttribute)
            {
                return OnAttributeTypeMatch(matchingAttribute);
            }

            return null;
        }

        protected abstract string OnAttributeTypeMatch(T attribute);
    }
}