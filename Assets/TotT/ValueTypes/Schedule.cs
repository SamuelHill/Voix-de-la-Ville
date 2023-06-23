using System;
using System.Linq;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Calendar;

    /// <summary>
    /// Which days of the week a LocationType is open for (TimeOfDay is handled by DailyOperation).
    /// With 7 days (DayOfWeek) and binary open states we get 128 possible schedules - 12 of the more
    /// common schedules are named in the ScheduleName enum.
    /// </summary>
    public readonly struct Schedule : IComparable<Schedule>, IEquatable<Schedule> {
        /// <summary>Indicates which days the schedule is open on (array of booleans indexed by DayOfWeek).</summary>
        private readonly bool[] _openOn;

        /// <param name="openOn">Which days the schedule is open on.</param>
        /// <remarks>This constructor checks that the Length of openOn is equal to DaysOfWeek (7)</remarks>
        private Schedule(bool[] openOn) {
            if (openOn.Length != DaysOfWeek) 
                throw new ArgumentException("openOn must be an array of length 7");
            _openOn = openOn;
        }
        /// <summary>Constructs commonly used schedules from a ScheduleName.</summary>
        private Schedule(ScheduleName schedule) : this(ScheduleByName[(int)schedule]) {}

        public bool IsOpen(DayOfWeek dayOfWeek) => _openOn[(int)dayOfWeek];

        /// <summary>
        /// Mapping between ScheduleName (index of outer array) and the associated
        /// openOn array used for constructing a Schedule.
        /// </summary>
        private static readonly bool[][] ScheduleByName = {
            new[] { true, true, true, true, true, true, true },    // Everyday,
            new[] { true, true, true, true, true, false, false },  // Weekdays,
            new[] { true, true, true, true, true, true, false },   // ClosedSunday,
            new[] { false, true, true, true, true, true, true },   // ClosedMonday,
            new[] { true, false, true, true, true, true, true },   // ClosedTuesday,
            new[] { true, true, false, true, true, true, true },   // ClosedWednesday,
            new[] { true, true, true, false, true, true, true },   // ClosedThursday,
            new[] { true, true, true, true, false, true, true },   // ClosedFriday,
            new[] { true, true, true, true, true, false, true },   // ClosedSaturday,
            new[] { true, true, true, true, false, false, false }, // MondayToThursday,
            new[] { false, true, true, true, true, false, false }, // TuesdayToFriday,
            new[] { false, false, false, true, true, true, true }, // ThursdayToSunday,
        };

        // *************************** Compare and Equality interfacing ***************************
        public int CompareTo(Schedule other) =>
            _openOn.Where((t, i) => t != other._openOn[i]).Select(t => t ? 1 : -1).FirstOrDefault();
        public bool Equals(Schedule other) => Equals(_openOn, other._openOn);
        public override bool Equals(object obj) => obj is Schedule other && Equals(other);
        public override int GetHashCode() => _openOn.GetHashCode();

        // ****************************************************************************************

        /// <summary>
        /// Fallback ToString option for Schedules whose _openOn does not have a corresponding ScheduleName.
        /// </summary>
        /// <returns>List of the days of the week that this Schedule is open on</returns>
        private string DayOfWeekList() => 
            string.Join(", ", _openOn.Select((open, i) => (open, i))
                                     .Where(x => x.open)
                                     .Select(x => ((DayOfWeek)x.i).ToString()));

        /// <returns>
        /// Commonly used schedules that map to a ScheduleName use that name for a string, all other schedules
        /// fall back on creating a list of the days of the week that this Schedule is open on.
        /// </returns>
        public override string ToString() {
            var schedule = Array.IndexOf(ScheduleByName, _openOn);
            return schedule != -1 ? ((ScheduleName)schedule).ToString() : DayOfWeekList();
        }
        /// <summary>
        /// For use by CsvReader. Takes a string, try's parsing as a ScheduleName, returns the associated Schedule.
        /// </summary>
        /// <remarks>No options currently for FromString to parse custom openOn arrays.</remarks>
        public static Schedule FromString(string scheduleString) {
            Enum.TryParse(scheduleString, out ScheduleName schedule);
            return new Schedule(schedule);
        }
    }
}
