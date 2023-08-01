using System;
using VdlV.Utilities;

namespace VdlV.ValueTypes {
    using static StringProcessing;

    /// <summary>
    /// Location reference - wraps name string for an individual location.
    /// </summary>
    public class Location : IComparable<Location>, IEquatable<Location> {
        /// <summary>Name of this location.</summary>
        private readonly string _name;
        
        /// <param name="name">Name of this location - will be transformed to title case.</param>
        public Location(string name) => _name = Title(name);

        // *************************** Compare and Equality interfacing ***************************

        public int CompareTo(Location other) => ReferenceEquals(this, other) ? 0 : other is null ? 1 : 
            string.Compare(_name, other._name, StringComparison.Ordinal);
        public bool Equals(Location other) => other is not null && ReferenceEquals(this, other);
        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => _name.GetHashCode();

        public static bool operator ==(Location l1, Location l2) => l1 is not null && l1.Equals(l2);
        public static bool operator !=(Location l1, Location l2) => !(l1 == l2);
        public static bool operator >(Location l1, Location l2) => l1.CompareTo(l2) > 0;
        public static bool operator <(Location l1, Location l2) => l1.CompareTo(l2) < 0;

        // Equality to Name string - can be used to reference locations by (unique) name in CSVs:
        public static bool operator ==(Location l, string potentialName) => l is not null && l._name == potentialName;
        public static bool operator !=(Location l, string potentialName) => !(l == potentialName);

        // ****************************************************************************************

        /// <returns>Name of this location.</returns>
        public override string ToString() => _name;
        /// <summary>For use by CsvReader.</summary>
        /// <param name="locationName">Name of the new location.</param>
        public static Location FromString(string locationName) => new(locationName);
    }
}
