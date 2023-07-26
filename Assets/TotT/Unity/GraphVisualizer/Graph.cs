#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Graph.cs" company="Ian Horswill">
// Copyright (C) 2019, 2020 Ian Horswill
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Linq;
using TotT.Simulator;
using TotT.Unity;
using TotT.ValueTypes;
using Unity.VisualScripting;
using Time = UnityEngine.Time;

namespace GraphVisualization
{
    /// <summary>
    /// An interactive graph visualization packaged as a Unity UI element
    /// </summary>
    public class Graph : Graphic
    {
        #region Editor-visible fields
        public Rect Bounds = new(0, 0, 1920, 1080);

        /// <summary>
        /// Styles available for drawing nodes in this graph
        /// </summary>
        [Tooltip("Styles in which to render nodes.")]
        public List<NodeStyle> NodeStyles = new List<NodeStyle>();

        /// <summary>
        /// Styles available for drawing edges in this graph
        /// </summary>
        [Tooltip("Styles in which to render edges.")]
        public List<EdgeStyle> EdgeStyles = new List<EdgeStyle>();

        /// <summary>
        /// The NodeStyle with the specified name, or null
        /// </summary>
        public NodeStyle NodeStyleNamed(string styleName)
        {
            return NodeStyles.Find(s => s.Name == styleName);
        }

        /// <summary>
        /// The EdgeStyle with the specified name, or null
        /// </summary>
        public EdgeStyle EdgeStyleNamed(string styleName)
        {
            return EdgeStyles.Find(s => s.Name == styleName);
        }

        /// <summary>
        /// Prefab to use for making nodes
        /// </summary>
        [Tooltip("Prefab to instantiate to make a new node for this graph.")]
        public GameObject NodePrefab;
        /// <summary>
        /// Prefab to use for making edges
        /// </summary>
        [Tooltip("Prefab to instantiate to make a new edge for this graph.")]
        public GameObject EdgePrefab;
        
        /// <summary>
        /// The strength of the force that moves adjacent nodes together
        /// </summary>
        [Tooltip("The strength of the force that moves adjacent nodes together")]
        public float SpringStiffness = 1f;
        /// <summary>
        /// The strength of the force that moves non-adjacent nodes apart.
        /// </summary>
        [Tooltip("The strength of the force that moves non-adjacent nodes apart.  Set this to 0 to eliminate repulsion calculations.")]
        public float RepulsionGain = 100000;

        public float SiblingRepulsionBoost = 5;

        /// <summary>
        /// Rate (0-1) at which nodes slow down when no forces are applied to them.
        /// </summary>
        [Tooltip("Rate (0-1) at which nodes slow down when no forces are applied to them.")]
        public float NodeDamping = 0.5f;

        /// <summary>
        /// Degree to which nodes and edges are dimmed when some other node is selected.
        /// 0 = completely dimmed, 1 = not dimmed.
        /// </summary>
        [Tooltip("Degree to which nodes and edges are dimmed when some other node is selected.  0 = completely dimmed, 1 = not dimmed.")]
        public float GreyOutFactor = 0.5f;
        /// <summary>
        /// How far to keep nodes from the edge of the Rect for this UI element.
        /// </summary>
        public float Border = 100;
        /// <summary>
        /// Text object in which to display additional information about a node, or null if no info to be displayed.
        /// </summary>
        [Tooltip("Text object in which to display additional information about a node, or None if no info to be displayed.")]
        public TMPro.TextMeshProUGUI ToolTip;
        /// <summary>
        /// Name of the string property of a selected node to be displayed in the ToolTop element.
        /// </summary>
        [Tooltip("Name of the string property of a selected node to be displayed in the ToolTop element.")]
        public string ToolTipProperty;
        #endregion

        #region Node and edge data structures
        /// <summary>
        /// All GraphNode objects in this Graph, one per node/key
        /// </summary>
        internal readonly List<GraphNode> nodes = new List<GraphNode>();
        private readonly List<INodeDriver> nodeDrivers = new List<INodeDriver>();
        /// <summary>
        /// All GraphEdge objects in this Graph, one per graph edge
        /// </summary>
        private readonly List<IEdgeDriver> edgeDrivers = new List<IEdgeDriver>();
        private readonly List<GraphEdge> edges = new List<GraphEdge>();
        private readonly Dictionary<GraphNode, List<GraphNode>> adjacencyLists = new Dictionary<GraphNode, List<GraphNode>>();

