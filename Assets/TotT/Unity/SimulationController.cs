using System.Linq;
using TotT.Simulator;
using TotT.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Input;

namespace TotT.Unity {
    using static GUIManager;

    // ReSharper disable once UnusedMember.Global
    public class SimulationController : MonoBehaviour {
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public TalkOfTheTown TalkOfTheTown;
        // ReSharper disable once ConvertToConstant.Global
        public int StartYear = 1915;
        // ReSharper disable once ConvertToConstant.Global
        public bool PrettyNamesOnly = true;
        // ReSharper disable once UnassignedField.Global
        public Vector2Int TownCenter;
        // ReSharper disable once UnassignedField.Global
        public Tilemap Tilemap;
        // ReSharper disable once UnassignedField.Global
        public Tile OccupiedLot;
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore FieldCanBeMadeReadOnly.Global

        private TileManager _tileManager;
        private SimulationInfo _simulationInfo;
        // ReSharper disable InconsistentNaming
        private bool SimulationRunning = true; // make public for unity inspector control of start
        private bool SimulationSingleStep;
        private bool ProfileRuleExecutionTime;
        private bool GUIRunOnce;
        // ReSharper restore InconsistentNaming

        // ReSharper disable once UnusedMember.Global
        internal void Start() {
            TED.Comparer<Vector2Int>.Default = new GridComparer();
            TalkOfTheTown = new TalkOfTheTown(StartYear);
            _tileManager = new TileManager(Tilemap, TownCenter, OccupiedLot);
            _simulationInfo = new SimulationInfo(TalkOfTheTown, _tileManager);
            TalkOfTheTown.InitSimulator();
            _simulationInfo.ProcessInitialLocations();
            AvailableTables(PrettyNamesOnly
                ? TalkOfTheTown.Simulation.Tables.Where(t => !t.Name.Contains("_")).ToList()
                : TalkOfTheTown.Simulation.Tables.ToList());
            ActiveTables(new[] { "Agents", "Parents", "Vocations", "WhereTheyAt" });
            AddPopulationInfo(() => $"Population of {_simulationInfo.Population}");
            AddSelectedTileInfo(_simulationInfo.SelectedLocation); }

        // ReSharper disable once UnusedMember.Global
        internal void Update() {
            if (GetKeyDown(KeyCode.Escape)) SimulationRunning = !SimulationRunning;
            if (!SimulationRunning && GetKeyDown(KeyCode.Space)) SimulationSingleStep = true;
            if (GetKeyDown(KeyCode.BackQuote)) ProfileRuleExecutionTime = !ProfileRuleExecutionTime;
            if (GetKeyDown(KeyCode.F1)) ToggleShowTables();
            if (SimulationRunning || SimulationSingleStep) {
                try { TalkOfTheTown.UpdateSimulator(); } catch { SimulationRunning = false; throw; }
                _simulationInfo.ProcessLots();
                SimulationSingleStep = false; }
            _tileManager.UpdateSelectedTile(); }

        // ReSharper disable once UnusedMember.Global
        internal void OnGUI() {
            if (!GUIRunOnce) {
                CustomSkins();
                InitAllTables();
                GUIRunOnce = true; }
            ShowStrings();
            ShowActiveTables();
            ChangeActiveTables();
            if (ProfileRuleExecutionTime) RuleExecutionTimes();
            if (!SimulationRunning) ShowPaused();
        }
    }
}