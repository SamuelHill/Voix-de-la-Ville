using System.Linq;
using TED;
using static TED.Language;

namespace TotT.Utilities {

    // ReSharper disable InconsistentNaming
    public static class SimpleCFG {
        public abstract class TextGenerator {
            public abstract string Generate { get; }
            public Function<string> GenerateName =>
                Member<string>(this, nameof(Generate), false);

            public static implicit operator TextGenerator(string s) => new Constant(s);
        }

        public class Constant : TextGenerator {
            public Constant(string value) => Generate = value;
            public override string Generate { get; }
        }

        public class Terminal : TextGenerator {
            private readonly string[] _values;
            public Terminal(params string[] values) => _values = values;
            public override string Generate => _values.RandomElement();
        }

        public class Sequence : TextGenerator {
            private readonly TextGenerator[] _segments;
            public Sequence(params TextGenerator[] segments) => _segments = segments;
            public override string Generate =>
                string.Join(" ", _segments.Select(t => t.Generate));
        }

        public class Grammar : TextGenerator {
            private readonly Sequence[] _options;
            public Grammar(params Sequence[] options) => _options = options;
            public override string Generate => _options.RandomElement().Generate;
        }

        public static Terminal Terms(params string[] values) => new(values);
        public static Sequence Order(params TextGenerator[] segments) => new(segments);
        public static Grammar Named(params Sequence[] options) => new(options);


        private static readonly Sequence SaintSomething = Order("St", 
            Terms("Asmodeus", "Bael", "Balam", "Beleth", "Belial", "Paimon", "Purson", "Zagon"));

        private static readonly Sequence Hospital = Order(
            Terms("Health Care", "Medical", "Wellness"), 
            Terms("Center", "Clinic", "Hospital"));

        public static readonly Grammar HospitalName = Named(SaintSomething, 
                                                             Order(SaintSomething, Hospital), 
                                                             Order("Memorial", Hospital));

        private static readonly Sequence ChildCare = Order(
            Terms("Sunshine", "Rainbow", "Pumpkin", "Shinning Stars", "Fantastic Friends", "Little Wonders", 
                  "Dragonfly", "Friendly Faces", "New Beginnings", "Wildflower", "Butterfly", "Precious Angel"), 
            Terms("Academy", "Child Care", "Daycare", "Learning Center", "Nursery", "Preschool"));

        public static readonly Grammar DaycareName = Named(ChildCare, Order("The", ChildCare));
    }
}
