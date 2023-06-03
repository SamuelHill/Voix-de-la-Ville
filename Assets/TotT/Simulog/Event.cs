using TED;
using TED.Interpreter;

namespace TotT.Simulog
{
    public class Event<T> {
        public readonly TablePredicate<T, float> Predicate;

        public Event(TablePredicate<T, float> predicate) => Predicate = predicate;

        //public TableGoal this[Term<T> arg] => new Instance(this, arg);

        class Instance {
            private Event<T> Event;
            private Term<T> Arg;

            public Instance(Event<T> @event, Term<T> arg) {
                Event = @event;
                Arg = arg;
            }

            public TableGoal At(Term<float> t) => Event.Predicate[Arg,t];
        }
    }
}