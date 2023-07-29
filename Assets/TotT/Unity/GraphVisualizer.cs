using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TED;
using TED.Interpreter;
using TED.Utilities;
using TotT.Unity.GraphVisualization;
using TotT.Utilities;
using UnityEngine;
using static TotT.Unity.GraphVisualization.GraphColors;

namespace TotT.Unity {
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GraphVisualizer : Graph {
        private static readonly Dictionary<string, Color> NonStandardColorNames;

        public static GraphVisualizer Current;

        public static bool GraphVisible => Current.nodes.Count == 0;

        private readonly DictionaryWithDefault<string, EdgeStyle> _edgeStyles;
        private readonly DictionaryWithDefault<string, NodeStyle> _nodeStyles;

        static GraphVisualizer() => NonStandardColorNames = new Dictionary<string, Color>(
                                              NamedColors.Select(c => new KeyValuePair<string, Color>(c.Name, c.Color)));
        public GraphVisualizer() {
            _nodeStyles = new DictionaryWithDefault<string, NodeStyle>(MakeNodeStyle);
            _edgeStyles = new DictionaryWithDefault<string, EdgeStyle>(MakeEdgeStyle);
            Current = this;
        }

        private NodeStyle MakeNodeStyle(string colorName) {
            var s = NodeStyles[0].Clone();
            var f = typeof(Color).GetField(
                colorName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (f == null) {
                if (NonStandardColorNames.TryGetValue(colorName, out var namedColor)) s.Color = namedColor;
                else Debug.Log($"No such color as {colorName}");
            } else s.Color = (Color)f.GetValue(null);
            return s;
        }

        private EdgeStyle MakeEdgeStyle(string colorName) {
            var s = EdgeStyles[0].Clone();
            var f = typeof(Color).GetField(colorName, BindingFlags.Static | BindingFlags.IgnoreCase);
            if (f == null) {
                if (NonStandardColorNames.TryGetValue(colorName, out var namedColor)) s.Color = namedColor;
                else Debug.Log($"No such color as {colorName}");
            } else s.Color = (Color)f.GetValue(null);
            return s;
        }

        private void SetGraph<T>(GraphViz<T> g) {
            GUIManager.ChangeTable = false; // hide buttons
            Clear();
            EnsureEdgeNodesInGraph(g);
            if (g.Nodes.Count == 0) return;
            GUIManager.ShowTiles(false);

            foreach (var n in g.Nodes) {
                var attrs = g.NodeAttributes[n];
                var nodeStyle = NodeStyles[0];
                if (attrs != null && attrs.TryGetValue("fillcolor", out var attr)) nodeStyle = _nodeStyles[(string)attr];
                if (attrs != null && attrs.TryGetValue("rgbcolor", out var rgbcolor)) nodeStyle = (Color)rgbcolor;
                AddNode(n, n.ToString(), nodeStyle);
            }
            foreach (var e in g.Edges) {
                var attrs = e.Attributes;
                var edgeStyle = EdgeStyles[0];
                if (attrs != null && attrs.TryGetValue("color", out var attr)) edgeStyle = _edgeStyles[(string)attr];
                if (attrs != null && attrs.TryGetValue("rgbcolor", out var rgbcolor)) edgeStyle = (Color)rgbcolor;
                AddEdge(e.StartNode, e.EndNode, e.Label, edgeStyle);
            }
            UpdateTopologyStats();
            PlaceComponents();
            RepopulateMesh();
        }

        private static void EnsureEdgeNodesInGraph<T>(GraphViz<T> g) {
            foreach (var e in g.Edges) {
                if (!g.Nodes.Contains(e.StartNode)) g.AddNode(e.StartNode);
                if (!g.Nodes.Contains(e.EndNode)) g.AddNode(e.EndNode);
            }
        }

        public static void ShowGraph<T>(GraphViz<T> g) => FindObjectOfType<GraphVisualizer>().SetGraph(g);

        public static void SetTableDescription() => SetDescriptionMethod<TablePredicate>(TableDescription);
        private static string TableDescription(TablePredicate p) {
            var b = new StringBuilder();
            b.Append("<b>");
            b.AppendLine(p.DefaultGoal.ToString().Replace("[", "</b>["));
            b.AppendFormat("{0} rows\n", p.Length);
            b.Append("<size=16>");
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (p.UpdateMode) {
                case UpdateMode.BaseTable:
                    b.Append("Base table");
                    break;
                case UpdateMode.Operator:
                    b.Append("Operator result");
                    break;
                default:
                    if (p.Rules != null)
                        foreach (var r in p.Rules) b.AppendLine(r.ToString());
                    break;
            }
            return b.ToString();
        }

        public static GraphViz<TGraph> TraceToDepth<TGraph, T>(int maxDepth, T start, Func<T, IEnumerable<(
                                                                   T neighbor, string label, string color)>> edges) where T : TGraph {
            var g = new GraphViz<TGraph>();

            void Walk(T node, int depth) {
                if (depth > maxDepth || g.Nodes.Contains(node)) return;
                g.AddNode(node);
                foreach (var edge in edges(node)) {
                    Walk(edge.neighbor, depth + 1);
                    g.AddEdge(new GraphViz<TGraph>.Edge(node, edge.neighbor, true, edge.label,
                                                        new Dictionary<string, object> { { "color", edge.color } }));
                }
            }

            Walk(start, 0);
            return g;
        }
    }
}
