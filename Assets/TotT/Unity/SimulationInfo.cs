using System;
using System.Collections.Generic;
using System.Linq;
using TotT.Simulator;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;

namespace TotT.Unity {
    using LocationRow = ValueTuple<Location, LocationType, LocationCategory, Vector2Int, TimePoint>;
    using NewLocationRow = ValueTuple<Location, LocationType, Vector2Int, TimePoint>;
    using static StaticTables;
    using static StringProcessing;

    public class SimulationInfo { // Info strings from TalkOfTheTown & table interfacing for TileManager
        private readonly TalkOfTheTown _talkOfTheTown;
        private readonly TileManager _tileManager;

        public SimulationInfo(TalkOfTheTown talkOfTheTown, TileManager tileManager) {
            _talkOfTheTown = talkOfTheTown;
            _tileManager = tileManager; }

        private const byte NumInRow = 3; // Used with ListWithRows

        // **************************************** Tiles *****************************************

        private IEnumerable<Vector2Int> ToDelete() => 
            from row in _talkOfTheTown.VacatedLocations select row.Item3;

        private static Color LocationColor(LocationType locationType) =>
            LocationColorsIndex[locationType].Item2;

        private static (Vector2Int, Color)[] ToOccupyAndColor(IEnumerable<NewLocationRow> locations) =>
            (from row in locations select (row.Item3, LocationColor(row.Item2))).ToArray();

        private bool ProcessNewLocations() =>
            _tileManager.OccupyAndColorLots(ToOccupyAndColor(_talkOfTheTown.NewLocations));

        private void ProcessPrimordialLocations() =>
            _tileManager.OccupyAndColorLots(ToOccupyAndColor(PrimordialLocations));

        public void ProcessInitialLocations() {
            ProcessPrimordialLocations();
            ProcessNewLocations();
            _tileManager.Tilemap.RefreshAllTiles(); }

        public void ProcessLots() {
            if (ProcessNewLocations() || _tileManager.DeleteLots(ToDelete()))
                _tileManager.Tilemap.RefreshAllTiles(); }

        // ************************************* Info Strings *************************************

        public int Population => _talkOfTheTown.AgentsVitalStatusIndex.RowsMatching(VitalStatus.Alive).Count();

        public string SelectedLocation() => _tileManager.SelectedLot is null ? null :
            SelectedLotToString((Vector2Int)_tileManager.SelectedLot);

        private LocationRow RowAtLot(Vector2Int selectedLot) => 
            _talkOfTheTown.LocationsPositionIndex[selectedLot];

        private string SelectedLotToString(Vector2Int selectedLot) {
            var locationRow = RowAtLot(selectedLot);
            return LocationInfo(locationRow) + EveryoneAtLocation(locationRow); }

        private static string LocationInfo(LocationRow row) =>
            $"{row.Item1} ({row.Item2}) located at x: {row.Item4.x}, y: {row.Item4.y}\n" +
            $"Founded on {row.Item5} ({TalkOfTheTown.Time.YearsSince(row.Item5)} years ago)\n";

        private string EveryoneAtLocation(LocationRow row) =>
            PeopleAtLocation(row.Item1) + BuriedAtCemetery(row.Item2);

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