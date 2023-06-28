using System;
using System.Collections.Generic;
using System.Linq;
using GraphVisualization;
using TED;
using TotT.Simulator;
using TotT.Utilities;
using UnityEngine;
using static UnityEngine.Input;

namespace TotT.Unity {
    using static StringProcessing;

    // ReSharper disable once InconsistentNaming
    /// <summary>OnGUI based layout logic (of tables and strings) as well as control interfaces.</summary>
    public static class GUIManager {
        private const string ShowHideTables = "Show/hide tables?";
        private const int ShowTablesWidth = 121 + LabelBorders;
        private const string ChangeTables = "Change tables?";
        private const int ChangeTablesWidth = 105 + LabelBorders;
        private const int LabelBorders = 10;
        private const int TopMiddleRectHeight = 30; // allows for TopMiddleRectStacks
        private const int TableSelectorToolbarWidth = 250;
        private const int SelectionGridWidth = 760;
        private const int TileSize = 16;
        private const int TableDisplayNameCutoff = 20;

        private static readonly Dictionary<string, GUITable> Tables = new();
        private static Dictionary<string, string> _displayNameToTableName = new();
        private static string[] _tableDisplayNames;

        private static readonly string[] TableSelector = { "Table 1", "Table 2", "Table 3", "Table 4" };
        private static string[] _activeTables;  // should also be a string[4]
        private static int _tableToChange;
        private static int _changeTableSelector;
        public static bool ChangeTable;
        private static bool _showTables = true;
        private static readonly Rect ChangeTablesRect = new(0, 0, ChangeTablesWidth, TopMiddleRectHeight);
        private static readonly Rect ShowTablesRect = new(ChangeTablesWidth, 0, ShowTablesWidth, TopMiddleRectHeight);

        // ReSharper disable once CollectionNeverUpdated.Global
        public static readonly DictionaryWithDefault<TablePredicate, Dictionary<string, Action>> tableButtons =
            new(_ => new Dictionary<string, Action>());
        public static void Button(this TablePredicate p, string buttonLabel, Action action) =>
            tableButtons[p][buttonLabel] = action;

        public static void Colorize(this TablePredicate p, Func<uint, Color> colorizer) =>
            p.Property["Colorizer"] = colorizer;
        public static void Colorize<TColumn>(this TablePredicate p, Var<TColumn> column, Func<TColumn, Color> colorizer)
            => Colorize(p, rowNumber => {
                var lookup = p.ColumnValueFromRowNumber(column);
                return colorizer(lookup(rowNumber));
            });
        public static void Colorize<TColumn>(this TablePredicate p, Var<TColumn> column) =>
            Colorize(p, column, DefaultColorizer<TColumn>());

        private static readonly Dictionary<Type, Delegate> DefaultColorizerTable = new();

        public static void SetDefaultColorizer<T>(Func<T,Color> colorizer) => DefaultColorizerTable[typeof(T)] = colorizer;
        private static Func<T, Color> DefaultColorizer<T>() => (Func<T, Color>)DefaultColorizerTable[typeof(T)];

        // *************************************** GUI setup **************************************

