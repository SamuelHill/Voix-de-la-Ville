using TED;
using TED.Interpreter;
using VdlV.Time;

namespace VdlV.Simulog {
    using static Clock;

    // TODO : Allow Indexing for columns but not Keys - demotes Keys to Indexes

    public class EventChronicle<T1> : TablePredicate<T1, TimePoint> {
        public EventChronicle(TablePredicate<T1> table, Var<TimePoint> time) :
            this(table.Name, (Var<T1>)table.DefaultVariables[0], time) => Add.If(table, CurrentTimePoint[time]);
        private EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<TimePoint> time) : 
            base(name + "Chronicle", arg1, time) {}
    }

    public class EventChronicle<T1, T2> : TablePredicate<T1, T2, TimePoint> {
        public EventChronicle(TablePredicate<T1, T2> table, Var<TimePoint> time) :
            this(table.Name, (Var<T1>)table.DefaultVariables[0], (Var<T2>)table.DefaultVariables[1], 
                 time) => Add.If(table, CurrentTimePoint[time]);
        private EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<TimePoint> time) :
            base(name + "Chronicle", arg1, arg2, time) { }
    }

    public class EventChronicle<T1, T2, T3> : TablePredicate<T1, T2, T3, TimePoint> {
        public EventChronicle(TablePredicate<T1, T2, T3> table, Var<TimePoint> time) :
            this(table.Name, (Var<T1>)table.DefaultVariables[0], (Var<T2>)table.DefaultVariables[1],
                 (Var<T3>)table.DefaultVariables[2], time) => Add.If(table, CurrentTimePoint[time]);
        private EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2,
                               IColumnSpec<T3> arg3, IColumnSpec<TimePoint> time) :
            base(name + "Chronicle", arg1, arg2, arg3, time) { }
    }

    public class EventChronicle<T1, T2, T3, T4> : TablePredicate<T1, T2, T3, T4, TimePoint> {
        public EventChronicle(TablePredicate<T1, T2, T3, T4> table, Var<TimePoint> time) :
            this(table.Name, (Var<T1>)table.DefaultVariables[0], (Var<T2>)table.DefaultVariables[1],
                 (Var<T3>)table.DefaultVariables[2], (Var<T4>)table.DefaultVariables[3], time) 
            => Add.If(table, CurrentTimePoint[time]);
        private EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                               IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<TimePoint> time) :
            base(name + "Chronicle", arg1, arg2, arg3, arg4, time) { }
    }

    public class EventChronicle<T1, T2, T3, T4, T5> : TablePredicate<T1, T2, T3, T4, T5, TimePoint> {
        public EventChronicle(TablePredicate<T1, T2, T3, T4, T5> table, Var<TimePoint> time) :
            this(table.Name, (Var<T1>)table.DefaultVariables[0], (Var<T2>)table.DefaultVariables[1],
                 (Var<T3>)table.DefaultVariables[2], (Var<T4>)table.DefaultVariables[3], 
                 (Var<T5>)table.DefaultVariables[4], time) => Add.If(table, CurrentTimePoint[time]);
        private EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                               IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<TimePoint> time) :
            base(name + "Chronicle", arg1, arg2, arg3, arg4, arg5, time) { }
    }

    public class EventChronicle<T1, T2, T3, T4, T5, T6> : TablePredicate<T1, T2, T3, T4, T5, T6, TimePoint> {
        public EventChronicle(TablePredicate<T1, T2, T3, T4, T5, T6> table, Var<TimePoint> time) :
            this(table.Name, (Var<T1>)table.DefaultVariables[0], (Var<T2>)table.DefaultVariables[1],
                 (Var<T3>)table.DefaultVariables[2], (Var<T4>)table.DefaultVariables[3], (Var<T5>)table.DefaultVariables[4],
                 (Var<T6>)table.DefaultVariables[5], time) => Add.If(table, CurrentTimePoint[time]);
        private EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                               IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<TimePoint> time) :
            base(name + "Chronicle", arg1, arg2, arg3, arg4, arg5, arg6, time) { }
    }

    public class EventChronicle<T1, T2, T3, T4, T5, T6, T7> : TablePredicate<T1, T2, T3, T4, T5, T6, T7, TimePoint> {
        public EventChronicle(TablePredicate<T1, T2, T3, T4, T5, T6, T7> table, Var<TimePoint> time) :
            this(table.Name, (Var<T1>)table.DefaultVariables[0], (Var<T2>)table.DefaultVariables[1], 
                (Var<T3>)table.DefaultVariables[2], (Var<T4>)table.DefaultVariables[3], (Var<T5>)table.DefaultVariables[4], 
                (Var<T6>)table.DefaultVariables[5], (Var<T7>)table.DefaultVariables[6], time) => Add.If(table, CurrentTimePoint[time]);
        private EventChronicle(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, 
            IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7, IColumnSpec<TimePoint> time) :
            base(name + "Chronicle", arg1, arg2, arg3, arg4, arg5, arg6, arg7, time) { }
    }
}
