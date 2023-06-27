using System;
using System.Linq;
using TED;
using TED.Interpreter;
using TotT.ValueTypes;

namespace TotT.Simulog {

    // ReSharper disable once InconsistentNaming
    public static class SimuLang {
        public static Goal Goals(params Goal[] goals) =>
            goals.Length == 0 ? null : goals.Aggregate((current, goal) => current & goal);

        public static Existent<T> Exists<T>(string name, Var<T> arg) => new(name, arg);
        public static Existent<T> Exists<T>(string name, Var<T> arg, Var<TimePoint> startArg) => new(name, arg, startArg);

        public static Affinity<T1, T2> Affinity<T1, T2>(string name, Var<(T1, T2)> pair, 
            Var<T1> main, Var<T2> other, Var<int> value) => new(name, pair, main, other, value);
        public static FloatAffinity<T1, T2> Affinity<T1, T2>(string name, Var<(T1, T2)> pair,
            Var<T1> main, Var<T2> other, Var<float> value) => new(name, pair, main, other, value);

        public static Relationship<T1, T2> Relationship<T1, T2>(string name, Var<(T1, T2)> pair, 
            Var<T1> main, Var<T2> other, Var<bool> state) => new(name, pair, main, other, state);

        public static Effect Set<TKey, TCol>(TablePredicate table, Var<TKey> key, Var<TCol> column, Term<TCol> newValue) => 
            Effect.Set(table, key, column, newValue);
        public static Effect Add(TableGoal tableGoal) => Effect.Add(tableGoal);

        public static Event<T1> Event<T1>(string name, IColumnSpec<T1> arg) => new(name, arg);
        public static Event<T1, T2> Event<T1, T2>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) => new(name, arg1, arg2);
        public static Event<T1, T2, T3> Event<T1, T2, T3>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) 
            => new(name, arg1, arg2, arg3);
        public static Event<T1, T2, T3, T4> Event<T1, T2, T3, T4>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
            => new(name, arg1, arg2, arg3, arg4);
        public static Event<T1, T2, T3, T4, T5> Event<T1, T2, T3, T4, T5>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5)
            => new(name, arg1, arg2, arg3, arg4, arg5);
        public static Event<T1, T2, T3, T4, T5, T6> Event<T1, T2, T3, T4, T5, T6>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
            => new(name, arg1, arg2, arg3, arg4, arg5, arg6);
        public static Event<T1, T2, T3, T4, T5, T6, T7> Event<T1, T2, T3, T4, T5, T6, T7>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            => new(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
}
