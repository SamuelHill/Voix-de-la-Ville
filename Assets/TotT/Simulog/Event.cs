using TED;
using TED.Interpreter;

namespace TotT.Simulog {
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
}