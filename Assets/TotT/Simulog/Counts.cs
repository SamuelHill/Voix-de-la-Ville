using TED;
using TED.Interpreter;
using static TED.Language;

namespace TotT.Simulog {
    public class Counts<T1, T2> : TablePredicate<T1, T2> {
        public readonly TablePredicate<T1, int> Count;

        public Counts(string name, Var<T1> countBy, IColumnSpec<T2> arg, Var<int> count) : base(name, countBy.Indexed, arg) =>
            Count = CountsBy($"{name}Count", this, countBy, count);

        public Counts<T1, T2> By(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class Counts<T1, T2, T3> : TablePredicate<T1, T2, T3> {
        public readonly TablePredicate<T1, int> Count;

        public Counts(string name, Var<T1> countBy, IColumnSpec<T2> arg1, IColumnSpec<T3> arg2,
                      Var<int> count) : base(name, countBy.Indexed, arg1, arg2) =>
            Count = CountsBy($"{name}Count", this, countBy, count);

        public Counts<T1, T2, T3> By(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class Counts<T1, T2, T3, T4> : TablePredicate<T1, T2, T3, T4> {
        public readonly TablePredicate<T1, int> Count;

        public Counts(string name, Var<T1> countBy, IColumnSpec<T2> arg1, IColumnSpec<T3> arg2, 
                      IColumnSpec<T4> arg3, Var<int> count) : base(name, countBy.Indexed, arg1, arg2, arg3) =>
            Count = CountsBy($"{name}Count", this, countBy, count);

        public Counts<T1, T2, T3, T4> By(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class Counts<T1, T2, T3, T4, T5> : TablePredicate<T1, T2, T3, T4, T5> {
        public readonly TablePredicate<T1, int> Count;

        public Counts(string name, Var<T1> countBy, IColumnSpec<T2> arg1, IColumnSpec<T3> arg2,
                      IColumnSpec<T4> arg3, IColumnSpec<T5> arg4, Var<int> count) 
            : base(name, countBy.Indexed, arg1, arg2, arg3, arg4) =>
            Count = CountsBy($"{name}Count", this, countBy, count);

        public Counts<T1, T2, T3, T4, T5> By(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class Counts<T1, T2, T3, T4, T5, T6> : TablePredicate<T1, T2, T3, T4, T5, T6> {
        public readonly TablePredicate<T1, int> Count;

        public Counts(string name, Var<T1> countBy, IColumnSpec<T2> arg1, IColumnSpec<T3> arg2,
                      IColumnSpec<T4> arg3, IColumnSpec<T5> arg4, IColumnSpec<T6> arg5,
                      Var<int> count) : base(name, countBy.Indexed, arg1, arg2, arg3, arg4, arg5) =>
            Count = CountsBy($"{name}Count", this, countBy, count);

        public Counts<T1, T2, T3, T4, T5, T6> By(params Goal[] goals) {
            If(goals);
            return this;
        }
    }

    public class Counts<T1, T2, T3, T4, T5, T6, T7> : TablePredicate<T1, T2, T3, T4, T5, T6, T7> {
        public readonly TablePredicate<T1, int> Count;

        public Counts(string name, Var<T1> countBy, IColumnSpec<T2> arg1, IColumnSpec<T3> arg2, 
                      IColumnSpec<T4> arg3, IColumnSpec<T5> arg4, IColumnSpec<T6> arg5, IColumnSpec<T7> arg6, 
                      Var<int> count) : base(name, countBy.Indexed, arg1, arg2, arg3, arg4, arg5, arg6) => 
            Count = CountsBy($"{name}Count", this, countBy, count);

        public Counts<T1, T2, T3, T4, T5, T6, T7> By(params Goal[] goals) {
            If(goals);
            return this;
        }
    }
}
