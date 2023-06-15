using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using TotT.Simulator;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;
using static UnityEngine.Input; // Event.current.mousePosition doesn't work as expected for Rect check

namespace TotT.Unity {
    using static StringProcessing;
    using static GUIManager;

    // ReSharper disable once InconsistentNaming
    /// <summary>OnGUI based TablePredicate viewer.</summary>
    public class GUITable {
        private const int ColumnPadding = 5;
        private const int LabelHeight = 16; // same as fontSize + padding
        private const int TablePadding = 8; // using height padding for left side offset as well
        private const int DefaultTableHeight = 260; // (260 * 4) + (8 * 5) = 1080...
        private const int TableWidthOffset = 50; // account for the scrollbar and then some
        private const int DefaultNumRowsToDisplay = (DefaultTableHeight - TablePadding) / LabelHeight - 5;
        // - 5 from display rows; two for the bold title and header row, and three to not get cutoff/crowd the space
        // this could probably be calculated dynamically based on label margins but for now this offset math is good enough.
        // to fix, update DefaultNumRowsToDisplay, ScrollHeight, NumRowsToHeight... everything derived from LabelHeight
        
        private readonly TablePredicate _predicate;
        private string[][] _buffer;
        private int[] _longestBufferStrings;
        private readonly float[] _columnWidths;
        private readonly string[] _headings;
        private int[] _headingLengths;
        private GUIContent _noEntries;
        private int _noEntriesWidth;
        private uint _bufferedRows;
        private uint _previousRowCount;
        private bool _usingScroll;
        private float _scrollPosition;
        private float _oldScroll;
        private Month _lastMonth;
        private Comparison<uint> _sortComparer;
        private uint[] _sortBuffer;

        public GUITable(TablePredicate predicate, int numRows = DefaultNumRowsToDisplay) {
            _predicate = predicate;
            _headings = (from heading in predicate.ColumnHeadings select Heading(heading)).ToArray();
            _headingLengths = new int[_headings.Length];
            BuildBuffer(numRows);
            _longestBufferStrings = new int[NumColumns];
            _columnWidths = new float[NumColumns];
            //_sortComparer = predicate.RowComparison(0);
        }

        public void NewNumRows(int numRows) {
            if (numRows != NumDisplayRows) BuildBuffer(numRows);
        }

        private void BuildBuffer(int numRows) {
            _buffer = new string[numRows][];
            for (var i = 0; i < numRows; i++)
                _buffer[i] = new string[NumColumns];
        }

        // *********************************** Table properties ***********************************
        private string Name => _predicate.Name;
        private int NumColumns => _headings.Length;
        private uint RowCount => _predicate.Length;
        private bool Scrolled => Math.Abs(_scrollPosition - _oldScroll) > 0.0001f;
        private uint ScrollRow => (uint)Math.Floor(_scrollPosition);
        private int[] LongestStrings => _longestBufferStrings.Zip(_headingLengths, Mathf.Max).ToArray();
        private int LongestRow => LongestStrings.Sum() + _longestBufferStrings.Length * ColumnPadding;
        private int TableWidth => RowCount == 0 ? _noEntriesWidth : LongestRow;
        private int NumDisplayRows => _buffer.Length;
        private bool DefaultTable => NumDisplayRows == DefaultNumRowsToDisplay;

        private bool UpdateEveryTick => _predicate.IsDynamic && _predicate.IsIntensional;
        private bool UpdateMonthly => _predicate.IsDynamic && _predicate.IsExtensional;
        private bool SetLastMonth() { // only using Set naming for TrySet...
            _lastMonth = TalkOfTheTown.Time.Month;
            return true;
        }
        private bool TrySetLastMonth() => _lastMonth != TalkOfTheTown.Time.Month && SetLastMonth();
        private bool MonthlyUpdate() => UpdateMonthly && TrySetLastMonth();
        private bool UpdateCheck => UpdateEveryTick || MonthlyUpdate();
        private bool UpdateRowCount() {
            if (_previousRowCount == RowCount) return false;
            _previousRowCount = RowCount;
            return true;
        }
        private bool RowCountChange => !_usingScroll && UpdateRowCount();

        private GUILayoutOption ScrollHeight => GUILayout.Height((NumDisplayRows + 3) * LabelHeight);
        private GUILayoutOption ColumnWidth(int i)
        {
            var len = LongestStrings[i];
            if (len > _columnWidths[i])
                _columnWidths[i] = len;
            else if (len < _columnWidths[i]-2)
                _columnWidths[i] -= 0.05f;
            return GUILayout.Width(_columnWidths[i] + ColumnPadding);
        }

        private Rect TableRect(int x, int y, int height) => // no width control - size of columns is calculated
            new(x, y, TableWidth + TableWidthOffset, height); // height control via num rows
        private Rect LeftSideTables(int tableNum) => // special case for the 4 tables on the left side
            TableRect(TablePadding, tableNum * (DefaultTableHeight + TablePadding) + TablePadding, 
                DefaultTableHeight); // used in conjunction with DefaultNumRowsToDisplay - num rows dictates height
        private static int NumRowsToHeight(int numRows) => (numRows + 5) * LabelHeight + 2 * TablePadding;
        private Rect TableRect(int x, int y) => TableRect(x, y, NumRowsToHeight(NumDisplayRows));
        
