﻿using System;
using TED.Interpreter;
using static TED.Language;

namespace TotT.Utilities {
    using ValueTypes; // not using System.DayOfWeek

    public static class Calendar {
        public const byte Months = 12; // Enum.GetValues(typeof(Month)).Length;
        public const byte DaysOfWeek = 7; // Enum.GetValues(typeof(DayOfWeek)).Length;
        private const byte TimesOfDay = 2; // Enum.GetValues(typeof(TimeOfDay)).Length;
        private const byte NumWeeksPerMonth = 4;
        public const byte DaysPerMonth = DaysOfWeek * NumWeeksPerMonth; // 28
        private const byte TicksPerMonth = DaysPerMonth * TimesOfDay; // 56
        private const ushort NumTicks = Months * TicksPerMonth; // 672

        public const byte NineMonths = 9 * DaysPerMonth; // 252

        public static byte CheckDayInRange(byte day) => day is > DaysPerMonth or 0 ?
            throw new ArgumentException($"day not in range 1 to {DaysPerMonth}") : day;
        public static ushort CheckTickInCalendar(ushort tick) => tick >= NumTicks ?
            throw new ArgumentException($"tick not in range 0 to {NumTicks - 1}") : tick;

        // Reverse calculation of clock tick from month, day, and time
        public static ushort CalcCalendarTick(Month month, byte day, TimeOfDay time = TimeOfDay.AM) =>
            (ushort)((byte)month * TicksPerMonth + (CheckDayInRange(day) - 1) * TimesOfDay + (byte)time);

        // Normal calculations from clock tick to various values
        private static uint YearFromClock(uint clock) => clock / NumTicks;
        public static int CalcYear(uint clock, int year) => (int)(YearFromClock(clock) + year);
        public static ushort CalendarFromClock(uint clock) => (ushort)(clock % NumTicks);
        public static Month CalcMonth(ushort calendar) => (Month)(calendar / TicksPerMonth);
        public static byte CalcDay(ushort calendar) => (byte)(calendar % TicksPerMonth / TimesOfDay + 1);
        public static DayOfWeek CalcDayOfWeek(ushort calendar) => (DayOfWeek)(calendar / TimesOfDay % DaysOfWeek);
        public static TimeOfDay CalcTimeOfDay(ushort calendar) => (TimeOfDay)(calendar % TimesOfDay);

        public static int MonthNumber(Month month) => (int)month + 1;
        public static Month ToMonth(string month) => (Month)(int.Parse(month) - 1);
        public static byte ToDay(string day) => CheckDayInRange(byte.Parse(day));
        public static int ToYear(string year) => int.Parse(year);

        public static bool IsScheduled(Schedule schedule, DayOfWeek dayOfWeek) => 
            schedule.IsOpen(dayOfWeek);
        public static bool IsOperating(DailyOperation operation, TimeOfDay timeOfDay) =>
            operation is DailyOperation.AllDay ||
            (operation is DailyOperation.Morning && timeOfDay == TimeOfDay.AM) ||
            (operation is DailyOperation.Evening && timeOfDay == TimeOfDay.PM);

        private static float ChancePerDay(float chance) => chance / TimesOfDay;
        private static float ChancePerWeek(float chance) => chance / (TimesOfDay * DaysOfWeek);
        private static float ChancePerMonth(float chance) => chance / TicksPerMonth;
        public static float ChancePerYear(float chance) => chance / NumTicks;

        public static Goal PerDay(float chance) => Prob[ChancePerDay(chance)];
        public static Goal PerWeek(float chance) => Prob[ChancePerWeek(chance)];
        public static Goal PerMonth(float chance) => Prob[ChancePerMonth(chance)];
        public static Goal PerYear(float chance) => Prob[ChancePerYear(chance)];
    }
}