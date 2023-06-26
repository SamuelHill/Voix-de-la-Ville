using System;
using System.Collections.Generic;
using System.Linq;
using TotT.Simulator;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;

namespace TotT.Unity {
    using LocationRow = ValueTuple<Vector2Int, Location, LocationType, TimePoint>;
    using static StaticTables;
    using static StringProcessing;

    /// <summary>Info strings from TalkOfTheTown tables and interfacing for TileManager</summary>
    public class SimulationInfo {
        private readonly TalkOfTheTown _talkOfTheTown;
        private readonly TileManager _tileManager;

        public SimulationInfo(TalkOfTheTown talkOfTheTown, TileManager tileManager) {
            _talkOfTheTown = talkOfTheTown;
            _tileManager = tileManager; }

        private const byte NumInRow = 3; // Used with ListWithRows

        // **************************************** Tiles *****************************************
        private static Color LocationColor(LocationType locationType) =>
            LocationColorsIndex[locationType].Item2;

        private void ProcessPrimordialLocations() =>
            _tileManager.OccupyAndColorLots((from row in PrimordialLocation
                                             select (row.Item3, LocationColor(row.Item2))).ToArray());
        private bool ProcessNewLocations() =>
            _tileManager.OccupyAndColorLots((from row in _talkOfTheTown.NewLocation 
                                             select (row.Item1, LocationColor(row.Item3))).ToArray());
        private bool ProcessLocationDeletions() =>
            _tileManager.DeleteLots(_talkOfTheTown.VacatedLocation);

        public void ProcessInitialLocations() {
            ProcessPrimordialLocations();
            ProcessNewLocations();
            _tileManager.Tilemap.RefreshAllTiles(); }
        public void ProcessLots() {
            if (ProcessNewLocations() || ProcessLocationDeletions())
                _tileManager.Tilemap.RefreshAllTiles(); }

        // ************************************* Info Strings *************************************
        public int Population => _talkOfTheTown.PopulationCountIndex[true].Item2;

        public string SelectedLocation() => _tileManager.SelectedLot is null ? null :
            SelectedLotToString((Vector2Int)_tileManager.SelectedLot);
        private string SelectedLotToString(Vector2Int selectedLot) {
            var locationRow = RowAtLot(selectedLot);
            return LocationInfo(locationRow) + EveryoneAtLocation(locationRow);
        }

        private LocationRow RowAtLot(Vector2Int selectedLot) =>
            _talkOfTheTown.LocationsPositionIndex[selectedLot];
        private static string LocationInfo(LocationRow row) =>
            $"{row.Item2} ({row.Item3}) located at x: {row.Item1.x}, y: {row.Item1.y}\n" +
            $"Founded on {row.Item4} ({TalkOfTheTown.Time.YearsAgo(row.Item4)})\n";

        private string EveryoneAtLocation(LocationRow row) =>
            PeopleAtLocation(row.Item2) + BuriedAtCemetery(row.Item3);

        private IEnumerable<string> ListPeopleAtLocation(Location location) => 
            _talkOfTheTown.WhereTheyAtLocationIndex.RowsMatching(location).Select(p => p.Item1.FullName);
        private bool ListPeopleIfPresent(Location location, out IEnumerable<string> peopleList) {
            peopleList = ListPeopleAtLocation(location).ToArray();
            return peopleList.Any(); }
        private string PeopleAtLocation(Location location) => 
            ListPeopleIfPresent(location, out var peopleAtLocation) ? 
                ListWithRows("People at location", peopleAtLocation, NumInRow) : 
                "Nobody is here right now";

        // Add location argument to ListBuried and BuriedAt if there is more than one cemetery
        private IEnumerable<string> ListPeopleBuried() =>
            _talkOfTheTown.Buried.Select(p => p.FullName).ToArray();
        private string BuriedAtCemetery(LocationType locationType) =>
            locationType != LocationType.Cemetery || !_talkOfTheTown.Buried.Any() ? "" : 
                ListWithRows("\n\nBuried at location", ListPeopleBuried(), NumInRow);
    }
}
