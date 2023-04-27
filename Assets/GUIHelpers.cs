using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using UnityEngine;
using static UnityEngine.Input;

// ReSharper disable InconsistentNaming
public static class GUIManager {
    internal static Dictionary<string, GUITable> Tables = new();
    internal static string[] TableNames;
    internal static GUIContent[] GUITableNames;
    internal static string[] ActiveTables;  // should also be a string[4]
    internal static string[] TableSelector = { "Table 1", "Table 2", "Table 3", "Table 4" };
    internal static int TableToChange;
    internal static bool ShowTables = true;
    internal static int ChangeTableSelector;
    internal static bool ChangeTable;
    // set with a call to GetTableToolbarSize, only called once in the OnGUI function via SetupGUI
    internal static int TableToolbarSize;

    #region Display Constants
    internal const string ShowHideTables = "Show/hide tables?";
    internal const int ShowTablesWidth = 121 + LabelBorders;
    internal const string ChangeTables = "Change tables?";
    internal const int ChangeTablesWidth = 105 + LabelBorders;
    internal const int LabelBorders = 10;
    internal const int TopMiddleRectHeight = 30; // allows for TopMiddleRectStacks
    internal const int TableToolbarWidth = 250;
    internal const int MaxToolbarSize = 600;
    internal const int TileSize = 16;
    #endregion

    // Some GUIStrings will always be displayed - these can just go in the list. Others,
    // like pause need to be addressed with external logic so its easiest to keep separate
    internal static GUIString paused = new("Simulation is paused", CenteredRect);
    internal static List<GUIString> guiStrings = new() {
        new GUIString(TalkOfTheTown.Time.ToString, TopRightRect), };

    // Called once each in Start 
    public static void SetAvailableTables(List<TablePredicate> tables) {
        foreach (var table in tables) Tables[table.Name] = new GUITable(table);
        TableNames = Tables.Keys.ToArray();
        GUITableNames = (from available in TableNames 
                         select new GUIContent(available)).ToArray(); }
    public static void SetActiveTables(string[] activeTables) => ActiveTables = activeTables;
    public static void AddSelectedTileInfo(Func<string> tileString) =>
        guiStrings.Add(new GUIString(tileString, SelectedTileInfoRect));
    public static void AddPopulationInfo(Func<string> populationString) =>
        guiStrings.Add(new GUIString(populationString, BottomRightRect));

    // Called once each in SetupGUI
    public static void GetTableToolbarSize() => TableToolbarSize = 
        (from content in GUITableNames select (int)GUI.skin.button.CalcSize(content).x).Sum();
    public static void InitAllTables() { foreach (var table in Tables.Values) table.Initialize(); }

    // Called in OnGUI
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
        ChangeTableSelector = Array.IndexOf(TableNames, ActiveTables[TableToChange]);
        // depending on the length of all table names either use a tool ar or a selection grid
        if (TableToolbarSize > MaxToolbarSize) {
            var selectionGridHeight = Mathf.CeilToInt(TableNames.Length / 5f) * TopMiddleRectHeight;
            var selectionGridRect = TopMiddleRectStack(MaxToolbarSize, selectionGridHeight, 3);
            ChangeTableSelector = GUI.SelectionGrid(selectionGridRect, ChangeTableSelector, TableNames, 5);
        } else ChangeTableSelector = GUI.Toolbar(TopMiddleRectStack(TableToolbarSize, 3),
            ChangeTableSelector, GUITableNames, GUI.skin.button, GUI.ToolbarButtonSize.FitToContents);
        // update the active table to change with the selected table
        ActiveTables[TableToChange] = TableNames[ChangeTableSelector]; }

    public static void RuleExecutionTimes() => GUI.Label(CenteredRect(400, 400), 
        string.Concat(from table in TalkOfTheTown.Simulation.Tables
                           where table.RuleExecutionTime > 0
                           orderby -table.RuleExecutionTime
                           select $"{table.Name} {table.RuleExecutionTime}\n"));

    #region Rects
    internal static Rect ChangeTablesRect = new(0, 0, ChangeTablesWidth, TopMiddleRectHeight);
    internal static Rect ShowTablesRect = new(ChangeTablesWidth, 0, ShowTablesWidth, TopMiddleRectHeight);
    internal static Rect TopMiddleRectStack(int width, int num = 1) => TopMiddleRectStack(width, TopMiddleRectHeight, num);
    // height should always be a multiple of TopMiddleRectHeight (unless its the final row)
    internal static Rect TopMiddleRectStack(int width, int height, int num) =>
        new(Screen.width / 2 - width / 2, (LabelBorders * num) + (TopMiddleRectHeight * (num - 1)), width, height);

    internal static Rect CenteredRect(int width, int height) =>
        new(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
    internal static Rect TopRightRect(int width, int height) =>
        new(Screen.width - (width + LabelBorders), LabelBorders, width, height);
    internal static Rect BottomMiddleRect(int width, int height) =>
        new(Screen.width / 2 - width / 2, Screen.height - (height + LabelBorders), width, height);
    internal static Rect BottomRightRect(int width, int height) => 
        new(Screen.width - (width + LabelBorders), Screen.height - (height + LabelBorders), width, height);

    internal static Rect SelectedTileInfoRect(int width, int height) =>
        new(mousePosition.x + TileSize, (Screen.height - mousePosition.y) + TileSize, width, height);
    #endregion
}

