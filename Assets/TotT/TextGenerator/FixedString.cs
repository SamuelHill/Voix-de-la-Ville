using System.Text;

namespace TotT.TextGenerator
{
    public class FixedString : TextGenerator
    {
        public readonly string Text;
        public FixedString(string text)
        {
            Text = text;
        }

        public override bool Generate(StringBuilder output, BindingList b)
        {
            output.Append(Text);
            return true;
        }
    }
}
