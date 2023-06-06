using System;
using System.Linq;
using TED;
using TED.Tables;
using TED.Interpreter;
using static TED.Language;

namespace TotT.Simulog {

    public class Existence<T> {
        private readonly IColumnSpec<T> _state;
        private readonly string _name;
        private readonly IColumnSpec[] _columns;
        public TablePredicate ExistenceTable;
        public TablePredicate NewExistenceTable;

        public Existence(string name, IColumnSpec<T> state, params IColumnSpec[] columns) {
            _name = name;
            _state = state;
            _columns = columns;
            ExistenceTable = Predicate(_name, _columns.Append(_state));
            NewExistenceTable = Predicate(_name, _columns);
        }
    }

    public class Existence<T1, T2, T3, T4, T5, T6> {
        private readonly Var<T1> _key;
        private readonly Var<T6> _state;
        private readonly (T6 start, T6 end) _states;
        private readonly TablePredicate<T1, T2, T3, T4, T5, T6> _existenceTable;
        private readonly TablePredicate<T1, T2, T3, T4, T5> _newExistenceTable;

        public TableGoal<T1, T2, T3, T4, T5, T6> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6]
            => new(_existenceTable, arg1, arg2, arg3, arg4, arg5, arg6);

        public TableGoal<T1, T2, T3, T4, T5> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5]
            => new(_newExistenceTable, arg1, arg2, arg3, arg4, arg5);

        public static implicit operator Goal(Existence<T1, T2, T3, T4, T5, T6> e) => e._existenceTable;

        public Existence(string name, Var<T1> key, IColumnSpec<T2> arg1, IColumnSpec<T3> arg2, 
            IColumnSpec<T4> arg3, IColumnSpec<T5> arg4, Var<T6> state, (T6 start, T6 end) states) {
            _key = key;
            _state = state;
            _states = states;
            _existenceTable = Predicate(name, _key.Key, arg1, arg2, arg3, arg4, _state.Indexed);
            _newExistenceTable = Predicate($"New{name}", _key.Key, arg1, arg2, arg3, arg4);
            _existenceTable.Add[_key, arg1.TypedVariable, arg2.TypedVariable, arg3.TypedVariable, arg4.TypedVariable, _states.start].If(_newExistenceTable);
        }

        public void New(params Goal[] goals) => _newExistenceTable.If(goals);

        public void End(params Goal[] goals) => 
            _existenceTable.Set(_key.TypedVariable, _state.TypedVariable, _states.end).If(goals);

        public static Existence<T1, T2, T3, T4, T5, bool> BoolExistence(string name, Var<T1> key, IColumnSpec<T2> arg1,
            IColumnSpec<T3> arg2, IColumnSpec<T4> arg3, IColumnSpec<T5> arg4, Var<bool> state) => 
            new(name, key, arg1, arg2, arg3, arg4, state, (true, false));
    }
}