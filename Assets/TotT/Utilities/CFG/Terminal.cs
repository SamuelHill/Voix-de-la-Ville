namespace TotT.Utilities.CFG {
    using static Randomize;

    public class Terminal : ITerminal {
        private readonly string[] _values;
        public Terminal(params string[] values) => _values = values;

        string ITerminal.Value() => _values.RandomElement();
    }
}
