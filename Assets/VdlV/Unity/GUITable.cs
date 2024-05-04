using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using VdlV.Time;
using VdlV.Utilities;
using UnityEngine;

namespace VdlV.Unity {
    using static Array;
    using static Clock;
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
        #region Constants (layout & sizing)
        private const int ColumnPadding = 5;
        private const int LabelHeight = 16; // same as fontSize + padding
        private const int TablePadding = 8; // using height padding for left side offset as well
        private const int DefaultTableHeight = 260; // (260 * 4) + (8 * 5) = 1080...
        internal const int ScrollbarOffset = 20;
        private const int DefaultNumRowsToDisplay = (DefaultTableHeight - TablePadding) / LabelHeight - 5;
        // - 5 from display rows; two for the bold title and header row, and three to not get cutoff/crowd the space
        // this could probably be calculated dynamically based on label margins but for now this offset math is good enough.
        // to fix, update DefaultNumRowsToDisplay, ScrollHeight, REPLHeight... everything derived from LabelHeight
        private const int RowHeight = 22; // instead of math, manual count the pixels on screen... ~22?
        #endregion

        #region Fields & Properties
        #region Predicate Title & Header
        private readonly TablePredicate _predicate;
        private string Name => _predicate.Name;
        private string NoEntriesText => $"No entries in table {Name}";
        private GUIContent NoEntries => new(NoEntriesText);
        // CalcSize can only be called in GUI code, _noEntriesWidth is the width of NoEntries or
        // the LongestRow (choosing the greater of the two).
        private int _noEntriesWidth;

        private uint RowCount => _predicate.Length;
        private string TitleText => $"{Name} ({RowCount})";
        private GUIContent Title => new(TitleText);
        // CalcSize can only be called in GUI code, _titleWidth is the width of Title and
        // the sum of the widths of any buttons assigned to this table (_buttonWidths).
        private int _titleWidth;

        // Buttons are assigned in GUIManager, but to prevent excess width calculation we store
        // the calculated widths once and retrieve them from this dictionary thenceforth.
        private readonly Dictionary<string, int> _buttonWidths = new();

        // Derived from _predicate.ColumnHeadings but only calculated at instantiation.
        private readonly string[] _headings;
        private int NumColumns => _headings.Length;
        private int TotalPadding => NumColumns * ColumnPadding;
        #endregion

        #region Buffer & string/row lengths
        private readonly string[][] _buffer;
        private int NumDisplayRows => _buffer.Length;

        private uint _bufferedRows;

        // CalcSize results when called on all buffer strings/headers:
        private readonly int[] _longestBufferStrings;
        private readonly int[] _headingLengths;
        private int[] LongestStrings => _longestBufferStrings.Zip(_headingLengths, Mathf.Max).ToArray();
        private int LongestRow => LongestStrings.Sum() + TotalPadding;

        // _columnWidths are driven by LongestStrings, but with a decay rate to prevent the columns
        // from rapidly changing widths in dynamic intensional tables.
        private readonly float[] _columnWidths;
        public float TableWidth {
            get {
                var toReturn = 0f;
                if (RowCount == 0) toReturn = _noEntriesWidth;
                else {
                    for (var i = 0; i < NumColumns; i++)
                        toReturn += _columnWidths[i];
                }
                return Max(toReturn, _titleWidth) + (_usingScroll ? ScrollbarOffset : 0) + TotalPadding;
            }
        }

        // ReSharper disable once InconsistentNaming
        private int MaxREPLHeight => (int)(NumDisplayRows * 1.5 * LabelHeight);
        // ReSharper disable once InconsistentNaming
        public int REPLHeight => _bufferedRows >= NumDisplayRows ? MaxREPLHeight : _bufferedRows > 0 ?
                                     MaxREPLHeight - (int)((NumDisplayRows - _bufferedRows) * RowHeight) :
                                     MaxREPLHeight - (NumDisplayRows - 1) * RowHeight;
        #endregion

        #region Sorting
        private uint[] _sortBuffer;             // Lazy initialize _sortBuffer, resized with RowCount
        private int _sortColumn = -1;           // which column is being compared (allows for sorting to be disabled)
        private Comparison<uint> _sortComparer; // the comparer used for sorting
        private bool _newlySorted;              // indicates when an Update is needed for a new sort condition
        #endregion

