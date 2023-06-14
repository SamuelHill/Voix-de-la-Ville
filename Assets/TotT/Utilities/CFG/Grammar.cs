namespace TotT.Utilities.CFG {
    public class Grammar {
        private readonly Production[] _productions;
        public Grammar(params Production[] productions) => _productions = productions;

        public string Generate() => _productions.RandomElement().Generate();
    }
}
