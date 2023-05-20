using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using UnityEngine;
using static UnityEngine.Input;

// ReSharper disable InconsistentNaming
public static class GUIManager {
    #region Fields - Table display and interactive state
    internal static Dictionary<string, GUITable> Tables = new();
    internal static Dictionary<string, string> DisplayNameToTableName = new();
    internal static string[] TableDisplayNames;
    internal static GUIContent[] GUITableDisplayNames;
    internal static string[] ActiveTables;  // should also be a string[4]
    internal static string[] TableSelector = { "Table 1", "Table 2", "Table 3", "Table 4" };
    internal static int TableToChange;
    internal static bool ShowTables = true;
    internal static int ChangeTableSelector;
    internal static bool ChangeTable;
    // set with a call to GetTableToolbarSize, only called once in the OnGUI function via SetupGUI
    internal static int TableToolbarSize;
    #endregion

    #region Display Constants
    internal const string ShowHideTables = "Show/hide tables?";
    internal const int ShowTablesWidth = 121 + LabelBorders;
    internal const string ChangeTables = "Change tables?";
    internal const int ChangeTablesWidth = 105 + LabelBorders;
    internal const int LabelBorders = 10;
    internal const int TopMiddleRectHeight = 30; // allows for TopMiddleRectStacks
    internal const int TableToolbarWidth = 250;
    internal const int SelectionGridWidth = 720;
    internal const int TileSize = 16;
    internal const int TableDisplayNameCutoff = 18;
    #endregion

    #region GUIStrings
    // Some GUIStrings will always be displayed - these can just go in the list. Others,
    // like pause need to be addressed with external logic so its easiest to keep separate
    internal static GUIString paused = new("Simulation is paused", CenteredRect);
    internal static List<GUIString> guiStrings = new() {
        new GUIString(TalkOfTheTown.Time.ToString, TopRightRect), };
    #endregion

