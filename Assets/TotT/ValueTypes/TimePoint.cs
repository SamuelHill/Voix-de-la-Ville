using System;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Calendar;

    public readonly struct TimePoint : IEquatable<TimePoint> {
        private readonly ushort _calender;
        public readonly int Year;

        public TimePoint(uint clock, int year) {
            _calender = CalendarFromClock(clock);
            Year = year; }
        private TimePoint(Month month, byte day, TimeOfDay time, int year) {
            _calender = CalcCalendarTick(month, day, time);
            Year = year; }

        private bool Equals(ushort calender, int year) => _calender == calender && year == Year;
        public bool Equals(TimePoint other) => Equals(other._calender, other.Year);
        public override bool Equals(object obj) => obj is TimePoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_calender, Year);

        public static implicit operator Date(TimePoint t) => new(CalcMonth(t._calender), CalcDay(t._calender));
        public static bool operator ==(TimePoint t, Date d) => d.Equals(t);
        public static bool operator !=(TimePoint t, Date d) => !(t == d);

        public int IsNextCalendarYear(ushort calendar) => calendar <= _calender ? 0 : 1;

        public override string ToString() => $"{MonthNumber(CalcMonth(_calender))}/{CalcDay(_calender)}/{Year}";
        public static TimePoint FromString(string timePointString) {
            var timePoint = timePointString.Split('/');
            return new TimePoint(ToMonth(timePoint[0]), ToDay(timePoint[1]), TimeOfDay.AM, ToYear(timePoint[2]));
        }
    }
}