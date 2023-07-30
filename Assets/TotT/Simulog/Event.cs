using TED;
using TED.Interpreter;
using TotT.Simulator;
using TotT.Time;

// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TotT.Simulog {
    using static Clock;
    using static Variables;

    public class Event<T1> : TablePredicate<T1>, IEvent {

        public Event(string name, IColumnSpec<T1> arg1) : base(name, arg1) { }

        #region Chronicle
        private TablePredicate<T1, TimePoint> _chronicle;
        
        public TablePredicate<T1, TimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, TimePoint>(Name + "Chronicle", 
                    (Var<T1>)DefaultVariables[0], time);
                _chronicle.Add.If(this, CurrentTimePoint[time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;
        #endregion

        public Event<T1> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1> this[Term<T1> arg1] => new(this, arg1);
        
    }

    public class Event<T1, T2> : TablePredicate<T1, T2>, IEvent {

        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) :
            base(name, arg1, arg2) { }

        #region Chronicle
        private TablePredicate<T1, T2, TimePoint> _chronicle;
        
        public TablePredicate<T1, T2, TimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, TimePoint>(Name + "Chronicle", 
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], time);
                _chronicle.Add.If(this, CurrentTimePoint[time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;
        #endregion

        public Event<T1, T2> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2> this[Term<T1> arg1, Term<T2> arg2] => 
            new(this, arg1, arg2);
        
    }

    public class Event<T1, T2, T3> : TablePredicate<T1, T2, T3>, IEvent {

        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) :
            base(name, arg1, arg2, arg3) { }

        #region Chronicle
        private TablePredicate<T1, T2, T3, TimePoint> _chronicle;
        
        public TablePredicate<T1, T2, T3, TimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, TimePoint>(Name + "Chronicle",
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2], time);
                _chronicle.Add.If(this, CurrentTimePoint[time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;
        #endregion

        public Event<T1, T2, T3> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3] =>
            new (this, arg1, arg2, arg3);
    }

    public class Event<T1, T2, T3, T4> : TablePredicate<T1, T2, T3, T4>, IEvent {

        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, 
                     IColumnSpec<T3> arg3, IColumnSpec<T4> arg4) :
            base(name, arg1, arg2, arg3, arg4) { }

        #region Chronicle
        private TablePredicate<T1, T2, T3, T4, TimePoint> _chronicle;
        
        public TablePredicate<T1, T2, T3, T4, TimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, TimePoint>(Name + "Chronicle",
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2],
                    (Var<T4>)DefaultVariables[3], time);
                _chronicle.Add.If(this, CurrentTimePoint[time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;
        #endregion

        public Event<T1, T2, T3, T4> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4> this[Term<T1> arg1, Term<T2> arg2, 
                                                       Term<T3> arg3, Term<T4> arg4] =>
            new(this, arg1, arg2, arg3, arg4);
    }

    public class Event<T1, T2, T3, T4, T5> : TablePredicate<T1, T2, T3, T4, T5>, IEvent {

        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5) :
            base(name, arg1, arg2, arg3, arg4, arg5) { }

        #region Chronicle
        private TablePredicate<T1, T2, T3, T4, T5, TimePoint> _chronicle;
        
        public TablePredicate<T1, T2, T3, T4, T5, TimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, T5, TimePoint>(Name + "Chronicle",
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2],
                    (Var<T4>)DefaultVariables[3], (Var<T5>)DefaultVariables[4], time);
                _chronicle.Add.If(this, CurrentTimePoint[time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;
        #endregion

        public Event<T1, T2, T3, T4, T5> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4, T5> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4, T5> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, 
                                                           Term<T4> arg4, Term<T5> arg5] =>
            new(this, arg1, arg2, arg3, arg4, arg5);
    }

    public class Event<T1, T2, T3, T4, T5, T6> : TablePredicate<T1, T2, T3, T4, T5, T6>, IEvent {

        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6) { }

        #region Chronicle
        private TablePredicate<T1, T2, T3, T4, T5, T6, TimePoint> _chronicle;
        
        public TablePredicate<T1, T2, T3, T4, T5, T6, TimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, T5, T6, TimePoint>(Name + "Chronicle", 
                    (Var<T1>)DefaultVariables[0], (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2], 
                    (Var<T4>)DefaultVariables[3], (Var<T5>)DefaultVariables[4], (Var<T6>)DefaultVariables[5], time);
                _chronicle.Add.If(this, CurrentTimePoint[time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;
        #endregion

        public Event<T1, T2, T3, T4, T5, T6> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4, T5, T6> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4, T5, T6> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, 
                                                               Term<T4> arg4, Term<T5> arg5, Term<T6> arg6] =>
            new(this, arg1, arg2, arg3, arg4, arg5, arg6);
    }

    public class Event<T1, T2, T3, T4, T5, T6, T7> : TablePredicate<T1, T2, T3, T4, T5, T6, T7>, IEvent {

        public Event(string name, IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3,
                     IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7) :
            base(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7) { }

        #region Chronicle
        private TablePredicate<T1, T2, T3, T4, T5, T6, T7, TimePoint> _chronicle;
        
        public TablePredicate<T1, T2, T3, T4, T5, T6, T7, TimePoint> Chronicle {
            get {
                _chronicle ??= new TablePredicate<T1, T2, T3, T4, T5, T6, T7, TimePoint>(Name + "Chronicle", (Var<T1>)DefaultVariables[0],
                    (Var<T2>)DefaultVariables[1], (Var<T3>)DefaultVariables[2], (Var<T4>)DefaultVariables[3], (Var<T5>)DefaultVariables[4],
                    (Var<T6>)DefaultVariables[5], (Var<T7>)DefaultVariables[6], time);
                _chronicle.Add.If(this, CurrentTimePoint[time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;
        #endregion

        public Event<T1, T2, T3, T4, T5, T6, T7> OccursWhen(params Goal[] conditions) {
            If(conditions);
            return this;
        }
        public Event<T1, T2, T3, T4, T5, T6, T7> Causes(params Effect[] effects) {
            foreach (var e in effects) e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1, T2, T3, T4, T5, T6, T7> this[Term<T1> arg1, Term<T2> arg2, Term<T3> arg3, Term<T4> arg4,
                                                                   Term<T5> arg5, Term<T6> arg6, Term<T7> arg7] =>
            new(this, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
}
