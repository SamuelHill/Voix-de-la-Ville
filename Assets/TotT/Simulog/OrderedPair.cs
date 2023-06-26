using System;

namespace TotT.Simulog {
    public class OrderedPair<T> : IComparable<OrderedPair<T>>, IEquatable<OrderedPair<T>> where T : IComparable<T>, IEquatable<T> {
        public readonly T Main;
        public readonly T Other;

        public OrderedPair(T main, T other) {
            Main = main;
            Other = other;
        }

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(OrderedPair<T> other) {
            var mainCompareTo = Main.CompareTo(other.Main);
            return mainCompareTo != 0 ? mainCompareTo : Other.CompareTo(other.Other);
        }
        public bool Equals(OrderedPair<T> other) => other is not null && ReferenceEquals(this, other);
        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => HashCode.Combine(Main, Other);

        // ****************************************************************************************

        public override string ToString() => $"{Main}, {Other}";
    }
}
