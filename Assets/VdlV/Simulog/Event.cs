using TED;
using TED.Interpreter;
using VdlV.Simulator;

namespace VdlV.Simulog {
    using static Variables;

    public class Event<T1> : TablePredicate<T1> {
        public Event(string name, IColumnSpec<T1> arg1) : base(name, arg1) { }
        
        private EventChronicle<T1> _chronicle;
        
        public EventChronicle<T1> Chronicle {
            get {
                _chronicle ??= new EventChronicle<T1>(this, time);
                return _chronicle;
            }
        }

        public Event<T1> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }
    }

    public class Event<T1, T2> : TablePredicate<T1, T2> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) :
            base(name, arg1, arg2) { }
        
        private EventChronicle<T1, T2> _chronicle;

        public EventChronicle<T1, T2> Chronicle {
            get {
                _chronicle ??= new EventChronicle<T1, T2>(this, time);
                return _chronicle;
            }
        }

        public Event<T1, T2> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }
    }

    public class Event<T1, T2, T3> : TablePredicate<T1, T2, T3> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) :
            base(name, arg1, arg2, arg3) { }
        
        private EventChronicle<T1, T2, T3> _chronicle;

        public EventChronicle<T1, T2, T3> Chronicle {
            get {
                _chronicle ??= new EventChronicle<T1, T2, T3>(this, time);
                return _chronicle;
            }
        }

        public Event<T1, T2, T3> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }
    }

    public class Event<T1, T2, T3, T4> : TablePredicate<T1, T2, T3, T4> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                     IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) :
            base(name, arg1, arg2, arg3, arg4) { }
        
        private EventChronicle<T1, T2, T3, T4> _chronicle;

        public EventChronicle<T1, T2, T3, T4> Chronicle {
            get {
                _chronicle ??= new EventChronicle<T1, T2, T3, T4>(this, time);
                return _chronicle;
            }
        }

        public Event<T1, T2, T3, T4> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }
    }

    public class Event<T1, T2, T3, T4, T5> : TablePredicate<T1, T2, T3, T4, T5> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) :
            base(name, arg1, arg2, arg3, arg4, arg5) { }
        
        private EventChronicle<T1, T2, T3, T4, T5> _chronicle;

        public EventChronicle<T1, T2, T3, T4, T5> Chronicle {
            get {
                _chronicle ??= new EventChronicle<T1, T2, T3, T4, T5>(this, time);
                return _chronicle;
            }
        }

        public Event<T1, T2, T3, T4, T5> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4, T5> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }
    }

    public class Event<T1, T2, T3, T4, T5, T6> : TablePredicate<T1, T2, T3, T4, T5, T6> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6) { }
        
        private EventChronicle<T1, T2, T3, T4, T5, T6> _chronicle;

        public EventChronicle<T1, T2, T3, T4, T5, T6> Chronicle {
            get {
                _chronicle ??= new EventChronicle<T1, T2, T3, T4, T5, T6>(this, time);
                return _chronicle;
            }
        }

        public Event<T1, T2, T3, T4, T5, T6> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4, T5, T6> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }
    }

    public class Event<T1, T2, T3, T4, T5, T6, T7> : TablePredicate<T1, T2, T3, T4, T5, T6, T7> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }
        
        private EventChronicle<T1, T2, T3, T4, T5, T6, T7> _chronicle;
        
        public EventChronicle<T1, T2, T3, T4, T5, T6, T7> Chronicle {
            get {
                _chronicle ??= new EventChronicle<T1, T2, T3, T4, T5, T6, T7>(this, time);
                return _chronicle;
            }
        }

        public Event<T1, T2, T3, T4, T5, T6, T7> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4, T5, T6, T7> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }
    }
}
