using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using TotT.Simulator;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;

namespace TotT.Unity {
    using static Array;
    using static Color;
    using static Event;
    using static GUI;
    using static GUILayout;
    using static GUIManager;
    using static Input; // Event.current.mousePosition doesn't work as expected for Rect check
    using static Math;
    using static Screen;
    using static StringProcessing;

    // ReSharper disable once InconsistentNaming
    /// <summary>OnGUI based TablePredicate viewer.</summary>
    public class GUITable {
        private const int ColumnPadding = 5;
        private const int LabelHeight = 16; // same as fontSize + padding
        private const int TablePadding = 8; // using height padding for left side offset as well
        private const int DefaultTableHeight = 260; // (260 * 4) + (8 * 5) = 1080...
        private const int ScrollbarOffset = 20;
        private const int DefaultNumRowsToDisplay = (DefaultTableHeight - TablePadding) / LabelHeight - 5;
        // - 5 from display rows; two for the bold title and header row, and three to not get cutoff/crowd the space
        // this could probably be calculated dynamically based on label margins but for now this offset math is good enough.
        // to fix, update DefaultNumRowsToDisplay, ScrollHeight, NumRowsToHeight... everything derived from LabelHeight
        
        private readonly TablePredicate _predicate;
        private readonly string[][] _buffer;
        private readonly string[] _headings;
        private readonly Color[] _rowColors;
        private readonly float[] _columnWidths;
        private int[] _longestBufferStrings;
        private int[] _headingLengths;
        private GUIContent _noEntries;
        private int _noEntriesWidth;
        private uint _bufferedRows;
        private uint _previousRowCount;
        private bool _usingScroll;
        private bool _newlySorted;
        private float _scrollPosition;
        private float _oldScroll;
        private Month _lastMonth;
        private Comparison<uint> _sortComparer;
        private uint[] _sortBuffer;
        private readonly Func<uint, Color> _colorizer;

        public GUITable(TablePredicate predicate, int numRows = DefaultNumRowsToDisplay) {
            _predicate = predicate;
            _headings = (from heading in predicate.ColumnHeadings select Heading(heading)).ToArray();
            _headingLengths = new int[_headings.Length];
            _buffer = new string[numRows][];
            for (var i = 0; i < numRows; i++)
                _buffer[i] = new string[NumColumns];
            _rowColors = new Color[numRows];
            _longestBufferStrings = new int[NumColumns];
            _columnWidths = new float[NumColumns];
            _colorizer = predicate.Property.TryGetValue("Colorizer", out var func) ? 
                             (Func<uint,Color>)func : _ => white;
        }

        // *********************************** Table properties ***********************************
        private int NumColumns => _headings.Length;
        private uint RowCount => _predicate.Length;
        private bool Scrolled => Abs(_scrollPosition - _oldScroll) > 0.0001f;
        private uint ScrollRow => (uint)Floor(_scrollPosition);
        private int[] LongestStrings => _longestBufferStrings.Zip(_headingLengths, Mathf.Max).ToArray();
        private int LongestRow => LongestStrings.Sum() + NumColumns * ColumnPadding;
        private float TableWidth {
            get {
                if (RowCount == 0) return _noEntriesWidth;
                var sum = 0f;
                for (var i = 0; i < NumColumns; i++) sum += _columnWidths[i];
                return sum;
            }
        }
        private int NumDisplayRows => _buffer.Length;
        private bool DefaultTable => NumDisplayRows == DefaultNumRowsToDisplay;
        private bool UpdateEveryTick => _predicate.IsDynamic && _predicate.IsIntensional;
        private bool UpdateMonthly => _predicate.IsDynamic && _predicate.IsExtensional;
        private bool SetLastMonth() { // only using Set naming style for TrySet...
            _lastMonth = TalkOfTheTown.Time.Month;
            return true;
        }
        private bool TrySetLastMonth() => _lastMonth != TalkOfTheTown.Time.Month && SetLastMonth();
        private bool MonthlyUpdate() => UpdateMonthly && TrySetLastMonth();
        private bool UpdateCheck => UpdateEveryTick || MonthlyUpdate() || _newlySorted;
        public bool NewActivity { get; private set; }
        private bool UpdateRowCount() {
            if (_previousRowCount == RowCount) {
                NewActivity = false;
                return false;
            }
            _previousRowCount = RowCount;
            NewActivity = true;
            return true;
        }
        private bool RowCountChange => !_usingScroll && UpdateRowCount();

        private GUILayoutOption ScrollHeight => Height((NumDisplayRows + 3) * LabelHeight);
        private GUILayoutOption ColumnWidth(int i) {
            var len = LongestStrings[i] + ColumnPadding;
            if (len > _columnWidths[i]) _columnWidths[i] = len;
            else if (len < _columnWidths[i] - 1) _columnWidths[i] -= 0.002f;
            return Width(_columnWidths[i]);
        }

        private Rect TableRect(int x, int y, int height) => // no width control - size of columns is calculated
            new(x, y, TableWidth + (_usingScroll ? ScrollbarOffset : 0) + NumColumns * ColumnPadding, height);
        private Rect LeftSideTables(int tableNum) => // special case for the 4 tables on the left side
            TableRect(TablePadding, tableNum * (DefaultTableHeight + TablePadding) + TablePadding, 
                DefaultTableHeight); // used in conjunction with DefaultNumRowsToDisplay - num rows dictates height

        private static int NumRowsToHeight(int numRows) => (numRows + 5) * LabelHeight + 2 * TablePadding;
        private Rect TableRect(int x, int y) => TableRect(x, y, NumRowsToHeight(NumDisplayRows));
        
