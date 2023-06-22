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

    public class Relationship<T> : TablePredicate<OrderedPair<T>, T, T, bool, TimePoint, TimePoint> where T : IComparable<T>, IEquatable<T> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        private static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TimePoint> _start = (Var<TimePoint>)"start";
        private static readonly Var<TimePoint> _end = (Var<TimePoint>)"end";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        public readonly TablePredicate<OrderedPair<T>, T, T> Start;
        public readonly TablePredicate<OrderedPair<T>, T, T> End;

        public Relationship(string name, Var<OrderedPair<T>> pairVar, Var<T> main, Var<T> other) :
            base(name, pairVar.Key, main.Indexed, other.Indexed, _exists.Indexed, _start, _end) {
            Start = new TablePredicate<OrderedPair<T>, T, T>($"{name}Start", pairVar, main, other);
            End = new TablePredicate<OrderedPair<T>, T, T>($"{name}End", pairVar, main, other);
            Add[pairVar, main, other, true, time, TimePoint.Eschaton].If(Start, TalkOfTheTown.Time.CurrentTimePoint[time]);
            Set(pairVar, _end, time).If(End, TalkOfTheTown.Time.CurrentTimePoint[time]);
            Set(pairVar, _exists, false).If(End);
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
        }

        public Relationship<T> StartWhen(params Goal[] conditions) {
            Start.If(conditions);
            return this;
        }

        public Relationship<T> EndWhen(params Goal[] conditions) {
            End.If(conditions);
            return this;
        }

        public Relationship<T> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<OrderedPair<T>>)DefaultVariables[0], (Var<T>)DefaultVariables[1], 
                      (Var<T>)DefaultVariables[2], true, time, TimePoint.Eschaton].Where(conditions);
            return this;
        }

        public Relationship<T> StartWith(params Goal[] conditions) {
            Add[(Var<OrderedPair<T>>)DefaultVariables[0], (Var<T>)DefaultVariables[1],
                (Var<T>)DefaultVariables[2], true, time, TimePoint.Eschaton].If(conditions);
            return this;
        }

        public RelationshipGoal this[Term<OrderedPair<T>> arg1, Term<T> arg2, Term<T> arg3] => 
            new(this, arg1, arg2, arg3, true, __, __);

        public class RelationshipGoal : TableGoal<OrderedPair<T>, T, T, bool, TimePoint, TimePoint> {
            public RelationshipGoal(TablePredicate predicate, Term<OrderedPair<T>> arg1, Term<T> arg2,
                                    Term<T> arg3, Term<bool> arg4, Term<TimePoint> arg5, Term<TimePoint> arg6)
                : base(predicate, arg1, arg2, arg3, arg4, arg5, arg6) { }

            public RelationshipGoal Ended => new(TablePredicate, Arg1, Arg2, Arg3, false, Arg5, Arg6);

            public RelationshipGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, Arg2, Arg3, __, t, Arg6);
            public RelationshipGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, Arg2, Arg3, __, Arg5, t);
        }

    }
}
