using System;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static StringProcessing;

    public class Person {
        private readonly Guid _id;
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly string FirstName;
        public readonly string LastName; // readonly means no last name change via Set side effects

        public string FullName => FirstName + " " + LastName;

        public Person(string firstName, string lastName) {
            _id = Guid.NewGuid();
            FirstName = Title(firstName);
            LastName = Title(lastName); }

        public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
        public override int GetHashCode() => _id.GetHashCode();
        public static bool operator ==(Person p, string potentialName) => p != null && p.FullName == potentialName;
        public static bool operator !=(Person p, string potentialName) => !(p == potentialName);

        public override string ToString() => FullName;
        public static Person FromString(string personName) {
            var person = personName.Split(' ');
            return new Person(person[0], person[1]); }
    }
}