    #region Called once each in Start
    public static void SetAvailableTables(List<TablePredicate> tables) {
        foreach (var table in tables) Tables[table.Name] = new GUITable(table);
        TableDisplayNames = Tables.Keys.Select(n => n.Length > TableDisplayNameCutoff ?
            n[..TableDisplayNameCutoff] + "…" : n).ToArray();
        GUITableDisplayNames = (from available in TableDisplayNames
            select new GUIContent(available)).ToArray();
        DisplayNameToTableName = TableDisplayNames.Zip(Tables.Keys.ToArray(), 
            (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v); }
    public static void SetActiveTables(string[] activeTables) => ActiveTables = activeTables;
    public static void AddSelectedTileInfo(Func<string> tileString) =>
        guiStrings.Add(new GUIString(tileString, SelectedTileInfoRect, false));
    public static void AddPopulationInfo(Func<string> populationString) =>
        guiStrings.Add(new GUIString(populationString, BottomRightRect));
    #endregion

    #region Called once each in SetupGUI
    public static void CustomSkins() {
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        GUI.skin.label.fontSize = 14;
        GUI.skin.label.padding.top = 1;
        GUI.skin.label.padding.bottom = 1;
        GUI.skin.label.fixedHeight = 0; }
    public static void GetTableToolbarSize() => TableToolbarSize = 
        (from content in GUITableDisplayNames select (int)GUI.skin.button.CalcSize(content).x).Sum();
    public static void InitAllTables() { foreach (var table in Tables.Values) table.Initialize(); }
    #endregion

    #region Called in OnGUI
    public static void ShowPaused() => paused.OnGUI();
    public static void ShowStrings() { foreach (var guiString in guiStrings) guiString.OnGUI(); }
    public static void ShowActiveTables() {
        if (!ShowTables) return;
        for (var i = 0; i < ActiveTables.Length; i++) Tables[ActiveTables[i]].OnGUI(i); }
    public static void ChangeActiveTables() {
        // Change and show/hide toggles:
        GUI.BeginGroup(TopMiddleRectStack(ChangeTablesWidth + ShowTablesWidth));
        ChangeTable = GUI.Toggle(ChangeTablesRect, ChangeTable, ChangeTables);
        ShowTables = GUI.Toggle(ShowTablesRect, ShowTables, ShowHideTables);
        GUI.EndGroup();
        if (!ChangeTable || !ShowTables) return;
        // If we are trying to change tables:
        TableToChange = GUI.Toolbar(TopMiddleRectStack(TableToolbarWidth, 2), TableToChange, TableSelector);
        ChangeTableSelector = Array.IndexOf(Tables.Keys.ToArray(), ActiveTables[TableToChange]);
        // Build the selection grid:
        var selectionGridHeight = Mathf.CeilToInt(TableDisplayNames.Length / 5f) * TopMiddleRectHeight;
        var selectionGridRect = TopMiddleRectStack(SelectionGridWidth, selectionGridHeight, 3);
        ChangeTableSelector = GUI.SelectionGrid(selectionGridRect, ChangeTableSelector, TableDisplayNames, 5);
        // update the active table to change with the selected table
        ActiveTables[TableToChange] = DisplayNameToTableName[TableDisplayNames[ChangeTableSelector]]; }
    public static void RuleExecutionTimes() => GUI.Label(CenteredRect(480, 350),
        string.Concat((from table in TalkOfTheTown.Simulation.Tables
                            where table.RuleExecutionTime > 0 && !table.Name.Contains("initially")
                            orderby -table.RuleExecutionTime
                            select $"{table.Name} {table.RuleExecutionTime}\n").Take(20)));
    #endregion

    #region Rects - display containers
    // Special Top Middle Rects
    internal static Rect ChangeTablesRect = new(0, 0, ChangeTablesWidth, TopMiddleRectHeight);
    internal static Rect ShowTablesRect = new(ChangeTablesWidth, 0, ShowTablesWidth, TopMiddleRectHeight);
    internal static Rect TopMiddleRectStack(int width, int num = 1) => TopMiddleRectStack(width, TopMiddleRectHeight, num);
    // height should always be a multiple of TopMiddleRectHeight (unless its the final row)
    internal static Rect TopMiddleRectStack(int width, int height, int num) =>
        new(Screen.width / 2 - width / 2, (LabelBorders * num) + (TopMiddleRectHeight * (num - 1)), width, height);

    // Generic Rects
    internal static Rect CenteredRect(int width, int height) =>
        new(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
    internal static Rect TopRightRect(int width, int height) =>
        new(Screen.width - (width + LabelBorders), LabelBorders, width, height);
    internal static Rect BottomMiddleRect(int width, int height) =>
        new(Screen.width / 2 - width / 2, Screen.height - (height + LabelBorders), width, height);
    internal static Rect BottomRightRect(int width, int height) => 
        new(Screen.width - (width + LabelBorders), Screen.height - (height + LabelBorders), width, height);

    // Mouse following Rect
    internal static Rect SelectedTileInfoRect(int width, int height) =>
        new(mousePosition.x + TileSize, (Screen.height - mousePosition.y) + TileSize, width, height);
    #endregion

    #region Label helpers
    public static void BoldLabel(string label, params GUILayoutOption[] options) {
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUILayout.Label(label, options);
        GUI.skin.label.fontStyle = FontStyle.Normal; }
    public static void Label(string label, bool bold, params GUILayoutOption[] options) {
        if (bold) BoldLabel(label, options);
        else GUILayout.Label(label, options); }
    public static void ButtonLabel(string label, out bool pressed, params GUILayoutOption[] options) {
        GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
        pressed = GUILayout.Button(label, GUI.skin.label, options);
        GUI.skin.label.fontStyle = FontStyle.Normal; }
    public static void TableTitleToggle(int tableNum) {
        if (TableToChange == tableNum)
            ChangeTable = !ChangeTable;
        else ChangeTable = true;
        TableToChange = tableNum; }
    public static void TableTitle(int tableNum, string title) {
        if (tableNum >= 0) {
            ButtonLabel(title, out var pressed);
            if (pressed) TableTitleToggle(tableNum);
        } else BoldLabel(title); }
    #endregion
}

public class GUITable {
    #region Table display constants
    internal const int ColumnPadding = 5;
    internal const int LabelHeight = 16; // same as fontSize + padding
    internal const int TablePadding = 8; // using height padding for left side offset as well
    internal const int DefaultTableHeight = 260; // (260 * 4) + (8 * 5) = 1080...
    internal const int TableWidthOffset = 50; // account for the scrollbar and then some
    internal const int DefaultNumRowsToDisplay = (DefaultTableHeight - TablePadding) / LabelHeight - 5;
    // - 5 from display rows; two for the bold title and header row, and three to not get cutoff/crowd the space
    // this could probably be calculated dynamically based on label margins but for now this offset math is good enough.
    // to fix, update DefaultNumRowsToDisplay, ScrollHeight, NumRowsToHeight... everything derived from LabelHeight
    #endregion
    
    #region Fields
    internal TablePredicate predicate;
    internal string[][] buffer;
    internal int[] longestBufferStrings;
    internal string[] headings;
    internal int[] headingLengths;
    internal GUIContent noEntries;
    internal int noEntriesWidth;
    internal uint bufferedRows;
    internal uint previousRowCount;
    internal bool usingScroll;
    internal float scrollPosition;
    internal float oldScroll;
    internal Month lastMonth;
    #endregion

    #region Properties & property helper functions
    internal string Name => predicate.Name;
    internal int NumColumns => headings.Length;
    internal uint RowCount => predicate.Length;
    internal bool Scrolled => Math.Abs(scrollPosition - oldScroll) > 0.0001f;
    internal uint ScrollRow => (uint)Math.Floor(scrollPosition);
    internal int[] LongestStrings => longestBufferStrings.Zip(headingLengths, Mathf.Max).ToArray();
    internal int LongestRow => LongestStrings.Sum() + longestBufferStrings.Length * ColumnPadding;
    internal int TableWidth => RowCount == 0 ? noEntriesWidth : LongestRow;
    internal int NumDisplayRows => buffer.Length;
    internal bool DefaultTable => NumDisplayRows == DefaultNumRowsToDisplay;

    // Properties related to logic about when to call Update:
    internal bool UpdateEveryTick => predicate.IsDynamic && predicate.IsIntensional;
    internal bool UpdateMonthly => predicate.IsDynamic && predicate.IsExtensional;
    internal bool SetLastMonth() { 
        lastMonth = TalkOfTheTown.Time.Month;
        return true; }
    internal bool TrySetLastMonth() => lastMonth != TalkOfTheTown.Time.Month && SetLastMonth();
    internal bool MonthlyUpdate() => UpdateMonthly && TrySetLastMonth();
    internal bool UpdateCheck => UpdateEveryTick || MonthlyUpdate();
    internal bool UpdateRowCount() {
        if (previousRowCount == RowCount) return false;
        previousRowCount = RowCount;
        return true; }
    internal bool RowCountChange => !usingScroll && UpdateRowCount();
    #endregion

    #region Construction and helpers
    public GUITable(TablePredicate predicate, int numRows = DefaultNumRowsToDisplay) {
        this.predicate = predicate;
        headings = (from heading in predicate.ColumnHeadings
            select Utils.Heading(heading)).ToArray();
        headingLengths = new int[headings.Length];
        BuildBuffer(numRows);
        longestBufferStrings = new int[NumColumns]; }
    public void NewNumRows(int numRows) {
        if (numRows != NumDisplayRows) BuildBuffer(numRows); }
    internal void BuildBuffer(int numRows) {
        buffer = new string[numRows][];
        for (var i = 0; i < numRows; i++)
            buffer[i] = new string[NumColumns]; }
    #endregion

    #region Table Updates
    public void Initialize() {
        // This is how I am doing bold label display for now, so ditto for size calc:
        GUI.skin.label.fontStyle = FontStyle.Bold;
        CalcStringLengths(headings, ref headingLengths);
        GUI.skin.label.fontStyle = FontStyle.Normal;
        // If the table is empty now or could be empty in future ticks...
        if (RowCount == 0 || predicate.IsIntensional) {
            // Consider the no entries conditions as well
            noEntries = new GUIContent($"No entries in table {Name}");
            noEntriesWidth = (int)GUI.skin.label.CalcSize(noEntries).x;
            noEntriesWidth = Math.Max(noEntriesWidth, LongestRow); }
        // GUITables must be initialized after Time (inside TalkOfTheTown):
        if (UpdateMonthly) lastMonth = TalkOfTheTown.Time.Month;
        // Normal update, try to get row strings for the buffer:
        Update(); }
    public void Update() {
        bufferedRows = predicate.RowRangeToStrings(ScrollRow, buffer);
        // update string lengths per tick - prevents permanent growth due to singular long entries
        var updatedBufferStrings = new int[NumColumns];
        for (var i = 0; i < bufferedRows; i++) CalcStringLengths(buffer[i], ref updatedBufferStrings);
        // overwrite the longest strings for per tick update, pass in to CalcStringLength for permanent growth
        longestBufferStrings = updatedBufferStrings; }
    internal void CalcStringLengths(string[] strings, ref int[] stringLengths) {
        for (var i = 0; i < NumColumns; i++) {
            var stringLength = (int)GUI.skin.label.CalcSize(new GUIContent(strings[i])).x;
            if (stringLengths[i] < stringLength) stringLengths[i] = stringLength; } }
    #endregion

    #region GUILayout helper functions
    // no width control - size of columns is calculated
    internal Rect TableRect(int x, int y, int height) => new(x, y, TableWidth + TableWidthOffset, height);
    // height control via num rows with a special case for the 4 tables on the left side
    internal Rect LeftSideTables(int tableNum) => TableRect(TablePadding, 
        tableNum * (DefaultTableHeight + TablePadding) + TablePadding, DefaultTableHeight);
    internal static int NumRowsToHeight(int numRows) => (numRows + 5) * LabelHeight + 2 * TablePadding;
    internal Rect TableRect(int x, int y) => TableRect(x, y, NumRowsToHeight(NumDisplayRows));
    // Scroll check based on Rects (why doesn't Event.current.mousePosition work?)
    internal static bool ScrollingInRect(Rect rect) =>
        rect.Contains(new Vector2(mousePosition.x, Screen.height - mousePosition.y)) && 
        Event.current.type is EventType.ScrollWheel or EventType.MouseDrag;
    // Display options
    internal GUILayoutOption ScrollHeight => GUILayout.Height((NumDisplayRows + 3) * LabelHeight);
    internal GUILayoutOption ColumnWidth(int i) => GUILayout.Width(LongestStrings[i] + ColumnPadding);
    #endregion

    #region Table Layout
    internal void LayoutRow(string[] strings, bool header = false) {
        GUILayout.BeginHorizontal();
        for (var i = 0; i < NumColumns; i++)
            GUIManager.Label(strings[i], header, ColumnWidth(i));
        GUILayout.EndHorizontal(); }

    internal void OnGUI(Rect screenRect, int tableNum = -1) {
        GUILayout.BeginArea(screenRect); // table area
        // Title and Header:
        GUIManager.TableTitle(tableNum, Name);
        LayoutRow(headings, true);
        GUILayout.BeginHorizontal(); // table and scroll bar area
        // Table contents:
        GUILayout.BeginVertical();
        if (RowCount == 0) GUILayout.Label($"No entries in table {Name}");
        else { foreach (var row in buffer) LayoutRow(row); }
        GUILayout.EndVertical();
        // Scrollbar and Update logic:
        if (RowCount != 0 && RowCount >= NumDisplayRows) {
            scrollPosition = GUILayout.VerticalScrollbar(scrollPosition,
                NumDisplayRows - 0.1f, 0f, RowCount, ScrollHeight);
            if (ScrollingInRect(screenRect)) scrollPosition += Event.current.delta.y;
            if (Scrolled || !usingScroll || UpdateCheck) {
                Update();
                oldScroll = scrollPosition; }
            if (!usingScroll) usingScroll = true;
        } else if (RowCountChange || UpdateCheck) Update();
        GUILayout.EndHorizontal();
        GUILayout.EndArea(); }
    #endregion

    public void OnGUI(int tableNum) { // Default check is not needed, more of a reminder that this
        // version of OnGUI is only meant to be called in the default case of left side display.
        if (DefaultTable) OnGUI(LeftSideTables(tableNum), tableNum); }
    public void OnGUI(int x, int y) => OnGUI(TableRect(x, y));
}

public class GUIString {
    internal const int WidthOffset = 26;
    internal const int HeightOffset = 6;

    internal string displayString;
    internal Func<string> GetStringFunc;
    internal bool staticStringCalcOnce;
    internal Func<int, int, Rect> DisplayFunc;
    internal int width;
    internal int height;
    internal bool centered = true;

    public GUIString(Func<string> getStringFunc, Func<int, int, Rect> displayFunc) {
        DisplayFunc = displayFunc;
        GetStringFunc = getStringFunc; }
    public GUIString(Func<string> getStringFunc, Func<int, int, Rect> displayFunc, bool centered) :
        this(getStringFunc, displayFunc) => this.centered = centered;
    public GUIString(string displayString, Func<int, int, Rect> displayFunc) {
        DisplayFunc = displayFunc;
        GetStringFunc = null;
        this.displayString = displayString;
        staticStringCalcOnce = true; }

    public void OnGUI() {
        UpdateString();
        if (displayString is null) return;
        // MiddleLeft doesn't allow fo WidthOffset/2 padding on the left
        if (!centered) GUI.skin.box.alignment = TextAnchor.MiddleLeft;
        GUI.Box(DisplayFunc(width, height), displayString);
        if (!centered) GUI.skin.box.alignment = TextAnchor.MiddleCenter; }

    internal void UpdateString() {
        if (GetStringFunc is not null) {
            if (IsNewString()) UpdateSize(); }
        if (!staticStringCalcOnce) return;
        staticStringCalcOnce = false;
        UpdateSize(); }
    internal bool IsNewString() {
        var newString = GetStringFunc();
        if (newString == displayString) return false;
        displayString = newString;
        return true; }
    internal void UpdateSize() {
        var displayStringSize = GUI.skin.box.CalcSize(new GUIContent(displayString));
        width = Mathf.CeilToInt(displayStringSize.x + WidthOffset);
        height = Mathf.CeilToInt(displayStringSize.y + HeightOffset); }
}
// ReSharper restore InconsistentNaming