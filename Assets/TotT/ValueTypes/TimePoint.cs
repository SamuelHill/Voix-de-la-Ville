using System;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Calendar;

    /// <summary>
    /// Record of a point in time at the finest temporal resolution.
    /// </summary>
    public readonly struct TimePoint : IEquatable<TimePoint> {
        /// <summary>Tick value within a year (0 to 671)</summary>
        public readonly ushort Calender;
        /// <summary>Year this point occurred during</summary>
        public readonly int Year;

        /// <param name="calender">Tick value within a year (0 to 671)</param>
        /// <param name="year">Year this point occurred during</param>
        public TimePoint(ushort calender, int year) {
            Calender = calender;
            Year = year;
        }

        public static TimePoint Eschaton = new TimePoint(ushort.MaxValue, int.MaxValue);

        /// <summary>Constructor from component parts</summary>
        /// <remarks>Only used for FromString, default constructor hooks up to Time more easily</remarks>
        private TimePoint(Month month, byte day, TimeOfDay time, int year) :
            this(CalcCalendarTick(month, day, time), year) {}

        // *************************** Functions for == and IEquatable: ***************************
        private bool Equals(ushort calender, int year) => Calender == calender && year == Year;
        public bool Equals(TimePoint other) => Equals(other.Calender, other.Year);
        public override bool Equals(object obj) => obj is TimePoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Calender, Year);

        // Every TimePoint also has an equivalent Date, this is a useful conversion to be able to make
        // as dates are inherently recurring each year and comparing TimePoints to Dates lets us see if
        // the current date is the anniversary of a TimePoint (with some loss of precision).
        public static implicit operator Date(TimePoint t) => new(CalcMonth(t.Calender), CalcDay(t.Calender));
        public static bool operator ==(TimePoint t, Date d) => d.Equals(t);
        public static bool operator !=(TimePoint t, Date d) => !(t == d);
        // ****************************************************************************************
        
        /// <returns>TimePoint in "mm/dd/yyyy" format.</returns>
        /// <remarks>Not reflective of full resolution - ignores TimeOfDay.</remarks>
        public override string ToString() => $"{MonthNumber(CalcMonth(Calender))}/{CalcDay(Calender)}/{Year}";
        /// <summary>
        /// For use by CsvReader. Takes a string (expecting "mm/dd/yyyy" format), try's parsing as a Month,
        /// Day, and Year, then returns the TimePoint made from this Month/Day/Year set.
        /// </summary>
        /// <remarks>Not reflective of full resolution - always gives the AM TimePoint.</remarks>
        public static TimePoint FromString(string timePointString) {
            var timePoint = timePointString.Split('/');
            return new TimePoint(ToMonth(timePoint[0]), ToDay(timePoint[1]), TimeOfDay.AM, ToYear(timePoint[2]));
        }
    }
}
