using System;
using TED;
using TED.Primitives;

namespace VdlV.Simulog {
    public class RelationshipInstance<T1, T2> : IComparable<RelationshipInstance<T1, T2>>, IEquatable<RelationshipInstance<T1, T2>> 
        where T1 : IComparable<T1>, IEquatable<T1> where T2 : IComparable<T2>, IEquatable<T2> {
        public readonly T1 Main;
        public readonly T2 Other;

        private RelationshipInstance(T1 main, T2 other) {
            Main = main;
            Other = other;
        }

        public static Function<T1, T2, RelationshipInstance<T1, T2>> NewRelationshipInstance =>
            new(nameof(NewRelationshipInstance), (main, other) => new RelationshipInstance<T1, T2>(main, other));
        public static PrimitiveTest<RelationshipInstance<T1, T2>, (T1, T2)> RelationshipInstanceEqualsTuple =>
            new(nameof(RelationshipInstanceEqualsTuple), (symPair, pair) => symPair.Equals(pair));

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(RelationshipInstance<T1, T2> other) {
            var mainCompareTo = Main.CompareTo(other.Main);
            return mainCompareTo != 0 ? mainCompareTo : Other.CompareTo(other.Other);
        }
        public bool Equals(RelationshipInstance<T1, T2> other) => other is not null && ReferenceEquals(this, other);
        private bool Equals((T1, T2) pair) => Main.Equals(pair.Item1) && Other.Equals(pair.Item2);
        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => HashCode.Combine(Main, Other);

        // ****************************************************************************************

        public override string ToString() => $"{Main}, {Other}";
    }
}
