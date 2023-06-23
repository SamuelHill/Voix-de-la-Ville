using TED;
using TED.Interpreter;
using TotT.ValueTypes;

namespace TotT.Simulog {
    internal interface IOccurrence {
        // ReSharper disable once UnusedMember.Global
        public IEvent Event => (IEvent)((TableGoal)this).TablePredicate;

        // ReSharper disable once UnusedMember.Global
        public Goal OccurredAt(Term<TimePoint> time) =>
            ((TableGoal)this).TablePredicate.AppendArgs((TableGoal)this, time);
    }
}
