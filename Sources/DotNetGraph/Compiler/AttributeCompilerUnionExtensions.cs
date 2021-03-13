using System;

namespace DotNetGraph.Compiler
{
    public static class AttributeCompilerUnionExtensions
    {
        public static TResult Convert<TResult>(
            this IAttributeCompilerUnion attributeCompiler,
            Func<IAttributeCompiler, TResult> caseAttributeCompiler,
            Func<IFormattedAttributeCompiler, TResult> caseFormattedAttributeCompiler)
        {
            if (attributeCompiler is IAttributeCompiler x)
            {
                return caseAttributeCompiler(x);
            }

            if (attributeCompiler is IFormattedAttributeCompiler y)
            {
                return caseFormattedAttributeCompiler(y);
            }

            throw new ArgumentOutOfRangeException(
                $"{nameof(IAttributeCompilerUnion)} is a closed hierarchy. " // TODO it is not closed and probably not a union
                + $"{attributeCompiler.GetType().FullName} is not supported.");
        }
    }
}