namespace TotT.Utilities.CFG {
    public readonly struct Constant : ITerminal {
        private readonly string _value;
        public Constant(string value) { _value = value; }

        string ITerminal.Value() => _value;
    }
}
