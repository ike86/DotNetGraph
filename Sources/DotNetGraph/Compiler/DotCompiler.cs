using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                new NodeShapeAttributeCompiler(),
                new NodeStyleAttributeCompiler(),
                new EdgeStyleAttributeCompiler(),
                new FontColorAttributeCompiler(),
                new FillColorAttributeCompiler(),
                new ColorAttributeCompiler(),
                new LabelAttributeCompiler(),
                new NodeWidthAttributeCompiler(),
                new NodeHeightAttributeCompiler(),
                new PenWidthAttributeCompiler(),
                new EdgeArrowTailAttributeCompiler(),
                new EdgeArrowHeadAttributeCompiler(),
                new PositionAttributeCompiler(),
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
                    Console.WriteLine(
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
}
