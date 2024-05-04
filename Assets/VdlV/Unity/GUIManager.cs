using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using VdlV.Simulator;
using VdlV.Time;
using VdlV.Utilities;
using VdlV.ValueTypes;
using UnityEngine;

namespace VdlV.Unity {
    using static BusinessStatus;
    using static VitalStatus;
    using static Array;
    using static Color;
    using static GraphVisualizer; // hide graphs on table change
    using static GUI;
    using static GUILayout;
    using static Input;
    using static Mathf;
    using static Screen;
    using static SaveManager;
    using static StaticTables; // used in PlaceColor
    using static StringProcessing;
    using static VoixDeLaVille; // used for RuleExecutionTime and PlaceColor
    using static Texture2D;

    // ReSharper disable InconsistentNaming
    /// <summary>OnGUI based layout logic (of tables and strings) as well as control interfaces.</summary>
    public static class GUIManager {
        private const string ShowHideTables = "Show/hide tables?";
        private const int ShowTablesWidth = 121 + LabelBorders;
        private const string ChangeTables = "Change tables?";
        private const int ChangeTablesWidth = 105 + LabelBorders;
        private const int LabelBorders = 10;
        private const int TopMiddleRectHeight = 30; // allows for TopMiddleRectStacks, also for button heights
        private const int DefaultButtonWidth = 100;
        private const int TableSelectorToolbarWidth = 250;
        private const int SelectionGridWidth = 760;
        private const int TileSize = 16;
        private const int TableDisplayNameCutoff = 21;
        private const string REPLTableTitle = "REPL results";
        private const int REPLQueryWidth = 400;
        private const int QueryCenterXOffset = REPLQueryWidth / 2;
        private const int REPLQueryHeight = 70;

        private static readonly Dictionary<Type, Delegate> DefaultColorizerTable = new();
        // ReSharper disable once CollectionNeverUpdated.Global
        public static readonly DictionaryWithDefault<TablePredicate, Dictionary<string, Action>> tableButtons =
            new(_ => new Dictionary<string, Action>());

        private static readonly Dictionary<string, GUITable> Tables = new();
        private static Dictionary<string, string> _displayNameToTableName = new();
        private static string[] _tableDisplayNames;

        private static readonly string[] DisplayTableSelector = { "Table 1", "Table 2", "Table 3", "Table 4" };
        private static string[] _activeTables;  // should also be a string[4]
        private static int _displayTableToChange;
        private static int _tableSelector;
        public static bool ShowREPLTable;
        public static bool PoppedTable;
        public static bool ShowTilemap = true;
        public static bool AlphabetizeTables;
        public static bool ChangeTable;
        private static bool ChangeTableNeedsScroll;
        private static Vector2 ChangeTableScroll;
        private static bool _showTables = true;
        private static readonly Rect ChangeTablesRect = new(0, 0, ChangeTablesWidth, TopMiddleRectHeight);
        private static readonly Rect ShowTablesRect = new(ChangeTablesWidth, 0, ShowTablesWidth, TopMiddleRectHeight);

        public static Func<int, int, int, Rect> GraphBoundRect;
        private static GUITable REPLTable;
        private static Rect REPLRect;
        private static string REPLQuery;
        private static string _previousREPLQuery;

        private static string _saveName;
        public static bool SavingWithName;

        // ********************************** GUITable extensions *********************************

        public static void TableButton(this TablePredicate p, string buttonLabel, Action action) =>
            tableButtons[p][buttonLabel] = action;

        public static void Colorize(this TablePredicate p, Func<uint, Color> colorizer) => p.Property["Colorizer"] = colorizer;
        public static void Colorize<TColumn>(this TablePredicate p, Var<TColumn> column, Func<TColumn, Color> colorizer) => 
            Colorize(p, rowNumber => { 
                var lookup = p.ColumnValueFromRowNumber(column);
                return colorizer(lookup(rowNumber));
            });
        public static void Colorize<TColumn>(this TablePredicate p, Var<TColumn> column) =>
            Colorize(p, column, DefaultColorizer<TColumn>());
        private static Func<T, Color> DefaultColorizer<T>() =>
            (Func<T, Color>)DefaultColorizerTable[typeof(T)];

