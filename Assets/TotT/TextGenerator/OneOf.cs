using System;
using System.Text;
using TotT.Utilities;

namespace TotT.TextGenerator
{
    public class OneOf : TextGenerator
    {
        public readonly struct Choice
        {
            public readonly double Weight;
            public readonly string Text;

            public Choice(double weight, string text)
            {
                Weight = weight;
                Text = text;
            }

            public static implicit operator Choice((double weight, string text) choice) =>
                new(choice.weight, choice.text);

            public static implicit operator Choice(string text) => new(1, text);
        }

        private readonly Choice[] choices;
        private readonly double[] cumulative;
        private double totalWeight;
        public OneOf(string name, params Choice[] choices) : base(name)
        {
            this.choices = choices;
            cumulative = new double[choices.Length];
            double weight = 0;
            for (var i = 0; i < choices.Length; i++)
            {
                weight += choices[i].Weight;
                cumulative[i] = weight;
            }

            totalWeight = weight;
        }

        public OneOf(params Choice[] choices) : this("text", choices) { }

        public override bool Generate(StringBuilder output, BindingList b)
        {
            var weight = Randomize.Double(totalWeight);
            var i = Array.BinarySearch(cumulative, weight);
            if (i < 0) i = ~i;
            output.Append(choices[i].Text);
            return true;
        }
    }
}