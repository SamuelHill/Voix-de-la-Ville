using System;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static StringProcessing;

    /// <summary>
    /// Location reference - wraps name string for an individual location.
    /// </summary>
    public class Location {
        /// <summary>'static' component of a location used for hashing.</summary>
        private readonly Guid _id;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        /// <summary>Name of this location.</summary>
        public string Name;

        private Location() => _id = Guid.NewGuid();
        /// <param name="name">Name of this location - will be transformed to title case.</param>
        public Location(string name) : this() => Name = Title(name);

        // Reference Equality setup:
        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => _id.GetHashCode();

        // Equality to Name string - can be used to reference locations by (unique) name in CSVs:
        public static bool operator ==(Location l, string potentialName) => l is not null && l.Name == potentialName;
        public static bool operator !=(Location l, string potentialName) => !(l == potentialName);

        /// <returns>Name of this location.</returns>
        public override string ToString() => Name;
        /// <summary>For use by CsvReader.</summary>
        /// <param name="locationName">Name of the new location.</param>
        public static Location FromString(string locationName) => new(locationName);
    }
}
