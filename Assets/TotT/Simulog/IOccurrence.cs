using TED;
using TED.Interpreter;

namespace TotT.Simulog {
    internal interface IOccurrence<TTimePoint> {
        // ReSharper disable once UnusedMember.Global
        public IEvent Event => (IEvent)((TableGoal)this).TablePredicate;

        // ReSharper disable once UnusedMember.Global
        public Goal OccurredAt(Term<TTimePoint> time) =>
            ((TableGoal)this).TablePredicate.AppendArgs((TableGoal)this, time);
    }
}
