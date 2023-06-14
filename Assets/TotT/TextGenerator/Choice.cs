using System;
using System.Linq;
using System.Text;
using TotT.Utilities;
using UnityEngine;

namespace TotT.TextGenerator
{
    public class Choice : TextGenerator
    {
        public readonly struct Option
        {
            public readonly double Weight;
            public readonly TextGenerator Generator;

            public Option(double weight, TextGenerator generator)
            {
                Weight = weight;
                Generator = generator;
            }

            public static implicit operator Option((double weight, TextGenerator g) choice) =>
                new(choice.weight, choice.g);

            public static implicit operator Option(TextGenerator g) => new(1, g);
        }

        private readonly Option[] choices;
        private double totalWeight;
        public Choice(string name, params Option[] choices) : base(name)
        {
            this.choices = choices;
        }

        public Choice(params Option[] choices) : this("Choice", choices) { }

        public override bool Generate(StringBuilder output, BindingList b)
        {
            var startingLength = output.Length;
            var sorted = new Option[choices.Length];
            for (var i = 0; i < sorted.Length; i++)
            {
                sorted[i] = new Option(Math.Pow(Randomize.Double(1), 1 / choices[i].Weight), choices[i].Generator);
            }
            Array.Sort(sorted, (a, b) => -a.Weight.CompareTo(b.Weight));
            return sorted.Any(choice =>
            {
                output.Length = startingLength; // backtrack
                return choice.Generator.Generate(output, b);
            });
        }
    }
}
