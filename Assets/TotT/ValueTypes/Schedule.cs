using System;
using System.Collections.Generic;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Calendar;

    public readonly struct Schedule {
        private readonly bool[] _openOn; // OpenOn.Length == Time.DaysOfWeek

        private Schedule(bool[] openOn) => _openOn = openOn;
        private Schedule(ScheduleName schedule) : this(ScheduleByName[(int)schedule]) { }

        public bool IsOpen(DayOfWeek dayOfWeek) => _openOn[(int)dayOfWeek];

        private static readonly bool[][] ScheduleByName = { // indexed by ScheduleName Enum
            new[] { true, true, true, true, true, true, true }, 
            new[] { true, true, true, true, true, false, false },
            new[] { true, true, true, true, true, true, false },
            new[] { false, true, true, true, true, true, true },
            new[] { true, false, true, true, true, true, true },
            new[] { true, true, false, true, true, true, true },
            new[] { true, true, true, false, true, true, true },
            new[] { true, true, true, true, false, true, true },
            new[] { true, true, true, true, true, false, true },
            new[] { true, true, true, true, false, false, false },
            new[] { false, true, true, true, true, false, false },
            new[] { false, false, false, true, true, true, true } };

        private string DayOfWeekList() {
            var strings = new List<string>();
            for (var i = 0; i < DaysOfWeek; i++)
                if (_openOn[i]) strings.Add(((DayOfWeek)i).ToString());
            return string.Join(", ", strings); }

        public override string ToString() {
            var schedule = Array.IndexOf(ScheduleByName, _openOn);
            return schedule != -1 ? ((ScheduleName)schedule).ToString() : DayOfWeekList(); }
        public static Schedule FromString(string scheduleString) {
            Enum.TryParse(scheduleString, out ScheduleName schedule);
            return new Schedule(schedule); }
    }
}