using System;
using TED;
using TED.Interpreter;
using TED.Tables;
using VdlV.Time;

namespace VdlV.Simulog {
    public class EventChronicle<T1> : TablePredicate<T1, TimePoint> {
        public EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<TimePoint> arg2) : 
            base(name + "Chronicle", arg1, arg2) {}
        public EventChronicle(string name, Action<Table> updateProc, IColumnSpec<T1> arg1, IColumnSpec<TimePoint> arg2) : 
            base(name + "Chronicle", updateProc, arg1, arg2) {}


        public ChronicleGoal this[Term<T1> arg1] => new(this, arg1);

        public class ChronicleGoal : TableGoal<T1>, IOccurrence {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public ChronicleGoal(EventChronicle<T1> e, Term<T1> arg1) : base(e, arg1) { }
        }
    }
}