        /// <summary>
        /// Mapping from client-side vertex objects ("keys") to internal GraphNode objects
        /// </summary>
        private readonly Dictionary<object, GraphNode> nodeDict = new Dictionary<object, GraphNode>();
        /// <summary>
        /// Set of pairs of nodes that are adjacent.  This relation is symmetric even when the edge is directed.
        /// Used to determine if nodes should repel one another, and if nodes should be dimmed when another node is selected.
        /// </summary>
        private readonly HashSet<(GraphNode, GraphNode)> adjacency = new HashSet<(GraphNode, GraphNode)>();

        /// <summary>
        /// The set of pairs of nodes that are siblings, i.e. that share a connection to the same node
        /// </summary>
        private HashSet<(GraphNode, GraphNode)> siblings;

        /// <summary>
        /// True if there is an edge from a to be *or* vice-versa.
        /// </summary>
        public bool Adjacent(GraphNode a, GraphNode b)
        {
            return adjacency.Contains((a, b));
        }

        private short[,] TopologicalDistance;

        private short[] ConnectedComponent;
        private List<short> ConnectedComponentSize = new List<short>();

        private int ConnectedComponentCount => ConnectedComponentSize.Count;
        #endregion

        #region Graph creation
        /// <summary>
        /// Remove all existing nodes and edges from graph
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
            edgeDrivers.Clear();
            nodeDict.Clear();
            adjacency.Clear();

            foreach (Transform child in transform)
            {
                if (child.GetComponent<GraphNode>() != null || child.GetComponent<GraphEdge>() != null)
                    Destroy(child.gameObject);
            }

            RepopulateMesh();
            GUIManager.ShowTiles(true);
        }

        public void GenerateFrom<TNode>(IEnumerable<TNode> keys, NodeFormatter<TNode> format, EdgeGenerator<TNode> edgeGenerator)
        {
            void MakeNode(TNode k)
            {
                var (label, style) = format(k);
                AddNode(k, label, style);
            }

            void WalkGeneration(TNode k)
            { 
                // Add node
                MakeNode(k);

                // Add edges and recurse
                foreach (var (f, t, l, s) in edgeGenerator(k))
                {
                    var fWalked = nodeDict.ContainsKey(f);
                    if (!fWalked)
                        MakeNode(f);
                    var tWalked = nodeDict.ContainsKey(t);
                    if (!tWalked)
                        MakeNode(t);
                    AddEdge(f, t, l, s);
                    if (!fWalked)
                        WalkGeneration(f);
                    if (!tWalked)
                        WalkGeneration(t);
                }
            }

            foreach (var k in keys)
                if (!nodeDict.ContainsKey(k))
                    WalkGeneration(k);
        }

        /// <summary>
        /// A procedure to be used by GenerateFrom() to enumerate edges of a given node
        /// </summary>
        /// <param name="node">Node to generate edges for.</param>
        /// <returns>Enumerated stream of edge information: from-node, to-node, label, and style.</returns>
        public delegate IEnumerable<(TNode start, TNode end, string label, EdgeStyle style)> EdgeGenerator<TNode>(TNode node);
        /// <summary>
        /// A procedure to be used by GenerateFrom() to generate labels and styles for nodes
        /// </summary>
        /// <param name="o">Node</param>
        /// <returns>Label and style for the node</returns>
        public delegate (string label, NodeStyle style) NodeFormatter<in TNode>(TNode o);

        public GraphBuilder<T> GetGraphBuilder<T>()
        {
            return new GraphBuilder<T>(this);
        }

