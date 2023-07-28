using TED;
using TED.Primitives;
using TotT.Utilities;
using TotT.ValueTypes;
using static TED.Language;
using static TotT.Time.TimeOfDay;

namespace TotT.Time {
    using static Calendar;

    /// <summary>
    /// Internal clock, keeps time during a simulation by ticking along with Simulator.Update
    /// </summary>
    public static class Clock {
        public static uint ClockTick;
        private static ushort CalendarClampedClock => CalendarFromClock(ClockTick);
        public static void Tick() => ClockTick++;

        static Clock() => OffsetInitialClockTick();

        private static void OffsetInitialClockTick(uint offset = 0) => ClockTick = InitialClockTick + offset;
        public static void InitializeClock(ushort calendarTick) => 
            OffsetInitialClockTick(CheckTickInCalendar(calendarTick));
        public static void InitializeClock(Month month, byte day = 1, TimeOfDay time = AM) => 
            OffsetInitialClockTick(CalcCalendarTick(month, day, time));

        private static Function<T> Property<T>(string property) => Member<T>(typeof(Clock), property, "Current", false);
        private static PrimitiveTest TestProperty(string property) => TestMember(typeof(Clock), property, false);

        public static int Year => CalcYear(ClockTick);
        public static Function<int> CurrentYear => Property<int>(nameof(Year));
        public static TimePoint TimePoint => new(ClockTick);
        public static Function<TimePoint> CurrentTimePoint => Property<TimePoint>(nameof(TimePoint));

        public static Month Month => CalcMonth(CalendarClampedClock);
        public static Function<Month> CurrentMonth => Property<Month>(nameof(Month));
        public static byte Day => CalcDay(CalendarClampedClock);
        public static Function<byte> CurrentDay => Property<byte>(nameof(Day));
        public static Date Date => new(Month, Day);
        public static Function<Date> CurrentDate => Property<Date>(nameof(Date));
        private static bool IsDate(Date date) => date.Equals(Month, Day);
        public static PrimitiveTest<Date> IsToday => TestMethod<Date>(IsDate, false);

        public static DayOfWeek DayOfWeek => CalcDayOfWeek(CalendarClampedClock);
        public static Function<DayOfWeek> CurrentDayOfWeek => Property<DayOfWeek>(nameof(DayOfWeek));
        public static TimeOfDay TimeOfDay => CalcTimeOfDay(CalendarClampedClock);
        public static Function<TimeOfDay> CurrentTimeOfDay => Property<TimeOfDay>(nameof(TimeOfDay));
        public static bool IsAM => TimeOfDay == AM;
        public static PrimitiveTest CurrentlyMorning => TestProperty(nameof(IsAM));
        private static bool InOperation(DailyOperation operation) => IsOperating(operation, TimeOfDay);
        public static PrimitiveTest<DailyOperation> CurrentlyOperating => TestMethod<DailyOperation>(InOperation, false);
        private static bool IsOpen(Schedule schedule) => IsScheduled(schedule, DayOfWeek);
        public static PrimitiveTest<Schedule> CurrentlyOpen => TestMethod<Schedule>(IsOpen, false);

        private static byte MonthsSince(Month prevMonth) => Calendar.MonthsSince(Month, prevMonth);
        private static byte MonthsSince(Date prevDate) => (byte)(MonthsSince(prevDate.Month) - (Day < prevDate.Day ? 1 : 0));
        private static byte DaysSince(byte prevDay) => Calendar.DaysSince(Day, prevDay);
        private static ushort DaysSince(Date prevDate) => (ushort)(MonthsSince(prevDate) * DaysPerMonth + DaysSince(prevDate.Day));
        private static bool NineMonthsPastDate(Date date) => DaysSince(date) >= NineMonths;
        public static PrimitiveTest<Date> NineMonthsPast => TestMethod<Date>(NineMonthsPastDate, false);

        private static int YearsSince(TimePoint timePoint) => (int)((ClockTick - timePoint.Clock) / NumTicks);
        public static Function<TimePoint, int> YearsOld => Method<TimePoint, int>(YearsSince, false);

        public static string YearsAgo(TimePoint timePoint) {
            var yearsAgo = YearsSince(timePoint);
            return yearsAgo switch {
                0 => "Within the past year", 1 => "One year ago", _ => $"{yearsAgo.ToCardinal()} years ago"
            };
        }
        public new static string ToString() => $"{DayOfWeek} {TimeOfDay} - {Month} {Day.Suffixed()}, {Year}";
    }
}
