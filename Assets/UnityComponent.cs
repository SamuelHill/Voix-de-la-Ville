using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Input;

using LocationRow = System.ValueTuple<Location, UnityEngine.Vector2Int, int, Date>;

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

    #region Tilemap helpers
    internal Vector3Int LotToTile(Vector2Int lot) => (Vector3Int)(TownCenter + lot);
    internal Vector2Int TileToLot(Vector3Int tile) => (Vector2Int)tile - TownCenter;
    internal bool TrySelectTile(out Vector3Int tile) {
        tile = Tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePosition));
        return Tilemap.HasTile(tile); }
    #endregion

    internal bool SimulationRunning = true;
    internal bool SimulationSingleStep;
    internal bool DebugRuleExecutionTime;
    internal Vector3Int? SelectedLocationTile;
    // ReSharper disable once InconsistentNaming
    internal bool GUIRunOnce;

    internal void Start() {
        TED.Comparer<Vector2Int>.Default = new GridComparer();
        TalkOfTheTown = new TalkOfTheTown(StartYear);
        TalkOfTheTown.InitSimulator();
        ProcessInitialLocations();
        GUIManager.SetAvailableTables(new List<TablePredicate> {
            TalkOfTheTown.Agents,
            TalkOfTheTown.Dead,
            TalkOfTheTown.Couples,
            TalkOfTheTown.Parents,
            TalkOfTheTown.Locations,
            TalkOfTheTown.Vocations,
            TalkOfTheTown.Homes,
            TalkOfTheTown.LocationColors,
            TalkOfTheTown.NewLocations,
            TalkOfTheTown.VacatedLocations,
            TalkOfTheTown.UsedLots,
            TalkOfTheTown.Aptitude,
            TalkOfTheTown.WhereTheyAt,
        });
        GUIManager.SetActiveTables(new[] { "Agents", "Couples", "Parents", "Homes" });
        GUIManager.AddSelectedTileInfo(SelectedLocation);
        GUIManager.AddPopulationInfo(Population); }

    internal void Update() {
        if (GetKeyDown(KeyCode.Escape)) SimulationRunning = !SimulationRunning;
        if (!SimulationRunning && GetKeyDown(KeyCode.Space)) SimulationSingleStep = true;
        if (GetKeyDown(KeyCode.BackQuote)) DebugRuleExecutionTime = !DebugRuleExecutionTime;
        if (SimulationRunning || SimulationSingleStep) {
            TalkOfTheTown.UpdateSimulator();
            ProcessLots();
            SimulationSingleStep = false; }
        if (TrySelectTile(out var tile)) SelectedLocationTile = tile; }

    internal void OnGUI() {
        if (!GUIRunOnce) {
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;
            GUIManager.GetTableToolbarSize();
            GUIManager.InitAllTables();
            GUIRunOnce = true; }
        GUIManager.ShowStrings();
        GUIManager.ShowActiveTables();
        GUIManager.ChangeActiveTables();
        if (DebugRuleExecutionTime) GUIManager.RuleExecutionTimes();
        if (!SimulationRunning) GUIManager.ShowPaused(); }

    #region Process Location Tiles
    internal void ProcessInitialLocations() {
        var newTiles = LocationsToTiles(TalkOfTheTown.Locations);
        OccupyTiles(newTiles.Select(t => t.Item1).ToArray());
        foreach (var newTile in newTiles)
            SetTileColor(newTile.Item1, newTile.Item2);
        Tilemap.RefreshAllTiles(); }
    internal void ProcessLots() {
        var vacated = LocationsToTiles(TalkOfTheTown.VacatedLocations);
        if (vacated.Length > 0) DeleteTiles(vacated.Select(v => v.Item1).ToArray());
        var newTiles = LocationsToTiles(TalkOfTheTown.NewLocations);
        if (newTiles.Length > 0) OccupyTiles(newTiles.Select(t => t.Item1).ToArray());
        foreach (var newTile in newTiles) 
            SetTileColor(newTile.Item1, newTile.Item2);
        if (newTiles.Length + vacated.Length > 0) Tilemap.RefreshAllTiles(); }
    internal (Vector3Int, Location)[] LocationsToTiles(IEnumerable<LocationRow> locations) =>
        (from location in locations 
            select (LotToTile(location.Item2), location.Item1)).ToArray();
    internal Color GetLocationColor(Location location) =>
        TalkOfTheTown.LocationColorsIndex[location.Type].Item2;
    internal void SetTileColor(Vector3Int tile, Location location) =>
        Tilemap.SetColor(tile, GetLocationColor(location));
    internal void SetTiles(Vector3Int[] tiles, TileBase tileToSet) => 
        Tilemap.SetTiles(tiles, Enumerable.Repeat(tileToSet, tiles.Length).ToArray());
    internal void DeleteTiles(Vector3Int[] tiles) => SetTiles(tiles, null);
    internal void OccupyTiles(Vector3Int[] tiles) => SetTiles(tiles, OccupiedLot);
    #endregion


    internal LocationRow LocationInfo(Vector3Int selectedTile) =>
        TalkOfTheTown.LocationsPositionIndex[TileToLot(selectedTile)];
    internal IEnumerable<Person> PeopleAtLocation(LocationRow locationInfo) =>
        from pair in TalkOfTheTown.WhereTheyAt where ReferenceEquals(pair.Item2, locationInfo.Item1) select pair.Item1;


    internal string LocationRowToString(LocationRow locationRow) => 
        $"{locationRow.Item1}:\nLocated at {locationRow.Item2},\n" + 
        $"Founded on {locationRow.Item4}, {locationRow.Item3} " +
        $"({TalkOfTheTown.Time.YearsSince(locationRow.Item4, locationRow.Item3)} years ago)";
    internal string SelectedLocation() {
        return SelectedLocationTile is null
            ? "No location selected..."
            : LocationRowToString(LocationInfo((Vector3Int)SelectedLocationTile)) + 
              $"\nPeople at location: {string.Join(", ", PeopleAtLocation(LocationInfo((Vector3Int)SelectedLocationTile)))}"; }

    internal string Population() => $"Population of {TalkOfTheTown.Agents.Length - TalkOfTheTown.Dead.Length}";
}