        /// <summary>
        /// Add a single node to the graph.
        /// </summary>
        /// <param name="node">Node to add</param>
        /// <param name="label">Label to attach to node</param>
        /// <param name="style">Style in which to render node, and apply physics to it.  If null, the first entry in NodeStyles will be used.</param>
        public void AddNode(object node, string label, NodeStyle style = null)
        {
            if (!nodeDict.ContainsKey(node))
            {
                if (label == null)
                    label = node.ToString();
                if (style == null)
                    style = NodeStyles[0];
                var go = Instantiate(style.Prefab != null?style.Prefab:NodePrefab, transform);
                go.name = label;
                var rect = rectTransform.rect;
                var position = new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
                var internalNode = go.GetComponent<GraphNode>();
                var index = nodes.Count;
                nodes.Add(internalNode);
                foreach (var driver in go.GetComponents<INodeDriver>())
                {
                    driver.Initialize(this, node, label, style, position, index);
                    nodeDrivers.Add(driver);
                }
                nodeDict[node] = internalNode;
            }
        }

        /// <summary>
        /// Add a single edge to the graph.
        /// </summary>
        /// <param name="start">Node from which edge starts.</param>
        /// <param name="end">Node the edge leads to.</param>
        /// <param name="label">Label for the edge</param>
        /// <param name="style">Style in which to render the label.  If null, this will use the style whose name is the same as the label, if any, otherwise the first entry in EdgeStyles.</param>
        public void AddEdge(object start, object end, string label, EdgeStyle style = null)
        {
            AddNode(start, null);  // In case it isn't already defined.
            var startNode = nodeDict[start];
            AddNode(end, null);    // In case it isn't already defined.
            var endNode = nodeDict[end];

            if (label == null)
                label = "";
            if (style == null)
                style = EdgeStyleNamed(label)??EdgeStyles[0];
            var go = Instantiate(style.Prefab != null?style.Prefab:EdgePrefab, transform);
            go.name = label;
            var graphEdge = go.GetComponent<GraphEdge>();
            graphEdge.Offset = 10*edges.Count(e => e.StartNode.Equals(start) && e.EndNode.Equals(end));
            edges.Add(graphEdge);
            foreach (var driver in go.GetComponents<IEdgeDriver>())
            {
                driver.Initialize(this, startNode, endNode, label, style);
                edgeDrivers.Add(driver);
            }

            AddNeighbor(startNode, endNode);
            AddNeighbor(endNode, startNode);

            // force rebuild of siblings table
            siblings = null;
        }

        private void AddNeighbor(GraphNode startNode, GraphNode endNode)
        {
            adjacency.Add((startNode, endNode));
            if (!adjacencyLists.TryGetValue(startNode, out var adjacencyList))
                adjacencyLists[startNode] = adjacencyList = new List<GraphNode>();
            adjacencyList.Add(endNode);
        }

        /// <summary>
        /// Use Floyd-Warshall to compute the topological distances between all pairs of nodes in the graph
        /// </summary>
        protected void UpdateTopologyStats()
        {
            UpdateTopologicalDistances();

            //for (var i = 0; i < n; i++)
            //for (var j = 0; j < n; j++)
            //    if (TopologicalDistance[i,j] == short.MaxValue)
            //        TopologicalDistance[i,j] = 1;

            targetEdgeLength = Mathf.Clamp(3*Mathf.Sqrt(0.3f*(Bounds.width * Bounds.height) / nodes.Count), 50, 300);

            var narrowAxis = Mathf.Min(Bounds.width, Bounds.height);

            if (targetEdgeLength * Diameter > narrowAxis)
                targetEdgeLength = narrowAxis / Diameter;

            UpdateConnectedComponents();
        }

        protected void UpdateTopologicalDistances()
        {
            var n = nodes.Count;
            TopologicalDistance = new short[n, n];
            for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                TopologicalDistance[i, j] = (i == j) ? (short)0 : short.MaxValue;

            foreach (var e in edges)
            {
                var i = e.StartNode.Index;
                var j = e.EndNode.Index;
                TopologicalDistance[i, j] = TopologicalDistance[j, i] = 1;
            }

            for (var k = 0; k < n; k++)
            for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
                TopologicalDistance[i, j] = TopologicalDistance[j, i] = (short)Math.Min(TopologicalDistance[i, j],
                    TopologicalDistance[i, k] + TopologicalDistance[k, j]);

            Diameter = 0;
            for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
            {
                var d = TopologicalDistance[i, j];
                if (d < short.MaxValue)
                    Diameter = Math.Max(Diameter, d);
            }
        }

