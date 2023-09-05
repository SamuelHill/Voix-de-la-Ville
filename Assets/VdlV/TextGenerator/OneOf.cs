using System;
using System.Text;
using VdlV.Utilities;

namespace VdlV.TextGenerator {
    using static Array;
    using static Randomize;

    // ReSharper disable MemberCanBePrivate.Global
    public class OneOf : TextGenerator {
        public readonly struct Choice {
            public readonly double Weight;
            public readonly string Text;

            public Choice(double weight, string text) {
                Weight = weight;
                Text = text;
            }

            public static implicit operator Choice((double weight, string text) choice) =>
                new(choice.weight, choice.text);

            public static implicit operator Choice(string text) => new(1, text);
        }

        private readonly Choice[] _choices;
        private readonly double[] _cumulative;
        private readonly double _totalWeight;

        public OneOf(params Choice[] choices) {
            _choices = choices;
            _cumulative = new double[choices.Length];
            double weight = 0;
            for (var i = 0; i < choices.Length; i++) {
                weight += choices[i].Weight;
                _cumulative[i] = weight;
            }
            _totalWeight = weight;
        }

        public override bool Generate(StringBuilder output, BindingList b, Random rng) {
            var weight = Double(_totalWeight, rng);
            var i = BinarySearch(_cumulative, weight);
            if (i < 0) i = ~i;
            output.Append(_choices[i].Text);
            return true;
        }
    }
}