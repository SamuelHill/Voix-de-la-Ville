using System;
using System.Collections.Generic;
using System.Linq;
using TED.Utilities;
using VdlV.Simulator;
using VdlV.Step;
using VdlV.Time;
using VdlV.Utilities;
using VdlV.ValueTypes;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VdlV.Unity {
    using LocationRow = ValueTuple<Vector2Int, Location, LocationType, TimePoint>;
    using static LocationType;
    using static GraphVisualizer;
    using static GUI;
    using static GUIManager;
    using static Input;
    using static SaveManager;
    using static StaticTables;
    using static StringProcessing;
    using static VoixDeLaVille;

    // ReSharper disable once UnusedMember.Global
    /// <summary>Handles running the simulation and interfacing with the GUI and Tile Managers.</summary>
    public class SimulationController : MonoBehaviour {
        // ReSharper disable MemberCanBePrivate.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global, UnassignedField.Global, InconsistentNaming
        public bool SiftingEnabled;
        public bool RecordPerformanceData;
        public bool AlphabeticalTables = true;
        public bool PrettyNamesOnly = true;
        public Vector2Int TownCenter;
        public Tilemap Tilemap;
        public Tile OccupiedLot;
        public GameObject GraphVizGameObject;
        public GameObject StepGameObject;
        // ReSharper restore InconsistentNaming, UnassignedField.Global, ConvertToConstant.Global, FieldCanBeMadeReadOnly.Global, MemberCanBePrivate.Global

        private const byte NumInRow = 3; // Used with ListWithRows

        private RunStepCode _runStepCode;
        private TileManager _tileManager;
        private GraphVisualizer _graphVisualizer;
        private Vector2 _graphCenterPoint;
        private Vector2 _graphMaxDimensions;
        private bool _simulationRunning = true; // make public for unity inspector control of start
        // ReSharper disable once InconsistentNaming
        private bool _runningBeforeREPL;
        private bool _simulationSingleStep;
        private bool _profileRuleExecutionTime;
        private bool _guiRunOnce;

        // ReSharper disable once UnusedMember.Global
        internal void Start() {
            RecordingPerformance = RecordPerformanceData;
            Sifting = SiftingEnabled;
            _runStepCode = StepGameObject.GetComponent<RunStepCode>();
            TED.Comparer<Vector2Int>.Default = new GridComparer();
            _tileManager = new TileManager(Tilemap, TownCenter, OccupiedLot);
            _graphVisualizer = GraphVizGameObject.GetComponent<GraphVisualizer>();
            GraphScreenCoordinates();
            GraphBoundRect = REPLContainer;
            InitSimulator();
            AvailableTables((PrettyNamesOnly ?
                                 Simulation.Tables.Where(t => !t.Name.Contains("_")) :
                                 Simulation.Tables).Append(Simulation.Exceptions).Append(Simulation.Problems),
                            AlphabeticalTables);
            ActiveTables(new[] { "Character", "Parent", "Employment", "WhereTheyAt" });
            AddPopulationInfo(Population);
            ProcessInitialLocations();
            AddSelectedTileInfo(SelectedLocation);
            LoadFromSave(Simulation);
        }

        // ReSharper disable once UnusedMember.Global
        internal void Update() {
            if (GetKeyDown(KeyCode.Escape)) {
                _simulationRunning = !_simulationRunning;
                _runningBeforeREPL = _simulationRunning;
                SavingWithName = false;
            }
            if (!_simulationRunning && GetKeyDown(KeyCode.Space)) _simulationSingleStep = true;
            if (GetKeyDown(KeyCode.BackQuote)) _profileRuleExecutionTime = !_profileRuleExecutionTime;
            if (GetKeyDown(KeyCode.F1)) ToggleShowTables();
            if (GetKeyDown(KeyCode.F2)) {
                if (!ShowREPLTable) _runningBeforeREPL = _simulationRunning;
                ToggleREPLTable();
                _simulationRunning = !ShowREPLTable && _runningBeforeREPL;
            }
            if (!ShowREPLTable) _tileManager.UpdateSelectedTile();
            if (GetKeyDown(KeyCode.F4)) Save(Simulation);
            if (GetKeyDown(KeyCode.F5)) {
                _simulationRunning = false;
                SavingWithName = true;
            }
            if (!_simulationRunning && !_simulationSingleStep) return;
            try { UpdateSimulator(); } catch (Exception e) {
                Debug.LogException(e);
                _simulationRunning = false;
                throw;
            }
            ProcessLots();
            if (SiftingEnabled) {
                if (_runStepCode.PauseOnDeath() || _runStepCode.PauseOnMarriage()) _simulationRunning = false;
                _runStepCode.ProcessGossip();
                _runStepCode.GetNews();
            }
            _simulationSingleStep = false;
            if (!(PoppedTable & _simulationRunning)) return;
            _simulationRunning = false;
            PoppedTable = false;
        }

        // ReSharper disable once UnusedMember.Global
        internal void OnGUI() {
            if (!_guiRunOnce) {
                CustomSkins();
                InitAllTables();
                _guiRunOnce = true;
            }
            ShowStrings();
            ShowActiveTables();
            ChangeActiveTables();
            ShowFlowButtons();
            ShowREPL();
            if (SiftingEnabled) StepGUIControl();
            SaveNameText();
            _tileManager.SetVisibility(ShowTilemap);
            if (_profileRuleExecutionTime) RuleExecutionTimes();
            if (!_simulationRunning) ShowPaused();
        }

        // ************************************ Location Tiles ************************************

        private static Color LocationColor(LocationType locationType) => 
            LocationColorsIndex[locationType].Item2;

        private void ProcessPrimordialLocations() =>
            _tileManager.OccupyAndColorLots((from row in PrimordialLocation
                                             select (row.Item3, LocationColor(row.Item2))).ToArray());
        private bool ProcessNewLocations() =>
            _tileManager.OccupyAndColorLots((from row in CreatedLocation
                                             select (row.Item1, LocationColor(row.Item3))).ToArray());
        private bool ProcessLocationDeletions() =>
            _tileManager.DeleteLots(VacatedLocation);

        private void ProcessInitialLocations() {
            ProcessPrimordialLocations();
            ProcessNewLocations();
            _tileManager.Tilemap.RefreshAllTiles();
        }
        private void ProcessLots() {
            var newTiles = ProcessNewLocations();
            if (ProcessLocationDeletions() || newTiles)
                _tileManager.Tilemap.RefreshAllTiles();
        }

        // ************************************* Info Strings *************************************

        private static string Population() => $"Population of {PopulationCount}";
        private static int PopulationCount {
            get {
                try {
                    return PopulationCountIndex[true].Item2;
                } catch (KeyNotFoundException) {
                    return 0;
                }
            }
        }

        #region Tile hover (selected location)
        private string SelectedLocation() => _tileManager.SelectedLot is null ? null : 
            SelectedLotToString((Vector2Int)_tileManager.SelectedLot);
        private static string SelectedLotToString(Vector2Int selectedLot) {
            var locationRow = RowAtLot(selectedLot);
            return LocationInfo(locationRow) + EveryoneAtLocation(locationRow);
        }

        private static LocationRow RowAtLot(Vector2Int selectedLot) =>
            LocationsPositionIndex[selectedLot];
        private static string LocationInfo(LocationRow row) =>
            $"{row.Item2} ({row.Item3}) located at x: {row.Item1.x}, y: {row.Item1.y}\n" +
            $"Founded on {row.Item4} ({Clock.YearsAgo(row.Item4)})\n";

        private static string EveryoneAtLocation(LocationRow row) =>
            PeopleAtLocation(row.Item2) + BuriedAtCemetery(row.Item3);

        private static IEnumerable<string> ListPeopleAtLocation(Location location) =>
            WhereTheyAtLocationIndex.RowsMatching(location).Select(p => p.Item1.FullName);
        private static bool ListPeopleIfPresent(Location location, out IEnumerable<string> peopleList) {
            peopleList = ListPeopleAtLocation(location).ToArray();
            return peopleList.Any();
        }
        private static string PeopleAtLocation(Location location) =>
            ListPeopleIfPresent(location, out var peopleAtLocation) ?
                ListWithRows("People at location", peopleAtLocation, NumInRow) :
                "Nobody is here right now";

        // Add location argument to ListBuried and BuriedAt if there is more than one cemetery
        private static IEnumerable<string> ListPeopleBuried() =>
            Buried.Select(p => p.FullName).ToArray();
        private static string BuriedAtCemetery(LocationType locationType) => 
            locationType != Cemetery || !Buried.Any() ? "" : 
                ListWithRows("\n\nBuried at location", ListPeopleBuried(), NumInRow);
        #endregion

        // ************************************ Button Layouts ************************************

        private static void ShowFlowButtons() {
            if (!GraphVisible) return;
            if (Button(DataFlowButton(true), "Show dataflow"))
                ShowGraph(DataflowVisualizer.MakeGraph(Simulation));
            if (Button(DataFlowButton(false), "Update graph"))
                ShowGraph(UpdateFlowVisualizer.MakeGraph(Simulation));
        }

        // ************************************* REPL Layout **************************************
        // ReSharper disable InconsistentNaming

        private void GraphScreenCoordinates() {
            var screenCenter = new Vector2(Screen.width, Screen.height) / 2;
            var right = screenCenter.x + _graphVisualizer.RightBorder;
            var left = screenCenter.x + _graphVisualizer.LeftBorder;
            var width = right - left;
            var top = screenCenter.y - _graphVisualizer.TopBorder;
            var bottom = screenCenter.y - _graphVisualizer.BottomBorder;
            var height = bottom - top;
            _graphMaxDimensions = new Vector2(width, height);
            var center = _graphMaxDimensions / 2;
            _graphCenterPoint = new Vector2(left + center.x, top + center.y);
        }

        private Vector2 ClampREPLDimensions(int width, int height) =>
            new(width > _graphMaxDimensions.x ? _graphMaxDimensions.x : width,
                height > _graphMaxDimensions.y ? _graphMaxDimensions.y : height);
        private Rect REPLContainer(int width, int height, int offset) {
            var dimensions = ClampREPLDimensions(width, height);
            var centerOffsets = dimensions / 2;
            var center = _graphCenterPoint - centerOffsets;
            return new Rect(center.x, center.y - offset, dimensions.x, dimensions.y);
        }

        // ************************************* STEP Layout **************************************

        private void StepGUIControl() {
            if (ChangeTable) return;
            _runStepCode.ShowDeath();
            _runStepCode.ShowGossip();
            _runStepCode.ShowMarriage();
            _runStepCode.ShowNews();
        }
    }
}
