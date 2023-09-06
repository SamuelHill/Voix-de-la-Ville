using TED;
using TED.Interpreter;
using VdlV.Simulator;
using VdlV.Time;

namespace VdlV.Simulog {
    using static Variables;

    public class Event<T1> : TablePredicate<T1> {
        public Event(string name, IColumnSpec<T1> arg1) : base(name, arg1) { Arg1 = arg1; }

        internal readonly IColumnSpec<T1> Arg1;

        private EventChronicle<T1> _chronicle;
        
        // ReSharper disable once MemberCanBePrivate.Global
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

        public new EventGoal this[Term<T1> arg1] => new (this, arg1);

        public class EventGoal : TableGoal<T1> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(Event<T1> e, Term<T1> arg1) : base(e, arg1) { }

            public Goal At(Term<TimePoint> time) => 
                ((Event<T1>)TablePredicate).Chronicle[Arg1, time];
        }
    }

    public class Event<T1, T2> : TablePredicate<T1, T2> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) : base(name, arg1, arg2) {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        internal readonly IColumnSpec<T1> Arg1;
        internal readonly IColumnSpec<T2> Arg2;

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

        public new EventGoal this[Term<T1> arg1, Term<T2> arg2] => new(this, arg1, arg2);

        public class EventGoal : TableGoal<T1, T2> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(Event<T1, T2> e, Term<T1> arg1, Term<T2> arg2) : base(e, arg1, arg2) { }

            public Goal At(Term<TimePoint> time) =>
                ((Event<T1, T2>)TablePredicate).Chronicle[Arg1, Arg2, time];
        }
    }

    public class Event<T1, T2, T3> : TablePredicate<T1, T2, T3> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) : 
            base(name, arg1, arg2, arg3) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        internal readonly IColumnSpec<T1> Arg1;
        internal readonly IColumnSpec<T2> Arg2;
        internal readonly IColumnSpec<T3> Arg3;

        private EventChronicle<T1, T2, T3> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
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

        public new EventGoal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] =>
            new(this, arg1, arg2, arg3);

        public class EventGoal : TableGoal<T1, T2, T3> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(Event<T1, T2, T3> e, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) :
                base(e, arg1, arg2, arg3) { }

            public Goal At(Term<TimePoint> time) =>
                ((Event<T1, T2, T3>)TablePredicate).Chronicle[Arg1, Arg2, Arg3, time];
        }
    }

    public class Event<T1, T2, T3, T4> : TablePredicate<T1, T2, T3, T4> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                     IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) :
            base(name, arg1, arg2, arg3, arg4) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        internal readonly IColumnSpec<T1> Arg1;
        internal readonly IColumnSpec<T2> Arg2;
        internal readonly IColumnSpec<T3> Arg3;
        internal readonly IColumnSpec<T4> Arg4;

        private EventChronicle<T1, T2, T3, T4> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
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

        public new EventGoal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4] =>
            new(this, arg1, arg2, arg3, arg4);

        public class EventGoal : TableGoal<T1, T2, T3, T4> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(Event<T1, T2, T3, T4> e, Term<T1> arg1, Term<T2> arg2, 
                             Term<T3> arg3, Term<T4> arg4) :
                base(e, arg1, arg2, arg3, arg4) { }

            public Goal At(Term<TimePoint> time) =>
                ((Event<T1, T2, T3, T4>)TablePredicate).Chronicle[Arg1, Arg2, Arg3, Arg4, time];
        }
    }

    public class Event<T1, T2, T3, T4, T5> : TablePredicate<T1, T2, T3, T4, T5> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) :
            base(name, arg1, arg2, arg3, arg4, arg5) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }

        internal readonly IColumnSpec<T1> Arg1;
        internal readonly IColumnSpec<T2> Arg2;
        internal readonly IColumnSpec<T3> Arg3;
        internal readonly IColumnSpec<T4> Arg4;
        internal readonly IColumnSpec<T5> Arg5;

        private EventChronicle<T1, T2, T3, T4, T5> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
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

        public new EventGoal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4, Term<T5> arg5] =>
            new(this, arg1, arg2, arg3, arg4, arg5);

        public class EventGoal : TableGoal<T1, T2, T3, T4, T5> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(Event<T1, T2, T3, T4, T5> e, Term<T1> arg1, Term<T2> arg2,
                             Term<T3> arg3, Term<T4> arg4, Term<T5> arg5) :
                base(e, arg1, arg2, arg3, arg4, arg5) { }

            public Goal At(Term<TimePoint> time) =>
                ((Event<T1, T2, T3, T4, T5>)TablePredicate).Chronicle[
                    Arg1, Arg2, Arg3, Arg4, Arg5, time];
        }
    }

    public class Event<T1, T2, T3, T4, T5, T6> : TablePredicate<T1, T2, T3, T4, T5, T6> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
        }

        internal readonly IColumnSpec<T1> Arg1;
        internal readonly IColumnSpec<T2> Arg2;
        internal readonly IColumnSpec<T3> Arg3;
        internal readonly IColumnSpec<T4> Arg4;
        internal readonly IColumnSpec<T5> Arg5;
        internal readonly IColumnSpec<T6> Arg6;

        private EventChronicle<T1, T2, T3, T4, T5, T6> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
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

        public new EventGoal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4,
                                  Term<T5> arg5, Term<T6> arg6] =>
            new(this, arg1, arg2, arg3, arg4, arg5, arg6);

        public class EventGoal : TableGoal<T1, T2, T3, T4, T5, T6> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(Event<T1, T2, T3, T4, T5, T6> e, Term<T1> arg1, Term<T2> arg2,
                             Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6) :
                base(e, arg1, arg2, arg3, arg4, arg5, arg6) { }

            public Goal At(Term<TimePoint> time) =>
                ((Event<T1, T2, T3, T4, T5, T6>)TablePredicate).Chronicle[
                    Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, time];
        }
    }

    public class Event<T1, T2, T3, T4, T5, T6, T7> : TablePredicate<T1, T2, T3, T4, T5, T6, T7> {
        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
            Arg7 = arg7;
        }

        internal readonly IColumnSpec<T1> Arg1;
        internal readonly IColumnSpec<T2> Arg2;
        internal readonly IColumnSpec<T3> Arg3;
        internal readonly IColumnSpec<T4> Arg4;
        internal readonly IColumnSpec<T5> Arg5;
        internal readonly IColumnSpec<T6> Arg6;
        internal readonly IColumnSpec<T7> Arg7;

        private EventChronicle<T1, T2, T3, T4, T5, T6, T7> _chronicle;
        
        // ReSharper disable once MemberCanBePrivate.Global
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

        public new EventGoal this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4,
                                  Term<T5> arg5, Term<T6> arg6, Term<T7> arg7] => 
            new(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        public class EventGoal : TableGoal<T1, T2, T3, T4, T5, T6, T7> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(Event<T1, T2, T3, T4, T5, T6, T7> e, Term<T1> arg1, Term<T2> arg2, 
                             Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7) : 
                base(e, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }

            public Goal At(Term<TimePoint> time) =>
                ((Event<T1, T2, T3, T4, T5, T6, T7>)TablePredicate).Chronicle[
                    Arg1, Arg2, Arg3, Arg4, Arg5, Arg6, Arg7, time];
        }
    }
}
