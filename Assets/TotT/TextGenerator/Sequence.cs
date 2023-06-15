using System.Linq;
using System.Text;

namespace TotT.TextGenerator
{
    public class Sequence : TextGenerator
    {
        public readonly TextGenerator[] Generators;
        public Sequence(TextGenerator[] generators)
        {
            Generators = generators;
        }


        public override bool  Generate(StringBuilder output, BindingList b)
            => Generators.All(g => g.Generate(output, b));
    }
}
