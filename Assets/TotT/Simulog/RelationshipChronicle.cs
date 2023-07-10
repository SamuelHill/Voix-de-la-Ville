using System;
using TED;
using TED.Interpreter;
using TotT.Simulator;
using TotT.Unity;
using TotT.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace TotT.Simulog {
    using static Variables;

    public class RelationshipChronicle<T1, T2> : TablePredicate<RelationshipInstance<T1, T2>, T1, T2, bool, TimePoint, TimePoint> 
        where T1 : IComparable<T1>, IEquatable<T1> where T2 : IComparable<T2>, IEquatable<T2> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        private static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TimePoint> _start = (Var<TimePoint>)"start";
        private static readonly Var<TimePoint> _end = (Var<TimePoint>)"end";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        public readonly TablePredicate<T1, T2> Start;
        public readonly TablePredicate<T1, T2> End;

        public RelationshipChronicle(string name, Var<RelationshipInstance<T1, T2>> pairVar, Var<T1> main, Var<T2> other) :
            base(name, pairVar.Key, main.Indexed, other.Indexed, _exists.Indexed, _start, _end) {
            Start = new TablePredicate<T1, T2>($"{name}Start", main, other);
            End = new TablePredicate<T1, T2>($"{name}End", main, other);
            Add[pairVar, main, other, true, time, TimePoint.Eschaton]
               .If(Start, RelationshipInstance<T1, T2>.NewRelationshipInstance[main, other, pairVar], TalkOfTheTown.Time.CurrentTimePoint[time]);
            Set(pairVar, _end, time)
               .If(End, this[pairVar, main, other, true, __, TimePoint.Eschaton], TalkOfTheTown.Time.CurrentTimePoint[time]);
            Set(pairVar, _exists, false)
               .If(End, this[pairVar, main, other, true, __, TimePoint.Eschaton]);
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
        }

        public RelationshipChronicle<T1, T2> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<RelationshipInstance<T1, T2>>)DefaultVariables[0], (Var<T1>)DefaultVariables[1],
                      (Var<T2>)DefaultVariables[2], true, time, TimePoint.Eschaton].Where(conditions);
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
                (Var<T2>)DefaultVariables[2], true, time, TimePoint.Eschaton].If(conditions);
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
