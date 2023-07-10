using TED;
using TED.Interpreter;

namespace TotT.Simulog {
    public abstract class GenericEvent<T1, TTimePoint> : TablePredicate<T1>, IEvent {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";

        protected abstract Function<TTimePoint> CurrentTime { get; }

        protected GenericEvent(string name, IColumnSpec<T1> arg1) : base(name, arg1) { }

        private TablePredicate<T1, TTimePoint> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<T1, TTimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, TTimePoint>(Name + "Chronicle", (Var<T1>)DefaultVariables[0], _time);
                _chronicle.Add.If(this, CurrentTime[_time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public GenericEvent<T1, TTimePoint> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public GenericEvent<T1, TTimePoint> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1> this[Term<T1> arg1] => new EventGoal(this, arg1);

        private class EventGoal : TableGoal<T1>, IOccurrence<TTimePoint> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(GenericEvent<T1, TTimePoint> e, Term<T1> arg1) : base(e, arg1) { }
        }
    }

    public abstract class GenericEvent<T1, T2, TTimePoint> : TablePredicate<T1, T2>, IEvent {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";

        protected abstract Function<TTimePoint> CurrentTime { get; }

        protected GenericEvent(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) :
            base(name, arg1, arg2) { }

        private TablePredicate<T1, T2, TTimePoint> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<T1, T2, TTimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, TTimePoint>(Name + "Chronicle", 
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], _time);
                _chronicle.Add.If(this, CurrentTime[_time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public GenericEvent<T1, T2, TTimePoint> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public GenericEvent<T1, T2, TTimePoint> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2> this[Term<T1> arg1, Term<T2> arg2] =>
            new EventGoal(this, arg1, arg2);

        private class EventGoal : TableGoal<T1, T2>, IOccurrence<TTimePoint> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(GenericEvent<T1, T2, TTimePoint> e, Term<T1> arg1, Term<T2> arg2) :
                base(e, arg1, arg2) { }
        }
    }

    public abstract class GenericEvent<T1, T2, T3, TTimePoint> : TablePredicate<T1, T2, T3>, IEvent {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";

        protected abstract Function<TTimePoint> CurrentTime { get; }

        protected GenericEvent(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) :
            base(name, arg1, arg2, arg3) { }

        private TablePredicate<T1, T2, T3, TTimePoint> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<T1, T2, T3, TTimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, TTimePoint>(Name + "Chronicle",
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2], _time);
                _chronicle.Add.If(this, CurrentTime[_time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public GenericEvent<T1, T2, T3, TTimePoint> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public GenericEvent<T1, T2, T3, TTimePoint> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] =>
            new EventGoal(this, arg1, arg2, arg3);

        private class EventGoal : TableGoal<T1, T2, T3>, IOccurrence<TTimePoint> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(GenericEvent<T1, T2, T3, TTimePoint> e, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) :
                base(e, arg1, arg2, arg3) { }
        }
    }

    public abstract class GenericEvent<T1, T2, T3, T4, TTimePoint> : TablePredicate<T1, T2, T3, T4>, IEvent {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";

        protected abstract Function<TTimePoint> CurrentTime { get; }

        protected GenericEvent(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                               IColumnSpec<T4> arg4) :
            base(name, arg1, arg2, arg3, arg4) { }

