using System;
using System.Text;

namespace VdlV.TextGenerator {
    public class FixedString : TextGenerator {
        private readonly string _text;
        public FixedString(string text) => _text = text;

        public override bool Generate(StringBuilder output, BindingList b, Random rng) {
            output.Append(_text);
            return true;
        }
    }
}
