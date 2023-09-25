using System;
using TED;
using TED.Interpreter;
using UnityEngine;
using VdlV.Unity;
using VdlV.Simulator;
using VdlV.Time;
using static TED.Language;

namespace VdlV.Simulog {
    using static Clock;
    using static TimePoint;
    using static Variables;
    using static Color;

    // ReSharper disable MemberCanBePrivate.Global
    public class RelationshipChronicle<T1, T2> : TablePredicate<RelationshipInstance<T1, T2>, T1, T2, bool, TimePoint, TimePoint> 
        where T1 : IComparable<T1>, IEquatable<T1> where T2 : IComparable<T2>, IEquatable<T2> {
        public readonly TablePredicate<T1, T2> Start;
        public readonly TablePredicate<T1, T2> End;

        // ReSharper disable once InconsistentNaming
        private static readonly Var<RelationshipInstance<T1, T2>> pair = (Var<RelationshipInstance<T1, T2>>)"pair";

        public RelationshipChronicle(string name, Var<T1> main, Var<T2> other) :
            base(name, pair.Key, main.Indexed, other.Indexed, exists.Indexed, start, end) {
            Start = new TablePredicate<T1, T2>($"{name}Start", main, other);
            End = new TablePredicate<T1, T2>($"{name}End", main, other);

            Add[pair, main, other, true, time, Eschaton].If(Start, 
                RelationshipInstance<T1, T2>.NewRelationshipInstance[main, other, pair], CurrentTimePoint[time]);
            Set(pair, end, time).If(End, 
                this[pair, main, other, true, __, Eschaton], CurrentTimePoint[time]);
            Set(pair, exists, false).If(End, 
                this[pair, main, other, true, __, Eschaton]);

            this.Colorize(exists, s => s ? white : gray);
        }

        public RelationshipChronicle<T1, T2> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<RelationshipInstance<T1, T2>>)DefaultVariables[0], (Var<T1>)DefaultVariables[1],
                      (Var<T2>)DefaultVariables[2], true, time, Eschaton].Where(conditions);
            return this;
        }

        public RelationshipChronicle<T1, T2> StartWhen(params Goal[] conditions) {
            Start.If(conditions);
            return this;
        }

        public RelationshipChronicle<T1, T2> EndWhen(params Goal[] conditions) {
            End.If(conditions);
            return this;
        }

        public RelationshipChronicle<T1, T2> StartWith(params Goal[] conditions) {
            Add[(Var<RelationshipInstance<T1, T2>>)DefaultVariables[0], (Var<T1>)DefaultVariables[1],
                (Var<T2>)DefaultVariables[2], true, time, Eschaton].If(conditions);
            return this;
        }

        public RelationshipGoal this[Term<RelationshipInstance<T1, T2>> arg1, Term<T1> arg2, Term<T2> arg3] => 
            new(this, arg1, arg2, arg3, true, __, __);

        public class RelationshipGoal : TableGoal<RelationshipInstance<T1, T2>, T1, T2, bool, TimePoint, TimePoint> {
            public RelationshipGoal(TablePredicate predicate, Term<RelationshipInstance<T1, T2>> arg1, Term<T1> arg2,
                                    Term<T2> arg3, Term<bool> arg4, Term<TimePoint> arg5, Term<TimePoint> arg6)
                : base(predicate, arg1, arg2, arg3, arg4, arg5, arg6) { }

            public RelationshipGoal Ended => new(TablePredicate, Arg1, Arg2, Arg3, false, Arg5, Arg6);

            public RelationshipGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, Arg2, Arg3, __, t, Arg6);
            public RelationshipGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, Arg2, Arg3, __, Arg5, t);
        }
    }
}
