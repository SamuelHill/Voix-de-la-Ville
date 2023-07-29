#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphEdge.cs" company="Ian Horswill">
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

using UnityEngine;
using UnityEngine.EventSystems;

namespace TotT.Unity.GraphVisualization {
    /// <summary>
    /// Component that drives individual edges in a Graph.
    /// These are created by Graph.AddEdge().  Do not instantiate one yourself.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GraphEdge : UIBehaviour, IEdgeDriver {
        public GraphNode StartNode;
        public GraphNode EndNode;
        // ReSharper disable once NotAccessedField.Global
        public string Label;
        public float Offset;
        public EdgeStyle Style;
        private TMPro.TextMeshProUGUI _text;
        private RectTransform _rectTransform;

        /// <summary>
        /// Called by Graph.AddEdge after object creation.
        /// </summary>
        public void Initialize(Graph g, GraphNode startNode, GraphNode endNode, string label, EdgeStyle style) {
            StartNode = startNode;
            EndNode = endNode;
            Label = label;
            Style = style;
            _text = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (_text != null) {
                _text.text = label;
                if (style.Font != null) _text.font = style.Font;
                if (style.FontSize != 0) _text.fontSize = style.FontSize;
                _text.fontStyle = style.FontStyle;
                _text.color = Style.Color;
            }
            _rectTransform = GetComponent<RectTransform>();
            UpdatePosition();
        }

        /// <summary>
        /// Updates position and rotation of edge text based on positions of endpoints
        /// </summary>
        private void UpdatePosition() {
            // Move to midpoint of start and end
            var startPos = StartNode.Position;
            var endPos = EndNode.Position;
            _rectTransform.localPosition = (startPos + endPos) * 0.5f;

            // Rotate parallel to line from start to end
            var offset = endPos - startPos;
            var angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            _rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        }

        public void Update() => UpdatePosition();

        /// <summary>
        /// Called to adjust color of edge label when selected node in the graph changes.
        /// When there is a selected node, edges not adjacent to it are grayed out.
        /// </summary>
        public void SelectionChanged(Graph g, GraphNode selectedNode) {
            if (_text == null) return;
            var brightness = 1f;
            if (selectedNode != null) {
                var highlight = StartNode == selectedNode || EndNode == selectedNode;
                brightness = highlight ? 2 : g.GreyOutFactor;
            }
            _text.color = Style.Color * brightness;
        }
    }
}
