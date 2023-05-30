using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace TotT.Utilities {
    public static class StringProcessing {
        private static readonly CultureInfo CultureInfo = Thread.CurrentThread.CurrentCulture;
        private static readonly TextInfo TextInfo = CultureInfo.TextInfo;
        private const string Initially = "Initially";
        private const string Update = "Update";

        public static string Title(string title) => TextInfo.ToTitleCase(title);

        private static string VariableSpacing(string variable) =>
            string.Join(" ", Regex.Split(variable, @"(?=[A-Z][^A-Z])"));
        public static string Heading(string heading) => Title(VariableSpacing(heading));

        private static string ListWithRows(IEnumerable<string> strings, int numInRow) =>
            // Concat an empty string to the beginning, takes place of title
            string.Join(",\n", new[] { "" }.Concat(strings).ToList().Select(
                (s, i) => new { s, i }).ToLookup(
                str => str.i / numInRow, str => str.s).Select(
                // remove the extra ", " that gets joined on the empty string
                row => string.Join(", ", row)))[2..];
        public static string ListWithRows(string title, IEnumerable<string> strings, int numInRow) =>
            $"{title}: {ListWithRows(strings, numInRow)}";

        public static string TableShortName(string tableName, int maxLength) =>
            // Special case for add and initially as they can produce identical names when
            // naively shortening the table name...
            CheckAddOrInit(tableName, out var nameParse) ?
                nameParse.isAdd ? AddTableName(nameParse.name, maxLength)
                    : InitiallyTableName(nameParse.name, maxLength)
                // Update and Regular tables just get built and set to max length as needed
                : ToLength(CheckUpdate(tableName, out var updateParse) 
                    ? UpdateTable(updateParse.name, updateParse.updateField) : tableName, maxLength);

        private static bool CheckAddOrInit(string tableName, out (string name, bool isAdd) nameParse) {
            var tableSplit = tableName.Split("__");
            nameParse = tableSplit.Length > 1 ? 
                new ValueTuple<string, bool>(tableSplit[0], tableSplit[1] == "add") : 
                (null, false); // will be ignored if using the returned bool
            return tableSplit.Length > 1; }
        private static string AddTableName(string name, int maxLength) => 
            name.Length >= maxLength - 4 ? $"{name[..(maxLength - 4)]}…Add" : $"{name}Add";
        private static string InitiallyTableName(string name, int maxLength) =>
            name.Length > maxLength - 4 ? $"{ToLength(name, maxLength - 5)}Init" :
            name.Length == maxLength - 4 ? $"{name}Init" :
            name.Length + Initially.Length > maxLength ? 
                $"{name}{ToLength(Initially, maxLength - name.Length - 1)}" : 
                $"{name}{Initially}";

        private static bool CheckUpdate(string tableName, out (string name, string updateField) nameParse) {
            var tableSplit = tableName.Split("_");
            nameParse = tableSplit.Length > 2 ?
                new ValueTuple<string, string>(tableSplit[0], tableSplit[1]) :
                (null, null);
            return tableSplit.Length > 2; }
        private static string UpdateTable(string name, string updateField) =>
            $"{name}{Title(updateField)}{Update}";

        private static string ToLength(string name, int maxLength) => 
            name.Length > maxLength ? $"{name[..(maxLength - 1)]}…" : name;
    }
}