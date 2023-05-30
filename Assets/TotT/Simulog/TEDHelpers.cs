using System.Linq;
using TED;
using TED.Interpreter;
using static TED.Language;

namespace TotT.Simulog {
    // ReSharper disable once InconsistentNaming
    public static class TEDHelpers {
        public static Goal Goals(params Goal[] goals) =>
            goals.Length == 0 ? null : goals.Aggregate((current, goal) => current & goal);

        private static Function<int, int> Incr => Function<int, int>("Incr", i => i + 1);

        public static TablePredicate<TKey, int> Increment<TKey>(TablePredicate table,
            Var<TKey> key, Var<int> column, Definition<TKey, int> assignPrevious) {
            var columnPrevious = (Var<int>)$"{column.Name}Previous";
            return table.Set(key, column).If(assignPrevious[key, columnPrevious], column == Incr[columnPrevious]);
        }
    }
}