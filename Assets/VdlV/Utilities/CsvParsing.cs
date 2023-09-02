using System;
using System.Linq;
using TED.Utilities;
using VdlV.Time;
using VdlV.ValueTypes;
using UnityEngine;

namespace VdlV.Utilities {
    using static Randomize;

    /// <summary>
    /// Handles declaring CsvReader parsers for all relevant ValueTypes and gives a function
    /// for passing in the name of a CSV file and mapping to that file in the Data folder.
    /// </summary>
    public static class CsvParsing {
        public static string Csv(string filename) => $"Assets/Data/{filename}.csv";

        private static readonly System.Random PersonalityRng;
        static CsvParsing() { PersonalityRng = MakeRng(); }

        public static void DeclareParsers() {
            CsvReader.DeclareParser(typeof(Vector2Int), ParseVector2Int);
            CsvReader.DeclareParser(typeof(Color), ParseColor);
            CsvReader.DeclareParser(typeof(Date), ParseDate);
            CsvReader.DeclareParser(typeof(TimePoint), ParseTimePoint);
            CsvReader.DeclareParser(typeof(Schedule), ParseSchedule);
            CsvReader.DeclareParser(typeof(Sexuality), ParseSexuality);
            CsvReader.DeclareParser(typeof(Person), ParsePerson);
            CsvReader.DeclareParser(typeof(Location), ParseLocation);
        }

        private static object ParsePerson(string personString) => Person.FromString(personString, PersonalityRng);
        private static object ParseLocation(string locationString) => Location.FromString(locationString);
        private static object ParseDate(string dateString) => Date.FromString(dateString);
        private static object ParseTimePoint(string dateString) => TimePoint.FromString(dateString);
        private static object ParseSchedule(string scheduleString) => Schedule.FromString(scheduleString);
        private static object ParseSexuality(string sexualityString) => Sexuality.FromString(sexualityString);

        private static object ParseColor(string htmlColorString) =>
            ColorUtility.TryParseHtmlString(htmlColorString, out var color) ? color : Color.white;

        private static object ParseVector2Int(string vector2String) {
            var ints = CommaSeparatedInts(vector2String);
            return ints.Length == 2 ? new Vector2Int(ints[0], ints[1]) :
                       throw new ArgumentOutOfRangeException(
                           $"Expecting 2 comma separated ints for Vector2Int, {ints.Length} provided");
        }
        private static int[] CommaSeparatedInts(string intsString) => 
            (from i in intsString.Split(',') select int.Parse(i)).ToArray();
    }
}
