using System;
using System.Linq;
using TED;
using TED.Interpreter;
using TED.Tables;
using VdlV.Time;
using VdlV.Utilities;
using VdlV.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace VdlV.Simulator {
    using static Variables;
    using static CsvParsing;

    /// <summary>
    /// All static tables (Datalog style EDBs - in TED these can be extensional or intensional).
    /// </summary>
    public static class StaticTables {
        // ***************************************** Names ****************************************
        public static TablePredicate<string> FemaleNames;
        public static TablePredicate<string> MaleNames;
        public static TablePredicate<string> Surnames;

        // ***************************************** Enums ****************************************
        //public static TablePredicate<Facet> Facets;
        public static TablePredicate<Vocation> Jobs;

        // ************************************* Primordial(s) ************************************
        public static TablePredicate<Person, int, Date, Sex, Sexuality> PrimordialBeing;
        public static TablePredicate<Location, LocationType, Vector2Int, TimePoint> PrimordialLocation;

        // ************************************* General Info *************************************
        public static TablePredicate<LocationType, LocationCategory, DailyOperation, Schedule> LocationInformation;
        private static TablePredicate<Vocation, LocationType> _vocationLocations;
        private static TablePredicate<TimeOfDay, DailyOperation> _operatingTimes;
        public static TablePredicate<LocationType, Vocation, TimeOfDay> VocationShift;
        public static TablePredicate<Vocation, int> PositionsPerJob;
        public static TablePredicate<ActionType, LocationCategory> ActionToCategory;
        // for GUI:
        private static TablePredicate<LocationCategory, Color> _categoryColors;
        private static TablePredicate<LocationType, Color> _locationColors;
        public static KeyIndex<(LocationType, Color), LocationType> LocationColorsIndex;

        // ************************************** Collections *************************************
        public static readonly LocationType[] permanentLocationTypes = {
            LocationType.Cemetery, LocationType.CityHall, LocationType.DayCare,
            LocationType.FireStation, LocationType.Hospital, LocationType.School
        };

        public static void InitStaticTables() {
            FemaleNames = FromCsv("FemaleNames", Csv("female_names"), firstName);
            MaleNames = FromCsv("MaleNames", Csv("male_names"), firstName);
            Surnames = FromCsv("Surnames", Csv("english_surnames"), lastName);

            //Facets = EnumTable("Facets", facet);
            Jobs = EnumTable("Jobs", job);

            PrimordialBeing = FromCsv("PrimordialBeing", Csv("agents"), 
                person, age, dateOfBirth, sex, sexuality);
            PrimordialLocation = FromCsv("PrimordialLocation", Csv("locations"),
                location, locationType, position, founding);

            LocationInformation = FromCsv("LocationInformation", Csv("locationInformation"),
                locationType.Key, locationCategory.Indexed, operation, schedule);
            _vocationLocations = FromCsv("VocationLocations", Csv("vocationLocations"), job.Indexed, locationType.Indexed);
            _operatingTimes = FromCsv("OperatingTimes", Csv("operatingTimes"), timeOfDay, operation);
            VocationShift = Predicate("VocationShift", locationType.Indexed, job.Indexed, timeOfDay);
            VocationShift.Initially.Where(_vocationLocations, LocationInformation, _operatingTimes);
            PositionsPerJob = FromCsv("PositionsPerJob", Csv("positionsPerJob"), job.Key, positions); // per time of day
            ActionToCategory = FromCsv("ActionToCategory", Csv("actionCategories"), actionType, locationCategory);

            _categoryColors = FromCsv("CategoryColors", Csv("locationColors"), locationCategory.Key, color);
            _locationColors = Predicate("LocationColors", locationType.Key, color);
            _locationColors.Initially.Where(LocationInformation, _categoryColors);
            LocationColorsIndex = _locationColors.KeyIndex(locationType);
        }

        private static TablePredicate<T> EnumTable<T>(string name, IColumnSpec<T> column) where T : Enum {
            var table = Predicate(name, column);
            table.AddRows(Enum.GetValues(typeof(T)).Cast<T>());
            return table;
        }
    }
}
