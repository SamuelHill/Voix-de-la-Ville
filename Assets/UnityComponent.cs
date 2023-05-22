using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Input;
using AgentRow = System.ValueTuple<Person, int, Date, Sex, Sexuality, VitalStatus>;
using LocationRow = System.ValueTuple<Location, LocationType, UnityEngine.Vector2Int, int, Date, LocationCategories>;
using NewLocationRow = System.ValueTuple<Location, LocationType, UnityEngine.Vector2Int, int, Date>;

internal class GridComparer : EqualityComparer<Vector2Int> {
    public override bool Equals(Vector2Int x, Vector2Int y) => x == y;
    public override int GetHashCode(Vector2Int v) => (v.x * 40 + v.y) * 2;
}

public class UnityComponent : MonoBehaviour {
    public TalkOfTheTown TalkOfTheTown;
    public int StartYear = 1915;
    public Vector2Int TownCenter; // offsets the town's location in terms of tiles on screen
    public Tilemap Tilemap;
    // for the tile to be able to change color: https://github.com/Unity-Technologies/2d-extras/issues/96
    public Tile OccupiedLot;
    public bool PrettyNamesOnly = true;

    internal bool SimulationRunning = true;
    internal bool SimulationSingleStep;
    internal bool DebugRuleExecutionTime;
    // ReSharper disable once InconsistentNaming
    internal bool GUIRunOnce;
    internal Vector3Int? SelectedLocationTile;

    internal void Start() {
        TED.Comparer<Vector2Int>.Default = new GridComparer();
        TalkOfTheTown = new TalkOfTheTown(StartYear);
        TalkOfTheTown.InitSimulator();
        ProcessInitialLocations();
        GUIManager.SetAvailableTables(PrettyNamesOnly
            ? TalkOfTheTown.Simulation.Tables.Where(t => !t.Name.Contains("_")).ToList()
            : TalkOfTheTown.Simulation.Tables.ToList());
        GUIManager.SetActiveTables(new[] { "Agents", "Parents", "Vocations", "WhereTheyAt" });
        GUIManager.AddPopulationInfo(() => $"Population of {Population}");
        GUIManager.AddSelectedTileInfo(SelectedLocation); }

    internal void Update() {
        if (GetKeyDown(KeyCode.Escape)) SimulationRunning = !SimulationRunning;
        if (!SimulationRunning && GetKeyDown(KeyCode.Space)) SimulationSingleStep = true;
        if (GetKeyDown(KeyCode.BackQuote)) DebugRuleExecutionTime = !DebugRuleExecutionTime;
        if (GetKeyDown(KeyCode.F1)) GUIManager.ShowTables = !GUIManager.ShowTables;
        if (SimulationRunning || SimulationSingleStep) {
            try { TalkOfTheTown.UpdateSimulator(); }
            catch { SimulationRunning = false; throw; }
            ProcessLots();
            SimulationSingleStep = false; }
        UpdateSelectedLocation(); }

    internal void OnGUI() {
        if (!GUIRunOnce) {
            GUIManager.CustomSkins();
            GUIManager.GetTableToolbarSize();
            GUIManager.InitAllTables();
            GUIRunOnce = true; }
        GUIManager.ShowStrings();
        GUIManager.ShowActiveTables();
        GUIManager.ChangeActiveTables();
        if (DebugRuleExecutionTime) GUIManager.RuleExecutionTimes();
        if (!SimulationRunning) GUIManager.ShowPaused(); }

    #region Tilemap Coordinates to/from Simulation Coordinates
    internal Vector3Int LotToTile(Vector2Int lot) => (Vector3Int)(TownCenter + lot);
    internal Vector2Int TileToLot(Vector3Int tile) => (Vector2Int)tile - TownCenter;
    #endregion

    #region Location Info Indexer Wrappers
    internal Color GetLocationColor(LocationType locationType) =>
        TalkOfTheTown.LocationColorsIndex[locationType].Item2;
    internal LocationRow GetLocationInfo(Vector3Int selectedTile) =>
        TalkOfTheTown.LocationsPositionIndex[TileToLot(selectedTile)];
    internal IEnumerable<AgentRow> LivingPeople() =>
        TalkOfTheTown.AgentsVitalStatusIndex.RowsMatching(VitalStatus.Alive);
    internal int Population => LivingPeople().ToArray().Length;
    #endregion

