using System;

namespace VdlV.Time {
    using static Calendar;

    /// <summary>
    /// Month and Day pairing - no TimeOfDay specificity or indication of Year.
    /// Represents a recurring time span (One full day with two TimesOfDay - AM/PM) within each year.
    /// </summary>
    public readonly struct Date : IComparable<Date>, IEquatable<Date> {
        /// <summary>Month (from Month Enum)</summary>
        public Month Month { get; }
        /// <summary>Day number within Month (1-28)</summary>
        public byte Day { get; }
        
        /// <param name="month">Month (from Month Enum)</param>
        /// <param name="day">Day number within Month, checks that value is in the range 1 to 28</param>
        public Date(Month month, byte day) {
            Month = month;
            Day = CheckDayInRange(day);
        }

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(Date other) {
            var monthComparison = Month.CompareTo(other.Month);
            return monthComparison != 0 ? monthComparison : Day.CompareTo(other.Day);
        }
        public bool Equals(Month month, byte date) => month == Month && date == Day;
        public bool Equals(Date other) => Equals(other.Month, other.Day);
        public override bool Equals(object obj) => obj is Date other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Month, Day);
        public static bool operator ==(Date d1, Date d2) => d1.Equals(d2);
        public static bool operator !=(Date d1, Date d2) => !(d1 == d2);

        // ****************************************************************************************

        /// <returns>Date in "mm/dd" format.</returns>
        public override string ToString() => $"{MonthNumber(Month)}/{Day}";

        /// <summary>
        /// For use by CsvReader. Takes a string (expecting "mm/dd" format), try's parsing as a
        /// Month and Day, and returns the Date made from this Month/Day pair.
        /// </summary>
        public static Date FromString(string dateString) {
            var date = dateString.Split('/');
            return new Date(ToMonth(date[0]), ToDay(date[1]));
        }
    }
}
