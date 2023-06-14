using System.Text;

namespace TotT.TextGenerator
{
    public abstract class TextGenerator
    {
        public readonly string Name;

        protected TextGenerator(string name)
        {
            Name = name;
        }

        public string Generate()
        {
            var b = new StringBuilder();
            Generate(b, null);
            return b.ToString();
        }

        public abstract bool Generate(StringBuilder output, BindingList b);

        public static implicit operator TextGenerator(string s) => new FixedString(s);
    }
}
