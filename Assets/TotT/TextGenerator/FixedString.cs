using System.Text;

namespace TotT.TextGenerator
{
    public class FixedString : TextGenerator
    {
        public FixedString(string name) : base(name)
        {
        }

        public override bool Generate(StringBuilder output, BindingList b)
        {
            output.Append(Name);
            return true;
        }
    }
}
