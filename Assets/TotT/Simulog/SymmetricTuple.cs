using System;
using System.Collections.Generic;
using TED;
using TED.Primitives;

namespace TotT.Simulog {
    public readonly struct SymmetricTuple<T> : IComparable<SymmetricTuple<T>>, IEquatable<SymmetricTuple<T>> where T : IComparable<T>, IEquatable<T> {
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
        public static Function<(T, T), SymmetricTuple<T>> SymmetricTupleFromTuple =>
            new(nameof(SymmetricTupleFromTuple), pair => new SymmetricTuple<T>(pair.Item1, pair.Item2));
        public static PrimitiveTest<SymmetricTuple<T>, (T, T)> SymmetricTupleEqualsTuple =>
            new(nameof(SymmetricTupleEqualsTuple), (symPair, pair) => symPair.Equals(pair));

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(SymmetricTuple<T> other) {
            var aComparison = Item1.CompareTo(other.Item1);
            return aComparison != 0 ? aComparison : Item2.CompareTo(other.Item2);
        }
        public bool Equals(SymmetricTuple<T> pair) =>
            EqualityComparer<T>.Default.Equals(Item1, pair.Item1) && 
            EqualityComparer<T>.Default.Equals(Item2, pair.Item2);
        private bool Equals(ValueTuple<T, T> pair) => Equals(new SymmetricTuple<T>(pair));
        public override bool Equals(object obj) => obj is SymmetricTuple<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Item1, Item2);

        // ****************************************************************************************

        public override string ToString() => $"{Item1}, {Item2}";
    }
}
