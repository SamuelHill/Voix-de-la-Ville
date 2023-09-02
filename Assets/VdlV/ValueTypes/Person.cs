using System;
using VdlV.Utilities;

namespace VdlV.ValueTypes {
    using static StringProcessing;

    /// <summary>
    /// Person reference - wraps first and last name strings for an individual person.
    /// </summary>
    public class Person : IComparable<Person>, IEquatable<Person> {
        /// <summary>First name of this person.</summary>
        private readonly string _firstName;
        /// <summary>Maiden name of this person.</summary>
        private readonly string _maidenName;
        /// <summary>Last name of this person.</summary>
        public string LastName;
        /// <summary>First and last name of this person.</summary>
        public string FullName;

        private readonly Personality _personality;

        /// <param name="firstName">First name of this person - will be transformed to title case.</param>
        /// <param name="lastName">Last name of this person - will be transformed to title case.</param>
        /// <param name="rng">...</param>
        public Person(string firstName, string lastName, Random rng) {
            _firstName = Title(firstName);
            LastName = Title(lastName);
            _maidenName = LastName;
            FullName = _firstName + " " + LastName;
            _personality = new Personality(rng);
        }

        private void NewLastName(string lastName) {
            LastName = Title(lastName);
            FullName = _firstName + " " + LastName;
        }
        public bool TakeLastName(Person other) {
            NewLastName(other.LastName);
            return true;
        }

        public int Compatibility(Person otherPerson) => _personality.Compatibility(otherPerson._personality);
        public int Similarity(Person otherPerson) => _personality.Similarity(otherPerson._personality);
        public sbyte Facet(Facet facet) => _personality.Facet(facet);

        // *************************** Compare and Equality interfacing ***************************

        public int CompareTo(Person other) => ReferenceEquals(this, other) ? 0 : other is null ? 1 : 
            string.Compare(FullName, other.FullName, StringComparison.Ordinal);
        public bool Equals(Person other) => other is not null && ReferenceEquals(this, other);
        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => HashCode.Combine(_firstName, _maidenName, _personality);

        public static bool operator ==(Person p1, Person p2) => p1 is not null && p1.Equals(p2);
        public static bool operator !=(Person p1, Person p2) => !(p1 == p2);
        public static bool operator >(Person p1, Person p2) => p1.CompareTo(p2) > 0;
        public static bool operator <(Person p1, Person p2) => p1.CompareTo(p2) < 0;

        // Equality to FullName string - can be used to reference people by (unique) name in CSVs:
        public static bool operator ==(Person p, string potentialName) => p is not null && p.FullName == potentialName;
        public static bool operator !=(Person p, string potentialName) => !(p == potentialName);

        // ****************************************************************************************

        /// <returns>FullName of this person.</returns>
        public override string ToString() => FullName;
        /// <summary>
        /// For use by CsvReader, splits the input string on a single space then creates a new
        /// person from the two names found in the split (first and last for the new person).
        /// </summary>
        /// <param name="personName">FullName of the new person</param>
        /// <param name="rng">...</param>
        public static Person FromString(string personName, Random rng) {
            var person = personName.Split(' ');
            return new Person(person[0], person[1], rng);
        }
    }
}