public class GUITable {
    #region Table display constants
    internal const int ColumnPadding = 5;
    internal const int LabelHeight = 21; // calculated once via:
    // (int)GUI.skin.label.CalcSize(new GUIContent("Any string here will do")).y
    internal const int TableHeight = 255;
    internal const int TablePadding = 15;
    internal const int TableWidthOffset = 50;
    internal const int numRowsToDisplay = (TableHeight - TablePadding) / LabelHeight - 2;  // 9, but ~flexible~
    #endregion

    internal TablePredicate predicate;
    internal string[][] buffer;
    internal string[] headings;
    internal int[] longestStrings;
    internal GUIContent noEntries;
    internal int noEntriesWidth;
    internal uint bufferedRows;
    internal uint previousRowCount;
    internal bool usingScroll;
    internal float scrollPosition;
    internal float oldScroll;

    internal string Name => predicate.Name;
    internal int NumColumns => headings.Length;
    internal uint RowCount => predicate.Length;
    internal bool Scrolled => Math.Abs(scrollPosition - oldScroll) > 0.0001f;
    internal uint ScrollRow => (uint)Math.Floor(scrollPosition);
    internal int LongestRow => longestStrings.Sum() + longestStrings.Length * ColumnPadding;
    internal int TableWidth => RowCount == 0 ? noEntriesWidth : LongestRow;

    public GUITable(TablePredicate predicate) {
        this.predicate = predicate;
        headings = (from heading in predicate.ColumnHeadings 
                    select Utils.Heading(heading)).ToArray();
        buffer = new string[numRowsToDisplay][];
        for (var i = 0; i < numRowsToDisplay; i++)
            buffer[i] = new string[NumColumns];
        longestStrings = new int[NumColumns]; }

    public void Initialize() {
        UpdateLongestStrings(headings);
        if (RowCount == 0 || predicate.IsIntensional) {
            noEntries = new GUIContent($"No entries in table {Name}");
            noEntriesWidth = (int)GUI.skin.label.CalcSize(noEntries).x;
            noEntriesWidth = Math.Max(noEntriesWidth, LongestRow); }
        Update(); }
    public void Update() {
        bufferedRows = predicate.RowRangeToStrings(ScrollRow, buffer);
        for (var i = 0; i < bufferedRows; i++) UpdateLongestStrings(buffer[i]); }
    internal void UpdateLongestStrings(string[] strings) {
        for (var i = 0; i < NumColumns; i++) {
            var stringLength = (int)GUI.skin.label.CalcSize(new GUIContent(strings[i])).x;
            if (longestStrings[i] < stringLength) longestStrings[i] = stringLength; } }
    internal bool UpdateRowCount() {
        if (previousRowCount == RowCount) return false;
        previousRowCount = RowCount;
        return true; }

    internal Rect PaddedTableRect(int x, int y) => new(x + TablePadding, y + TablePadding,
        TableWidth + TablePadding + TableWidthOffset, TableHeight);
    internal Rect LeftSideTables(int tableNum) => PaddedTableRect(tableNum, tableNum * (TableHeight + TablePadding));

    internal GUILayoutOption ScrollHeight = GUILayout.Height((numRowsToDisplay + 1) * LabelHeight);
    internal GUILayoutOption ColumnWidth(int i) => GUILayout.Width(longestStrings[i] + ColumnPadding);

    internal void LayoutRow(string[] strings, bool header = false) {
        if (header) GUI.skin.label.fontStyle = FontStyle.Bold;
        GUILayout.BeginHorizontal();
        for (var i = 0; i < NumColumns; i++)
            GUILayout.Label(strings[i], ColumnWidth(i));
        GUILayout.EndHorizontal();
        if (header) GUI.skin.label.fontStyle = FontStyle.Normal; }

    public void OnGUI(int tableNum) {
        GUILayout.BeginArea(LeftSideTables(tableNum));
        LayoutRow(headings, true);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if (RowCount == 0) GUILayout.Label($"No entries in table {Name}");
        else { foreach (var row in buffer) LayoutRow(row); }
        GUILayout.EndVertical();
        GUILayout.Space(TablePadding);
        if (RowCount != 0 && RowCount >= numRowsToDisplay) {
            scrollPosition = GUILayout.VerticalScrollbar(scrollPosition,
                numRowsToDisplay - 0.1f, 0f, RowCount, ScrollHeight);
            if (Scrolled || !usingScroll) {
                Update();
                oldScroll = scrollPosition; }
            if (!usingScroll) usingScroll = true; }
        else if (!usingScroll && UpdateRowCount()) Update();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}

public class GUIString {
    internal const int LabelWidthOffset = 26;
    internal const int LabelHeightOffset = 6;

    internal string displayString;
    internal Func<string> GetStringFunc;
    internal bool staticStringCalcOnce;
    internal int width;
    internal int height;
    internal Func<int, int, Rect> DisplayFunc;

    public GUIString(Func<string> getStringFunc, Func<int, int, Rect> displayFunc) {
        DisplayFunc = displayFunc;
        GetStringFunc = getStringFunc; }
    public GUIString(string displayString, Func<int, int, Rect> displayFunc) {
        DisplayFunc = displayFunc;
        GetStringFunc = null;
        this.displayString = displayString;
        staticStringCalcOnce = true; }

    public void OnGUI() {
        UpdateString();
        if (displayString is not null) GUI.Box(DisplayFunc(width, height), displayString); }

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
        width = Mathf.CeilToInt(displayStringSize.x + LabelWidthOffset);
        height = Mathf.CeilToInt(displayStringSize.y + LabelHeightOffset); }
}
// ReSharper restore InconsistentNaming