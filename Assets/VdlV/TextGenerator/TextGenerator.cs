using System;
using System.Collections.Generic;
using System.Text;
using VdlV.Utilities;
using TED;

namespace VdlV.TextGenerator {
    using static Randomize;
    using static BindingList;

    // ReSharper disable MemberCanBePrivate.Global
    public abstract class TextGenerator {
        public string Random(Random rng) => Generate(Global, rng);
        public Function<string> GenerateRandom {
            get {
                var rng = MakeRng();
                return new Function<string>(nameof(GenerateRandom),
                    () => Random(rng), false);
            }
        }
        public Func<string> GenerateRandomString {
            get {
                var rng = MakeRng();
                return () => Random(rng);
            }
        }

        public string RandomUnique(Random rng) => GenerateUnique(Global, rng);
        public Function<string> GenerateRandomUnique {
            get {
                var rng = MakeRng();
                return new Function<string>(nameof(GenerateRandomUnique), 
                    () => RandomUnique(rng), false);
            }
        }

        public string Generate(BindingList parameters, Random rng) {
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
