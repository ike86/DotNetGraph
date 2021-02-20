using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotNetGraph.Attributes;
using DotNetGraph.Core;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;
using DotNetGraph.SubGraph;

namespace DotNetGraph.Compiler
{
    public class DotCompiler
    {
        private static readonly Regex ValidIdentifierPattern = new Regex("^([a-zA-Z\\200-\\377_][a-zA-Z\\200-\\3770-9_]*|[-]?(.[0-9]+|[0-9]+(.[0-9]+)?))$");

        private readonly DotGraph _graph;

        public DotCompiler(DotGraph graph)
        {
            _graph = graph;
            AttributeCompilers = new List<IAttributeCompilerUnion>
            {
                new DotNodeShapeAttributeCompiler(),
                new DotNodeStyleAttributeCompiler(),
                new DotEdgeStyleAttributeCompiler(),
                new DotFontColorAttributeCompiler(),
                new DotFillColorAttributeCompiler(),
                new DotColorAttributeCompiler(),
                new DotLabelAttributeCompiler(),
                new DotNodeWidthAttributeCompiler(),
                new DotNodeHeightAttributeCompiler(),
                new DotPenWidthAttributeCompiler(),
                new DotEdgeArrowTailAttributeCompiler(),
                new DotEdgeArrowHeadAttributeCompiler(),
                new DotPositionAttributeCompiler(),
            };
        }

        public ICollection<IAttributeCompilerUnion> AttributeCompilers { get; }

        public string Compile(bool indented = false, bool formatStrings = true)
        {
            var builder = new StringBuilder();

            CompileGraph(builder, indented, formatStrings);

            return builder.ToString();
        }

        private void CompileGraph(StringBuilder builder, bool indented, bool formatStrings)
        {
            var indentationLevel = 0;

            if (_graph.Strict)
                builder.Append("strict ");

            builder.Append(_graph.Directed ? "digraph " : "graph ");

            builder.Append($"{SurroundStringWithQuotes(_graph.Identifier, formatStrings)} {{ ");

            builder.AddIndentationNewLine(indented);

            indentationLevel++;

            foreach (var element in _graph.Elements)
            {
                if (element is DotEdge edge)
                {
                    CompileEdge(builder, edge, indented, indentationLevel, formatStrings);
                }
                else if (element is DotNode node)
                {
                    CompileNode(builder, node, indented, indentationLevel, formatStrings);
                }
                else if (element is DotSubGraph subGraph)
                {
                    CompileSubGraph(builder, subGraph, indented, indentationLevel, formatStrings);
                }
                else
                {
                    throw new DotException($"Graph body can't contain element of type: {element.GetType()}");
                }
            }

            indentationLevel--;

            builder.Append("}");
        }

        private void CompileSubGraph(StringBuilder builder, DotSubGraph subGraph, bool indented, int indentationLevel, bool formatStrings)
        {
            builder.AddIndentation(indented, indentationLevel);

            builder.Append($"subgraph {SurroundStringWithQuotes(subGraph.Identifier, formatStrings)} {{ ");

            builder.AddIndentationNewLine(indented);

            indentationLevel++;

            CompileSubGraphAttributes(builder, subGraph.Attributes, formatStrings);

            foreach (var element in subGraph.Elements)
            {
                if (element is DotEdge edge)
                {
                    CompileEdge(builder, edge, indented, indentationLevel, formatStrings);
                }
                else if (element is DotNode node)
                {
                    CompileNode(builder, node, indented, indentationLevel, formatStrings);
                }
                else if (element is DotSubGraph subSubGraph)
                {
                    CompileSubGraph(builder, subSubGraph, indented, indentationLevel, formatStrings);
                }
                else
                {
                    throw new DotException($"Subgraph body can't contain element of type: {element.GetType()}");
                }
            }

            indentationLevel--;

            builder.AddIndentation(indented, indentationLevel);

            builder.Append("} ");

            builder.AddIndentationNewLine(indented);
        }

        private void CompileSubGraphAttributes(StringBuilder builder, ReadOnlyCollection<IDotAttribute> attributes, bool formatStrings)
        {
            if (attributes.Count == 0)
                return;

            foreach (var attribute in attributes)
            {
                if (attribute is DotSubGraphStyleAttribute subGraphStyleAttribute)
                {
                    builder.Append($"style={SurroundStringWithQuotes(subGraphStyleAttribute.Style.FlagsToString(), formatStrings)}; ");
                }
                else if (attribute is DotColorAttribute colorAttribute)
                {
                    builder.Append($"color=\"{colorAttribute.ToHex()}\"; ");
                }
                else if (attribute is DotLabelAttribute labelAttribute)
                {
                    builder.Append($"label={SurroundStringWithQuotes(labelAttribute.Text, formatStrings)}; ");
                }
                else
                {
                    throw new DotException($"Attribute type not supported: {attribute.GetType()}");
                }
            }
        }

