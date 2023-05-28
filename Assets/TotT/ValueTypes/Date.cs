using System;
using System.Linq;
using TotT.Simulator;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Randomize;

    public readonly struct Date {
        public Month Month { get; }
        public byte Day { get; }

        public Date(Month month, byte day) {
            Month = month;
            Day = Time.CheckDayInRange(day); }

        private static Month RandomMonth() =>
            Enum.GetValues(typeof(Month)).Cast<Month>().ToList().RandomElement();
        public static Date Random() => new(RandomMonth(), Byte(1, 28));

        public bool Equals(Month month, byte date) => month == Month && date == Day;
        private bool Equals(Date other) => Month == other.Month && Day == other.Day;
        public override bool Equals(object obj) => obj is Date other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Month, Day);
        public static bool operator ==(Date d1, Date d2) => d1.Month == d2.Month && d1.Day == d2.Day;
        public static bool operator !=(Date d1, Date d2) => !(d1 == d2);

        public override string ToString() => $"{(int)Month + 1}/{Day}";
        public static Date FromString(string dateString) {
            var date = dateString.Split('/');
            return new Date((Month)(int.Parse(date[0]) - 1), byte.Parse(date[1])); }
    }
}