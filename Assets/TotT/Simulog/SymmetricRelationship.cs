using System;
using TED.Interpreter;
using TED;
using static TED.Language;
using TotT.Unity;

namespace TotT.Simulog {
    using static SimuLang;

    public class SymmetricRelationship<T> : TablePredicate<SymmetricTuple<T>, T, T, bool> where T : IComparable<T>, IEquatable<T> {
        public readonly Event<T, T> Start;
        public readonly Event<T, T> End;

        public SymmetricRelationship(string name, Var<SymmetricTuple<T>> pair, Var<T> main, Var<T> other, Var<bool> state) :
            base(name, pair.Key, main.Indexed, other.Indexed, state.Indexed) {
            Start = Event($"{name}Start", main, other);
            End = Event($"{name}End", main, other);

            Add[pair, main, other, true].If(Start, SymmetricTuple<T>.NewSymmetricTuple[main, other, pair], 
                                            !this[pair, main, other, __], SymmetricTuple<T>.InOrder[pair, main, other]);
            Set(pair, state, false).If(End, Or[this[pair, main, other, true], 
                                                                     this[pair, other, main, true]]);
            Set(pair, state, true).If(Start, Or[this[pair, main, other, false], 
                                                                      this[pair, other, main, false]]);
            this.Colorize(state);
        }

        public SymmetricRelationship<T> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public SymmetricRelationship<T> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public SymmetricRelationship<T> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public SymmetricRelationship<T> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        private SymmetricRelationshipChronicle<T> _chronicle;
        private readonly Var<SymmetricRelationshipInstance<T>> _pair = (Var<SymmetricRelationshipInstance<T>>)"pair";

        // ReSharper disable once MemberCanBePrivate.Global
        public SymmetricRelationshipChronicle<T> Chronicle {
            get {
                _chronicle ??= new SymmetricRelationshipChronicle<T>(Name + "Chronicle", _pair, (Var<T>)DefaultVariables[1], (Var<T>)DefaultVariables[2]);
                _chronicle.StartWhen(Add);
                _chronicle.StartWhen(Set((Var<SymmetricTuple<T>>)DefaultVariables[0], (Var<bool>)DefaultVariables[3], true), this);
                _chronicle.EndWhen(Set((Var<SymmetricTuple<T>>)DefaultVariables[0], (Var<bool>)DefaultVariables[3], false), this);
                return _chronicle;
            }
        }

        public Goal this[Term<T> main, Term<T> other] => Or[
            new TableGoal<SymmetricTuple<T>, T, T, bool>(this, __, main, other, true), 
            new TableGoal<SymmetricTuple<T>, T, T, bool>(this, __, other, main, true)];
    }

    public class ExclusiveSymmetricRelationship<T> : TablePredicate<SymmetricTuple<T>, T, T, bool> where T : IComparable<T>, IEquatable<T> {
        public readonly Event<T, T> Start;
        public readonly Event<T, T> End;

        public ExclusiveSymmetricRelationship(string name, Var<SymmetricTuple<T>> pair, Var<T> main, Var<T> other, Var<bool> state) :
            base(name, pair, main, other, state) {
            Start = Event($"{name}Start", main, other);
            End = Event($"{name}End", main, other);

            Add[pair, main, other, true].If(Start, !this[__, main, __, true], !this[__, other, __, true],
                                            !this[__, __, main, true], !this[__, __, other, true], !this[pair, main, other, __],
                                            SymmetricTuple<T>.NewSymmetricTuple[main, other, pair], SymmetricTuple<T>.InOrder[pair, main, other]);
            Set(pair, state, false).If(End, Or[this[pair, main, other, true],
                                                                     this[pair, other, main, true]]);
            Set(pair, state, true).If(Start, Or[this[pair, main, other, false],
                                                                      this[pair, other, main, false]]);
            this.Colorize(state);
        }

        public ExclusiveSymmetricRelationship<T> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public ExclusiveSymmetricRelationship<T> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public ExclusiveSymmetricRelationship<T> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public ExclusiveSymmetricRelationship<T> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        private SymmetricRelationshipChronicle<T> _chronicle;
        private readonly Var<SymmetricRelationshipInstance<T>> _pair = (Var<SymmetricRelationshipInstance<T>>)"pair";

        // ReSharper disable once MemberCanBePrivate.Global
        public SymmetricRelationshipChronicle<T> Chronicle {
            get {
                _chronicle ??= new SymmetricRelationshipChronicle<T>(Name + "Chronicle", _pair, (Var<T>)DefaultVariables[1], (Var<T>)DefaultVariables[2]);
                _chronicle.StartWhen(Add);
                _chronicle.StartWhen(Set((Var<SymmetricTuple<T>>)DefaultVariables[0], (Var<bool>)DefaultVariables[3], true), this);
                _chronicle.EndWhen(Set((Var<SymmetricTuple<T>>)DefaultVariables[0], (Var<bool>)DefaultVariables[3], false), this);
                return _chronicle;
            }
        }

        public Goal this[Term<T> main, Term<T> other] => Or[
            new TableGoal<SymmetricTuple<T>, T, T, bool>(this, __, main, other, true),
            new TableGoal<SymmetricTuple<T>, T, T, bool>(this, __, other, main, true)];
    }
}
