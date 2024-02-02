using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace VdlV.Utilities {
    using static CultureInfo;
    using static DateTime;
    using static Regex;
    using static Thread;

    /// <summary>
    /// Handles display functions like TitleCase, special case list joins, and shortening table names
    /// </summary>
    public static class StringProcessing {
        private static readonly TextInfo TextInfo = CurrentThread.CurrentCulture.TextInfo;

        public static string Title(string title) => TextInfo.ToTitleCase(title);

        private static string StandardizeSpacing(string str) =>
            string.Join(" ", Split(str, @"(?=[A-Z][^A-Z])"));

        public static string Heading(string heading) => Title(StandardizeSpacing(heading));

        private static string ListWithRows(IEnumerable<string> strings, int numInRow) =>
            // Concat an empty string to the beginning, takes place of title
            string.Join(",\n", new[] { "" }.Concat(strings).ToList().Select((s, i) => new { s, i })
                                           .ToLookup(str => str.i / numInRow, str => str.s).Select(
                                                // remove the extra ", " that gets joined on the empty string
                                                row => string.Join(", ", row)))[2..];
        public static string ListWithRows(string title, IEnumerable<string> strings, int numInRow) =>
            $"{title}: {ListWithRows(strings, numInRow)}";

        public static string NowString => Now.ToString("yyyy-MM-dd_HH-mm-ss");

        public static string QuoteString(string toQuote) => $"\"{toQuote}\"";

        public static int[] CommaSeparatedInts(string intsString) =>
            CommaSeparated(intsString, s => int.Parse(s, InvariantCulture));
        public static sbyte[] CommaSeparatedSbytes(string sbytesString) =>
            CommaSeparated(sbytesString, s => sbyte.Parse(s, InvariantCulture));

        public static T[] CommaSeparated<T>(string commaList, Func<string, T> parseFunc) =>
            (from i in commaList.Split(',') select parseFunc(i)).ToArray();



        #region Table Display Naming

        public static string TableShortName(string tableName, int maxLength) =>
            // Special case for add and initially as they can produce identical names when
            // naively shortening the table name...
            ParseIfAddOrInit(tableName, out var nameParse) ?
                nameParse.isAdd ? AddTable(nameParse.name, maxLength)
                    : InitiallyTable(nameParse.name, maxLength)
                // Update and Regular tables just get built and set to max length as needed
                : ShortenToMaxLength(ParseIfUpdate(tableName, out var updateParse)
                    ? UpdateTable(updateParse.name, updateParse.updateField) : tableName, maxLength);

        private static bool ParseIfAddOrInit(string tableName, out (string name, bool isAdd) nameParse) {
            var tableSplit = tableName.Split("__");
            nameParse = tableSplit.Length > 1 ?
                            new ValueTuple<string, bool>(tableSplit[0], tableSplit[1] == "add") :
                            (null, false);
            return tableSplit.Length > 1;
        }

        private static bool ParseIfUpdate(string tableName, out (string name, string updateField) nameParse) {
            var tableSplit = tableName.Split("_");
            nameParse = tableSplit.Length > 2 ?
                            new ValueTuple<string, string>(tableSplit[0], tableSplit[1]) :
                            (null, null);
            return tableSplit.Length > 2;
        }

        private static string ShortenToMaxLength(string name, int maxLength) =>
            name.Length > maxLength ? $"{name[..(maxLength - 1)]}…" : name;

        private static string AffixWithMaxLength(string name, string affix, int maxLength) =>
            $"{ShortenToMaxLength(name, maxLength - affix.Length)}{affix}";

        private static string AddTable(string name, int maxLength) => AffixWithMaxLength(name, "Add", maxLength);

        private static string InitiallyTable(string name, int maxLength) =>
            name.Length >= maxLength - 4 ? AffixWithMaxLength(name, "Init", maxLength) :
                ShortenToMaxLength($"{name}Initially", maxLength);

        private static string UpdateTable(string name, string updateField) => $"{name}{Title(updateField)}Update";

        #endregion
    }
}