        #region Updating
        private bool UpdateEveryTick => _predicate.IsDynamic && _predicate.IsIntensional;
        private bool UpdateMonthly => _predicate.IsDynamic && _predicate.IsExtensional;
        // Used in conjunction with UpdateMonthly to determine if an update is needed when the month changes
        private Month _lastMonth;

        private float _scrollPosition;
        private uint ScrollRow => (uint)Floor(_scrollPosition);

        private float _oldScroll;
        private bool Scrolled => Abs(_scrollPosition - _oldScroll) > 0.0001f;
        // _usingScroll is used to both decide if the scrollbar should be showing, but also
        // to block updating through RowCountChanged when RowCount is 0 and to force an update
        // when the RowCount grows beyond NumDisplayRows for the first time.
        private bool _usingScroll;

        // _previousRowCount sets NewActivity by comparison to RowCount in UpdateRowCount.
        private uint _previousRowCount;
        public bool NewActivity { get; private set; }
        #endregion

        #region Colors
        private readonly Color[] _rowColors;
        private readonly Func<uint, Color> _colorizer;
        #endregion
        #endregion

        public GUITable(TablePredicate predicate, int numRows = DefaultNumRowsToDisplay) {
            _predicate = predicate;
            _headings = (from heading in predicate.ColumnHeadings select Heading(heading)).ToArray();
            _headingLengths = new int[NumColumns];
            _buffer = new string[numRows][];
            for (var i = 0; i < numRows; i++)
                _buffer[i] = new string[NumColumns];
            _rowColors = new Color[numRows];
            _longestBufferStrings = new int[NumColumns];
            _columnWidths = new float[NumColumns];
            _colorizer = predicate.Property.TryGetValue("Colorizer", out var func) ? 
                             (Func<uint,Color>)func : _ => white;
        }

        // *********************************** Update Functions ***********************************
        private bool MonthChanged() {
            if (_lastMonth == Month()) return false;
            _lastMonth = Month();
            return true;
        }
        private bool CheckForUpdate() => UpdateEveryTick || (UpdateMonthly && MonthChanged()) || _newlySorted;
        private bool UpdateRowCount() {
            if (_previousRowCount == RowCount) {
                NewActivity = false;
                return false;
            }
            _previousRowCount = RowCount;
            NewActivity = true;
            return true;
        }
        private bool RowCountChanged() => !_usingScroll && UpdateRowCount();

        // *********************************** Layout Functions ***********************************

        private GUILayoutOption ScrollHeight() => Height((int)(NumDisplayRows * 1.3 * LabelHeight));
        private GUILayoutOption ColumnWidth(int i) {
            var len = LongestStrings[i] + ColumnPadding;
            if (len > _columnWidths[i]) _columnWidths[i] = len;
            else if (len < _columnWidths[i] - 2) _columnWidths[i] -= 0.005f;
            return Width(_columnWidths[i]);
        }
        
        public Rect LeftSideTables(int tableNum) => new(TablePadding, tableNum * 
            (DefaultTableHeight + TablePadding) + TablePadding, TableWidth, DefaultTableHeight);

        private static bool ScrollingInRect(Rect rect) => // Scroll check based on Rects
            rect.Contains(new Vector2(mousePosition.x, height - mousePosition.y)) &&
            current.type is EventType.ScrollWheel or EventType.MouseDrag;

        // ************************************ Table Updates *************************************

        public void Initialize() {
            // This is how I am doing bold label display for now, so ditto for size calc:
            skin.label.fontStyle = FontStyle.Bold;
            CalcStringLengths(_headings, _headingLengths);
            skin.label.fontStyle = FontStyle.BoldAndItalic;
            _titleWidth = (int)skin.label.CalcSize(Title).x;
            _titleWidth += _buttonWidths.Values.Sum();
            skin.label.fontStyle = FontStyle.Normal;
            // If the table is empty now or could be empty in future ticks...
            if (RowCount == 0 || _predicate.IsIntensional) {
                // Consider the no entries conditions as well
                _noEntriesWidth = (int)skin.label.CalcSize(NoEntries).x;
                _noEntriesWidth = Max(_noEntriesWidth, LongestRow);
            }
            // GUITables must be initialized after Clock (inside VoixDeLaVille):
            if (UpdateMonthly) _lastMonth = Month();
            // Normal update, try to get row strings for the buffer:
            Update();
        }

