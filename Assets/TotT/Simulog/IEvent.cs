using TED;

namespace TotT.Simulog {
    /// <summary>
    /// Interface for Event predicates, base class for Event predicates is TablePredicate
    /// (C# doesn't allow multiple inheritance).
    /// </summary>
    public interface IEvent {
        public TablePredicate ChronicleUntyped { get; }
    }
}
