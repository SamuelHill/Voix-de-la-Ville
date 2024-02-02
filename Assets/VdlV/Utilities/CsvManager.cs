using System;
using System.Numerics;
using TED.Utilities;
using VdlV.Time;
using VdlV.ValueTypes;
using UnityEngine;
using VdlV.Simulog;

namespace VdlV.Utilities {
    using static ColorUtility;
    using static CsvReader;
    using static CsvWriter;
    using static Randomize;
    using static SaveManager;
    using static StringProcessing;

    /// <summary>
    /// Handles declaring CsvReader parsers for all relevant ValueTypes and gives a function
    /// for passing in the name of a CSV file and mapping to that file in the Data folder.
    /// </summary>
    public static class CsvManager {
        private static readonly System.Random PersonalityRng;
        static CsvManager() => PersonalityRng = MakeRng();

        public static string CsvDataFile(string filename) => $"Assets/Data/{filename}.csv";

        public static void DeclareParsers() {
            DeclareParser(typeof(Person), ParsePerson);
            DeclareParser(typeof(Location), ParseLocation);
            DeclareParser(typeof(ValueTuple<Person, Person>), ParsePersonTuple);
            DeclareParser(typeof(SymmetricTuple<Person>), ParseSymmetricPersonTuple);
            DeclareParser(typeof(Date), ParseDate);
            DeclareParser(typeof(TimePoint), ParseTimePoint);
            DeclareParser(typeof(Schedule), ParseSchedule);
            DeclareParser(typeof(Sexuality), ParseSexuality);
            DeclareParser(typeof(Vector<sbyte>), ParseSbyteVector);
            DeclareParser(typeof(Vector2Int), ParseVector2Int);
            DeclareParser(typeof(Color), ParseColor);
        }

        private static Person ParsePerson(string personString) => (Person)DeserializeIfId(personString, Person.FromString, PersonalityRng);
        private static Location ParseLocation(string locationString) => (Location)DeserializeIfId(locationString, Location.FromString);

        // TODO: Automatically generate SymmetricTuples and ValueTuples parse functions for any potential pairings OR
        // TODO: Generate these as they are used (parse the program for all column types and declare parsers as needed)
        private static object ParseSymmetricPersonTuple(string symmetricPersonTupleString) =>
            SymmetricTuple<Person>.FromString(symmetricPersonTupleString, ParsePerson);
        private static object ParsePersonTuple(string personTupleString) {
            var temp = CommaSeparated(personTupleString, ParsePerson);
            return temp is { Length: 2 } ? new ValueTuple<Person, Person>(temp[0], temp[1]) :
                       throw new ArgumentException($"Couldn't convert string {personTupleString} to a ValueTuple<Person, Person>");
        }

        // TODO: Use the ISerializableValue interface that all of these implement...
        private static object ParseDate(string dateString) => Date.FromString(dateString);
        private static object ParseTimePoint(string dateString) => TimePoint.FromString(dateString);
        private static object ParseSchedule(string scheduleString) => Schedule.FromString(scheduleString);
        private static object ParseSexuality(string sexualityString) => Sexuality.FromString(sexualityString);

        private static object ParseSbyteVector(string vectorString) {
            var sbytes = CommaSeparatedSbytes(vectorString);
            return sbytes.Length == 16 ? new Vector<sbyte>(sbytes) :
                       throw new ArgumentOutOfRangeException(
                           $"Expecting 16 comma separated sbytes for Vector<sbyte>, {sbytes.Length} provided");
        }
        private static object ParseVector2Int(string vector2String) {
            var ints = CommaSeparatedInts(vector2String);
            return ints.Length == 2 ? new Vector2Int(ints[0], ints[1]) :
                       throw new ArgumentOutOfRangeException(
                           $"Expecting 2 comma separated ints for Vector2Int, {ints.Length} provided");
        }

        private static object ParseColor(string htmlColorString) =>
            TryParseHtmlString(htmlColorString, out var color) ? color : Color.white;

        public static void DeclareWriters() {
            DeclareWriter(typeof(Person), WritePerson);
            DeclareWriter(typeof(ValueTuple<Person, Person>), WritePersonTuple);
            DeclareWriter(typeof(SymmetricTuple<Person>), WriteSymmetricPersonTuple);
            DeclareWriter(typeof(Location), WriteLocation);
            DeclareWriter(typeof(Vector<sbyte>), WriteSbyteVector);
            DeclareWriter(typeof(Vector2Int), WriteVector2Int);
            DeclareWriter(typeof(Color), WriteColor);
        }

        private static string WritePerson(object personObject) => SerializedId(personObject);
        private static string WriteLocation(object locationObject) => SerializedId(locationObject);

        // TODO: Generate the write functions (similar to the parse functions todo)
        private static string WriteSymmetricPersonTuple(object symmetricPersonTupleObject) {
            var pair = (SymmetricTuple<Person>)symmetricPersonTupleObject;
            return QuoteString($"{WritePerson(pair.Item1)}, {WritePerson(pair.Item2)}");
        }
        private static string WritePersonTuple(object personTupleObject) {
            var pair = (ValueTuple<Person, Person>)personTupleObject;
            return QuoteString($"{WritePerson(pair.Item1)}, {WritePerson(pair.Item2)}");
        }

        // These are not needed - the CsvWriter falls back on ToStrings
        private static string WriteDate(object dateObject) => ((Date)dateObject).ToString();
        private static string WriteTimePoint(object dateObject) => ((TimePoint)dateObject).ToString();
        private static string WriteSchedule(object scheduleObject) => ((Schedule)scheduleObject).ToString();
        private static string WriteSexuality(object sexualityObject) => ((Sexuality)sexualityObject).ToString();

        private static string WriteSbyteVector(object vectorObject) {
            var vec = (Vector<sbyte>)vectorObject;
            return QuoteString($"{vec.ToString().TrimStart('<').TrimEnd('>')}");
        }
        private static string WriteVector2Int(object vector2Object) {
            var vec = (Vector2Int)vector2Object;
            return QuoteString($"{vec.x}, {vec.y}");
        } 

        private static string WriteColor(object colorObject) => $"#{ToHtmlStringRGB((Color)colorObject)}";
    }
}
