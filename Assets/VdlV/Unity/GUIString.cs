using System;
using UnityEngine;

namespace VdlV.Unity {
    // ReSharper disable once InconsistentNaming
    /// <summary>OnGUI based string viewer (static or dynamic).</summary>
    public class GUIString {
        private const int WidthOffset = 26;
        private const int HeightOffset = 6;

        private string _displayString;
        private readonly Func<string> _stringFunc;
        private readonly Func<int, int, Rect> _displayFunc;
        private bool _staticStringCalcOnce;
        private int _width;
        private int _height;
        private readonly bool _centered = true;

        public GUIString(Func<string> stringFunc, Func<int, int, Rect> displayFunc) {
            _displayFunc = displayFunc;
            _stringFunc = stringFunc;
        }
        public GUIString(Func<string> stringFunc, Func<int, int, Rect> displayFunc, bool centered) :
            this(stringFunc, displayFunc) => _centered = centered;
        public GUIString(string displayString, Func<int, int, Rect> displayFunc) {
            _displayFunc = displayFunc;
            _stringFunc = null;
            _displayString = displayString;
            _staticStringCalcOnce = true;
        }

        public void OnGUI() {
            UpdateString();
            if (_displayString is null) return;
            // MiddleLeft doesn't allow fo WidthOffset/2 padding on the left
            if (!_centered) GUI.skin.box.alignment = TextAnchor.MiddleLeft;
            GUI.Box(_displayFunc(_width, _height), _displayString);
            if (!_centered) GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        }

        public Rect GUIStringRect() => _displayFunc(_width, _height);

        private void UpdateString() {
            if (_stringFunc is not null) {
                if (IsNewString()) UpdateSize(); }
            if (!_staticStringCalcOnce) return;
            _staticStringCalcOnce = false;
            UpdateSize();
        }

        private bool IsNewString() {
            var newString = _stringFunc();
            if (newString == _displayString) return false;
            _displayString = newString;
            return true;
        }

        private void UpdateSize() {
            var displayStringSize = GUI.skin.box.CalcSize(new GUIContent(_displayString));
            _width = Mathf.CeilToInt(displayStringSize.x + WidthOffset);
            _height = Mathf.CeilToInt(displayStringSize.y + HeightOffset);
        }
    }
}