        private void Update() {
            if (_sortComparer != null) {
                if (_sortBuffer == null || _sortBuffer.Length < RowCount)
                    _sortBuffer = new uint[RowCount * 2];
                uint i;
                for (i = 0; i < RowCount; i++) _sortBuffer[i] = i;
                Sort(_sortBuffer, _sortComparer);
                _newlySorted = false;
                for (i = 0; i < NumDisplayRows && ScrollRow + i < RowCount; i++) {
                    _predicate.RowToStrings(_sortBuffer[ScrollRow + i], _buffer[i]);
                    _rowColors[i] = _colorizer(_sortBuffer[ScrollRow + i]);
                }
                _bufferedRows = i;
            } else {
                _bufferedRows = _predicate.RowRangeToStrings(ScrollRow, _buffer);
                for (var i = 0; i < NumDisplayRows && ScrollRow + i < RowCount; i++)
                    _rowColors[i] = _colorizer((uint)(ScrollRow + i));
            }
            // update string lengths per tick - prevents permanent growth due to singular long entries
            var updatedBufferStrings = new int[NumColumns];
            for (var i = 0; i < _bufferedRows; i++)
                CalcStringLengths(_buffer[i], updatedBufferStrings);
            Copy(updatedBufferStrings, _longestBufferStrings, NumColumns);
        }
        
        private void CalcStringLengths(IReadOnlyList<string> strings, IList<int> stringLengths) {
            for (var i = 0; i < NumColumns; i++) {
                var stringLength = (int)skin.label.CalcSize(new GUIContent(strings[i])).x;
                if (stringLengths[i] < stringLength) stringLengths[i] = stringLength;
            }
        }

        // ************************************* Table Layout *************************************

        private void LayoutHeaderRow(IReadOnlyList<string> strings) {
            BeginHorizontal();
            for (var i = 0; i < NumColumns; i++)
                if (HeaderButton(strings[i], ColumnWidth(i))) {
                    if (_sortColumn == i) {
                        _sortComparer = null;
                        _sortColumn = -1;
                    } else {
                        _sortComparer = _predicate.RowComparison(i);
                        _sortColumn = i;
                        _newlySorted = true;
                    }
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
        private void LayoutButtons() {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var binding in tableButtons[_predicate]) {
                if (!_buttonWidths.ContainsKey(binding.Key))
                    _buttonWidths[binding.Key] = (int)skin.button.CalcSize(new GUIContent(binding.Key)).x;
                if (Button(binding.Key, Width(_buttonWidths[binding.Key]))) binding.Value();
            }
        }

        public void OnGUI(int tableNum) => OnGUI(tableNum, LeftSideTables(tableNum));

        public void OnGUI(int tableNum, Rect rect) {
            BeginArea(rect);
            BeginHorizontal(); // Title and header
            TableTitle(tableNum, TitleText);
            LayoutButtons();
            Space((_usingScroll ? ScrollbarOffset : 0) + 2 * ColumnPadding);
            EndHorizontal(); // Title and header
            LayoutHeaderRow(_headings);
            BeginHorizontal(); // Table contents and scroll bar
            BeginVertical();   // Table contents
            if (RowCount == 0) Label(NoEntriesText);
            else for (var i = 0; i < _buffer.Length; i++) LayoutRow(_buffer[i], _rowColors[i]);
            EndVertical(); // Table contents
            // Scrollbar and Update logic:
            if (RowCount != 0 && RowCount >= NumDisplayRows) {
                _scrollPosition = VerticalScrollbar(_scrollPosition, NumDisplayRows - 0.1f,
                                                    0f, RowCount, ScrollHeight());
                if (ScrollingInRect(rect))
                    _scrollPosition = Clamp(_scrollPosition + current.delta.y, 0f, RowCount);
                if (Scrolled || !_usingScroll || CheckForUpdate()) {
                    Update();
                    _oldScroll = _scrollPosition;
                }
                _usingScroll = true; // Only needs to be set to true once, this is faster than a branch
            } else if (RowCountChanged() || CheckForUpdate()) Update();
            EndHorizontal(); // Table contents and scroll bar
            EndArea();
        }
    }
}
