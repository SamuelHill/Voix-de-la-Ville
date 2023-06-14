using System.Linq;

namespace TotT.Utilities.CFG {
    public class Production : ITerminal {
        private readonly ITerminal[] _terminals;
        public Production(params ITerminal[] terminals) => _terminals = terminals;

        public string Generate() => string.Join(" ", _terminals.Select(t => t.Value()));
        string ITerminal.Value() => Generate();
    }
}
