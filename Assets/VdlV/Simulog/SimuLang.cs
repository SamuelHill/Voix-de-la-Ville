using System;
using TED;
using TED.Interpreter;
using VdlV.Simulator;
using VdlV.Time;

namespace VdlV.Simulog {
    using static Variables;
    
    public static class SimuLang {
        // Only two types of effects - nothing for initially, no deletions (yet)
        public static Effect Set<TKey, TCol>(TablePredicate table, Var<TKey> key, 
            Var<TCol> column, Term<TCol> newValue) => Effect.Set(table, key, column, newValue);
        public static Effect Add(TableGoal tableGoal) => Effect.Add(tableGoal);

        #region Event constructors
        public static Event<T1> Event<T1>(string name, IColumnSpec<T1> arg) => new(name, arg);
        public static Event<T1, T2> Event<T1, T2>(string name, 
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) => new(name, arg1, arg2);
        public static Event<T1, T2, T3> Event<T1, T2, T3>(string name, 
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) => new(name, arg1, arg2, arg3);
        public static Event<T1, T2, T3, T4> Event<T1, T2, T3, T4>(string name, 
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
            IColumnSpec<T4> arg4) => new(name, arg1, arg2, arg3, arg4);
        public static Event<T1, T2, T3, T4, T5> Event<T1, T2, T3, T4, T5>(string name, 
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4,
            IColumnSpec<T5> arg5) => new(name, arg1, arg2, arg3, arg4, arg5);
        public static Event<T1, T2, T3, T4, T5, T6> Event<T1, T2, T3, T4, T5, T6>(string name,
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
            IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
            => new(name, arg1, arg2, arg3, arg4, arg5, arg6);
        public static Event<T1, T2, T3, T4, T5, T6, T7> Event<T1, T2, T3, T4, T5, T6, T7>(string name,
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4,
            IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            => new(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        #endregion

        #region Exists constructors
        // can't pass start Variable in as default argument (not compile time constant)...
        public static Existent<T> Exists<T>(string name, Var<T> arg) => new(name, arg, start);
        public static Existent<T> Exists<T>(string name, Var<T> arg, Var<TimePoint> startArg) => new(name, arg, startArg);

        public static Existent<T, T1> Exists<T, T1>(string name, Var<T> arg, IColumnSpec<T1> feature1) =>
            new(name, arg, start, feature1);
        public static Existent<T, T1> Exists<T, T1>(string name, Var<T> arg, IColumnSpec<T1> feature1, Var<TimePoint> startArg) =>
            new(name, arg, startArg, feature1);

        public static Existent<T, T1, T2> Exists<T, T1, T2>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2) =>
            new(name, arg, start, feature1, feature2);
        public static Existent<T, T1, T2> Exists<T, T1, T2>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, Var<TimePoint> startArg) =>
            new(name, arg, startArg, feature1, feature2);

        public static Existent<T, T1, T2, T3> Exists<T, T1, T2, T3>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3) =>
            new(name, arg, start, feature1, feature2, feature3);
        public static Existent<T, T1, T2, T3> Exists<T, T1, T2, T3>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, Var<TimePoint> startArg) =>
            new(name, arg, startArg, feature1, feature2, feature3);

        public static Existent<T, T1, T2, T3, T4> Exists<T, T1, T2, T3, T4>(string name, Var<T> arg,
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4) =>
            new(name, arg, start, feature1, feature2, feature3, feature4);
        public static Existent<T, T1, T2, T3, T4> Exists<T, T1, T2, T3, T4>(string name, Var<T> arg,
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, Var<TimePoint> startArg) =>
            new(name, arg, startArg, feature1, feature2, feature3, feature4);

