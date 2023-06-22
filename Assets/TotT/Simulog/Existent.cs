using TED;
using TED.Interpreter;
using TotT.Simulator;
using TotT.ValueTypes;
using static TED.Language;

namespace TotT.Simulog {
    using static Variables;

    public class Existent<T> : TablePredicate<T, bool, TimePoint, TimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        private static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TimePoint> _start = (Var<TimePoint>)"start";
        private static readonly Var<TimePoint> _end = (Var<TimePoint>)"end";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        public readonly TablePredicate<T> Start;
        public readonly TablePredicate<T> End;

        public Existent(string name, Var<T> arg1) : base(name, arg1.Key, _exists, _start, _end) {
            Start = new TablePredicate<T>($"{name}Start", arg1);
            End = new TablePredicate<T>($"{name}End", arg1);
            Add[arg1, true, time, TimePoint.Eschaton]
               .If(Start[arg1], TalkOfTheTown.Time.CurrentTimePoint[time]);
            Set(arg1, _end, time).If(End[arg1], TalkOfTheTown.Time.CurrentTimePoint[time]);
            Set(arg1, _exists, false).If(End[arg1]);
        }

        public Existent<T> StartWhen(params Goal[] conditions) {
            Start.If(conditions);
            return this;
        }

        public Existent<T> EndWhen(params Goal[] conditions) {
            End.If(conditions);
            return this;
        }

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, 
                                Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4) 
                : base(predicate, arg1, arg2, arg3, arg4) {}

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);

            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }
}
