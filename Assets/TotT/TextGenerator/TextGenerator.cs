using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace TotT.TextGenerator
{
    public abstract class TextGenerator
    {
        public string Random => Generate(null);

        public string RandomUnique => GenerateUnique(null);

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
