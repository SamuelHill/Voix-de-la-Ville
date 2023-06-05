using System;
using System.Linq;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Randomize;
    using static Calendar;

    public readonly struct Date : IEquatable<Date> {
        public Month Month { get; }
        public byte Day { get; }

        public Date(Month month, byte day) {
            Month = month;
            Day = CheckDayInRange(day); }

        private static Month RandomMonth() =>
            Enum.GetValues(typeof(Month)).Cast<Month>().ToList().RandomElement();
        public static Date Random() => new(RandomMonth(), Byte(1, 28));

        public bool Equals(Month month, byte date) => month == Month && date == Day;
        public bool Equals(Date other) => Equals(other.Month, other.Day);
        public override bool Equals(object obj) => obj is Date other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Month, Day);
        public static bool operator ==(Date d1, Date d2) => d1.Equals(d2);
        public static bool operator !=(Date d1, Date d2) => !(d1 == d2);

        public override string ToString() => $"{MonthNumber(Month)}/{Day}";
        public static Date FromString(string dateString) {
            var date = dateString.Split('/');
            return new Date(ToMonth(date[0]), ToDay(date[1])); }
    }
}