        public static Existent<T, T1, T2, T3, T4, T5> Exists<T, T1, T2, T3, T4, T5>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
            IColumnSpec<T5> feature5) =>
            new(name, arg, start, feature1, feature2, feature3, feature4, feature5);
        public static Existent<T, T1, T2, T3, T4, T5> Exists<T, T1, T2, T3, T4, T5>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
            IColumnSpec<T5> feature5, Var<TimePoint> startArg) =>
            new(name, arg, startArg, feature1, feature2, feature3, feature4, feature5);

        public static Existent<T, T1, T2, T3, T4, T5, T6> Exists<T, T1, T2, T3, T4, T5, T6>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
            IColumnSpec<T5> feature5, IColumnSpec<T6> feature6) =>
            new(name, arg, start, feature1, feature2, feature3, feature4, feature5, feature6);
        public static Existent<T, T1, T2, T3, T4, T5, T6> Exists<T, T1, T2, T3, T4, T5, T6>(string name, Var<T> arg,
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
            IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, Var<TimePoint> startArg) =>
            new(name, arg, startArg, feature1, feature2, feature3, feature4, feature5, feature6);

        public static Existent<T, T1, T2, T3, T4, T5, T6, T7> Exists<T, T1, T2, T3, T4, T5, T6, T7>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
            IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7) => 
            new(name, arg, start, feature1, feature2, feature3, feature4, feature5, feature6, feature7);
        public static Existent<T, T1, T2, T3, T4, T5, T6, T7> Exists<T, T1, T2, T3, T4, T5, T6, T7>(string name, Var<T> arg, 
            IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
            IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7, Var<TimePoint> startArg) => 
            new(name, arg, startArg, feature1, feature2, feature3, feature4, feature5, feature6, feature7);
        #endregion

        public static Affinity<T1, T2> Affinity<T1, T2>(string name, Var<(T1, T2)> pair, Var<T1> main, Var<T2> other, Var<int> value)
            where T1 : IComparable<T1>, IEquatable<T1> where T2 : IComparable<T2>, IEquatable<T2> => new(name, pair, main, other, value);
        public static FloatAffinity<T1, T2> Affinity<T1, T2>(string name, Var<(T1, T2)> pair, Var<T1> main, Var<T2> other, Var<float> value) 
            where T1 : IComparable<T1>, IEquatable<T1> where T2 : IComparable<T2>, IEquatable<T2> => new(name, pair, main, other, value);

        public static Relationship<T1, T2> Relationship<T1, T2>(string name, Var<(T1, T2)> pair, Var<T1> main, Var<T2> other, Var<bool> state) 
            where T1 : IComparable<T1>, IEquatable<T1> where T2 : IComparable<T2>, IEquatable<T2> => new(name, pair, main, other, state);
        public static SymmetricRelationship<T> Relationship<T>(string name, Var<SymmetricTuple<T>> pair, 
            Var<T> main, Var<T> other, Var<bool> state) where T : IComparable<T>, IEquatable<T> => new(name, pair, main, other, state);
        public static ExclusiveSymmetricRelationship<T> ExclusiveRelationship<T>(string name, Var<SymmetricTuple<T>> pair, 
            Var<T> main, Var<T> other, Var<bool> state) where T : IComparable<T>, IEquatable<T> => new(name, pair, main, other, state);

        public static RandomAssign<T1, T2> Assign<T1, T2>(string name, Var<T1> arg1, IColumnSpec<T2> arg2) => new(name, arg1, arg2);
        public static IntAssign<T1, T2> Assign<T1, T2>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                                                       IColumnSpec<int> arg3) => new(name, arg1, arg2, arg3);
        public static IntAssign<T1, T2> Assign<T1, T2>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
            IColumnSpec<int> arg3, TablePredicate<T2, int> capacities) => new(name, arg1, arg2, arg3, capacities);
        public static FloatAssign<T1, T2> Assign<T1, T2>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2,
                                                         IColumnSpec<float> arg3) => new(name, arg1, arg2, arg3);
        public static FloatAssign<T1, T2> Assign<T1, T2>(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2,
            IColumnSpec<float> arg3, TablePredicate<T2, int> capacities) => new(name, arg1, arg2, arg3, capacities);
        
        public static IntMatch<T> Match<T>(string name, IColumnSpec<T> arg1, IColumnSpec<T> arg2,
                                           IColumnSpec<int> arg3) => new(name, arg1, arg2, arg3, true);
        public static FloatMatch<T> Match<T>(string name, IColumnSpec<T> arg1, IColumnSpec<T> arg2, 
                                             IColumnSpec<float> arg3) => new(name, arg1, arg2, arg3, true);

        public static IntMatch<T> MatchAsymmetric<T>(string name, IColumnSpec<T> arg1, IColumnSpec<T> arg2, 
                                                     IColumnSpec<int> arg3) => new(name, arg1, arg2, arg3, false);
        public static FloatMatch<T> MatchAsymmetric<T>(string name, IColumnSpec<T> arg1, IColumnSpec<T> arg2,
                                                       IColumnSpec<float> arg3) => new(name, arg1, arg2, arg3, false);

        #region Counts (CountsBy wrapper)
        public static Counts<T1, T2> Counts<T1, T2>(string name, Var<T1> countBy, 
            IColumnSpec<T2> arg2, Var<int> count) => new(name, countBy, arg2, count);
        public static Counts<T1, T2> Counts<T1, T2>(string name, Var<T1> countBy, 
            IColumnSpec<T2> arg2) => new(name, countBy, arg2, count);
        public static Counts<T1, T2, T3> Counts<T1, T2, T3>(string name, Var<T1> countBy, 
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, Var<int> count) => new(name, countBy, arg2, arg3, count);
        public static Counts<T1, T2, T3> Counts<T1, T2, T3>(string name, Var<T1> countBy, 
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) => new(name, countBy, arg2, arg3, count);
        public static Counts<T1, T2, T3, T4> Counts<T1, T2, T3, T4>(string name, Var<T1> countBy, 
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, Var<int> count) 
            => new(name, countBy, arg2, arg3, arg4, count);
        public static Counts<T1, T2, T3, T4> Counts<T1, T2, T3, T4>(string name, Var<T1> countBy, 
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
            => new(name, countBy, arg2, arg3, arg4, count);
        public static Counts<T1, T2, T3, T4, T5> Counts<T1, T2, T3, T4, T5>(string name,
            Var<T1> countBy, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, 
            IColumnSpec<T5> arg5, Var<int> count) => new(name, countBy, arg2, arg3, arg4, arg5, count);
        public static Counts<T1, T2, T3, T4, T5> Counts<T1, T2, T3, T4, T5>(string name,
            Var<T1> countBy, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4,
            IColumnSpec<T5> arg5) => new(name, countBy, arg2, arg3, arg4, arg5, count);
        public static Counts<T1, T2, T3, T4, T5, T6> Counts<T1, T2, T3, T4, T5, T6>(string name,
            Var<T1> countBy, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4,
            IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, Var<int> count)
            => new(name, countBy, arg2, arg3, arg4, arg5, arg6, count);
        public static Counts<T1, T2, T3, T4, T5, T6> Counts<T1, T2, T3, T4, T5, T6>(string name,
            Var<T1> countBy, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4,
            IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
            => new(name, countBy, arg2, arg3, arg4, arg5, arg6, count);
        public static Counts<T1, T2, T3, T4, T5, T6, T7> Counts<T1, T2, T3, T4, T5, T6, T7>(string name, 
            Var<T1> countBy, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4,
            IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, Var<int> count) 
            => new(name, countBy, arg2, arg3, arg4, arg5, arg6, arg7, count);
        public static Counts<T1, T2, T3, T4, T5, T6, T7> Counts<T1, T2, T3, T4, T5, T6, T7>(string name,
            Var<T1> countBy, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4,
            IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            => new(name, countBy, arg2, arg3, arg4, arg5, arg6, arg7, count);
        #endregion
    }
}
