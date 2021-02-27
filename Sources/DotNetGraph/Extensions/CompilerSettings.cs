using System;
using System.Collections.Generic;
using DotNetGraph.Compiler;

namespace DotNetGraph.Extensions
{
    public class CompilerSettings
    {
        public bool IsIndented { get; set; }

        public bool ShouldFormatStrings { get; set; }

        public Action<ICollection<IAttributeCompilerUnion>>  ConfigureAttributeCompilers { get; set; }
    }
}