        public static void AvailableTables(IEnumerable<TablePredicate> tables) {
            foreach (var table in tables) Tables[table.Name] = new GUITable(table);
            _tableDisplayNames = Tables.Keys.Select(CutoffName).ToArray();
            _displayNameToTableName = _tableDisplayNames.Zip(Tables.Keys.ToArray(),
                (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }
        public static void ActiveTables(string[] activeTables) => _activeTables = activeTables;

        // Some GUIStrings will always be displayed - these can just go in the list. Others,
        // like pause need to be addressed with external logic so its easiest to keep separate
        private static readonly GUIString Paused = new("Simulation is paused", CenteredRect);
        private static readonly List<GUIString> GuiStrings = new() {
            new GUIString(TalkOfTheTown.Time.ToString, TopRightRect) };

        public static void AddSelectedTileInfo(Func<string> tileString) =>
            GuiStrings.Add(new GUIString(tileString, SelectedTileInfoRect, false));
        public static void AddPopulationInfo(Func<string> populationString) =>
            GuiStrings.Add(new GUIString(populationString, BottomRightRect));

        public static void CustomSkins() {
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = 14;
            GUI.skin.label.padding.top = 1;
            GUI.skin.label.padding.bottom = 1;
            GUI.skin.label.fixedHeight = 0; }
        public static void InitAllTables() {
            foreach (var table in Tables.Values) table.Initialize();
        }

        // ************************************** GUI control *************************************

        public static void ToggleShowTables() => _showTables = !_showTables;
        public static void ShowPaused() => Paused.OnGUI();
        public static void ShowStrings() {
            foreach (var guiString in GuiStrings) guiString.OnGUI();
        }
        public static void ShowActiveTables() {
            if (!_showTables) return;
            for (var i = 0; i < _activeTables.Length; i++) Tables[_activeTables[i]].OnGUI(i);
        }
        public static void ChangeActiveTables() {
            // Change and show/hide toggles:
            GUI.BeginGroup(TopMiddleRectStack(ChangeTablesWidth + ShowTablesWidth));
            ChangeTable = GUI.Toggle(ChangeTablesRect, ChangeTable, ChangeTables);
            _showTables = GUI.Toggle(ShowTablesRect, _showTables, ShowHideTables);
            GUI.EndGroup();
            if (!ChangeTable || !_showTables) return;
            TEDGraphVisualization.Current.Clear();  
            // If we are trying to change tables:
            _tableToChange = GUI.Toolbar(TopMiddleRectStack(TableSelectorToolbarWidth, 2), _tableToChange, TableSelector);
            _changeTableSelector = Array.IndexOf(Tables.Keys.ToArray(), _activeTables[_tableToChange]);
            // Build the selection grid:
            var selectionGridHeight = Mathf.CeilToInt(_tableDisplayNames.Length / 5f) * TopMiddleRectHeight;
            var selectionGridRect = TopMiddleRectStack(SelectionGridWidth, selectionGridHeight, 3);
            _changeTableSelector = GUI.SelectionGrid(selectionGridRect, _changeTableSelector, _tableDisplayNames, 5);
            // update the active table to change with the selected table
            _activeTables[_tableToChange] = _displayNameToTableName[_tableDisplayNames[_changeTableSelector]];
        }

        private static void PopTable(TablePredicate p) => _activeTables[0] = p.Name; // Fill this in with something smarter
        private static readonly Dictionary<TablePredicate, uint> PreviousLengths = new();
        public static void PopTableIfNewActivity(TablePredicate p) {
            if (!PreviousLengths.TryGetValue(p, out var length)) 
                PreviousLengths[p] = length = p.Length;
            if (p.Length <= length) return;
            PopTable(p);
            PreviousLengths[p] = p.Length;
        }

        public static void RuleExecutionTimes() => GUI.Label(CenteredRect(480, 350),
            string.Concat((from table in TalkOfTheTown.Simulation.Tables
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
        public static Rect BottomMiddleRect(int width, int height) =>
            new(Screen.width / 2 - width / 2, Screen.height - (height + LabelBorders), width, height);
        // ReSharper restore PossibleLossOfFraction
        private static Rect BottomRightRect(int width, int height) =>
            new(Screen.width - (width + LabelBorders), Screen.height - (height + LabelBorders), width, height);

        private static Rect SelectedTileInfoRect(int width, int height) => // Rect following mouse position
            new(mousePosition.x + TileSize, (Screen.height - mousePosition.y) + TileSize, width, height);

        private static void BoldLabel(string label, params GUILayoutOption[] options) {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.normal.background = Texture2D.grayTexture;
            GUILayout.Label(label, options);
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.normal.background = Texture2D.blackTexture;
        }
        private static void ButtonLabel(string label, out bool pressed, params GUILayoutOption[] options) {
            GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
            pressed = GUILayout.Button(label, GUI.skin.label, options);
            GUI.skin.label.fontStyle = FontStyle.Normal;
        }
        public static bool HeaderButton(string label, params GUILayoutOption[] options) {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.normal.background = Texture2D.grayTexture;
            var pressed = GUILayout.Button(label, GUI.skin.label, options);
            GUI.skin.label.normal.background = Texture2D.blackTexture;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            return pressed;
        }

        private static void TableTitleToggle(int tableNum) {
            ChangeTable = _tableToChange != tableNum || !ChangeTable;
            _tableToChange = tableNum;
        }
        public static void TableTitle(int tableNum, string title) {
            if (tableNum >= 0) { // -1 for just a Bold Label (non left side tables)
                ButtonLabel(title, out var pressed);
                // Left side tables have an interface to the table change logic
                if (pressed) TableTitleToggle(tableNum);
            } else BoldLabel(title);
        }
    }
}