        private static bool ScrollingInRect(Rect rect) => // Scroll check based on Rects
            rect.Contains(new Vector2(mousePosition.x, height - mousePosition.y)) &&
            current.type is EventType.ScrollWheel or EventType.MouseDrag;

        // ************************************ Table control *************************************
        public void Initialize() {
            // This is how I am doing bold label display for now, so ditto for size calc:
            skin.label.fontStyle = FontStyle.Bold;
            CalcStringLengths(_headings, ref _headingLengths);
            skin.label.fontStyle = FontStyle.Normal;
            // If the table is empty now or could be empty in future ticks...
            if (RowCount == 0 || _predicate.IsIntensional) {
                // Consider the no entries conditions as well
                _noEntries = new GUIContent($"No entries in table {_predicate.Name}");
                _noEntriesWidth = (int)skin.label.CalcSize(_noEntries).x;
                _noEntriesWidth = Max(_noEntriesWidth, LongestRow); }
            // GUITables must be initialized after Time (inside TalkOfTheTown):
            if (UpdateMonthly) _lastMonth = TalkOfTheTown.Time.Month;
            // Normal update, try to get row strings for the buffer:
            Update();
        }

        private void Update() {
            if (_sortComparer != null) {
                if (_sortBuffer == null || _sortBuffer.Length < _predicate.Length) 
                    _sortBuffer = new uint[_predicate.Length * 2];
                for (uint i = 0; i < _predicate.Length; i++) _sortBuffer[i] = i;
                Sort(_sortBuffer, _sortComparer);
                _bufferedRows = 0;
                for (var i = 0; i < _buffer.Length && ScrollRow + i < _predicate.Length; i++) {
                    _predicate.RowToStrings(_sortBuffer[ScrollRow + i], _buffer[i]);
                    _bufferedRows++;
                    _rowColors[i] = _colorizer(_sortBuffer[ScrollRow + i]);
                }
            } else {
                _bufferedRows = _predicate.RowRangeToStrings(ScrollRow, _buffer);
                for (var i = 0; i < _rowColors.Length && ScrollRow + i < _predicate.Length; i++)
                    _rowColors[i] = _colorizer((uint)(ScrollRow + i));
            }
            // update string lengths per tick - prevents permanent growth due to singular long entries
            var updatedBufferStrings = new int[NumColumns];
            for (var i = 0; i < _bufferedRows; i++)
                CalcStringLengths(_buffer[i], ref updatedBufferStrings);
            // overwrite the longest strings for per tick update, pass in to CalcStringLength for permanent growth
            _longestBufferStrings = updatedBufferStrings;
            _newlySorted = false;
        }
        
        private void CalcStringLengths(IReadOnlyList<string> strings, ref int[] stringLengths) {
            for (var i = 0; i < NumColumns; i++) {
                var stringLength = (int)skin.label.CalcSize(new GUIContent(strings[i])).x;
                if (stringLengths[i] < stringLength) stringLengths[i] = stringLength;
            }
        }

        private void LayoutHeaderRow(IReadOnlyList<string> strings) {
            BeginHorizontal();
            for (var i = 0; i < NumColumns; i++)
                if (HeaderButton(strings[i], ColumnWidth(i))) {
                    _sortComparer = _predicate.RowComparison(i);
                    _newlySorted = true;
                }
            EndHorizontal();
        }
        private void LayoutRow(IReadOnlyList<string> strings, Color c) {
            BeginHorizontal();
            skin.label.normal.textColor = c;
            for (var i = 0; i < NumColumns; i++) 
                Label(strings[i], ColumnWidth(i));
            skin.label.normal.textColor = white;
            EndHorizontal();
        }

        private void OnGUI(Rect screenRect, int tableNum = -1) {
            BeginArea(screenRect); // table area
            // Title and Header:
            BeginHorizontal();
            TableTitle(tableNum, $"{_predicate.Name} ({_predicate.Length})");
            foreach (var binding in tableButtons[_predicate])
                if (Button(binding.Key, Width((int)skin.button.CalcSize(
                    new GUIContent(binding.Key)).x))) binding.Value();
            Space((_usingScroll ? ScrollbarOffset : 0) + 2 * ColumnPadding);
            EndHorizontal();
            LayoutHeaderRow(_headings);
            BeginHorizontal(); // table and scroll bar area
            // Table contents:
            BeginVertical();
            if (RowCount == 0) Label($"No entries in table {_predicate.Name}");
            else for (var i = 0; i < _buffer.Length; i++) LayoutRow(_buffer[i], _rowColors[i]);
            EndVertical();
            // Scrollbar and Update logic:
            if (RowCount != 0 && RowCount >= NumDisplayRows) {
                _scrollPosition = VerticalScrollbar(_scrollPosition,
                    NumDisplayRows - 0.1f, 0f, RowCount, ScrollHeight);
                if (ScrollingInRect(screenRect)) {
                    var scrollTo = _scrollPosition + current.delta.y;
                    _scrollPosition = scrollTo < 0f ? 0f : scrollTo > RowCount ? RowCount : scrollTo;
                }
                if (Scrolled || !_usingScroll || UpdateCheck) {
                    Update();
                    _oldScroll = _scrollPosition;
                }
                if (!_usingScroll) _usingScroll = true;
            } else if (RowCountChange || UpdateCheck) Update();
            EndHorizontal();
            EndArea();
        }

        public void OnGUI(int tableNum) { // Default check is not needed, more of a reminder that this
            // version of OnGUI is only meant to be called in the default case of left side display.
            if (DefaultTable) OnGUI(LeftSideTables(tableNum), tableNum);
        }
        public void OnGUI(int x, int y) => OnGUI(TableRect(x, y));
    }
}
