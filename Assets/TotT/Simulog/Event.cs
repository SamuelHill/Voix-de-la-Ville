using TED;
using TED.Interpreter;
using TotT.Simulator;
using TotT.ValueTypes;

namespace TotT.Simulog {
    public class Event<T1> : GenericEvent<T1, TimePoint> {
        public Event(string name, IColumnSpec<T1> arg1) : base(name, arg1) { }

        protected override Function<TimePoint> CurrentTime => TalkOfTheTown.Time.CurrentTimePoint;
    }

    public class Event<T1, T2> : GenericEvent<T1, T2, TimePoint> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) : base(name, arg1, arg2) { }

        protected override Function<TimePoint> CurrentTime => TalkOfTheTown.Time.CurrentTimePoint;
    }

    public class Event<T1, T2, T3> : GenericEvent<T1, T2, T3, TimePoint> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) :
            base(name, arg1, arg2, arg3) { }

        protected override Function<TimePoint> CurrentTime => TalkOfTheTown.Time.CurrentTimePoint;
    }

    public class Event<T1, T2, T3, T4> : GenericEvent<T1, T2, T3, T4, TimePoint> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) :
            base(name, arg1, arg2, arg3, arg4) { }

        protected override Function<TimePoint> CurrentTime => TalkOfTheTown.Time.CurrentTimePoint;
    }

    public class Event<T1, T2, T3, T4, T5> : GenericEvent<T1, T2, T3, T4, T5, TimePoint> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2,
                     IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) :
            base(name, arg1, arg2, arg3, arg4, arg5) { }

        protected override Function<TimePoint> CurrentTime => TalkOfTheTown.Time.CurrentTimePoint;
    }

    public class Event<T1, T2, T3, T4, T5, T6> : GenericEvent<T1, T2, T3, T4, T5, T6, TimePoint> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6) { }

        protected override Function<TimePoint> CurrentTime => TalkOfTheTown.Time.CurrentTimePoint;
    }

    public class Event<T1, T2, T3, T4, T5, T6, T7> : GenericEvent<T1, T2, T3, T4, T5, T6, T7, TimePoint> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }

        protected override Function<TimePoint> CurrentTime => TalkOfTheTown.Time.CurrentTimePoint;
    }
}
