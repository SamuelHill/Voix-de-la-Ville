using System;
using TED;
using TED.Interpreter; // for Goal
using TotT.Utilities; // for Randomize
using static TED.Language; // for Prob

namespace TotT.Time {
    using static DailyOperation;
    using static TimeOfDay;
    using static Randomize;

    /// <summary>
    /// Contains almost all maths for calculating any component of time, relates all subtypes of time -
    /// Month, Day, DayOfWeek, TimeOfDay, etc - to the same standard conceptual calendar.
    /// Also contains constants and associated maths for the start year and earliest year in which
    /// a primordial being/object can begin to exist.
    /// </summary>
    public static class Calendar {
        private const byte Months = 12;     // Enum.GetValues(typeof(Month)).Length;
        internal const byte DaysOfWeek = 7; // Enum.GetValues(typeof(DayOfWeek)).Length;
        private const byte TimesOfDay = 2;  // Enum.GetValues(typeof(TimeOfDay)).Length;
        private const byte NumWeeksPerMonth = 4;
        private const byte TicksPerWeek = DaysOfWeek * TimesOfDay;        // 14
        internal const byte DaysPerMonth = DaysOfWeek * NumWeeksPerMonth; // 28
        private const byte TicksPerMonth = DaysPerMonth * TimesOfDay;     // 56
        private const ushort NumTicks = Months * TicksPerMonth;           // 672

        internal const byte NineMonths = 9 * DaysPerMonth; // 252

        private const int StartYear = 1915;
        private const ushort PrimordialOffset = 100; // years before start that TimePoints can exist at
        internal const uint InitialClockTick = NumTicks * PrimordialOffset;
        private const int CalcYearOffset = StartYear - PrimordialOffset;

        internal static byte CheckDayInRange(byte day) => 
            day is > DaysPerMonth or 0 ? 
                throw new ArgumentException($"day not in range 1 to {DaysPerMonth}") : day;
        internal static ushort CheckTickInCalendar(ushort tick) => 
            tick >= NumTicks ? 
                throw new ArgumentException($"tick not in range 0 to {NumTicks - 1}") : tick;

        internal static ushort CalcCalendarTick(Month month, byte day, TimeOfDay time = AM) =>
            (ushort)((byte)month * TicksPerMonth + (CheckDayInRange(day) - 1) * TimesOfDay + (byte)time);
        internal static uint CalcClockTick(int year, Month month, byte day, TimeOfDay time = AM) => 
            (uint)(NumTicks * (year - CalcYearOffset) + CalcCalendarTick(month, day, time));

        private static uint ClockTickToYears(uint clock) => clock / NumTicks;
        internal static int CalcYear(uint clock) => (int)(ClockTickToYears(clock) + CalcYearOffset);
        internal static ushort CalendarFromClock(uint clock) => (ushort)(clock % NumTicks);
        internal static Month CalcMonth(ushort calendar) => (Month)(calendar / TicksPerMonth);
        internal static byte CalcDay(ushort calendar) => (byte)(calendar % TicksPerMonth / TimesOfDay + 1);
        internal static DayOfWeek CalcDayOfWeek(ushort calendar) => (DayOfWeek)(calendar / TimesOfDay % DaysOfWeek);
        internal static TimeOfDay CalcTimeOfDay(ushort calendar) => (TimeOfDay)(calendar % TimesOfDay);

        internal static int MonthNumber(Month month) => (int)month + 1;
        internal static Month ToMonth(string month) => (Month)(int.Parse(month) - 1);
        internal static byte ToDay(string day) => CheckDayInRange(byte.Parse(day));
        internal static int ToYear(string year) => int.Parse(year);

        private static Month RandomMonth() => (Month)Byte(Months - 1);
        private static byte RandomDay() => Byte(1, DaysPerMonth);
        private static Date Random() => new(RandomMonth(), RandomDay());
        // ReSharper disable once InconsistentNaming
        public static readonly Function<Date> RandomDate = new(nameof(RandomDate), Random, false);

        private static TimePoint TimePointFromDateAndAge(Date date, int age) => new(date, StartYear - age);
        // ReSharper disable InconsistentNaming
        public static readonly Function<Date, int, TimePoint> TimeOfBirth = new(nameof(TimeOfBirth), TimePointFromDateAndAge);
        public static readonly Function<TimePoint, Date> TimePointToDate = new(nameof(TimePointToDate), t => t);
        public static readonly Function<TimePoint, Month> TimePointToMonth = new(nameof(TimePointToMonth), t => ((Date)t).Month);
        // ReSharper restore InconsistentNaming

        internal static bool IsScheduled(Schedule schedule, DayOfWeek dayOfWeek) => schedule.IsOpen(dayOfWeek);
        internal static bool IsOperating(DailyOperation operation, TimeOfDay timeOfDay) =>
            operation is AllDay || (operation is Morning && timeOfDay == AM) || (operation is Evening && timeOfDay == PM);

        public static float ChancePerDay(float chance) => chance / TimesOfDay;
        private static float ChancePerWeek(float chance) => chance / TicksPerWeek;
        private static float ChancePerMonth(float chance) => chance / TicksPerMonth;
        private static float ChancePerYear(float chance) => chance / NumTicks;

        public static Goal PerDay(float chance) => Prob[ChancePerDay(chance)];
        public static Goal PerWeek(float chance) => Prob[ChancePerWeek(chance)];
        public static Goal PerMonth(float chance) => Prob[ChancePerMonth(chance)];
        public static Goal PerYear(float chance) => Prob[ChancePerYear(chance)];

        private static byte Since(byte current, byte previous, byte max) => (byte)((max + (current - previous)) % max);
        internal static byte MonthsSince(Month month, Month prevMonth) => Since((byte)month, (byte)prevMonth, Months);
        internal static byte DaysSince(byte day, byte prevDay) => Since(day, prevDay, DaysPerMonth);
        internal static int YearsSince(uint clock, TimePoint timePoint) => (int)ClockTickToYears(clock - timePoint.Clock);
    }
}