        private static void SetDefaultColorizer<T>(Func<T, Color> colorizer) => 
            DefaultColorizerTable[typeof(T)] = colorizer;
        public static void SetDefaultColorizers() {
            SetDefaultColorizer<Location>(PlaceColor);
            SetDefaultColorizer<bool>(s => s ? white : gray);
            SetDefaultColorizer<VitalStatus>(s => s == Alive ? white : gray);
            SetDefaultColorizer<BusinessStatus>(s => s == InBusiness ? white : gray);
        }

        public static Color PlaceColor(Location place) => LocationColorsIndex[LocationToType[place]].Item2;

        // *************************************** GUI setup **************************************

        public static void AvailableTables(IEnumerable<TablePredicate> tables) {
            foreach (var table in tables) Tables[table.Name] = new GUITable(table);
            _tableDisplayNames = Tables.Keys.Select(CutoffName).ToArray();
            ChangeTableNeedsScroll = _tableDisplayNames.Length > 150;
            _displayNameToTableName = _tableDisplayNames.Zip(Tables.Keys.ToArray(),
                (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            if (AlphabetizeTables) Sort(_tableDisplayNames);
        }
        public static void ActiveTables(string[] activeTables) => _activeTables = activeTables;

        // Some GUIStrings will always be displayed - these can just go in the list. Others,
        // like pause need to be addressed with external logic so its easiest to keep separate
        private static readonly GUIString Paused = new("Simulation is paused", CenteredRect);
        private static readonly List<GUIString> GuiStrings = new() {
            new GUIString(Clock.DateAndTime, TopRightRect)
        };

        public static void AddSelectedTileInfo(Func<string> tileString) =>
            GuiStrings.Add(new GUIString(tileString, SelectedTileInfoRect, false));
        public static void AddPopulationInfo(Func<string> populationString) =>
            GuiStrings.Add(new GUIString(populationString, BottomRightRect));

        public static void CustomSkins() {
            skin.box.alignment = TextAnchor.MiddleCenter;
            skin.label.fontSize = 14;
            skin.label.padding.top = 1;
            skin.label.padding.bottom = 1;
            skin.label.fixedHeight = 0;
        }
        public static void InitAllTables() { foreach (var table in Tables.Values) table.Initialize(); }

        // ************************************** GUI control *************************************

        public static void ToggleShowTables() => _showTables = !_showTables;
        public static void ToggleREPLTable() {
            if (ShowREPLTable) {
                ShowREPLTable = false;
                ShowTilemap = true;
            } else {
                ShowREPLTable = true;
                ChangeTable = false;
                Current.Clear();
                ShowTilemap = false;
            }
        }

        public static void ShowPaused() {
            if (SavingWithName) return;
            if (ShowREPLTable) return;
            if (ChangeTable && _showTables) return;
            Paused.OnGUI();
        }

        public static void ShowStrings() { foreach (var guiString in GuiStrings) guiString.OnGUI(); }

        public static void ShowActiveTables() {
            if (!_showTables) return;
            for (var i = 0; i < _activeTables.Length; i++) {
                if (_activeTables[i] != "") Tables[_activeTables[i]].OnGUI(i);
            }
        }
        public static void ChangeActiveTables() {
            // Change and show/hide toggles:
            BeginGroup(TopMiddleRectStack(ChangeTablesWidth + ShowTablesWidth));
            ChangeTable = Toggle(ChangeTablesRect, ChangeTable, ChangeTables);
            _showTables = Toggle(ShowTablesRect, _showTables, ShowHideTables);
            EndGroup();
            if (!ChangeTable || !_showTables) return;
            Current.Clear();
            // If we are trying to change tables:
            _displayTableToChange = Toolbar(TopMiddleRectStack(TableSelectorToolbarWidth, 2),
                                     _displayTableToChange, DisplayTableSelector);
            _tableSelector = IndexOf(Tables.Keys.ToArray(), _activeTables[_displayTableToChange]);
            // Build the selection grid:
            var selectionGridRect = SelectionGridRect();
            if (ChangeTableNeedsScroll) {
                ChangeTableScroll = BeginScrollView(CappedSelectionGridRect(), ChangeTableScroll, selectionGridRect, false, true);
                _tableSelector = SelectionGrid(selectionGridRect, _tableSelector, _tableDisplayNames, 5);
                GUI.EndScrollView(); // Conflicts with GUILayout...
            } else {
                BeginArea(selectionGridRect);
                _tableSelector = SelectionGrid(new Rect(selectionGridRect) {x = 0, y = 0}, _tableSelector, _tableDisplayNames, 5);
                EndArea(); 
            }
            // update the active table to change with the selected table
        }
        
        public static void ShowREPL() {
            if (!ShowREPLTable) return;
            if (REPLTable == null) REPLRect = GraphBoundRect(REPLQueryWidth, REPLQueryHeight, 0);
            else {
                var table = GraphBoundRect((int)REPLTable.TableWidth, REPLTable.REPLHeight, REPLQueryHeight);
                REPLTable.OnGUI(-1, table);
                REPLRect = new Rect(table.center.x - QueryCenterXOffset, table.yMax, REPLQueryWidth, REPLQueryHeight);
            }
            BeginArea(REPLRect);
            REPLArea();
            EndArea();
        }
        private static void REPLArea() {
            REPLQuery = TextArea(REPLQuery, MaxHeight(70));
            if(_previousREPLQuery == REPLQuery) return;
            _previousREPLQuery = REPLQuery;
            try {
                var query = Simulation.Repl.Query(REPLTableTitle, REPLQuery);
                query.ForceRecompute();
                REPLTable = new GUITable(query, 25);
                REPLTable.Initialize();
            } catch (Exception e) {
                Debug.LogException(e);
                REPLTable = null;
            }
        }
        
        // TODO: Fix text area captures ESC keypress
        public static void SaveNameText() {
            if (!SavingWithName) return;
            BeginArea(CenteredRect(300, 40));
            BeginArea(new Rect(0, 0, 240, 40));
            _saveName = TextArea(_saveName);
            EndArea();
            BeginArea(new Rect(245, 0, 55, 40));
            if (Button("Save", Width(50))) {
                Save(_saveName, Simulation);
                SavingWithName = false;
            }
            EndArea();
            EndArea();
        }

        public static bool IsMouseInGUIs() {
            var mousePositionForRects = new Vector2(mousePosition.x, height - mousePosition.y);
            return _activeTables.Where((table, i) => table != "" && Tables[table].LeftSideTables(i).Contains(mousePositionForRects)).Any() ||
                   GuiStrings.Any(guiString => guiString.GUIStringRect().Contains(mousePositionForRects)) ||
                   (ShowREPLTable && REPLRect.Contains(mousePositionForRects)) ||
                   TopMiddleRectStack(ChangeTablesWidth + ShowTablesWidth).Contains(mousePositionForRects) ||
                   (ChangeTable && _showTables && TopMiddleRectStack(TableSelectorToolbarWidth, 2).Contains(mousePositionForRects)) ||
                   (ChangeTable && _showTables && SelectionGridRect().Contains(mousePositionForRects)) ||
                   (GraphVisible && DataFlowButton(true).Contains(mousePositionForRects)) ||
                   (GraphVisible && DataFlowButton(false).Contains(mousePositionForRects)) ||
                   (!GraphVisible && RemoveGraphButton().Contains(mousePositionForRects)) ||
                   // TODO: Store the save name area sizes somewhere...
                   (SavingWithName && CenteredRect(300, 40).Contains(mousePositionForRects));
        }

        private static void PopTableIfNewActivity(string tableName) {
            if (!Tables.TryGetValue(tableName, out var table)) return;
            if (!table.NewActivity) return;
            _activeTables[0] = tableName;
            PoppedTable = true;
        }
        public static void PopTableIfNewActivity(TablePredicate p) => PopTableIfNewActivity(p.Name);

        public static void RuleExecutionTimes() => Label(CenteredRect(480, 350), 
            string.Concat((from table in Simulation.Tables 
                           where table.RuleExecutionTime > 0 && !table.Name.Contains("initially")
                           orderby -table.RuleExecutionTime 
                           select $"{table.Name} {table.RuleExecutionTime}\n").Take(20)));

        // ************************************ Display helpers ***********************************

        private static string CutoffName(string name) =>
            TableShortName(name, TableDisplayNameCutoff);

        private static Rect TopMiddleRectStack(int width, int num = 1) => TopMiddleRectStack(width, TopMiddleRectHeight, num);
        // ReSharper disable PossibleLossOfFraction
        /// <remarks>height should always be a multiple of TopMiddleRectHeight (unless its the final row)</remarks>
        private static Rect TopMiddleRectStack(int width, int height, int num) =>
            new(Screen.width / 2 - width / 2, (LabelBorders * num) + (TopMiddleRectHeight * (num - 1)), width, height);
        
        private static Rect CenteredRect(int width, int height) =>
            new(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
        private static Rect TopRightRect(int width, int height) =>
            new(Screen.width - (width + LabelBorders), LabelBorders, width, height);

        private static Rect BottomMiddleRect(int width, int height) =>
            new(Screen.width / 2 - width / 2, Screen.height - (height + LabelBorders), width, height);
        public static Rect RemoveGraphButton() =>
            BottomMiddleRect(DefaultButtonWidth, TopMiddleRectHeight);

        private static Rect BottomMiddleSplit(int width, int height, bool right) =>
            new(Screen.width / 2 - (width + 4) + (right ? 0 : width + 8), Screen.height - (height + LabelBorders), width, height);
        public static Rect DataFlowButton(bool right) =>
            BottomMiddleSplit(DefaultButtonWidth, TopMiddleRectHeight, right);
        // ReSharper restore PossibleLossOfFraction

        private static Rect BottomRightRect(int width, int height) =>
            new(Screen.width - (width + LabelBorders), Screen.height - (height + LabelBorders), width, height);

        private static Rect SelectedTileInfoRect(int width, int height) => // Rect following mouse position
            new(mousePosition.x + TileSize, (Screen.height - mousePosition.y) + TileSize, width, height);

        private static Rect SelectionGridRect() => 
            TopMiddleRectStack(SelectionGridWidth, CeilToInt(_tableDisplayNames.Length / 5f) * TopMiddleRectHeight, 3);

        private static Rect CappedSelectionGridRect() => 
            TopMiddleRectStack(SelectionGridWidth + GUITable.ScrollbarOffset, 30 * TopMiddleRectHeight, 3);

        private static void BoldLabel(string label, params GUILayoutOption[] options) {
            skin.label.fontStyle = FontStyle.Bold;
            Label(label, options);
            skin.label.fontStyle = FontStyle.Normal;
        }
        private static void ButtonLabel(string label, out bool pressed, params GUILayoutOption[] options) {
            skin.label.fontStyle = FontStyle.BoldAndItalic;
            pressed = Button(label, skin.label, options);
            skin.label.fontStyle = FontStyle.Normal;
        }
        public static bool HeaderButton(string label, params GUILayoutOption[] options) {
            skin.label.fontStyle = FontStyle.Bold;
            skin.label.normal.background = grayTexture;
            var pressed = Button(label, skin.label, options);
            skin.label.normal.background = blackTexture;
            skin.label.fontStyle = FontStyle.Normal;
            return pressed;
        }

        private static void TableTitleToggle(int tableNum) {
            ChangeTable = _displayTableToChange != tableNum || !ChangeTable;
            _displayTableToChange = tableNum;
        }
        public static void TableTitle(int tableNum, string title) {
            if (tableNum >= 0) { // -1 for just a Bold Label (non left side tables)
                ButtonLabel(title, out var pressed);
                // Left side tables have an interface to the table change logic
                if (pressed) TableTitleToggle(tableNum);
            } else BoldLabel(REPLTableTitle);
        }
    }
}
