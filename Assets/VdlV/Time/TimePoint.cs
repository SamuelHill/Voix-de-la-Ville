using System;
using VdlV.Utilities;

namespace VdlV.Time {
    using static Calendar;
    using static Enum;
    using static TimeOfDay;

    /// <summary>
    /// Record of a point in time at the finest possible temporal resolution.
    /// </summary>
    public readonly struct TimePoint : IComparable<TimePoint>, IEquatable<TimePoint>, ISerializableValue<TimePoint> {
        /// <summary>Tick value within the simulation</summary>
        public readonly uint Clock;

        /// <param name="clock">Tick value</param>
        public TimePoint(uint clock) => Clock = clock;

        // ReSharper disable once InconsistentNaming
        public static readonly TimePoint Eschaton = new(uint.MaxValue);
        private bool IsEschaton => Clock == uint.MaxValue;

        private TimePoint(Month month, byte day, int year, TimeOfDay time) :
            this(CalcClockTick(year, month, day, time)) { }
        internal TimePoint(Date date, int year, TimeOfDay time = AM) :
            this(date.Month, date.Day, year, time) { }

        // Every TimePoint also has an equivalent Date, this is a useful conversion to be able to make
        // as dates are inherently recurring each year and comparing TimePoints to Dates lets us see if
        // the current date is the anniversary of a TimePoint (with some loss of precision).
        public static implicit operator Date(TimePoint t) => 
            new(CalcMonth(CalendarFromClock(t.Clock)), CalcDay(CalendarFromClock(t.Clock)));

        // *************************** Compare and Equality interfacing ***************************

        public int CompareTo(TimePoint other) => Clock.CompareTo(other.Clock);
        public bool Equals(TimePoint other) => Clock == other.Clock;
        public override bool Equals(object obj) => obj is TimePoint other && Equals(other);
        public override int GetHashCode() => Clock.GetHashCode();

        public static bool operator ==(TimePoint t1, TimePoint t2) => t1.Equals(t2);
        public static bool operator !=(TimePoint t1, TimePoint t2) => !(t1 == t2);

        public static bool operator ==(TimePoint t, Date d) => d.Equals(t);
        public static bool operator !=(TimePoint t, Date d) => !(t == d);

        // ****************************************************************************************

        private int MonthNumber => MonthNumber(CalcMonth(CalendarFromClock(Clock)));
        private TimeOfDay Time => CalcTimeOfDay(CalendarFromClock(Clock));
        private string MiddleEndian => $"{MonthNumber}/{CalcDay(CalendarFromClock(Clock))}/{CalcYear(Clock)} {Time}";

        /// <returns>TimePoint in "M/d/yyyy tt" format.</returns>
        public override string ToString() => IsEschaton ? "Has not occurred" : MiddleEndian;

        /// <summary>
        /// For use by CsvReader. Takes a string (expecting "M/d/yyyy tt" format), try's parsing as a Month,
        /// Day, and Year, then returns the TimePoint made from this Month/Day/Year set.
        /// </summary>
        public static TimePoint FromString(string timePointString) {
            if (timePointString == "Has not occurred") return Eschaton;
            var timeSplit = timePointString.Split(' ');
            if (timeSplit.Length == 1) {
                var timePoint = timePointString.Split('/');
                return new TimePoint(ToMonth(timePoint[0]), ToDay(timePoint[1]), ToYear(timePoint[2]), AM);
            } else {
                var timePoint = timeSplit[0].Split('/');
                return new TimePoint(ToMonth(timePoint[0]), ToDay(timePoint[1]), ToYear(timePoint[2]), Parse<TimeOfDay>(timeSplit[1]));
            }
        }
    }
}
