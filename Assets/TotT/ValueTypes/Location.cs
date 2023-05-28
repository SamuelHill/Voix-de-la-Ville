using System;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static StringProcessing;

    public class Location {
        private readonly Guid _id;
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly string Name;

        private Location() => _id = Guid.NewGuid();
        public Location(string name) : this() => Name = Title(name);

        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => _id.GetHashCode();
        public static bool operator ==(Location l, string potentialName) => l != null && l.Name == potentialName;
        public static bool operator !=(Location l, string potentialName) => !(l == potentialName);

        public override string ToString() => Name;
        public static Location FromString(string locationString) => new(locationString);
    }
}