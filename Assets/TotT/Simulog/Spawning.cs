using TED;
using TED.Interpreter;

namespace TotT.Simulog {


    public class Spawning<T> {

        public TablePredicate Predicate;

        public Goal[] Trigger;

        public IColumnSpec<T> KeyColumn;

        private Spawning(TablePredicate predicate, params Goal[] trigger) {
            Predicate = predicate;
            Trigger = trigger; }

        public static Spawning<T> Spawn(TablePredicate predicate, Var<T> keyColumn, params Goal[] trigger) =>
            new(predicate, trigger) { KeyColumn = keyColumn };
    }
}