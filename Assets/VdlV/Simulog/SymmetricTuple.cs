using System;
using System.Collections.Generic;
using VdlV.Utilities;
using TED;
using TED.Primitives;

namespace VdlV.Simulog {
    using static StringProcessing;

    public readonly struct SymmetricTuple<T> : IComparable<SymmetricTuple<T>>, IEquatable<SymmetricTuple<T>> //, ISerializableValue<SymmetricTuple<T>> 
        where T : IComparable<T>, IEquatable<T> {
        public readonly T Item1;
        public readonly T Item2;

        private SymmetricTuple(T item1, T item2) {
            if (item1.CompareTo(item2) < 0) {
                Item1 = item1;
                Item2 = item2;
            } else {
                Item2 = item1; 
                Item1 = item2;
            }
        }
        private SymmetricTuple((T, T) pair) : this(pair.Item1, pair.Item2) { }

        public static Function<T, T, SymmetricTuple<T>> NewSymmetricTuple =>
            new(nameof(NewSymmetricTuple), (main, other) => new SymmetricTuple<T>(main, other));
        public static Function<(T, T), SymmetricTuple<T>> FromTuple =>
            new(nameof(FromTuple), pair => new SymmetricTuple<T>(pair.Item1, pair.Item2));
        public static PrimitiveTest<SymmetricTuple<T>, (T, T)> EqualsTuple =>
            new(nameof(EqualsTuple), (symPair, pair) => symPair.Equals(pair));
        public static PrimitiveTest<SymmetricTuple<T>, T, T> InOrder =>
            new(nameof(InOrder), (symPair, main, other) => symPair.Equals(main, other));

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(SymmetricTuple<T> other) {
            var aComparison = Item1.CompareTo(other.Item1);
            return aComparison != 0 ? aComparison : Item2.CompareTo(other.Item2);
        }
        private bool Equals(T item1, T item2) =>
            EqualityComparer<T>.Default.Equals(Item1, item1) &&
            EqualityComparer<T>.Default.Equals(Item2, item2);
        private bool Equals(ValueTuple<T, T> pair) => Equals(new SymmetricTuple<T>(pair));
        public bool Equals(SymmetricTuple<T> pair) => Equals(pair.Item1, pair.Item2);
        public override bool Equals(object obj) => obj is SymmetricTuple<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Item1, Item2);

        // ****************************************************************************************

        public override string ToString() => QuoteString($"{Item1}, {Item2}");
        
        public static SymmetricTuple<T> FromString(string tuple, Func<string, T> fromString) {
            var temp = CommaSeparated(tuple, fromString);
            return temp is { Length: 2 } ? new SymmetricTuple<T>(temp[0], temp[1]) :
                       throw new ArgumentException($"Couldn't convert string {tuple} to a SymmetricTuple<{typeof(T).Name}>");
        }
    }
}
