using System;
using System.Linq;
using System.Text;
using VdlV.Utilities;

namespace VdlV.TextGenerator {
    public class Choice : TextGenerator {
        public readonly struct Option {
            public readonly double Weight;
            public readonly TextGenerator Generator;

            public Option(double weight, TextGenerator generator) {
                Weight = weight;
                Generator = generator;
            }

            public static implicit operator Option((double weight, TextGenerator g) choice) =>
                new(choice.weight, choice.g);

            public static implicit operator Option(TextGenerator g) => new(1, g);
        }

        private readonly Option[] _choices;
        private double _totalWeight;
        public Choice(params Option[] choices) => _choices = choices;

        public override bool Generate(StringBuilder output, BindingList b) {
            var startingLength = output.Length;
            var sorted = new Option[_choices.Length];
            for (var i = 0; i < sorted.Length; i++)
                sorted[i] = new Option(Math.Pow(Randomize.Double(1), 1 / _choices[i].Weight), _choices[i].Generator);
            Array.Sort(sorted, (a, b) => -a.Weight.CompareTo(b.Weight));
            return sorted.Any(choice => {
                output.Length = startingLength; // backtrack
                return choice.Generator.Generate(output, b);
            });
        }
    }
}