    #region Process Location Tiles
    internal void SetTiles(Vector3Int[] tiles, TileBase tileToSet) =>
        Tilemap.SetTiles(tiles, Enumerable.Repeat(tileToSet, tiles.Length).ToArray());
    internal void DeleteTiles((Vector3Int, Color)[] tilesToDelete) =>
        SetTiles(tilesToDelete.Select(t => t.Item1).ToArray(), null);
    internal void OccupyTiles((Vector3Int, Color)[] tilesToOccupy) {
        SetTiles(tilesToOccupy.Select(t => t.Item1).ToArray(), OccupiedLot);
        foreach (var newTile in tilesToOccupy)
            Tilemap.SetColor(newTile.Item1, newTile.Item2); }
    
    internal bool SetIfLocations(IEnumerable<NewLocationRow> locations, Action<(Vector3Int, Color)[]> setFunc) {
        var tilesToSet = (from location in locations
            select (LotToTile(location.Item3), GetLocationColor(location.Item2))).ToArray(); ;
        if (tilesToSet.Length == 0) return false;
        setFunc(tilesToSet);
        return true; }
    internal bool DeleteTiles(IEnumerable<NewLocationRow> locations) => SetIfLocations(locations, DeleteTiles);
    internal bool OccupyTiles(IEnumerable<NewLocationRow> locations) => SetIfLocations(locations, OccupyTiles);

    internal void ProcessInitialLocations() {
        OccupyTiles(TalkOfTheTown.PrimordialLocations);
        OccupyTiles(TalkOfTheTown.NewLocations);
        Tilemap.RefreshAllTiles(); }
    internal void ProcessLots() {
        if (OccupyTiles(TalkOfTheTown.NewLocations) ||
            DeleteTiles(TalkOfTheTown.VacatedLocations)) 
            Tilemap.RefreshAllTiles(); }
    #endregion

    #region Selected Location: screen to tiles & GUIString getStringFunc
    internal bool TrySelectTile(out Vector3Int tile) {
        tile = Tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePosition));
        return Tilemap.HasTile(tile); }
    internal void UpdateSelectedLocation() =>
        SelectedLocationTile = TrySelectTile(out var tile) ? tile : null;
    internal string SelectedLocation() =>
        SelectedLocationTile is null ? null :
            LocationRowToString(GetLocationInfo((Vector3Int)SelectedLocationTile));
    #endregion

    #region Selected location string processing helpers
    internal string LocationRowToString(LocationRow locationRow) => 
        LocationInfo(locationRow) + EveryoneAtLocation(locationRow);
    internal string LocationInfo(LocationRow row) =>
        $"{row.Item1} ({row.Item2}) located at x: {row.Item3.x}, y: {row.Item3.y}\n" +
        $"Founded on {row.Item5}, {row.Item4} ({LocationAge(row)} years ago)\n";
    internal int LocationAge(LocationRow row) => TalkOfTheTown.Time.YearsSince(row.Item5, row.Item4);
    internal string PeopleAtLocation(Location l) {
        var peopleAtLocation = TalkOfTheTown.WhereTheyAtLocationIndex
            .RowsMatching(l).Select(p => p.Item1.FullName).ToArray();
        return peopleAtLocation.Length != 0 ? 
            $"People at location: {GroupUp(peopleAtLocation, 3)}" : 
            "Nobody is here right now"; }

    internal string EveryoneAtLocation(LocationRow row) => 
        PeopleAtLocation(row.Item1) + BuriedAtCemetery(row.Item2);
    internal string BuriedAtCemetery(LocationType locationType) => 
        locationType == LocationType.Cemetery && TalkOfTheTown.Buried.Length > 0 ? BuriedAtCemetery() : "";
    internal string BuriedAtCemetery() => 
        $"\n\nBuried at location: {GroupUp(TalkOfTheTown.Buried.Select(p => p.FullName).ToArray(), 3)}";

    internal string GroupUp(string[] strings, int itemsPerGroup) =>
        // Concat an empty string to the beginning, takes place of "People at location: "
        string.Join(",\n", new[] { "" }.Concat(strings).ToList().Select(
            (s, i) => new { s, i }).ToLookup(
            str => str.i / itemsPerGroup, str => str.s).Select(
            // remove the extra ", " that gets joined on the empty string
            row => string.Join(", ", row)))[2..];
    #endregion
}