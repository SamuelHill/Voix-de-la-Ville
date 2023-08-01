using TED;

namespace VdlV.Simulog {
    /// <summary>
    /// Interface for Event predicates, base class for Event predicates is TablePredicate
    /// (C# doesn't allow multiple inheritance).
    /// </summary>
    public interface IEvent {
        // ReSharper disable once UnusedMember.Global
        public TablePredicate ChronicleUntyped { get; }
    }
}
