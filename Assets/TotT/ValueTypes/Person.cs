using System;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static StringProcessing;

    /// <summary>
    /// Person reference - wraps first and last name strings for an individual person.
    /// </summary>
    public class Person : IComparable<Person> {
        /// <summary>'static' component of a person used for hashing.</summary>
        private readonly Guid _id;
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        /// <summary>First name of this person.</summary>
        public string FirstName;
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        /// <summary>Last name of this person.</summary>
        public string LastName;

        /// <summary>First and last name of this person.</summary>
        public readonly string FullName;

        private Person() => _id = Guid.NewGuid();
        /// <param name="firstName">First name of this person - will be transformed to title case.</param>
        /// <param name="lastName">Last name of this person - will be transformed to title case.</param>
        public Person(string firstName, string lastName) : this() {
            FirstName = Title(firstName);
            LastName = Title(lastName);
            FullName = FirstName + " " + LastName;
        }
        
        // Reference Equality setup:
        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => _id.GetHashCode();

        // Equality to FullName string - can be used to reference people by (unique) name in CSVs:
        public static bool operator ==(Person p, string potentialName) => p is not null && p.FullName == potentialName;
        public static bool operator !=(Person p, string potentialName) => !(p == potentialName);

        public override string ToString() => FullName;


        /// <summary>
        /// For use by CsvReader, splits the input string on a single space then creates a new
        /// person from the two names found in the split (first and last for the new person).
        /// </summary>
        /// <param name="personName">FullName of the new person</param>
        public static Person FromString(string personName) {
            var person = personName.Split(' ');
            return new Person(person[0], person[1]);
        }

        public int CompareTo(Person other)
        {
            return string.Compare(FullName, other.FullName, StringComparison.Ordinal);
        }
    }
}
