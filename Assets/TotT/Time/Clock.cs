using TED;
using TED.Primitives;
using TotT.Utilities;

namespace TotT.Time {
    using static Calendar;
    using static TimeOfDay;
    using static Month;

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

        public static int Year() => CalcYear(ClockTick);
        public static Month Month() => CalcMonth(CalendarClampedClock);
        public static byte Day() => CalcDay(CalendarClampedClock);
        public static DayOfWeek DayOfWeek() => CalcDayOfWeek(CalendarClampedClock);
        public static TimeOfDay TimeOfDay() => CalcTimeOfDay(CalendarClampedClock);
        public static TimePoint TimePoint() => new(ClockTick);
        public static Date Date() => new(Month(), Day());

        private static byte MonthsSince(Month prevMonth) => Calendar.MonthsSince(Month(), prevMonth);
        private static byte MonthsSince(Date prevDate) => (byte)(MonthsSince(prevDate.Month) - (Day() < prevDate.Day ? 1 : 0));
        private static byte DaysSince(byte prevDay) => Calendar.DaysSince(Day(), prevDay);
        private static ushort DaysSince(Date prevDate) => (ushort)(MonthsSince(prevDate) * DaysPerMonth + DaysSince(prevDate.Day));
        private static int YearsSince(TimePoint timePoint) => Calendar.YearsSince(ClockTick, timePoint);

        public static Month LastMonth() {
            var month = (int)Month() - 1;
            return month == -1 ? December : (Month)month;
        }

        // ReSharper disable InconsistentNaming
        public static readonly Function<int> CurrentYear = new($"Current{nameof(Year)}", Year, false);
        public static readonly Function<Month> CurrentMonth = new($"Current{nameof(Month)}", Month, false);
        public static readonly Function<Month> PreviousMonth = new($"Previous{nameof(LastMonth)}", Month, false);
        public static readonly Function<byte> CurrentDay = new($"Current{nameof(Day)}", Day, false);
        public static readonly Function<DayOfWeek> CurrentDayOfWeek = new($"Current{nameof(DayOfWeek)}", DayOfWeek, false);
        public static readonly Function<TimeOfDay> CurrentTimeOfDay = new($"Current{nameof(TimeOfDay)}", TimeOfDay, false);
        public static readonly Function<TimePoint> CurrentTimePoint = new($"Current{nameof(TimePoint)}", TimePoint, false);
        public static readonly Function<Date> CurrentDate = new($"Current{nameof(Date)}", Date, false);
        public static readonly Function<TimePoint, int> YearsOld = new(nameof(YearsOld), YearsSince, false);
        // ReSharper restore InconsistentNaming

        private static bool IsAM() => TimeOfDay() == AM;
        private static bool InOperation(DailyOperation operation) => IsOperating(operation, TimeOfDay());
        private static bool IsOpen(Schedule schedule) => IsScheduled(schedule, DayOfWeek());
        private static bool IsDate(Date date) => date.Equals(Month(), Day());
        private static bool NineMonthsPastDate(Date date) => DaysSince(date) >= NineMonths;

        // ReSharper disable InconsistentNaming
        public static readonly PrimitiveTest CurrentlyMorning = new(nameof(CurrentlyMorning), IsAM, false);
        public static readonly PrimitiveTest<Schedule> CurrentlyOpen = new(nameof(CurrentlyOpen), IsOpen, false);
        public static readonly PrimitiveTest<DailyOperation> CurrentlyOperating = new(nameof(CurrentlyOperating), InOperation, false);
        public static readonly PrimitiveTest<Date> IsToday = new(nameof(IsToday), IsDate, false);
        public static readonly PrimitiveTest<Date> NineMonthsPast = new(nameof(NineMonthsPast), NineMonthsPastDate, false);
        // ReSharper restore InconsistentNaming

        public static string YearsAgo(TimePoint timePoint) {
            var yearsAgo = YearsSince(timePoint);
            return yearsAgo switch {
                0 => "Within the past year", 1 => "One year ago", _ => $"{yearsAgo.ToCardinal()} years ago"
            };
        }
        public static string DateAndTime() => $"{DayOfWeek()} {TimeOfDay()} - {Month()} {Day().Suffixed()}, {Year()}";
    }
}
