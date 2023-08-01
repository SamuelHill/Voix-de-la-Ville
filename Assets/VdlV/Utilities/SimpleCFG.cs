using System.Linq;
using TED;
using static TED.Language;

namespace VdlV.Utilities {
    // ReSharper disable InconsistentNaming
    public static class SimpleCFG {
        private abstract class TextGenerator {
            public abstract string Generate { get; }
            public Function<string> GenerateName =>
                Member<string>(this, nameof(Generate), false);

            public static implicit operator TextGenerator(string s) => new Constant(s);
        }

        private class Constant : TextGenerator {
            public Constant(string value) => Generate = value;
            public override string Generate { get; }
        }

        private class Terminal : TextGenerator {
            private readonly string[] _values;
            public Terminal(params string[] values) => _values = values;
            public override string Generate => _values.RandomElement();
        }

        private class Sequence : TextGenerator {
            private readonly TextGenerator[] _segments;
            public Sequence(params TextGenerator[] segments) => _segments = segments;
            public override string Generate =>
                string.Join(" ", _segments.Select(t => t.Generate));
        }

        private class Grammar : TextGenerator {
            private readonly Sequence[] _options;
            public Grammar(params Sequence[] options) => _options = options;
            public override string Generate => _options.RandomElement().Generate;
        }

        private static Terminal Terms(params string[] values) => new(values);
        private static Sequence Order(params TextGenerator[] segments) => new(segments);
        private static Function<string> Name(params Sequence[] options) => new Grammar(options).GenerateName;

        private static readonly Sequence SaintSomething = Order("St", 
            Terms("Asmodeus", "Bael", "Balam", "Beleth", "Belial", "Paimon", "Purson", "Zagon"));

        private static readonly Sequence Hospital = Order(
            Terms("Health Care", "Medical", "Wellness"), 
            Terms("Center", "Clinic", "Hospital"));

        public static readonly Function<string> HospitalName = Name(SaintSomething, 
            Order(SaintSomething, Hospital), Order("Memorial", Hospital));
    }
}
