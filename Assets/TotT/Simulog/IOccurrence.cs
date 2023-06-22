using TED;
using TED.Interpreter;
using TotT.ValueTypes;

namespace TotT.Simulog {
    internal interface IOccurrence {
        public IEvent Event => (IEvent)((TableGoal)this).TablePredicate;

        public Goal OccurredAt(Term<TimePoint> time) => 
            ((TableGoal)this).TablePredicate.AppendArgs((TableGoal)this, time);
    }
}