        private TablePredicate<T1, T2, T3, T4, TTimePoint> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<T1, T2, T3, T4, TTimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, TTimePoint>(Name + "Chronicle",
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2],
                    (Var<T4>)DefaultVariables[3], _time);
                _chronicle.Add.If(this, CurrentTime[_time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public GenericEvent<T1, T2, T3, T4, TTimePoint> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public GenericEvent<T1, T2, T3, T4, TTimePoint> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3,
                                                               Term<T4> arg4] =>
            new EventGoal(this, arg1, arg2, arg3, arg4);

        private class EventGoal : TableGoal<T1, T2, T3, T4>, IOccurrence<TTimePoint> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(GenericEvent<T1, T2, T3, T4, TTimePoint> e, Term<T1> arg1, Term<T2> arg2,
                             Term<T3> arg3, Term<T4> arg4) :
                base(e, arg1, arg2, arg3, arg4) { }
        }
    }

    public abstract class GenericEvent<T1, T2, T3, T4, T5, TTimePoint> : TablePredicate<T1, T2, T3, T4, T5>, IEvent {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";

        protected abstract Function<TTimePoint> CurrentTime { get; }

        protected GenericEvent(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                               IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) :
            base(name, arg1, arg2, arg3, arg4, arg5) { }

        private TablePredicate<T1, T2, T3, T4, T5, TTimePoint> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<T1, T2, T3, T4, T5, TTimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, T5, TTimePoint>(Name + "Chronicle",
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2],
                    (Var<T4>)DefaultVariables[3], (Var<T5>)DefaultVariables[4], _time);
                _chronicle.Add.If(this, CurrentTime[_time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public GenericEvent<T1, T2, T3, T4, T5, TTimePoint> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public GenericEvent<T1, T2, T3, T4, T5, TTimePoint> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4, T5> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, 
                                                           Term<T4> arg4, Term<T5> arg5] =>
            new EventGoal(this, arg1, arg2, arg3, arg4, arg5);

        private class EventGoal : TableGoal<T1, T2, T3, T4, T5>, IOccurrence<TTimePoint> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(GenericEvent<T1, T2, T3, T4, T5, TTimePoint> e, Term<T1> arg1, Term<T2> arg2,
                             Term<T3> arg3, Term<T4> arg4, Term<T5> arg5) :
                base(e, arg1, arg2, arg3, arg4, arg5) { }
        }
    }

    public abstract class GenericEvent<T1, T2, T3, T4, T5, T6, TTimePoint> : TablePredicate<T1, T2, T3, T4, T5, T6>, IEvent {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";

        protected abstract Function<TTimePoint> CurrentTime { get; }

        protected GenericEvent(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                               IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6) { }

        private TablePredicate<T1, T2, T3, T4, T5, T6, TTimePoint> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<T1, T2, T3, T4, T5, T6, TTimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, T5, T6, TTimePoint>(Name + "Chronicle", 
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2], 
                    (Var<T4>)DefaultVariables[3], (Var<T5>)DefaultVariables[4], (Var<T6>)DefaultVariables[5], _time);
                _chronicle.Add.If(this, CurrentTime[_time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public GenericEvent<T1, T2, T3, T4, T5, T6, TTimePoint> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public GenericEvent<T1, T2, T3, T4, T5, T6, TTimePoint> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4, T5, T6> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, 
                                                               Term<T4> arg4, Term<T5> arg5, Term<T6> arg6] =>
            new EventGoal(this, arg1, arg2, arg3, arg4, arg5, arg6);

        private class EventGoal : TableGoal<T1, T2, T3, T4, T5, T6>, IOccurrence<TTimePoint> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(GenericEvent<T1, T2, T3, T4, T5, T6, TTimePoint> e, Term<T1> arg1, Term<T2> arg2,
                             Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6) :
                base(e, arg1, arg2, arg3, arg4, arg5, arg6) { }
        }
    }

    public abstract class GenericEvent<T1, T2, T3, T4, T5, T6, T7, TTimePoint> : TablePredicate<T1, T2, T3, T4, T5, T6, T7>, IEvent {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";

        protected abstract Function<TTimePoint> CurrentTime { get; }

        protected GenericEvent(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                               IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }

        private TablePredicate<T1, T2, T3, T4, T5, T6, T7, TTimePoint> _chronicle;

        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7, TTimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, T5, T6, T7, TTimePoint>(Name + "Chronicle", (Var<T1>)DefaultVariables[0],
                    (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2], (Var<T4>)DefaultVariables[3], (Var<T5>)DefaultVariables[4],
                    (Var<T6>)DefaultVariables[5], (Var<T7>)DefaultVariables[6], _time);
                _chronicle.Add.If(this, CurrentTime[_time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public GenericEvent<T1, T2, T3, T4, T5, T6, T7, TTimePoint> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public GenericEvent<T1, T2, T3, T4, T5, T6, T7, TTimePoint> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4, T5, T6, T7> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4,
                                                                   Term<T5> arg5, Term<T6> arg6, Term<T7> arg7] =>
            new EventGoal(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        private class EventGoal : TableGoal<T1, T2, T3, T4, T5, T6, T7>, IOccurrence<TTimePoint> {
            // ReSharper disable once SuggestBaseTypeForParameterInConstructor
            public EventGoal(GenericEvent<T1, T2, T3, T4, T5, T6, T7, TTimePoint> e, Term<T1> arg1, Term<T2> arg2,
                             Term<T3> arg3, Term<T4> arg4, Term<T5> arg5, Term<T6> arg6, Term<T7> arg7) :
                base(e, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }
        }
    }
}
