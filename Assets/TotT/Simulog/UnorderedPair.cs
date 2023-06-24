using System;
using System.Collections.Generic;

namespace TotT.Simulog {
    public readonly struct UnorderedPair<T> : IComparable<UnorderedPair<T>>, IEquatable<UnorderedPair<T>> where T : IComparable<T>, IEquatable<T> {
        public readonly T A;
        public readonly T B;

        public UnorderedPair(T a, T b) {
            if (a.CompareTo(b) < 0) {
                A = a;
                B = b;
            } else {
                B = a; 
                A = b;
            }
        }

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(UnorderedPair<T> other) {
            var aComparison = A.CompareTo(other.A);
            return aComparison != 0 ? aComparison : B.CompareTo(other.B);
        }
        public bool Equals(OrderedPair<T> pair) => Equals(pair.Main, pair.Other);
        public bool Equals(T main, T other) => Equals(new UnorderedPair<T>(main, other));
        public bool Equals(UnorderedPair<T> other) => EqualityComparer<T>.Default.Equals(A, other.A) && 
                                                      EqualityComparer<T>.Default.Equals(B, other.B);
        public override bool Equals(object obj) => obj is UnorderedPair<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(A, B);

        // ****************************************************************************************

        public override string ToString() => $"{A}, {B}";
    }
}
