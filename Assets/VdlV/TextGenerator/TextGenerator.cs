using System;
using System.Collections.Generic;
using System.Text;
using TED;

namespace VdlV.TextGenerator {
    using static Utilities.Randomize;

    public abstract class TextGenerator {
        public string Random(Random rng) => Generate(BindingList.Global, rng);
        public Function<string> GenerateRandom {
            get {
                var rng = MakeRng();
                return new Function<string>(nameof(GenerateRandom),
                    () => Random(rng),false);
            }
        }

        public string RandomUnique(Random rng) => GenerateUnique(BindingList.Global, rng);
        public Function<string> GenerateRandomUnique {
            get {
                var rng = MakeRng();
                return new Function<string>(nameof(GenerateRandomUnique), 
                    () => RandomUnique(rng), false);
            }
        }

        public string Generate(BindingList parameters, Random rng)
        {
            var b = new StringBuilder();
            Generate(b, parameters, rng);
            return b.ToString();
        }

        public string GenerateUnique(BindingList parameters, Random rng) {
            _previouslyGenerated ??= new HashSet<string>();
            var b = new StringBuilder();
            string generated;
            do {
                b.Clear();
                Generate(b, parameters, rng);
                generated = b.ToString();
            } while (_previouslyGenerated.Contains(generated));
            _previouslyGenerated.Add(generated);
            return b.ToString();
        }

        private HashSet<string> _previouslyGenerated;

        public abstract bool Generate(StringBuilder output, BindingList b, Random rng);

        public static implicit operator TextGenerator(string s) => new FixedString(s);
    }
}
