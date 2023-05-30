using System;
using System.Linq;
using TED;
using TED.Interpreter;
using TED.Tables;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace TotT.Simulator {
    using static Variables;
    using static CsvParsing;

    public static class StaticTables {
        // ***************************************** Names ****************************************
        public static TablePredicate<string> FemaleNames;
        public static TablePredicate<string> MaleNames;
        public static TablePredicate<string> Surnames;
        public static TablePredicate<string> HouseNames;

        // ***************************************** Enums ****************************************
        public static TablePredicate<Facet> Facets;
        public static TablePredicate<Vocation> Jobs;

        // ************************************* Primordial(s) ************************************
        public static TablePredicate<Person, int, Date, Sex, Sexuality> PrimordialBeings;
        public static TablePredicate<Location, LocationType, Vector2Int, int, Date> PrimordialLocations;

        // ************************************* General Info *************************************
        public static TablePredicate<LocationType, LocationCategory, DailyOperation, Schedule> LocationInformation;
        private static TablePredicate<LocationCategory, Color> _categoryColors;
        private static TablePredicate<LocationType, Color> _locationColors;
        public static KeyIndex<(LocationType, Color), LocationType> LocationColorsIndex;
        // ReSharper disable once MemberCanBePrivate.Global
        public static TablePredicate<Vocation, LocationType> VocationLocations; // unused
        public static TablePredicate<LocationType, Vocation, TimeOfDay> VocationShifts;
        public static TablePredicate<Vocation, int> PositionsPerJob;
        private static TablePredicate<TimeOfDay, DailyOperation> _operatingTimes;
        public static TablePredicate<ActionType, LocationCategory> ActionToCategory;

        public static void InitStaticTables() {
            FemaleNames = FromCsv("FemaleNames", Csv("female_names"), firstName);
            MaleNames = FromCsv("MaleNames", Csv("male_names"), firstName);
            Surnames = FromCsv("Surnames", Csv("english_surnames"), lastName);
            HouseNames = FromCsv("HouseNames", Csv("house_names"), locationName);

            Facets = EnumTable("Facets", facet);
            Jobs = EnumTable("Jobs", job);

            PrimordialBeings = FromCsv("PrimordialBeings", Csv("agents"), 
                person, age, dateOfBirth, sex, sexuality);
            PrimordialLocations = FromCsv("PrimordialLocations", Csv("locations"),
                location, locationType, position, founded, opening);

            LocationInformation = FromCsv("LocationInformation", Csv("locationInformation"),
                locationType.Key, locationCategory.Indexed, operation, schedule);
            _categoryColors = FromCsv("CategoryColors", Csv("locationColors"), locationCategory.Key, color);
            _locationColors = Predicate("LocationColors", locationType.Key, color);
            _locationColors.Unique = true;
            _locationColors.Initially.Where(LocationInformation, _categoryColors);
            LocationColorsIndex = _locationColors.KeyIndex(locationType);

            VocationLocations = FromCsv("VocationLocations", Csv("vocationLocations"), job.Indexed, locationType.Indexed);
            PositionsPerJob = FromCsv("PositionsPerJob", Csv("positionsPerJob"), job.Key, positions); // per time of day
            _operatingTimes = FromCsv("OperatingTimes", Csv("operatingTimes"), timeOfDay, operation);
            VocationShifts = Predicate("VocationShifts", locationType.Indexed, job.Indexed, timeOfDay);
            VocationShifts.Initially.Where(VocationLocations, LocationInformation, _operatingTimes);

            ActionToCategory = FromCsv("ActionToCategory", Csv("actionCategories"), actionType, locationCategory);
        }

        private static TablePredicate<T> EnumTable<T>(string name, IColumnSpec<T> column) where T : Enum {
            var table = Predicate(name, column);
            table.AddRows(Enum.GetValues(typeof(T)).Cast<T>());
            return table; }
    }
}