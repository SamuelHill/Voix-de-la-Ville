using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Input;

using LocationRow = System.ValueTuple<Location, LocationType, UnityEngine.Vector2Int, int, Date>;

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
        //TalkOfTheTown.SetREPL("Agents");
        GUIManager.SetAvailableTables(new List<TablePredicate> {
            TalkOfTheTown.Agents,
            TalkOfTheTown.Couples,
            TalkOfTheTown.Parents,
            TalkOfTheTown.Locations,
            TalkOfTheTown.NewLocations,
            TalkOfTheTown.JobsToFill,
            TalkOfTheTown.Homes,
            TalkOfTheTown.LocationColors,
            TalkOfTheTown.LocationInformation,
            TalkOfTheTown.Vocations,
            TalkOfTheTown.Aptitude,
            TalkOfTheTown.Personality,
            TalkOfTheTown.WhereTheyAt,
            //TalkOfTheTown.REPL
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
        SelectedLocationTile = TrySelectTile(out var tile) ? tile : null;
        //TalkOfTheTown.SetREPL("Agents");
    }

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

    #region Tilemap helpers
    internal Vector3Int LotToTile(Vector2Int lot) => (Vector3Int)(TownCenter + lot);
    internal Vector2Int TileToLot(Vector3Int tile) => (Vector2Int)tile - TownCenter;
    internal bool TrySelectTile(out Vector3Int tile) {
        tile = Tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePosition));
        return Tilemap.HasTile(tile); }
    internal LocationRow GetLocationInfo(Vector3Int selectedTile) =>
        TalkOfTheTown.LocationsPositionIndex[TileToLot(selectedTile)];
    #endregion

    #region Process Location Tiles
    internal void ProcessInitialLocations() {
        var newTiles = LocationsToTiles(TalkOfTheTown.PrimordialLocations);
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
    internal (Vector3Int, LocationType)[] LocationsToTiles(IEnumerable<LocationRow> locations) =>
        (from location in locations 
            select (LotToTile(location.Item3), location.Item2)).ToArray();
    internal Color GetLocationColor(LocationType locationType) =>
        TalkOfTheTown.LocationColorsIndex[locationType].Item2;
    internal void SetTileColor(Vector3Int tile, LocationType locationType) =>
        Tilemap.SetColor(tile, GetLocationColor(locationType));
    internal void SetTiles(Vector3Int[] tiles, TileBase tileToSet) => 
        Tilemap.SetTiles(tiles, Enumerable.Repeat(tileToSet, tiles.Length).ToArray());
    internal void DeleteTiles(Vector3Int[] tiles) => SetTiles(tiles, null);
    internal void OccupyTiles(Vector3Int[] tiles) => SetTiles(tiles, OccupiedLot);
    #endregion

    #region String processing
    internal string SelectedLocation() => 
        SelectedLocationTile is null ? null : 
            LocationRowToString(GetLocationInfo((Vector3Int)SelectedLocationTile));
    internal string LocationRowToString(LocationRow locationRow) => 
        LocationInfo(locationRow) + "\n" + PeopleAtLocation(locationRow.Item1);
    internal string LocationInfo(LocationRow row) =>
        $"{row.Item1} ({row.Item2}) located at x: {row.Item3.x}, y: {row.Item3.y}\nFounded on {row.Item5}, {row.Item4} ({LocationAge(row)} years ago)";
    internal int LocationAge(LocationRow row) => TalkOfTheTown.Time.YearsSince(row.Item5, row.Item4);
    internal string PeopleAtLocation(Location l) {
        var atLocation = TalkOfTheTown.WhereTheyAtLocationIndex.RowsMatching(l).Select(p => p.Item1.FullName).ToArray();
        return atLocation.Length != 0 ? $"People at location: {GroupUp(atLocation, 3)}" : "Nobody is here right now"; }
    internal string GroupUp(string[] strings, int itemsPerGroup) =>
        // Concat an empty string to the beginning - the starting line includes "People at location: "
        string.Join(",\n", new[] { "" }.Concat(strings).ToList().Select(
            (s, i) => new { s, i }).ToLookup(
            str => str.i / itemsPerGroup, str => str.s).Select(
            // remove the extra ", " that gets joined on the empty string in the first row
            row => string.Join(", ", row)))[2..];

    internal string Population() => $"Population of {TalkOfTheTown.AgentsVitalStatusIndex.RowsMatching(VitalStatus.Alive).ToArray().Length}";
    #endregion
}