        private static bool ScrollingInRect(Rect rect) => // Scroll check based on Rects
            rect.Contains(new Vector2(mousePosition.x, Screen.height - mousePosition.y)) &&
            Event.current.type is EventType.ScrollWheel or EventType.MouseDrag;

        // ************************************ Table control *************************************
        public void Initialize() {
            // This is how I am doing bold label display for now, so ditto for size calc:
            GUI.skin.label.fontStyle = FontStyle.Bold;
            CalcStringLengths(_headings, ref _headingLengths);
            GUI.skin.label.fontStyle = FontStyle.Normal;
            // If the table is empty now or could be empty in future ticks...
            if (RowCount == 0 || _predicate.IsIntensional) {
                // Consider the no entries conditions as well
                _noEntries = new GUIContent($"No entries in table {Name}");
                _noEntriesWidth = (int)GUI.skin.label.CalcSize(_noEntries).x;
                _noEntriesWidth = Math.Max(_noEntriesWidth, LongestRow); }
            // GUITables must be initialized after Time (inside TalkOfTheTown):
            if (UpdateMonthly) _lastMonth = TalkOfTheTown.Time.Month;
            // Normal update, try to get row strings for the buffer:
            Update();
        }

        private void Update() {
            if (_sortComparer != null)
            {
                if (_sortBuffer == null || _sortBuffer.Length < _predicate.Length)
                    _sortBuffer = new uint[_predicate.Length * 2];
                for (uint i = 0; i < _predicate.Length; i++)
                    _sortBuffer[i] = i;
                Array.Sort(_sortBuffer, _sortComparer);

                _bufferedRows = 0;
                for (int i = 0; i < _buffer.Length && ScrollRow + i < _predicate.Length; i++)
                {
                    _predicate.RowToStrings(_sortBuffer[ScrollRow + i], _buffer[i]);
                    _bufferedRows++;
                }
            } else 
                _bufferedRows = _predicate.RowRangeToStrings(ScrollRow, _buffer);

            // update string lengths per tick - prevents permanent growth due to singular long entries
            var updatedBufferStrings = new int[NumColumns];
            for (var i = 0; i < _bufferedRows; i++)
                CalcStringLengths(_buffer[i], ref updatedBufferStrings);
            // overwrite the longest strings for per tick update, pass in to CalcStringLength for permanent growth
            _longestBufferStrings = updatedBufferStrings;
        }

        private void CalcStringLengths(IReadOnlyList<string> strings, ref int[] stringLengths) {
            for (var i = 0; i < NumColumns; i++) {
                var stringLength = (int)GUI.skin.label.CalcSize(new GUIContent(strings[i])).x;
                if (stringLengths[i] < stringLength) stringLengths[i] = stringLength;
            }
        }

        private void LayoutRow(IReadOnlyList<string> strings) {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < NumColumns; i++)
                Label(strings[i], false, ColumnWidth(i));
            GUILayout.EndHorizontal();
        }

        private void LayoutHeaderRow(IReadOnlyList<string> strings) {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < NumColumns; i++)
                if (HeaderButton(strings[i], ColumnWidth(i)))
                    _sortComparer = _predicate.RowComparison(i);
            GUILayout.EndHorizontal();
        }

        private void OnGUI(Rect screenRect, int tableNum = -1) {
            GUILayout.BeginArea(screenRect); // table area
            // Title and Header:
            TableTitle(tableNum, $"{Name} ({_predicate.Length})");
            LayoutHeaderRow(_headings);
            GUILayout.BeginHorizontal(); // table and scroll bar area
            // Table contents:
            GUILayout.BeginVertical();
            if (RowCount == 0) GUILayout.Label($"No entries in table {Name}");
            else { foreach (var row in _buffer) LayoutRow(row); }
            GUILayout.EndVertical();
            // Scrollbar and Update logic:
            if (RowCount != 0 && RowCount >= NumDisplayRows) {
                _scrollPosition = GUILayout.VerticalScrollbar(_scrollPosition,
                    NumDisplayRows - 0.1f, 0f, RowCount, ScrollHeight);
                if (ScrollingInRect(screenRect)) _scrollPosition += Event.current.delta.y;
                if (Scrolled || !_usingScroll || UpdateCheck) {
                    Update();
                    _oldScroll = _scrollPosition;
                }
                if (!_usingScroll) _usingScroll = true;
            } else if (RowCountChange || UpdateCheck) Update();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public void OnGUI(int tableNum) { // Default check is not needed, more of a reminder that this
            // version of OnGUI is only meant to be called in the default case of left side display.
            if (DefaultTable) OnGUI(LeftSideTables(tableNum), tableNum);
        }
        public void OnGUI(int x, int y) => OnGUI(TableRect(x, y));
    }
}
