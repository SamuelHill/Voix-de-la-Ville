using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Input;

using LocationRow = System.ValueTuple<Location, UnityEngine.Vector2Int, int, Date>;
using TileLocation = System.ValueTuple<UnityEngine.Vector3Int, Location>;

internal class GridComparer : EqualityComparer<Vector2Int> {
    public override bool Equals(Vector2Int x, Vector2Int y) => x == y;
    public override int GetHashCode(Vector2Int v) => (v.x * 40 + v.y) * 2;
}

public class UnityComponent : MonoBehaviour {
    public TalkOfTheTown TalkOfTheTown;
    public int StartYear = 1915;
    internal bool SimulationRunning = true;
    internal bool SimulationSingleStep;
    internal GUIManager Manager;
    // ReSharper disable once InconsistentNaming
    internal bool GUIRunOnce;
    public Vector2Int TownCenter; // offsets the town's location in terms of tiles on screen
    public Tilemap Tilemap;
    // for the tile to be able to change color: https://github.com/Unity-Technologies/2d-extras/issues/96
    public Tile OccupiedLot;
    internal Vector3Int? SelectedLocationTile;

    internal void Start() {
        TED.Comparer<Vector2Int>.Default = new GridComparer();
        TalkOfTheTown = new TalkOfTheTown(StartYear);
        TalkOfTheTown.InitSimulator();
        ProcessInitialLocations();
        Manager = GUIManager.Manager;
        Manager.SetAvailableTables(new List<TablePredicate> {
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
        });
        Manager.SetActiveTables(new[] { "Agents", "Couples", "Parents", "Dead" });
        Manager.AddSelectedTileInfo(SelectedLocation);
    }

    internal void Update() {
        if (GetKeyDown(KeyCode.Escape)) SimulationRunning = !SimulationRunning;
        if (!SimulationRunning && GetKeyDown(KeyCode.Space)) SimulationSingleStep = true;
        if (SimulationRunning || SimulationSingleStep) {
            TalkOfTheTown.UpdateSimulator();
            ProcessLots();
            SimulationSingleStep = false; }
        if (TrySelectTile(out var tile)) SelectedLocationTile = tile; }

    internal void OnGUI() {
        if (!GUIRunOnce) {
            GUI.skin.box.alignment = TextAnchor.MiddleCenter;
            Manager.GetTableToolbarSize();
            Manager.InitAllTables();
            GUIRunOnce = true; }
        Manager.ShowTime();
        Manager.ShowTileInfo();
        Manager.ShowActiveTables();
        //GUI.Label(Manager.CenteredRect(400, 400), string.Concat(
        //    from table in TalkOfTheTown.Simulation.Tables 
        //    where table.RuleExecutionTime > 0 
        //    orderby -table.RuleExecutionTime 
        //    select $"{table.Name} {table.RuleExecutionTime}\n"));
        Manager.SelectActiveTables();
        if (!SimulationRunning) Manager.ShowPaused();
    }

    
    internal Color GetLocationColor(Location location) => 
        TalkOfTheTown.LocationColorsIndex[location.Type].Item2;
    internal void ProcessInitialLocations() {
        var newTiles = LocationsToTiles(TalkOfTheTown.Locations);
        OccupyTiles(newTiles.Select(t => t.Item1).ToArray());
        foreach (var newTile in newTiles)
            SetTileColor(newTile.Item1, newTile.Item2);
        RefreshAllTiles(); }
    internal void ProcessLots() {
        var vacated = LocationsToTiles(TalkOfTheTown.VacatedLocations);
        if (vacated.Length > 0) DeleteTiles(vacated.Select(v => v.Item1).ToArray());
        var newTiles = LocationsToTiles(TalkOfTheTown.NewLocations);
        if (newTiles.Length > 0) OccupyTiles(newTiles.Select(t => t.Item1).ToArray());
        foreach (var newTile in newTiles) 
            SetTileColor(newTile.Item1, newTile.Item2);
        if (newTiles.Length + vacated.Length > 0) RefreshAllTiles(); }
    internal Vector3Int LotToTile(Vector2Int lot) => (Vector3Int)(TownCenter + lot);
    internal Vector2Int TileToLot(Vector3Int tile) => (Vector2Int)tile - TownCenter;
    internal TileLocation[] LocationsToTiles(IEnumerable<LocationRow> locations) =>
        (from location in locations 
            select (LotToTile(location.Item2), location.Item1)).ToArray();
    internal void SetTiles(Vector3Int[] tiles, TileBase tileToSet) => 
        Tilemap.SetTiles(tiles, Enumerable.Repeat(tileToSet, tiles.Length).ToArray());
    internal void SetTileColor(Vector3Int tile, Location location) => 
        Tilemap.SetColor(tile, GetLocationColor(location));
    internal void DeleteTiles(Vector3Int[] tiles) => SetTiles(tiles, null);
    internal void OccupyTiles(Vector3Int[] tiles) => SetTiles(tiles, OccupiedLot);
    internal void RefreshAllTiles() => Tilemap.RefreshAllTiles();
    internal bool TrySelectTile(out Vector3Int tile) {
        tile = Tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(mousePosition));
        return Tilemap.HasTile(tile); }
    internal LocationRow LocationRowByTile(Vector3Int tile) => 
        TalkOfTheTown.LocationsPositionIndex[TileToLot(tile)];
    internal string LocationRowToString(LocationRow locationRow) => 
        $"{locationRow.Item1}:\nLocated at {locationRow.Item2},\n" + 
        $"Founded on {locationRow.Item4}, {locationRow.Item3} " +
        $"({TalkOfTheTown.Time.YearsSince(locationRow.Item4, locationRow.Item3)} years ago)";
    internal string TileInfo(Vector3Int tile) => LocationRowToString(LocationRowByTile(tile));
    internal string SelectedLocation() => SelectedLocationTile is null ?
        "No location selected..." : TileInfo((Vector3Int)SelectedLocationTile);
}