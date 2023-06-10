using JetBrains.Annotations;
using TED;
using TED.Interpreter;
using TMPro;
using TotT.Simulator;
using TotT.ValueTypes;
using static TED.Language;

namespace Assets.TotT.Simulog
{
    public class Existent<T> : TablePredicate<T, bool, TimePoint, TimePoint>
    {

        public Existent(string name, Var<T> arg1)
            : base(name, arg1.Key, _exists, _start, _end)
        {
            Start = new TablePredicate<T>($"{name}Start", arg1);
            End = new TablePredicate<T>($"{name}End", arg1);
            this.Add[arg1, true, Variables.time, TimePoint.Eschaton].If(Start[arg1], TalkOfTheTown.Time.CurrentTimePoint[Variables.time]);
            this.Set(arg1, _end, Variables.time).If(End[arg1], TalkOfTheTown.Time.CurrentTimePoint[Variables.time]);
            this.Set(arg1, _exists, false).If(End[arg1]);
        }

        public readonly TablePredicate<T> Start;
        public readonly TablePredicate<T> End;

        public Existent<T> StartWhen(params Goal[] conditions)
        {
            Start.If(conditions);
            return this;
        }

        public Existent<T> EndWhen(params Goal[] conditions)
        {
            End.If(conditions);
            return this;
        }

        private static Var<bool> _exists = (Var<bool>)"exists";
        private static Var<TimePoint> _start = (Var<TimePoint>)"start";
        private static Var<TimePoint> _end = (Var<TimePoint>)"end";

        public ExistentGoal this[Term<T> arg] => new ExistentGoal(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint>
        {
            public ExistentGoal([NotNull] TablePredicate predicate, [NotNull] Term<T> arg1, [NotNull] Term<bool> arg2, [NotNull] Term<TimePoint> arg3, [NotNull] Term<TimePoint> arg4) : base(predicate, arg1, arg2, arg3, arg4)
            {
            }

            public ExistentGoal Ended => new ExistentGoal(this.TablePredicate, this.Arg1, false, this.Arg3, this.Arg4);

            public ExistentGoal StartedAt(Term<TimePoint> t) => new ExistentGoal(this.TablePredicate, this.Arg1, __, t, this.Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new ExistentGoal(this.TablePredicate, this.Arg1, __, this.Arg3, t);
        }
    }
}