        /// <summary>
        /// Find all the connected components and their sizes, and note the component number of each node.
        /// </summary>
        protected void UpdateConnectedComponents()
        {
            ConnectedComponentSize.Clear();
            ConnectedComponent = new short[nodes.Count];
            Array.Fill(ConnectedComponent, (short)-1);

            void Walk(GraphNode node)
            {
                var index = node.Index;
                if (ConnectedComponent[index] >= 0)
                    return;
                var componentNumber = ConnectedComponentCount - 1;
                ConnectedComponent[index] = (short)componentNumber;
                ConnectedComponentSize[componentNumber]++;
                foreach (var n in nodes)
                    if (n != node && Adjacent(n, node))
                        Walk(n);
            }

            foreach (var n in nodes)
            {
                if (ConnectedComponent[n.Index] < 0)
                {
                    ConnectedComponentSize.Add(0);
                    Walk(n);
                }
            }
        }
        #endregion

        protected void PlaceComponents()
        {
            void PlaceSingleComponent(int component, Rect rect)
            {
                foreach (var n in nodes)
                    if (ConnectedComponent[n.Index] == component)
                        n.Position = n.PreviousPosition = new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
            }

            void Place(int startComponent, int endComponent, Rect region)
            {
                System.Diagnostics.Debug.Assert(endComponent >= startComponent);
                if (startComponent == endComponent)
                    PlaceSingleComponent(startComponent, region);
                else
                {
                    Rect r1;
                    Rect r2;
                    var p = region.position;
                    if (region.width > region.height)
                    {
                        var halfWidth = region.width / 2;
                        var size = new Vector2(halfWidth, region.height);
                        r1 = new Rect(p, size);
                        p.x += halfWidth;
                        r2 = new Rect(p, size);
                    }
                    else
                    {
                        var halfHeight = region.height / 2;
                        var size = new Vector2(region.width, halfHeight);
                        r1 = new Rect(p, size);
                        p.y += halfHeight;
                        r2 = new Rect(p, size);
                    }

                    var midpoint = (startComponent + endComponent) / 2;
                    Place(startComponent, midpoint, r1);
                    Place(midpoint+1, endComponent, r2);
                }
            }

            Place(0, ConnectedComponentCount-1, Bounds);
        }

        #region Unity message handlers
        /// <summary>
        /// Update physics simulation of nodes
        /// </summary>
        public void FixedUpdate()
        {
            if (nodes.Count == 0) return;
            MakeSiblingsIfNecessary();
            UpdatePhysics();
        }

        /// <summary>
        /// Creates/recreates the siblings table, which is a hashset of pairs of siblings.
        /// </summary>
        private void MakeSiblingsIfNecessary()
        {
            if (siblings != null)
                return;

            siblings = new HashSet<(GraphNode, GraphNode)>();
            foreach (var pair in adjacencyLists)
            {
                var neighbors = pair.Value;
                for (var i = 0 ; i < neighbors.Count; i++)
                for (var j = i + 1; j < neighbors.Count; j++)
                {
                    var n1 = neighbors[i];
                    var n2 = neighbors[j];
                    siblings.Add((n1, n2));
                    siblings.Add((n2, n1));
                }
            }
        }

        /// <summary>
        /// Update display of nodes and edges
        /// </summary>
        public void Update()
        {
            if (nodes.Count == 0) return;
            if (selectionChanged)
            {
                SelectionChanged();
                selectionChanged = false;
            }
            RepopulateMesh();
        }

