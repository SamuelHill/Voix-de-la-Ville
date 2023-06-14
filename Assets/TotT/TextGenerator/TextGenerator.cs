using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace TotT.TextGenerator
{
    public abstract class TextGenerator
    {
        public string Random 
        {
            get
            {
                var b = new StringBuilder();
                Generate(b, null);
                return b.ToString();
            }
        }

        public string RandomUnique
        {
            get
            {
                previouslyGenerated ??= new HashSet<string>();

                var b = new StringBuilder();
                string generated;
                do
                {
                    b.Clear();
                    Generate(b, null);
                    generated = b.ToString();
                } while (previouslyGenerated.Contains(generated));

                previouslyGenerated.Add(generated);


                return b.ToString();
            }
        }

        private HashSet<string> previouslyGenerated;

        public abstract bool Generate(StringBuilder output, BindingList b);

        public static implicit operator TextGenerator(string s) => new FixedString(s);
    }
}
