using System.Linq;
using TED.Utilities;
using TotT.Simulator;
using TotT.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TotT.Unity {
    using static GUI;
    using static GUIManager;
    using static Input;
    using static SimulationInfo;
    using static TalkOfTheTown;
    using static GraphVisualizer;

    // ReSharper disable once UnusedMember.Global
    /// <summary>Handles running the simulation and interfacing with the GUI and Tile Managers.</summary>
    public class SimulationController : MonoBehaviour {
        // ReSharper disable MemberCanBePrivate.Global
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

        private TileManager _tileManager;
        private SimulationInfo _simulationInfo;
        private bool _simulationRunning = true; // make public for unity inspector control of start
        private bool _simulationSingleStep;
        private bool _profileRuleExecutionTime;
        private bool _guiRunOnce;

        // ReSharper disable once UnusedMember.Global
        internal void Start() {
            TED.Comparer<Vector2Int>.Default = new GridComparer();
            TalkOfTheTown.RecordPerformanceData = RecordPerformanceData;
            _tileManager = new TileManager(Tilemap, TownCenter, OccupiedLot);
            _simulationInfo = new SimulationInfo(_tileManager);
            InitSimulator();
            _simulationInfo.ProcessInitialLocations();
            var simulationTables = PrettyNamesOnly
                ? Simulation.Tables.Where(t => !t.Name.Contains("_"))
                : Simulation.Tables;
            AvailableTables(simulationTables.Append(Simulation.Exceptions).Append(Simulation.Problems));
            ActiveTables(new[] { "Character", "Parent", "Employment", "WhereTheyAt" });
            AddPopulationInfo(() => $"Population of {Population}");
            AddSelectedTileInfo(_simulationInfo.SelectedLocation);
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
                _simulationInfo.ProcessLots();
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

        private static void ShowFlowButtons() {
            if (!GraphVisible) return;
            if (Button(DataFlowButton(true), "Show dataflow"))
                ShowGraph(DataflowVisualizer.MakeGraph(Simulation));
            if (Button(DataFlowButton(false), "Update graph"))
                ShowGraph(UpdateFlowVisualizer.MakeGraph(Simulation));
        }
    }
}
