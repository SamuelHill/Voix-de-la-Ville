using TED;
using TED.Interpreter;
using VdlV.Time;

namespace VdlV.Simulog {
    // ReSharper disable UnusedMember.Global
    // ReSharper disable SuspiciousTypeConversion.Global
    internal interface IOccurrence {
        public IEvent Event => (IEvent)((TableGoal)this).TablePredicate;
        
        public Goal OccurredAt(Term<TimePoint> time) =>
            ((TableGoal)this).TablePredicate.AppendArgs((TableGoal)this, time);
    }
}
