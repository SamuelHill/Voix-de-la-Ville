using TED;
using TED.Interpreter;
using TED.Primitives;
using static TED.Language;

namespace TotT.Simulog {
    using static SimuLang;

    public class Relationship<T1, T2> : TablePredicate<(T1, T2), T1, T2, bool> {
        public readonly Event<T1, T2> Start;
        public readonly Event<T1, T2> End;

        private Function<T1, T2, (T1, T2)> NewTuple =>
            new($"New{Name}Tuple", (main, other) => (main, other));

        public Relationship(string name, Var<(T1, T2)> pair, Var<T1> main, Var<T2> other, Var<bool> state) :
            base(name, pair.Key, main.Indexed, other.Indexed, state.Indexed) {
            Start = Event($"{name}Start", main, other);
            End = Event($"{name}End", main, other);

            Add[pair, main, other, true].If(Start, NewTuple[main, other, pair], !this[pair, main, other, __]);
            Set(pair, state, false).If(End, this[pair, main, other, true]);
            Set(pair, state, true).If(Start, this[pair, main, other, false]);
        }

        public Relationship<T1, T2> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Relationship<T1, T2> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Relationship<T1, T2> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Relationship<T1, T2> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public RelationshipGoal this[Term<T1> main, Term<T2> other] => new(this, __, main, other, true);

        public class RelationshipGoal : TableGoal<(T1, T2), T1, T2, bool> {
            public RelationshipGoal(TablePredicate predicate, Term<(T1, T2)> pair, Term<T1> main, Term<T2> other, Term<bool> value)
                : base(predicate, pair, main, other, value) { }
        }
    }

    public class AffinityRelationship<T1, T2> : TablePredicate<(T1, T2), T1, T2, bool> {
        public readonly Event<T1, T2> Start;
        public readonly Event<T1, T2> End;

        private static PrimitiveTest<int, int> StartTransitionPoint => 
            new(nameof(StartTransitionPoint), (t, value) => t >= 0 ? value > t : value < t);
        private static PrimitiveTest<int, int> EndTransitionPoint =>
            new(nameof(EndTransitionPoint), (t, value) => t >= 0 ? value < t : value > t);

        public AffinityRelationship(string name, Affinity<T1, T2> affinity, Var<bool> state, int start, int end) :
            base(name, ((Var<(T1, T2)>)affinity.DefaultVariables[0]).Key, ((Var<T1>)affinity.DefaultVariables[1]).Indexed, 
                 ((Var<T2>)affinity.DefaultVariables[2]).Indexed, state.Indexed) {
            var pair = (Var<(T1, T2)>)affinity.DefaultVariables[0];
            var main = (Var<T1>)affinity.DefaultVariables[1];
            var other = (Var<T2>)affinity.DefaultVariables[2];
            var value = (Var<int>)affinity.DefaultVariables[3];

            Add[pair, main, other, true].If(affinity[pair, main, other, value], StartTransitionPoint[start, value], !this[pair, main, other, __]);
            Set(pair, state, false).If(affinity[pair, main, other, value], EndTransitionPoint[end, value], this[pair, main, other, true]);
            Set(pair, state, true).If(affinity[pair, main, other, value], StartTransitionPoint[start, value], this[pair, main, other, false]);

            Start = Event($"{name}Start", main, other);
            Start.OccursWhen(Add);
            Start.OccursWhen(Set(pair, state, true), this);
            End = Event($"{name}End", main, other);
            End.OccursWhen(Set(pair, state, false), this);
        }

        public AffinityRelationship<T1, T2> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }
        public AffinityRelationship<T1, T2> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public RelationshipGoal this[Term<T1> main, Term<T2> other] => new(this, __, main, other, true);

        public class RelationshipGoal : TableGoal<(T1, T2), T1, T2, bool> {
            public RelationshipGoal(TablePredicate predicate, Term<(T1, T2)> pair, Term<T1> main, Term<T2> other, Term<bool> value)
                : base(predicate, pair, main, other, value) { }
        }
    }

    public class FloatAffinityRelationship<T1, T2> : TablePredicate<(T1, T2), T1, T2, bool> {
        public readonly Event<T1, T2> Start;
        public readonly Event<T1, T2> End;

        private static PrimitiveTest<float, float> StartTransitionPoint =>
            new(nameof(StartTransitionPoint), (t, value) => t >= 0 ? value > t : value < t);
        private static PrimitiveTest<float, float> EndTransitionPoint =>
            new(nameof(EndTransitionPoint), (t, value) => t >= 0 ? value < t : value > t);

        public FloatAffinityRelationship(string name, FloatAffinity<T1, T2> affinity, Var<bool> state, float start, float end) :
            base(name, ((Var<(T1, T2)>)affinity.DefaultVariables[0]).Key, ((Var<T1>)affinity.DefaultVariables[1]).Indexed,
                 ((Var<T2>)affinity.DefaultVariables[2]).Indexed, state.Indexed) {
            var pair = (Var<(T1, T2)>)affinity.DefaultVariables[0];
            var main = (Var<T1>)affinity.DefaultVariables[1];
            var other = (Var<T2>)affinity.DefaultVariables[2];
            var value = (Var<float>)affinity.DefaultVariables[3];

            Add[pair, main, other, true].If(affinity[pair, main, other, value], StartTransitionPoint[start, value], !this[pair, main, other, __]);
            Set(pair, state, false).If(affinity[pair, main, other, value], EndTransitionPoint[end, value], this[pair, main, other, true]);
            Set(pair, state, true).If(affinity[pair, main, other, value], StartTransitionPoint[start, value], this[pair, main, other, false]);

            Start = Event($"{name}Start", main, other);
            Start.OccursWhen(Add);
            Start.OccursWhen(Set(pair, state, true), this);
            End = Event($"{name}End", main, other);
            End.OccursWhen(Set(pair, state, false), this);
        }

        public FloatAffinityRelationship<T1, T2> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }
        public FloatAffinityRelationship<T1, T2> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public RelationshipGoal this[Term<T1> main, Term<T2> other] => new(this, __, main, other, true);

        public class RelationshipGoal : TableGoal<(T1, T2), T1, T2, bool> {
            public RelationshipGoal(TablePredicate predicate, Term<(T1, T2)> pair, Term<T1> main, Term<T2> other, Term<bool> value)
                : base(predicate, pair, main, other, value) { }
        }
    }
}
