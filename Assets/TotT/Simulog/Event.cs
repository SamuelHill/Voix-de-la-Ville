using Assets.TotT.Simulog;
using TED;
using TED.Interpreter;
using TotT.Simulator;
using TotT.ValueTypes;

namespace TotT.Simulog {
    public class Event<T1> : TablePredicate<T1>, IEvent
    {
        public Event(string name, IColumnSpec<T1> arg1) : base(name, arg1) { }

        private TablePredicate<T1, TimePoint> _chronicle;

        public TablePredicate<T1, TimePoint> Chronicle
        {
            get
            {
                if (_chronicle == null)
                    _chronicle =
                        new TablePredicate<T1, TimePoint>(Name + "Chronicle", (Var<T1>)DefaultVariables[0], Variables.time);
                _chronicle.Add.If(this, TalkOfTheTown.Time.CurrentTimePoint[Variables.time]);
                return _chronicle;
            }
        }

        public TablePredicate ChronicleUntyped => Chronicle;

        public Event<T1> OccursWhen(params Goal[] conditions)
        {
            If(conditions);
            return this;
        }

        public Event<T1> Causes(params Effect[] effects)
        {
            foreach (var e in effects)
                e.GenerateCode(DefaultGoal);
            return this;
        }

        public override TableGoal<T1> this[Term<T1> arg1] => new EventGoal(this, arg1);

        public class EventGoal : TableGoal<T1>, IOccurrence
        {
            public EventGoal(Event<T1> e, Term<T1> arg1) : base(e, arg1)
            { }
        }

    }


#if notdef
    public class Event<T1, T2, T3, TTime> {
        public readonly TablePredicate<T1, T2, T3, TTime> Predicate;

        public readonly Term<TTime> RecordTime;

        public Event(TablePredicate<T1, T2, T3, TTime> predicate, Term<TTime> recordTime) {
            Predicate = predicate;
            RecordTime = recordTime;
        }

        public void Perform(Definition<T1> assignArg1, Term<T2> arg2, Definition<T3> assignArg3, Goal ready) =>
            Predicate[assignArg1.Arg1, arg2, assignArg3.Arg1, RecordTime].If(ready, assignArg1, assignArg3);
        public void Perform(Definition<T1, string> assignArg1, Term<string> arg1String, Term<T2> arg2, Definition<T3> assignArg3, Goal ready) =>
            Predicate[assignArg1.Arg1, arg2, assignArg3.Arg1, RecordTime].If(ready, assignArg1[assignArg1.Arg1, arg1String], assignArg3);


        //public TableGoal this[Term<T> arg] => new Instance(this, arg);

        class Instance {
            private Event<T1, T2, T3, TTime> Event;
            private Term<T1> Arg1;
            private Term<T2> Arg2;
            private Term<T3> Arg3;

            public Instance(Event<T1, T2, T3, TTime> @event, Term<T1> arg1, Term<T2> arg2, Term<T3> arg3) {
                Event = @event;
                Arg1 = arg1;
                Arg2 = arg2;
                Arg3 = arg3;
            }

            public TableGoal At(Term<TTime> t) => Event.Predicate[Arg1, Arg2, Arg3, t];
        }
    }
#endif
}