        private void CompileEdge(StringBuilder builder, DotEdge edge, bool indented, int indentationLevel, bool formatStrings)
        {
            builder.AddIndentation(indented, indentationLevel);

            CompileEdgeEndPoint(builder, edge.Left, formatStrings);

            builder.Append(_graph.Directed ? " -> " : " -- ");

            CompileEdgeEndPoint(builder, edge.Right, formatStrings);

            CompileAttributes(builder, edge.Attributes, formatStrings);

            builder.Append("; ");

            builder.AddIndentationNewLine(indented);
        }

        private void CompileEdgeEndPoint(StringBuilder builder, IDotElement endPoint, bool formatStrings)
        {
            if (endPoint is DotString leftEdgeString)
            {
                builder.Append(SurroundStringWithQuotes(leftEdgeString.Value, formatStrings));
            }
            else if (endPoint is DotNode leftEdgeNode)
            {
                builder.Append(SurroundStringWithQuotes(leftEdgeNode.Identifier, formatStrings));
            }
            else
            {
                throw new DotException($"Endpoint of an edge can't be of type: {endPoint.GetType()}");
            }
        }

        private void CompileNode(StringBuilder builder, DotNode node, bool indented, int indentationLevel, bool formatStrings)
        {
            builder.AddIndentation(indented, indentationLevel);

            builder.Append(SurroundStringWithQuotes(node.Identifier, formatStrings));

            CompileAttributes(builder, node.Attributes, formatStrings);

            builder.Append("; ");

            builder.AddIndentationNewLine(indented);
        }

        public interface IAttributeCompilerUnion
        {
        }

        public interface IAttributeCompiler : IAttributeCompilerUnion
        {
            string Compile(IDotAttribute attribute);
        }

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

        public interface IFormattedAttributeCompiler : IAttributeCompilerUnion
        {
            string Compile(IDotAttribute attribute, bool shouldFormat);
        }
        
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
        
        public class DotNodeShapeAttributeCompiler : AttributeCompilerBase<DotNodeShapeAttribute>
        {
            protected override string OnAttributeTypeMatch(DotNodeShapeAttribute attribute)
            {
                return $"shape={attribute.Shape.ToString().ToLowerInvariant()}";
            }
        }
        
        public class DotNodeStyleAttributeCompiler : FormattedAttributeCompilerBase<DotNodeStyleAttribute>
        {
            protected override string OnAttributeTypeMatch(DotNodeStyleAttribute attribute, bool shouldFormat)
            {
                return $"style={SurroundStringWithQuotes(attribute.Style.FlagsToString(), shouldFormat)}";
            }
        }
        
        public class DotEdgeStyleAttributeCompiler : FormattedAttributeCompilerBase<DotEdgeStyleAttribute>
        {
            protected override string OnAttributeTypeMatch(DotEdgeStyleAttribute attribute, bool shouldFormat)
            {
                return $"style={SurroundStringWithQuotes(attribute.Style.FlagsToString(), shouldFormat)}";
            }
        }
        
        public class DotFontColorAttributeCompiler : AttributeCompilerBase<DotFontColorAttribute>
        {
            protected override string OnAttributeTypeMatch(DotFontColorAttribute attribute)
            {
                return $"fontcolor=\"{attribute.ToHex()}\"";
            }
        }
        
        public class DotFillColorAttributeCompiler : AttributeCompilerBase<DotFillColorAttribute>
        {
            protected override string OnAttributeTypeMatch(DotFillColorAttribute attribute)
            {
                return $"fillcolor=\"{attribute.ToHex()}\"";
            }
        }
        
        public class DotColorAttributeCompiler : AttributeCompilerBase<DotColorAttribute>
        {
            protected override string OnAttributeTypeMatch(DotColorAttribute attribute)
            {
                return $"color=\"{attribute.ToHex()}\"";
            }
        }
        
