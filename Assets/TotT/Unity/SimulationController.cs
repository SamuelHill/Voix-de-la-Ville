using System.Linq;
using GraphVisualization;
using TED.Utilities;
using TotT.Simulator;
using TotT.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Input;

namespace TotT.Unity {
    using static GUIManager;

    // ReSharper disable once UnusedMember.Global
    /// <summary>Handles running the simulation and interfacing with the GUI and Tile Managers.</summary>
    public class SimulationController : MonoBehaviour {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public TalkOfTheTown TalkOfTheTown;
        // ReSharper disable once ConvertToConstant.Global
        public bool PrettyNamesOnly = true;
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
            TalkOfTheTown = new TalkOfTheTown();
            _tileManager = new TileManager(Tilemap, TownCenter, OccupiedLot);
            _simulationInfo = new SimulationInfo(TalkOfTheTown, _tileManager);
            TalkOfTheTown.InitSimulator();
            _simulationInfo.ProcessInitialLocations();
            var simulationTables = PrettyNamesOnly
                ? TalkOfTheTown.Simulation.Tables.Where(t => !t.Name.Contains("_"))
                : TalkOfTheTown.Simulation.Tables;
            AvailableTables(simulationTables.Append(TalkOfTheTown.Simulation.Exceptions).Append(TalkOfTheTown.Simulation.Problems));
            ActiveTables(new[] { "Character", "Parent", "Employment", "WhereTheyAt" });
            AddPopulationInfo(() => $"Population of {_simulationInfo.Population}");
            AddSelectedTileInfo(_simulationInfo.SelectedLocation);
        }

        // ReSharper disable once UnusedMember.Global
        internal void Update() {
            if (GetKeyDown(KeyCode.Escape)) _simulationRunning = !_simulationRunning;
            if (!_simulationRunning && GetKeyDown(KeyCode.Space)) _simulationSingleStep = true;
            if (GetKeyDown(KeyCode.BackQuote)) _profileRuleExecutionTime = !_profileRuleExecutionTime;
            if (GetKeyDown(KeyCode.F1)) ToggleShowTables();
            if (_simulationRunning || _simulationSingleStep) {
                try { TalkOfTheTown.UpdateSimulator(); } catch { _simulationRunning = false; throw; }
                _simulationInfo.ProcessLots();
                _simulationSingleStep = false;
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
            if (_profileRuleExecutionTime) RuleExecutionTimes();
            if (!_simulationRunning) ShowPaused();
        }

        private void ShowFlowButtons()
        {
            var buttonWidth = 100;
            var buttonHeight = 50;
            var buttonStart = 70;
            if (GUI.Button(new Rect(Screen.width-buttonWidth, buttonStart, buttonWidth, buttonHeight),
                    "Show dataflow"))
                TEDGraphVisualization.ShowGraph(DataflowVisualizer.MakeGraph(TalkOfTheTown.Simulation));
            if (GUI.Button(new Rect(Screen.width-buttonWidth, buttonStart+buttonHeight, buttonWidth, buttonHeight),
                    "Update graph"))
                TEDGraphVisualization.ShowGraph(UpdateFlowVisualizer.MakeGraph(TalkOfTheTown.Simulation));
        }
    }
}
