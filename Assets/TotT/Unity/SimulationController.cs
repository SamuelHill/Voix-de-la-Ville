using System;
using System.Collections.Generic;
using System.Linq;
using TED.Utilities;
using TotT.Simulator;
using TotT.Time;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TotT.ValueTypes.LocationType;

namespace TotT.Unity {
    using LocationRow = ValueTuple<Vector2Int, Location, LocationType, TimePoint>;
    using static GraphVisualizer;
    using static GUI;
    using static GUIManager;
    using static Input;
    using static StaticTables;
    using static StringProcessing;
    using static TalkOfTheTown;

    // ReSharper disable once UnusedMember.Global
    /// <summary>Handles running the simulation and interfacing with the GUI and Tile Managers.</summary>
    public class SimulationController : MonoBehaviour {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public bool PrettyNamesOnly = true;
        // ReSharper disable once ConvertToConstant.Global
        // ReSharper disable once UnassignedField.Global
        public bool RecordPerformanceData;
        // ReSharper disable once UnassignedField.Global
        public Vector2Int TownCenter;
        // ReSharper disable once UnassignedField.Global
        public Tilemap Tilemap;
        // ReSharper disable once UnassignedField.Global
        public Tile OccupiedLot;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore MemberCanBePrivate.Global

        private const byte NumInRow = 3; // Used with ListWithRows

        private TileManager _tileManager;
        private bool _simulationRunning = true; // make public for unity inspector control of start
        private bool _simulationSingleStep;
        private bool _profileRuleExecutionTime;
        private bool _guiRunOnce;

        // ReSharper disable once UnusedMember.Global
        internal void Start() {
            TED.Comparer<Vector2Int>.Default = new GridComparer();
            RecordingPerformance = RecordPerformanceData;
            _tileManager = new TileManager(Tilemap, TownCenter, OccupiedLot);
            InitSimulator();
            AvailableTables((PrettyNamesOnly ? 
                Simulation.Tables.Where(t => !t.Name.Contains("_")) :
                Simulation.Tables).Append(Simulation.Exceptions).Append(Simulation.Problems));
            ActiveTables(new[] { "Character", "Parent", "Employment", "WhereTheyAt" });
            AddPopulationInfo(Population);
            ProcessInitialLocations();
            AddSelectedTileInfo(SelectedLocation);
        }

        // ReSharper disable once UnusedMember.Global
        internal void Update() {
            if (GetKeyDown(KeyCode.Escape)) _simulationRunning = !_simulationRunning;
            if (!_simulationRunning && GetKeyDown(KeyCode.Space)) _simulationSingleStep = true;
            if (GetKeyDown(KeyCode.BackQuote)) _profileRuleExecutionTime = !_profileRuleExecutionTime;
            if (GetKeyDown(KeyCode.F1)) ToggleShowTables();
            if (_simulationRunning || _simulationSingleStep) {
                try { UpdateSimulator(); } catch {
                    _simulationRunning = false;
                    throw;
                }
                ProcessLots();
                _simulationSingleStep = false;
                if (PoppedTable & _simulationRunning) {
                    _simulationRunning = false;
                    PoppedTable = false;
                }
            }
            _tileManager.UpdateSelectedTile();
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
            _tileManager.SetVisibility(ShowTilemap);
            if (_profileRuleExecutionTime) RuleExecutionTimes();
            if (!_simulationRunning && !ChangeTable) ShowPaused();
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
        private static int PopulationCount => PopulationCountIndex[true].Item2;

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
    }
}