        public class DotLabelAttributeCompiler : FormattedAttributeCompilerBase<DotLabelAttribute>
        {
            protected override string OnAttributeTypeMatch(DotLabelAttribute attribute, bool shouldFormat)
            {
                return $"label={SurroundStringWithQuotes(attribute.Text, shouldFormat)}";
            }
        }
        
        public class DotNodeWidthAttributeCompiler : AttributeCompilerBase<DotNodeWidthAttribute>
        {
            protected override string OnAttributeTypeMatch(DotNodeWidthAttribute attribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "width={0:F2}", attribute.Value);
            }
        }
        
        public class DotNodeHeightAttributeCompiler : AttributeCompilerBase<DotNodeHeightAttribute>
        {
            protected override string OnAttributeTypeMatch(DotNodeHeightAttribute attribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "height={0:F2}", attribute.Value);
            }
        }
        
        public class DotPenWidthAttributeCompiler : AttributeCompilerBase<DotPenWidthAttribute>
        {
            protected override string OnAttributeTypeMatch(DotPenWidthAttribute attribute)
            {
                return string.Format(CultureInfo.InvariantCulture, "penwidth={0:F2}", attribute.Value);
            }
        }
        
        public class DotEdgeArrowTailAttributeCompiler : AttributeCompilerBase<DotEdgeArrowTailAttribute>
        {
            protected override string OnAttributeTypeMatch(DotEdgeArrowTailAttribute attribute)
            {
                return $"arrowtail={attribute.ArrowType.ToString().ToLowerInvariant()}";
            }
        }
        
        public class DotEdgeArrowHeadAttributeCompiler : AttributeCompilerBase<DotEdgeArrowHeadAttribute>
        {
            protected override string OnAttributeTypeMatch(DotEdgeArrowHeadAttribute attribute)
            {
                return $"arrowhead={attribute.ArrowType.ToString().ToLowerInvariant()}";
            }
        }
        
        public class DotPositionAttributeCompiler : AttributeCompilerBase<DotPositionAttribute>
        {
            protected override string OnAttributeTypeMatch(DotPositionAttribute attribute)
            {
                return $"pos=\"{attribute.Position.X},{attribute.Position.Y}!\"";
            }
        }

        private void CompileAttributes(StringBuilder builder, ReadOnlyCollection<IDotAttribute> attributes, bool formatStrings)
        {
            if (attributes.Count == 0)
                return;

            builder.Append("[");

            var attributeValues = new List<string>();

            foreach (var attribute in attributes)
            {
                if (AttributeCompilers
                    .Select(c => c
                        .Convert(
                            attributeCompiler =>  attributeCompiler.Compile(attribute),
                            formattedAttributeCompiler => formattedAttributeCompiler.Compile(attribute, formatStrings)))
                        .FirstOrDefault(x => x != null)
                    is string value)
                {
                    attributeValues.Add(value);
                }
                else
                {
                    throw new DotException(
                        $"Attribute type not supported: {attribute.GetType()}."
                        + $"Add an implementation of {nameof(IAttributeCompiler)} to {nameof(AttributeCompilers)}, "
                        + $"which is supported by {nameof(AttributeCompilerUnionExtensions.Convert)}.");
                }
            }

            builder.Append(string.Join(",", attributeValues));

            builder.Append("]");
        }

        internal static string SurroundStringWithQuotes(string value, bool format)
        {
            var formatted = FormatString(value, format);
            return ValidIdentifierPattern.IsMatch(value) ? formatted : "\"" + formatted + "\"";
        }

        internal static string FormatString(string value, bool format)
        {
            if (!format)
                return value;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");
        }
    }
        
    public static class AttributeCompilerUnionExtensions
    {
        public static TResult Convert<TResult>(
            this DotCompiler.IAttributeCompilerUnion attributeCompiler,
            Func<DotCompiler.IAttributeCompiler, TResult> caseAttributeCompiler,
            Func<DotCompiler.IFormattedAttributeCompiler, TResult> caseFormattedAttributeCompiler)
        {
            if (attributeCompiler is DotCompiler.IAttributeCompiler x)
            {
                return caseAttributeCompiler(x);
            }

            if (attributeCompiler is DotCompiler.IFormattedAttributeCompiler y)
            {
                return caseFormattedAttributeCompiler(y);
            }

            throw new ArgumentOutOfRangeException(
                $"{nameof(DotCompiler.IAttributeCompilerUnion)} is a closed hierarchy. "
                + $"{attributeCompiler.GetType().FullName} is not supported.");
        }
    }
}
