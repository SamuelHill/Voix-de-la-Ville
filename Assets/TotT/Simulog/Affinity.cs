using TED;
using TED.Interpreter;
using static TED.Language;

namespace TotT.Simulog {
    using static SimuLang;

    public class Affinity<T1, T2, TValue> : TablePredicate<(T1, T2), T1, T2, TValue> {

        public readonly Event<T1, T2, TValue> Change;

        public readonly TablePredicate<(T1, T2)> Unchanged;

        public Function<T1, T2, (T1, T2)> NewTuple => new($"New{Name}Tuple", (main, other) => (main, other));

        public Affinity(string name, Var<(T1, T2)> pair, Var<T1> main, Var<T2> other, Var<TValue> value) :
            base(name, pair.Key, main.Indexed, other.Indexed, value) {
            Change = Event($"{name}Change", main.Indexed, other.Indexed, value);
            var setVar1 = new Var<TValue>($"{value.Name}SetValue1");
            var setVar2 = new Var<TValue>($"{value.Name}SetValue2");
            Add[pair, main, other, value].If(Change, !this[__, main, other, __], NewTuple[main, other, pair]);
            Set(pair, value, setVar1).If(Change, this[pair, main, other, setVar2], setVar1 == setVar2 + value);
            Unchanged = Predicate($"{name}Unchanged", pair).If(this, !Change[main, other, __]);
        }

        public Affinity<T1, T2, TValue> UpdateWhen(params Goal[] conditions) {
            Change.OccursWhen(conditions);
            return this;
        }
        public Affinity<T1, T2, TValue> UpdateCauses(params Effect[] effects) {
            Change.Causes(effects);
            return this;
        }

        public AffinityGoal this[Term<T1> arg1, Term<T2> arg2, Term<TValue> arg3] => new (this, __, arg1, arg2, arg3);

        public class AffinityGoal : TableGoal<(T1, T2), T1, T2, TValue> {
            public AffinityGoal(TablePredicate predicate, Term<(T1, T2)> pair, Term<T1> main, Term<T2> other, Term<TValue> value)
                : base(predicate, pair, main, other, value) { }

        }
    }
}
