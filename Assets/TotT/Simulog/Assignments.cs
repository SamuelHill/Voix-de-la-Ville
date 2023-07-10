using TED;
using TED.Interpreter;
using static TED.Language;

namespace TotT.Simulog {
    public class RandomAssign<T1, T2> : TablePredicate<T1, T2> {
        public readonly TablePredicate<T1, T2> Assignments;

        public RandomAssign(string name, Var<T1> arg1, IColumnSpec<T2> arg2) : base(name, arg1.Indexed, arg2) => 
            Assignments = AssignRandomly($"{name}Assignments", this);

        public RandomAssign<T1, T2> When(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class IntAssign<T1, T2> : TablePredicate<T1, T2, int>{
        public readonly TablePredicate<T1, T2> Assignments;

        public IntAssign(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                         IColumnSpec<int> arg3) : base(name, arg1, arg2, arg3) =>
            Assignments = AssignGreedily($"{name}Assignments", this);

        public IntAssign(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<int> arg3, 
                         TablePredicate<T2, int> capacities) : base(name, arg1, arg2, arg3) =>
            Assignments = AssignGreedily($"{name}Assignments", this, capacities);

        public IntAssign<T1, T2> When(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class FloatAssign<T1, T2> : TablePredicate<T1, T2, float> {
        public readonly TablePredicate<T1, T2> Assignments;

        public FloatAssign(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                           IColumnSpec<float> arg3) : base(name, arg1, arg2, arg3) =>
            Assignments = AssignGreedily($"{name}Assignments", this);

        public FloatAssign(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<float> arg3,
                           TablePredicate<T2, int> capacities) : base(name, arg1, arg2, arg3) =>
            Assignments = AssignGreedily($"{name}Assignments", this, capacities);

        public FloatAssign<T1, T2> When(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class IntMatch<T> : TablePredicate<T, T, int> {
        public readonly TablePredicate<T, T> Matches;

        public IntMatch(string name, IColumnSpec<T> arg1, IColumnSpec<T> arg2, IColumnSpec<int> arg3, 
                        bool symmetric) : base(name, arg1, arg2, arg3) => 
            Matches = symmetric ? MatchGreedily($"{name}Matches", this) : 
                          MatchGreedilyAsymmetric($"{name}Matches", this);

        public IntMatch<T> When(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class FloatMatch<T> : TablePredicate<T, T, float> {
        public readonly TablePredicate<T, T> Matches;

        public FloatMatch(string name, IColumnSpec<T> arg1, IColumnSpec<T> arg2, IColumnSpec<float> arg3,
                          bool symmetric) : base(name, arg1, arg2, arg3) => 
            Matches = symmetric ? MatchGreedily($"{name}Matches", this) :
                          MatchGreedilyAsymmetric($"{name}Matches", this);

        public FloatMatch<T> When(params Goal[] goals) {
            If(goals);
            return this;
        }
    }
}
