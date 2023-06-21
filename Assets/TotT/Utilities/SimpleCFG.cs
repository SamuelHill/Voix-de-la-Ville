using System.Linq;

namespace TotT.Utilities {

    // ReSharper disable InconsistentNaming
    public static class SimpleCFG {
        public abstract class TextGenerator {
            public abstract string Generate();

            public static implicit operator TextGenerator(string s) => new Constant(s);
        }

        public class Constant : TextGenerator {
            private readonly string _value;
            public Constant(string value) => _value = value;
            public override string Generate() => _value;
        }

        public class Terminal : TextGenerator {
            private readonly string[] _values;
            public Terminal(params string[] values) => _values = values;
            public override string Generate() => _values.RandomElement();
        }

        public class Sequence : TextGenerator {
            private readonly TextGenerator[] _segments;
            public Sequence(params TextGenerator[] segments) => _segments = segments;
            public override string Generate() =>
                string.Join(" ", _segments.Select(t => t.Generate()));
        }

        public class Grammar {
            private readonly Sequence[] _options;
            public Grammar(params Sequence[] options) => _options = options;
            public string Generate() => _options.RandomElement().Generate();
        }

        public static Terminal Terms(params string[] values) => new(values);
        public static Sequence Order(params TextGenerator[] segments) => new(segments);
        public static Grammar Named(params Sequence[] options) => new(options);

        private static readonly Sequence SaintSomething = Order("St", 
            Terms("Asmodeus", "Bael", "Balam", "Beleth", "Belial", "Paimon", "Purson", "Zagon"));

        private static readonly Sequence HospitalName = Order(
            Terms("Health Care", "Medical", "Wellness"), 
            Terms("Center", "Clinic", "Hospital"));

        public static readonly Grammar HospitalNames = Named(SaintSomething, 
                                                             Order(SaintSomething, HospitalName), 
                                                             Order("Memorial", HospitalName));

        private static readonly Sequence ChildCare = Order(
            Terms("Sunshine", "Rainbow", "Pumpkin", "Shinning Stars", "Fantastic Friends", "Little Wonders", 
                  "Dragonfly", "Friendly Faces", "New Beginnings", "Wildflower", "Butterfly", "Precious Angel"), 
            Terms("Academy", "Child Care", "Daycare", "Learning Center", "Nursery", "Preschool"));

        public static readonly Grammar DaycareNames = new(ChildCare, Order("The", ChildCare));
    }
}
