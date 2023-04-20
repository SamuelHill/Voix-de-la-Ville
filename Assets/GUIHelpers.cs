using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using UnityEngine;

// ReSharper disable InconsistentNaming
public class GUIManager {
    internal Dictionary<string, GUITable> Tables = new();
    internal string[] AvailableTableNames;
    internal GUIContent[] GUITableNames;
    internal string[] ActiveTables;  // should also be a string[4]
    internal string[] TableSelectorToolbar = { "Table 1", "Table 2", "Table 3", "Table 4" };
    internal int TableToChange;
    internal bool ShowTableToggle = true;
    internal int ChangeTableSelector;
    internal bool ChangeTableToggle;
    internal const string ShowTables = "Show/hide tables?";
    internal const int ShowTablesWidth = 121 + LabelBorders;
    internal const string ChangeTables = "Change tables?";
    internal const int ChangeTablesWidth = 105 + LabelBorders;
    internal const int LabelBorders = 10;
    internal const int TopMiddleRectHeight = 30; // allows for TopMiddleRectStacks
    internal const int TableToolbarWidth = 250;
    internal const int MaxToolbarSize = 600;
    // set with a call to GetTableToolbarSize, only called once in the OnGUI function via SetupGUI
    internal int TableToolbarSize;
    internal bool UseSelectionGrid => TableToolbarSize > MaxToolbarSize;

    internal GUIString paused = new("Simulation is paused", CenteredRect);
    internal GUIString time = new(TalkOfTheTown.Time.ToString, TopRightRect);
    internal GUIString tileInfo;

    static GUIManager() {}
    private GUIManager() {}
    public static GUIManager Manager { get; } = new();

    // Called once each in Start 
    public void SetAvailableTables(List<TablePredicate> tables) {
        foreach (var table in tables) Tables[table.Name] = new GUITable(table);
        AvailableTableNames = Tables.Keys.ToArray();
        GUITableNames = (from available in AvailableTableNames 
                         select new GUIContent(available)).ToArray(); }
    public void SetActiveTables(string[] activeTables) => ActiveTables = activeTables;
    public void AddSelectedTileInfo(Func<string> tileString) => tileInfo = new GUIString(tileString, BottomMiddleRect);

    // Called once each in SetupGUI
    public void GetTableToolbarSize() => TableToolbarSize = 
        (from content in GUITableNames select (int)GUI.skin.button.CalcSize(content).x).Sum();
    public void InitAllTables() {
        foreach (var table in Tables.Values) {
            table.InitializeLongestStrings();
            table.Update(); }
    }

    // Called in OnGUI
    public void ShowActiveTables() {
        if (!ShowTableToggle) return;
        for (var i = 0; i < ActiveTables.Length; i++) Tables[ActiveTables[i]].OnGUI(i); }
    public void SelectActiveTables() {
        ToggleGroup();
        if (!ChangeTableToggle || !ShowTableToggle) return;
        TableToChangeToolbar();
        ChangeTableSelector = Array.IndexOf(AvailableTableNames, ActiveTables[TableToChange]);
        SelectionGridOrToolbar();
        ActiveTables[TableToChange] = AvailableTableNames[ChangeTableSelector]; }
    public void ShowPaused() => paused.OnGUI();
    public void ShowTime() => time.OnGUI();
    public void ShowTileInfo() => tileInfo.OnGUI();

    // Display helpers
    internal void ToggleGroup() {
        GUI.BeginGroup(TopMiddleRectStack(ChangeTablesWidth + ShowTablesWidth));
        ChangeTableToggle = GUI.Toggle(new Rect(0, 0, ChangeTablesWidth, TopMiddleRectHeight),
            ChangeTableToggle, ChangeTables);
        ShowTableToggle = GUI.Toggle(new Rect(ChangeTablesWidth, 0, ShowTablesWidth, TopMiddleRectHeight),
            ShowTableToggle, ShowTables);
        GUI.EndGroup(); }
    internal void TableToChangeToolbar() =>
        TableToChange = GUI.Toolbar(TopMiddleRectStack(TableToolbarWidth, 2), 
            TableToChange, TableSelectorToolbar);
    internal void SelectionGridOrToolbar() {
        if (UseSelectionGrid) {
            var selectionGridHeight = Mathf.CeilToInt(AvailableTableNames.Length / 5f) * TopMiddleRectHeight;
            var selectionGridRect = TopMiddleRectStack(MaxToolbarSize, selectionGridHeight, 3);
            ChangeTableSelector = GUI.SelectionGrid(selectionGridRect, ChangeTableSelector, AvailableTableNames, 5);
        } else ChangeTableSelector = GUI.Toolbar(TopMiddleRectStack(TableToolbarSize, 3),
            ChangeTableSelector, GUITableNames, GUI.skin.button, GUI.ToolbarButtonSize.FitToContents); }

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
}

