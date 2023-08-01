using TED;
using TED.Interpreter;
using VdlV.Time;

namespace VdlV.Simulog {
    internal interface IOccurrence {
        // ReSharper disable once UnusedMember.Global
        public IEvent Event => (IEvent)((TableGoal)this).TablePredicate;

        // ReSharper disable once UnusedMember.Global
        public Goal OccurredAt(Term<TimePoint> time) =>
            ((TableGoal)this).TablePredicate.AppendArgs((TableGoal)this, time);
    }
}
