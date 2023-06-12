﻿using System.Linq;
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
        public int StartYear = 1915;
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
            if (_profileRuleExecutionTime) RuleExecutionTimes();
            if (!_simulationRunning) ShowPaused();
        }
    }
}