using System;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Calendar;

    /// <summary>
    /// Record of a point in time at the finest temporal resolution.
    /// </summary>
    public readonly struct TimePoint : IComparable<TimePoint>, IEquatable<TimePoint> {
        /// <summary>Tick value within a year (0 to 671)</summary>
        public readonly uint Clock;

        /// <param name="clock">Tick value within a year (0 to 671)</param>
        public TimePoint(uint clock) => Clock = clock;

        public static TimePoint Eschaton = new(uint.MaxValue);

        /// <summary>Constructor from component parts</summary>
        /// <remarks>Only used for FromString, default constructor hooks up to Time more easily</remarks>
        private TimePoint(Month month, byte day, int year, TimeOfDay time) :
            this(CalcClockTick(year, month, day, time)) {}

        // Every TimePoint also has an equivalent Date, this is a useful conversion to be able to make
        // as dates are inherently recurring each year and comparing TimePoints to Dates lets us see if
        // the current date is the anniversary of a TimePoint (with some loss of precision).
        public static implicit operator Date(TimePoint t) => new(CalcMonth(CalendarFromClock(t.Clock)), 
                                                                 CalcDay(CalendarFromClock(t.Clock)));

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(TimePoint other) => Clock.CompareTo(other.Clock);
        private bool Equals(uint clock) => Clock == clock;
        public bool Equals(TimePoint other) => Equals(other.Clock);
        public override bool Equals(object obj) => obj is TimePoint other && Equals(other);
        public override int GetHashCode() => Clock.GetHashCode();

        public static bool operator ==(TimePoint t1, TimePoint t2) => t1.Equals(t2);
        public static bool operator !=(TimePoint t1, TimePoint t2) => !(t1 == t2);

        public static bool operator ==(TimePoint t, Date d) => d.Equals(t);
        public static bool operator !=(TimePoint t, Date d) => !(t == d);

        // ****************************************************************************************

        /// <returns>TimePoint in "mm/dd/yyyy" format.</returns>
        /// <remarks>Not reflective of full resolution - ignores TimeOfDay.</remarks>
        public override string ToString() => $"{MonthNumber(CalcMonth(CalendarFromClock(Clock)))}/{CalcDay(CalendarFromClock(Clock))}/{CalcYear(Clock)}";
        /// <summary>
        /// For use by CsvReader. Takes a string (expecting "mm/dd/yyyy" format), try's parsing as a Month,
        /// Day, and Year, then returns the TimePoint made from this Month/Day/Year set.
        /// </summary>
        /// <remarks>Not reflective of full resolution - always gives the AM TimePoint.</remarks>
        public static TimePoint FromString(string timePointString) {
            var timePoint = timePointString.Split('/');
            return new TimePoint(ToMonth(timePoint[0]), ToDay(timePoint[1]), ToYear(timePoint[2]), TimeOfDay.AM);
        }
    }
}