public class GUITable {
    internal TablePredicate predicate;
    internal string[] Headings => predicate.ColumnHeadings;
    internal int[] longestStrings;
    internal int NumColumns => Headings.Length;
    internal uint RowCount => predicate.Length;
    internal uint previousRowCount;
    internal int numRowsToDisplay = (TableHeight / LabelHeight) - 2;  // 9, but ~flexible~
    internal string[][] buffer;
    internal uint bufferedRows;
    internal float scrollPosition;
    internal float oldScroll;
    internal bool Scrolled => Math.Abs(scrollPosition - oldScroll) > 0.001f;
    internal uint ScrollRow => (uint)Math.Floor(scrollPosition);
    #region Table display constants
    internal const int ColumnPadding = 5;
    internal const int LabelHeight = 21; // calculated once via:
    // (int)GUI.skin.label.CalcSize(new GUIContent("Any string here will do")).y
    internal const int TableHeight = 250;
    internal const int TablePadding = 15;
    internal const int TableWidthOffset = 50;
    #endregion

    public GUITable(TablePredicate predicate) {
        this.predicate = predicate;
        BuildBuffer();
        longestStrings = new int[NumColumns]; }

    public void RebuildBuffer(int newNumRows) {
        numRowsToDisplay = newNumRows;
        BuildBuffer(); }
    internal void BuildBuffer() {
        buffer = new string[numRowsToDisplay][];
        for (var i = 0; i < numRowsToDisplay; i++) 
            buffer[i] = new string[NumColumns]; }

    public void Update() => Update(ScrollRow);
    internal void Update(uint startRow) {
        bufferedRows = predicate.RowRangeToStrings(startRow, buffer);
        for (var i = 0; i < bufferedRows; i++) UpdateLongestStrings(buffer[i]); }
    internal void UpdateLongestStrings(string[] strings) {
        for (var i = 0; i < NumColumns; i++) {
            var stringLength = (int)GUI.skin.label.CalcSize(new GUIContent(strings[i])).x;
            if (longestStrings[i] < stringLength) longestStrings[i] = stringLength; }
    }

    public void InitializeLongestStrings() => UpdateLongestStrings(Headings);

    internal Rect PaddedTableRect(int x, int y) => new(x + TablePadding, y + TablePadding,
        longestStrings.Sum() + longestStrings.Length * ColumnPadding + TablePadding + TableWidthOffset, TableHeight);
    internal Rect LeftSideTables(int tableNum) => PaddedTableRect(tableNum, tableNum * (TableHeight + TablePadding));

    internal void LayoutRow(string[] strings) {
        try {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < NumColumns; i++) GUILayout.Label(strings[i],
                GUILayout.Width(longestStrings[i] + ColumnPadding));
            GUILayout.EndHorizontal(); }
        catch (ArgumentException e) {
            Debug.Log($"'{e.Message}' on strings {string.Join(", ", strings)}"); }
    }

    public void OnGUI(int tableNum) {
        GUILayout.BeginArea(LeftSideTables(tableNum));
        LayoutRow(Headings);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        for (var i = 0; i < bufferedRows; i++) LayoutRow(buffer[i]);
        GUILayout.EndVertical();
        GUILayout.Space(TablePadding);
        if (RowCount >= numRowsToDisplay) {
            scrollPosition = GUILayout.VerticalScrollbar(scrollPosition,
                numRowsToDisplay - 0.1f, 0f, RowCount,
                GUILayout.Height(TableHeight - (2 * LabelHeight)));
            if (Scrolled) {
                Update();
                oldScroll = scrollPosition; }
            // Get the final update based on table growth when moving to the scroll regime
            else if (previousRowCount < numRowsToDisplay && RowCount != previousRowCount) {
                Update();
                previousRowCount = RowCount; }
        }
        // This bit throws argument errors for changing the values before the scrollBar appears...
        else if (RowCount != previousRowCount) {
            Update();
            previousRowCount = RowCount; }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}

public class GUIString {
    internal string displayString;
    internal Func<string> GetStringFunc { get; }
    internal Func<int, int, Rect> DisplayFunc { get; }
    internal const int LabelWidthOffset = 26;
    internal const int LabelHeightOffset = 6;
    internal bool sizeCalcForStaticStrings;
    internal Vector2 displayStringSize;
    internal int width;
    internal int height;

    public GUIString(Func<string> getStringFunc, Func<int, int, Rect> displayFunc) {
        DisplayFunc = displayFunc;
        GetStringFunc = getStringFunc; }
    public GUIString(string displayString, Func<int, int, Rect> displayFunc) {
        DisplayFunc = displayFunc;
        GetStringFunc = null;
        this.displayString = displayString;
        sizeCalcForStaticStrings = true; }

    public void OnGUI() {
        UpdateString();
        GUI.Box(DisplayFunc(width, height), displayString); }

    internal void UpdateString() {
        if (GetStringFunc is not null) {
            if (IsNewString()) UpdateSize(); }
        if (!sizeCalcForStaticStrings) return;
        sizeCalcForStaticStrings = false;
        UpdateSize(); }
    internal bool IsNewString() {
        var newString = GetStringFunc();
        if (newString == displayString) return false;
        displayString = newString;
        return true; }
    internal void UpdateSize() {
        displayStringSize = GUI.skin.box.CalcSize(new GUIContent(displayString));
        width = Mathf.CeilToInt(displayStringSize.x + LabelWidthOffset);
        height = Mathf.CeilToInt(displayStringSize.y + LabelHeightOffset); }
}
// ReSharper restore InconsistentNaming