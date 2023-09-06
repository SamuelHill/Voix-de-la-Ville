using TED;
using VdlV.Time;
using VdlV.Utilities;

namespace VdlV.Simulog {
    using static Clock;
    using static ChronicleIndexing;

    public class EventChronicle<T1> : TablePredicate<T1, TimePoint> {
        public EventChronicle(Event<T1> table, Var<TimePoint> time) :
            base(table.Name + "Chronicle", (Var<T1>)table.Arg1, time) {
            Add.If(table, CurrentTimePoint[time]);
        }
    }

    public class EventChronicle<T1, T2> : TablePredicate<T1, T2, TimePoint> {
        public EventChronicle(Event<T1, T2> table, Var<TimePoint> time) :
            base(table.Name + "Chronicle", DemoteKey(table.Arg1), DemoteKey(table.Arg2), time) {
            Add.If(table, CurrentTimePoint[time]);
        }
    }

    public class EventChronicle<T1, T2, T3> : TablePredicate<T1, T2, T3, TimePoint> {
        public EventChronicle(Event<T1, T2, T3> table, Var<TimePoint> time) :
            base(table.Name + "Chronicle", DemoteKey(table.Arg1), 
                 DemoteKey(table.Arg2), DemoteKey(table.Arg3), time) {
            Add.If(table, CurrentTimePoint[time]);
        }
    }

    public class EventChronicle<T1, T2, T3, T4> : TablePredicate<T1, T2, T3, T4, TimePoint> {
        public EventChronicle(Event<T1, T2, T3, T4> table, Var<TimePoint> time) :
            base(table.Name + "Chronicle", DemoteKey(table.Arg1), DemoteKey(table.Arg2), 
                 DemoteKey(table.Arg3), DemoteKey(table.Arg4), time) {
            Add.If(table, CurrentTimePoint[time]);
        }
    }

    public class EventChronicle<T1, T2, T3, T4, T5> : TablePredicate<T1, T2, T3, T4, T5, TimePoint> {
        public EventChronicle(Event<T1, T2, T3, T4, T5> table, Var<TimePoint> time) :
            base(table.Name + "Chronicle", DemoteKey(table.Arg1), DemoteKey(table.Arg2),
                 DemoteKey(table.Arg3), DemoteKey(table.Arg4), DemoteKey(table.Arg5), time) {
            Add.If(table, CurrentTimePoint[time]);
        }
    }

    public class EventChronicle<T1, T2, T3, T4, T5, T6> : TablePredicate<T1, T2, T3, T4, T5, T6, TimePoint> {
        public EventChronicle(Event<T1, T2, T3, T4, T5, T6> table, Var<TimePoint> time) :
            base(table.Name + "Chronicle", DemoteKey(table.Arg1), DemoteKey(table.Arg2), DemoteKey(table.Arg3), 
                 DemoteKey(table.Arg4), DemoteKey(table.Arg5), DemoteKey(table.Arg6), time) {
            Add.If(table, CurrentTimePoint[time]);
        }
    }

    public class EventChronicle<T1, T2, T3, T4, T5, T6, T7> : TablePredicate<T1, T2, T3, T4, T5, T6, T7, TimePoint> {
        public EventChronicle(Event<T1, T2, T3, T4, T5, T6, T7> table, Var<TimePoint> time) :
            base(table.Name + "Chronicle", DemoteKey(table.Arg1), DemoteKey(table.Arg2), DemoteKey(table.Arg3),
                 DemoteKey(table.Arg4), DemoteKey(table.Arg5), DemoteKey(table.Arg6), DemoteKey(table.Arg7), time) {
            Add.If(table, CurrentTimePoint[time]);
        }
    }
}
