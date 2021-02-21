using DotNetGraph.Core;

namespace DotNetGraph.Compiler
{
    public abstract class FormattedAttributeCompilerBase<T> : IFormattedAttributeCompiler
        where T : IDotAttribute
    {
        public string Compile(IDotAttribute attribute, bool shouldFormat)
        {
            if (attribute is T matchingAttribute)
            {
                return OnAttributeTypeMatch(matchingAttribute, shouldFormat);
            }

            return null;
        }

        protected abstract string OnAttributeTypeMatch(T attribute, bool shouldFormat);
    }
}