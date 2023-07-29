#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphNode.cs" company="Ian Horswill">
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
using UnityEngine.UI;

namespace TotT.Unity.GraphVisualization {
    /// <summary>
    /// Component that drives individual nodes in a Graph visualization.
    /// These are created by Graph.AddNode().  Do not instantiate one yourself.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class GraphNode : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, 
                             IBeginDragHandler, IEndDragHandler, INodeDriver {
        /// <summary>
        /// The client-side object associated with this node.
        /// </summary>
        public object Key;
        // ReSharper disable once NotAccessedField.Global
        public string Label;
        private NodeStyle _style;
        private RectTransform _rectTransform;
        private Graph _graph;
        private TMPro.TextMeshProUGUI _labelMesh;
        private Image _image;

        /// <summary>
        /// The current position as computed by the spring physics system in Graph.cs
        /// </summary>
        internal Vector2 Position;
        /// <summary>
        /// The previous position as computed by the spring physics system in Graph.cs
        /// </summary>
        internal Vector2 PreviousPosition;
        /// <summary>
        /// Net force applied to this node by the various springs
        /// </summary>
        public Vector2 NetForce;

        public int Index;

        /// <summary>
        /// True if this node is in the process of being dragged by the mouse
        /// </summary>
        public bool IsBeingDragged;

        /// <summary>
        /// Called from Graph.AddNode after instantiation of the prefab for this node.
        /// </summary>
        public void Initialize(Graph g, object key, string label, NodeStyle style, Vector2 position, int index) {
            _graph = g;
            Key = key;
            Label = label;
            _style = style;
            Index = index;
            _labelMesh = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (_labelMesh != null) {
                _labelMesh.text = label;
                _labelMesh.color = style.Color;
                _labelMesh.fontSize = style.FontSize;
                if (style.Font != null) _labelMesh.font = style.Font;
                _labelMesh.fontStyle = style.FontStyle;
            }
            _image = GetComponentInChildren<Image>();
            SetImageColor(_style.Color);
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.localPosition = PreviousPosition = position;
        }

        private void SetImageColor(Color color) {
            if (_image == null) return;
            color.a = 0.3f;
            _image.color = color;
        }

        public void Update() {
            // Set gameObject's position to that computed by spring physics
            Vector3 p = Position;
            p.z = Foreground ? -1 : 1;
            _rectTransform.localPosition = p;
        }

        /// <summary>
        /// Update color of node text, based on whether it has been selected by the user.
        /// Called when node selected by mouse changes
        /// </summary>
        public void SelectionChanged(Graph g, GraphNode selected) {
            if (_labelMesh == null) return;
            var brightness = 1f;
            if (_graph.SelectedNode != null) brightness = Foreground ? 2 : _graph.GreyOutFactor;
            var newColor = brightness * _style.Color;
            _labelMesh.color = newColor;
            SetImageColor(newColor);
        }

        /// <summary>
        /// True if this node is in the foreground.
        /// Nodes are in the foreground unless some node they aren't adjacent to has been selected.
        /// </summary>
        private bool Foreground => _graph.SelectedNode == this ||
                                   _graph.SelectedNode == null ||
                                   _graph.Adjacent(this, _graph.SelectedNode);

        public void OnPointerEnter(PointerEventData eventData) => _graph.SelectedNode = this;

        public void OnPointerExit(PointerEventData eventData) {
            if (_graph.SelectedNode == this) _graph.SelectedNode = null;
        }

        public void OnDrag(PointerEventData data) {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform.parent as RectTransform, 
                    Input.mousePosition, data.pressEventCamera, 
                    out var p))
                PreviousPosition = Position = p;
        }

        public void OnBeginDrag(PointerEventData eventData) => IsBeingDragged = true;

        public void OnEndDrag(PointerEventData eventData) => IsBeingDragged = false;
    }
}
