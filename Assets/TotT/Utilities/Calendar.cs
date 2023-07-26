using System;
using System.Linq;
using TED.Interpreter;
using TotT.ValueTypes;
using static TED.Language;

namespace TotT.Utilities {
    using ValueTypes; // not using System.DayOfWeek
    using static DailyOperation;
    using static TimeOfDay;
    using static Enum;
    using static Randomize;

    /// <summary>
    /// Contains almost all maths for calculating any component of time, relates all subtypes of time -
    /// Month, Day, DayOfWeek, TimeOfDay, etc - to the same standard conceptual calendar.
    /// Also contains constants and associated maths for the start year and earliest year in which
    /// a primordial being/object can begin to exist.
    /// </summary>
    public static class Calendar {
        private const byte Months = 12;    // Enum.GetValues(typeof(Month)).Length;
        public const byte DaysOfWeek = 7;  // Enum.GetValues(typeof(DayOfWeek)).Length;
        private const byte TimesOfDay = 2; // Enum.GetValues(typeof(TimeOfDay)).Length;
        private const byte NumWeeksPerMonth = 4;
        public const byte DaysPerMonth = DaysOfWeek * NumWeeksPerMonth; // 28
        private const byte TicksPerMonth = DaysPerMonth * TimesOfDay;   // 56
        public const ushort NumTicks = Months * TicksPerMonth;          // 672

        public const byte NineMonths = 9 * DaysPerMonth; // 252

        private const int StartYear = 1915;
        private const ushort PrimordialOffset = 100; // years before start that TimePoints can exist at
        public const uint InitialClockTick = NumTicks * PrimordialOffset;
        private const int CalcYearOffset = StartYear - PrimordialOffset;

        public static byte CheckDayInRange(byte day) => 
            day is > DaysPerMonth or 0 ? 
                throw new ArgumentException($"day not in range 1 to {DaysPerMonth}") : day;
        public static ushort CheckTickInCalendar(ushort tick) => 
            tick >= NumTicks ? 
                throw new ArgumentException($"tick not in range 0 to {NumTicks - 1}") : tick;

        public static ushort CalcCalendarTick(Month month, byte day, TimeOfDay time = AM) =>
            (ushort)((byte)month * TicksPerMonth + (CheckDayInRange(day) - 1) * TimesOfDay + (byte)time);
        public static uint CalcClockTick(int year, Month month, byte day, TimeOfDay time = AM) => 
            (uint)(NumTicks * (year - CalcYearOffset) + CalcCalendarTick(month, day, time));

        private static uint YearFromClock(uint clock) => clock / NumTicks;
        public static int CalcYear(uint clock) => (int)(YearFromClock(clock) + CalcYearOffset);
        public static ushort CalendarFromClock(uint clock) => (ushort)(clock % NumTicks);
        public static Month CalcMonth(ushort calendar) => (Month)(calendar / TicksPerMonth);
        public static byte CalcDay(ushort calendar) => (byte)(calendar % TicksPerMonth / TimesOfDay + 1);
        public static DayOfWeek CalcDayOfWeek(ushort calendar) => (DayOfWeek)(calendar / TimesOfDay % DaysOfWeek);
        public static TimeOfDay CalcTimeOfDay(ushort calendar) => (TimeOfDay)(calendar % TimesOfDay);

        public static TimePoint TimePointFromDateAndAge(Date date, int age) => 
            new(date.Month, date.Day, StartYear - age, AM);

        public static int MonthNumber(Month month) => (int)month + 1;
        public static Month ToMonth(string month) => (Month)(int.Parse(month) - 1);
        public static byte ToDay(string day) => CheckDayInRange(byte.Parse(day));
        public static int ToYear(string year) => int.Parse(year);

        private static Month RandomMonth() => GetValues(typeof(Month)).Cast<Month>().ToList().RandomElement();
        private static byte RandomDay() => Byte(1, DaysPerMonth);
        public static Date Random() => new(RandomMonth(), RandomDay());

        public static bool IsScheduled(Schedule schedule, DayOfWeek dayOfWeek) => schedule.IsOpen(dayOfWeek);
        public static bool IsOperating(DailyOperation operation, TimeOfDay timeOfDay) =>
            operation is AllDay || (operation is Morning && timeOfDay == AM) || (operation is Evening && timeOfDay == PM);

        public static float ChancePerDay(float chance) => chance / TimesOfDay;
        private static float ChancePerWeek(float chance) => chance / (TimesOfDay * DaysOfWeek);
        private static float ChancePerMonth(float chance) => chance / TicksPerMonth;
        private static float ChancePerYear(float chance) => chance / NumTicks;

        public static Goal PerDay(float chance) => Prob[ChancePerDay(chance)];
        public static Goal PerWeek(float chance) => Prob[ChancePerWeek(chance)];
        public static Goal PerMonth(float chance) => Prob[ChancePerMonth(chance)];
        public static Goal PerYear(float chance) => Prob[ChancePerYear(chance)];

        private static byte Since(byte current, byte previous, byte max) => (byte)((max + (current - previous)) % max);
        public static byte MonthsSince(Month month, Month prevMonth) => Since((byte)month, (byte)prevMonth, Months);
        public static byte DaysSince(byte day, byte prevDay) => Since(day, prevDay, DaysPerMonth);
    }
}
