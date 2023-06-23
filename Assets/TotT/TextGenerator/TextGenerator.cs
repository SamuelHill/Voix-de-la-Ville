using System.Collections.Generic;
using System.Text;
using TED;
using TED.Primitives;
using static TED.Language;

namespace TotT.TextGenerator
{
    public abstract class TextGenerator
    {
        public string Random => Generate(BindingList.Global);
        public Function<string> GenerateRandom => Member<string>(this, nameof(Random), false);

        public string RandomUnique => GenerateUnique(BindingList.Global);
        public Function<string> GenerateRandomUnique => Member<string>(this, nameof(RandomUnique), false);

        public string Generate(BindingList parameters)
        {
            var b = new StringBuilder();
            Generate(b, parameters);
            return b.ToString();
        }

        public string GenerateUnique(BindingList parameters)
        {
            previouslyGenerated ??= new HashSet<string>();

            var b = new StringBuilder();
            string generated;
            do
            {
                b.Clear();
                Generate(b, parameters);
                generated = b.ToString();
            } while (previouslyGenerated.Contains(generated));

            previouslyGenerated.Add(generated);


            return b.ToString();
        }

        private HashSet<string> previouslyGenerated;

        public abstract bool Generate(StringBuilder output, BindingList b);

        public static implicit operator TextGenerator(string s) => new FixedString(s);
    }
}
