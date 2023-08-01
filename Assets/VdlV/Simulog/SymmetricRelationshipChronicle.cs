using System;
using TED;
using TED.Interpreter;
using VdlV.Simulator;
using VdlV.Time;
using VdlV.Unity;
using UnityEngine;
using static TED.Language;

namespace VdlV.Simulog {
    using static Variables;

    public class SymmetricRelationshipChronicle<T> : TablePredicate<SymmetricRelationshipInstance<T>, T, T, bool, TimePoint, TimePoint> 
        where T : IComparable<T>, IEquatable<T> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        private static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TimePoint> _start = (Var<TimePoint>)"start";
        private static readonly Var<TimePoint> _end = (Var<TimePoint>)"end";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        public readonly TablePredicate<T, T> Start;
        public readonly TablePredicate<T, T> End;

        public SymmetricRelationshipChronicle(string name, Var<SymmetricRelationshipInstance<T>> pairVar, Var<T> main, Var<T> other) :
            base(name, pairVar.Key, main.Indexed, other.Indexed, _exists.Indexed, _start, _end) {
            Start = new TablePredicate<T, T>($"{name}Start", main, other);
            End = new TablePredicate<T, T>($"{name}End", main, other);
            Add[pairVar, main, other, true, time, TimePoint.Eschaton]
               .If(Start, SymmetricRelationshipInstance<T>.NewRelationshipInstance[main, other, pairVar], Clock.CurrentTimePoint[time]);
            Set(pairVar, _end, time)
               .If(End, this[pairVar, __, __, true, __, TimePoint.Eschaton], Clock.CurrentTimePoint[time]);
            Set(pairVar, _exists, false)
               .If(End, this[pairVar, __, __, true, __, TimePoint.Eschaton]);
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
        }

        public SymmetricRelationshipChronicle<T> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<SymmetricRelationshipInstance<T>>)DefaultVariables[0], (Var<T>)DefaultVariables[1],
                      (Var<T>)DefaultVariables[2], true, time, TimePoint.Eschaton].Where(conditions);
            return this;
        }

        public SymmetricRelationshipChronicle<T> StartWhen(params Goal[] conditions) {
            Start.If(conditions);
            return this;
        }

        public SymmetricRelationshipChronicle<T> EndWhen(params Goal[] conditions) {
            End.If(conditions);
            return this;
        }

        public SymmetricRelationshipChronicle<T> StartWith(params Goal[] conditions) {
            Add[(Var<SymmetricRelationshipInstance<T>>)DefaultVariables[0], (Var<T>)DefaultVariables[1],
                (Var<T>)DefaultVariables[2], true, time, TimePoint.Eschaton].If(conditions);
            return this;
        }

        public RelationshipGoal this[Term<SymmetricRelationshipInstance<T>> arg1, Term<T> arg2, Term<T> arg3] => 
            new(this, arg1, arg2, arg3, true, __, __);

        public class RelationshipGoal : TableGoal<SymmetricRelationshipInstance<T>, T, T, bool, TimePoint, TimePoint> {
            public RelationshipGoal(TablePredicate predicate, Term<SymmetricRelationshipInstance<T>> arg1, Term<T> arg2,
                                    Term<T> arg3, Term<bool> arg4, Term<TimePoint> arg5, Term<TimePoint> arg6)
                : base(predicate, arg1, arg2, arg3, arg4, arg5, arg6) { }

            public RelationshipGoal Ended => new(TablePredicate, Arg1, Arg2, Arg3, false, Arg5, Arg6);

            public RelationshipGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, Arg2, Arg3, __, t, Arg6);
            public RelationshipGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, Arg2, Arg3, __, Arg5, t);
        }

    }
}