        /// <summary>
        /// Call IGraphGenerator in this game object, if any.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            var generator = GetComponent<IGraphGenerator>();
            if (Application.isPlaying && generator != null)
            {
                Clear();
                generator.GenerateGraph(this);
            }
        }
        #endregion

        #region Highlighting and tooltip handling
        /// <summary>
        /// Do not use this directly.
        /// Internal field backing the SelectedNode property
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private GraphNode _selected;
        /// <summary>
        /// True if SelectedNode has changed since the last frame Update.
        /// </summary>
        private bool selectionChanged;
        /// <summary>
        /// Node over which the mouse is currently hovering, if any.  Else null.
        /// </summary>
        public GraphNode SelectedNode
        {
            get => _selected;
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    selectionChanged = true;
                    UpdateToolTip(value);
                }
            }
        }

        private static Dictionary<Type, Delegate> DescriptionMethod = new();

        private Delegate GetDescriptionMethod(object o)
        {
            for (var t = o.GetType();  t != null; t = t.BaseType)
                if (DescriptionMethod.TryGetValue(t, out var d))
                    return d;
            return null;
        }

        public static void SetDescriptionMethod<T>(Func<T, string> method) => DescriptionMethod[typeof(T)] = method;

        /// <summary>
        /// Update the ToolTop UI element, if any, based on the selected node, if any.
        /// </summary>
        /// <param name="node"></param>
        private void UpdateToolTip(GraphNode node)
        {
            if (ToolTip == null)
                return;
            if (node == null)
                ToolTip.text = "";
            else
            {
                var key = node.Key;
                var t = key.GetType();
                string text;
                switch (key)
                {
                    case IDescribable d:
                        text = d.Description;
                        break;

                    default:
                        var m = GetDescriptionMethod(key);
                        if (m != null)
                            text = (string)m.DynamicInvoke(key);
                        else
                            text = key.ToString();
                        break;

                }

                text ??= "";
                ToolTip.text = text.Trim();
                ToolTip.transform.SetSiblingIndex(nodes.Count + edges.Count);
            }
        }

        /// <summary>
        /// Dim/un-dim nodes based on selected node.
        /// </summary>
        private void SelectionChanged()
        {
            foreach (var n in nodeDrivers)
                n.SelectionChanged(this, SelectedNode);
            foreach (var e in edgeDrivers)
                e.SelectionChanged(this, SelectedNode);
        }
        #endregion
        
        #region Physics update
        /// <summary>
        /// The "ideal" length for edges.
        /// This is the length we'd have if all the nodes were arrayed in a regular grid.
        /// </summary>
        public float targetEdgeLength;

        /// <summary>
        /// Compute forces on nodes and update their positions.
        /// This just updates the internal Position field of the GraphNodes.  The actual
        /// on-screen position is updated once per frame in the Update method.
        /// </summary>
        void UpdatePhysics()
        {
            if (nodes.Count == 0 || TopologicalDistance == null) return;
            foreach (var n in nodes)
                n.NetForce = Vector2.zero;

            for (var i = 0; i < nodes.Count; i++)
                for (var j = i+1; j < nodes.Count; j++)
                    ApplySpringForce(i, j);

            // Keep nodes on screen
            foreach (var n in nodes)
            {
                UpdatePosition(n);
                n.Position = new Vector2(
                    Mathf.Clamp(n.Position.x, Bounds.xMin+Border, Bounds.xMax-Border),
                    Mathf.Clamp(n.Position.y, Bounds.yMin+Border, Bounds.yMax-Border));
            }
        }

        /// <summary>
        /// Update position of a single node based on forces already computed.
        /// </summary>
        /// <param name="n"></param>
        private void UpdatePosition(GraphNode n)
        {
            if (n.IsBeingDragged)
                return;
            var saved = n.Position;
            n.Position = (2-NodeDamping) * n.Position - (1-NodeDamping) * n.PreviousPosition + (Time.fixedDeltaTime * Time.fixedDeltaTime) * n.NetForce;
            n.PreviousPosition = saved;
        }
        
        /// <summary>
        /// Apply a repulsive force between two non-adjacent nodes.
        /// </summary>
        private void PushApart(GraphNode a, GraphNode b)
        {
            var offset = (a.Position - b.Position);
            var areSiblings = siblings.Contains((a, b));
            var siblingGain = areSiblings ? SiblingRepulsionBoost : 1;
            var force = Mathf.Max(0, siblingGain * Mathf.Log(RepulsionGain / Mathf.Max(1,offset.sqrMagnitude))) * offset;

            if (!areSiblings && SelectedNode != null && SelectedNode.IsBeingDragged && (a == SelectedNode || b == SelectedNode))
                force *= 100;

            a.NetForce += force;
            b.NetForce -= force;
        }

        /// <summary>
        /// Apply a spring force to two adjacent nodes to move them closer to targetEdgeLength.
        /// </summary>
        /// <param name="e">Edge connecting nodes</param>
        private void ApplySpringForce(int i, int j)
        {
            var springLength = TopologicalDistance[i,j];
            var start = nodes[i];
            var end = nodes[j];
            var offset = start.Position - end.Position;
            var len = offset.magnitude;
            Vector2 force = Vector2.zero;
            if (len > 0.1f)
            {
                if (springLength == short.MaxValue)
                {
                    if (len < 2*targetEdgeLength)
                        force = offset * (ComponentRepulsionGain/ (len * len * len));
                }
                else
                {
                    var lengthError = (targetEdgeLength * springLength) - len;
                    force = (SpringStiffness * lengthError / len) * offset;
                }
            }

            start.NetForce += force;
            end.NetForce -= force;
        }
        #endregion

        #region Edge rendering
        /// <summary>
        /// List of triangles to render.  Used as scratch by OnPopulateMesh.
        /// </summary>
        private static readonly List<UIVertex> TriBuffer = new List<UIVertex>();

        public float ComponentRepulsionGain = 100000000f;
        public int Diameter;

        /// <summary>
        /// Recompute triangles for lines and arrowheads representing edges
        /// </summary>
        /// <param name="vh">VertexHelper object passed in by Unity</param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // Add a solid-colored tri to TriBuffer.
            void AddTri(Vector2 v1, Vector2 v2, Vector2 v3, float z, Color c)
            {
                var uiv = UIVertex.simpleVert;
                uiv.color = c;
                uiv.position = new Vector3(v1.x, v1.y, z);
                TriBuffer.Add(uiv);
                uiv.position = new Vector3(v2.x, v2.y, z);
                TriBuffer.Add(uiv);
                uiv.position = new Vector3(v3.x, v3.y, z);
                TriBuffer.Add(uiv);
            }

            // A solid-colored quad to TriBuffer
            void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float z, Color c)
            {
                AddTri(v1, v2, v3, z, c);
                AddTri(v1, v3, v4, z, c);
            }

            // Add the representation of an edge (line or arrow) to TriBuffer
            void DrawEdge(Vector2 start, Vector2 end, EdgeStyle style, float z, Color c, float perpendicularOffset)
            {
                var offset = end - start;
                var shift = Perpendicular(perpendicularOffset * offset.normalized);
                start += shift;
                end += shift;
                var length = offset.magnitude;
                if (length > 1)  // arrows less than one pixel long will disappear
                {
                    // Draw line connecting start and end
                    var unit = offset / length;
                    var perp = new Vector2(unit.y, -unit.x);
                    var halfWidthPerp = (style.LineWidth * 0.5f) * perp;
                    var arrowheadBase = end - (style.ArrowheadLength * style.LineWidth) * unit;

                    AddQuad(start + halfWidthPerp,
                        arrowheadBase + halfWidthPerp,
                        arrowheadBase-halfWidthPerp,
                        start-halfWidthPerp,
                        z,
                        c);

                    // Draw arrowhead if directed edge
                    if (style.IsDirected)
                    {
                        var arrowheadHalfWidthPerp = style.ArrowheadWidth * halfWidthPerp;
                        AddTri(end,
                            arrowheadBase - arrowheadHalfWidthPerp,
                            arrowheadBase + arrowheadHalfWidthPerp,
                            z,
                            c);
                    }
                }
            }

            // Throw away previous geometry
            vh.Clear();
            TriBuffer.Clear();
            // Add edges to TriBuffer
            foreach (var e in edges)
            {
                var foreground = SelectedNode == null || SelectedNode == e.StartNode || SelectedNode == e.EndNode;
                var brightnessFactor = foreground?1:GreyOutFactor;
                DrawEdge(e.StartNode.Position, e.EndNode.Position,
                    e.Style,
                    foreground ? 0 : 1,
                    e.Style.Color * brightnessFactor,
                    5+e.Offset);
            }
            // Add TriBuffer to vh.
            vh.AddUIVertexTriangleStream(TriBuffer);
        }

        private static Vector2 Perpendicular(Vector2 v) => new(-v.y, v.x);

        /// <summary>
        /// Tell Unity we need to recompute the mesh.
        /// </summary>
        protected void RepopulateMesh()
        {
            SetVerticesDirty();
        }
        #endregion

        void OnGUI() {
            if (nodes.Count == 0) return;
            if (GUI.Button(GUIManager.RemoveGraphButton(), "Remove graph")) 
                Clear();
        }
    }
}
