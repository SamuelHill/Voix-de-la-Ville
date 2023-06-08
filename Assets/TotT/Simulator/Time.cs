using TED;
using TED.Primitives;
using TotT.Utilities;
using TotT.ValueTypes;
using static TED.Language;

namespace TotT.Simulator {
    using static Calendar;

    /// <summary>
    /// Internal clock, keeps time during a simulation by ticking along with Simulator.Update
    /// </summary>
    public class Time {
        private readonly int _offset;
        private uint _clock;

        #pragma warning disable IDE1006
        // ReSharper disable once InconsistentNaming
        private ushort _calendar => CalendarFromClock(_clock);
        #pragma warning restore IDE1006

        public void Tick() => _clock++;

        public Time(int year) => _offset = year;
        public Time(int year, ushort tick) : this(year) => _clock = CheckTickInCalendar(tick);
        public Time(int year, Month month, byte day = 1, TimeOfDay time = TimeOfDay.AM) :
            this(year) => _clock = CalcCalendarTick(month, day, time);

        private Function<T> Property<T>(string property) => Member<T>(this, property, "Current", false);
        private PrimitiveTest TestProperty(string property) => TestMember(this, property, false);

        public int Year => CalcYear(_clock, _offset);
        public Function<int> CurrentYear => Property<int>(nameof(Year));
        public TimePoint TimePoint => new(_calendar, Year);
        public Function<TimePoint> CurrentTimePoint => Property<TimePoint>(nameof(TimePoint));

        public Month Month => CalcMonth(_calendar);
        public Function<Month> CurrentMonth => Property<Month>(nameof(Month));
        public byte Day => CalcDay(_calendar);
        public Function<byte> CurrentDay => Property<byte>(nameof(Day));
        public Date Date => new(Month, Day);
        public Function<Date> CurrentDate => Property<Date>(nameof(Date));
        private bool IsDate(Date date) => date.Equals(Month, Day);
        public PrimitiveTest<Date> IsToday => TestMethod<Date>(IsDate, false);

        public DayOfWeek DayOfWeek => CalcDayOfWeek(_calendar);
        public Function<DayOfWeek> CurrentDayOfWeek => Property<DayOfWeek>(nameof(DayOfWeek));
        public TimeOfDay TimeOfDay => CalcTimeOfDay(_calendar);
        public Function<TimeOfDay> CurrentTimeOfDay => Property<TimeOfDay>(nameof(TimeOfDay));
        public bool IsAM => TimeOfDay == TimeOfDay.AM;
        public PrimitiveTest CurrentlyMorning => TestProperty(nameof(IsAM));
        private bool InOperation(DailyOperation operation) => IsOperating(operation, TimeOfDay);
        public PrimitiveTest<DailyOperation> CurrentlyOperating => TestMethod<DailyOperation>(InOperation, false);
        private bool IsOpen(Schedule schedule) => IsScheduled(schedule, DayOfWeek);
        public PrimitiveTest<Schedule> CurrentlyOpen => TestMethod<Schedule>(IsOpen, false);

        private byte MonthsSince(Month prevMonth) => Calendar.MonthsSince(Month, prevMonth);
        private byte MonthsSince(Date prevDate) => (byte)(MonthsSince(prevDate.Month) - (Day < prevDate.Day ? 1 : 0));
        private byte DaysSince(byte prevDay) => Calendar.DaysSince(Day, prevDay);
        private ushort DaysSince(Date prevDate) => (ushort)(MonthsSince(prevDate) * DaysPerMonth + DaysSince(prevDate.Day));
        private bool NineMonthsPastDate(Date date) => DaysSince(date) >= NineMonths;
        public PrimitiveTest<Date> NineMonthsPast => TestMethod<Date>(NineMonthsPastDate, false);

        private int YearsSince(TimePoint timePoint) => ((Year - timePoint.Year) * NumTicks +
                                                        (_calendar - timePoint.Calender)) / NumTicks;
        public Function<TimePoint, int> YearsOld => Method<TimePoint, int>(YearsSince, false);

        public string YearsAgo(TimePoint timePoint) {
            var yearsAgo = YearsSince(timePoint);
            return yearsAgo switch {
                0 => "Within the past year", 1 => "One year ago", _ => $"{yearsAgo.ToNumeral()} years ago"
            };
        }
        public override string ToString() => $"{DayOfWeek} {TimeOfDay} - {Month} {Day.Suffixed()}, {Year}";
    }
}
