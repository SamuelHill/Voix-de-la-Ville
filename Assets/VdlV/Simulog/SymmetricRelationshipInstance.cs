using System;
using TED;
using TED.Primitives;

namespace VdlV.Simulog {
    public class SymmetricRelationshipInstance<T> : IComparable<SymmetricRelationshipInstance<T>>, IEquatable<SymmetricRelationshipInstance<T>> 
        where T : IComparable<T>, IEquatable<T> {
        public readonly T Main;
        public readonly T Other;

        private SymmetricRelationshipInstance(T main, T other) {
            if (main.CompareTo(other) < 0) {
                Main = main;
                Other = other;
            } else {
                Main = other;
                Other = main;
            }
        }

        public static Function<T, T, SymmetricRelationshipInstance<T>> NewRelationshipInstance =>
            new(nameof(NewRelationshipInstance), (main, other) => new SymmetricRelationshipInstance<T>(main, other));
        public static PrimitiveTest<SymmetricRelationshipInstance<T>, SymmetricTuple<T>> RelationshipInstanceEqualsTuple =>
            new(nameof(RelationshipInstanceEqualsTuple), (symPair, pair) => symPair.Equals(pair));

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(SymmetricRelationshipInstance<T> other) {
            var mainCompareTo = Main.CompareTo(other.Main);
            return mainCompareTo != 0 ? mainCompareTo : Other.CompareTo(other.Other);
        }
        public bool Equals(SymmetricRelationshipInstance<T> other) => other is not null && ReferenceEquals(this, other);
        private bool Equals(SymmetricTuple<T> pair) => Main.Equals(pair.Item1) && Other.Equals(pair.Item2);
        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => HashCode.Combine(Main, Other);

        // ****************************************************************************************

        public override string ToString() => $"{Main}, {Other}";
    }
}
