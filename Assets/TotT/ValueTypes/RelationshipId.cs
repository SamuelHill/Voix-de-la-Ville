using System;

namespace TotT.ValueTypes {
    public class RelationshipId<T> {
        private readonly Guid _id;
        public T Main;
        public T Other;

        public RelationshipId(T main, T other) {
            Main = main;
            Other = other;
            _id = Guid.NewGuid();
        }

        public override int GetHashCode() => _id.GetHashCode();
        public override string ToString() => $"{Main}, {Other}";
    }
}
