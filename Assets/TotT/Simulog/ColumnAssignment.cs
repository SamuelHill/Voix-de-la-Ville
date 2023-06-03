using TED;
using TED.Interpreter;

namespace TotT.Simulog
{
    public interface IColumnAssignment<T> {
        public Var<T> Column { get; }

        public bool AssignInGoal { get; }

        public Goal AssignToVar();
    }

    public class ColumnConstant<T> : IColumnAssignment<T> {
        public Var<T> Column { get; }
        private Term<T> Value { get; }

        public bool AssignInGoal { get; }

        public ColumnConstant(Var<T> column, T value, bool assignInGoal = true) {
            Column = column;
            Value = value;
            AssignInGoal = assignInGoal;
        }

        public Goal AssignToVar()
        {

            return Column == Value;
        }
    }

    public class ColumnFunction<T> : IColumnAssignment<T> {
        public Var<T> Column { get; }
        private Function<T> Value { get; }

        public bool AssignInGoal => true;

        public ColumnFunction(Var<T> column, Function<T> value) {
            Column = column;
            Value = value; }

        public Goal AssignToVar() => Column == Value;
    }
}