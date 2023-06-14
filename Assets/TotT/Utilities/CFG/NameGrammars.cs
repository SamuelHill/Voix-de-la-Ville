namespace TotT.Utilities.CFG {
    public static class NameGrammars {
        // ReSharper disable InconsistentNaming
        private static readonly Constant St = new("St");

        private static readonly Terminal Liturgical = new("Asmodeus");

        private static readonly Production SaintSomething = new(St, Liturgical);

        public static readonly Grammar HospitalNames = new(SaintSomething);

    }
}
