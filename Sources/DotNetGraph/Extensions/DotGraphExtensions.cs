using DotNetGraph.Compiler;

namespace DotNetGraph.Extensions
{
    public static class DotGraphExtensions
    {
        public static string Compile(this DotGraph graph, bool indented = false, bool formatStrings = true)
        {
            return new DotCompiler(graph).Compile(indented, formatStrings);
        }
        
        public static string Compile(
            this DotGraph graph,
            CompilerSettings settings)
        {
            var compiler = new DotCompiler(graph);
            settings.ConfigureAttributeCompilers(compiler.AttributeCompilers);
            return compiler.Compile(settings.IsIndented, settings.ShouldFormatStrings);
        }
    }
}