using TED.Primitives;
using TED;
using TotT.Utilities;
using TotT.ValueTypes;
using static TED.Language;

namespace TotT.Simulator {
    using static Calendar;
    
    public class Time {
        private uint _clock;
        private readonly int _offset;

        public void Tick() => _clock++;

        public Time(int year) => _offset = year;
        public Time(int year, ushort tick) : this(year) => _clock = CheckTickInCalendar(tick);
        public Time(int year, Month month, byte day = 1, TimeOfDay time = TimeOfDay.AM) :
            this(year) => _clock = CalcCalendarTick(month, day, time);

        private Function<T> Property<T>(string property) => 
            Member<T>(this, property, "Current", false);
        private PrimitiveTest TestProperty(string property) => 
            TestMember(this, property, false);

        public int Year => CalcYear(_clock, _offset);
        public Month Month => CalcMonth(CalendarFromClock(_clock));
        public byte Day => CalcDay(CalendarFromClock(_clock));
        public Date Date => new(Month, Day);
        public TimePoint TimePoint => new(_clock, Year);
        public DayOfWeek DayOfWeek => CalcDayOfWeek(CalendarFromClock(_clock));
        public TimeOfDay TimeOfDay => CalcTimeOfDay(CalendarFromClock(_clock));

        public Function<int> CurrentYear => Property<int>(nameof(Year));
        public Function<Date> CurrentDate => Property<Date>(nameof(Date));
        public Function<TimePoint> CurrentTimePoint => Property<TimePoint>(nameof(TimePoint));
        public Function<TimeOfDay> CurrentTimeOfDay => Property<TimeOfDay>(nameof(TimeOfDay));

        public bool IsAM => TimeOfDay == TimeOfDay.AM;
        public PrimitiveTest CurrentlyMorning => TestProperty(nameof(IsAM));

        private bool InOperation(DailyOperation operation) => IsOperating(operation, TimeOfDay);
        public PrimitiveTest<DailyOperation> CurrentlyOperating => TestMethod<DailyOperation>(InOperation, false);

        private bool IsOpen(Schedule schedule) => IsScheduled(schedule, DayOfWeek);
        public PrimitiveTest<Schedule> CurrentlyOpen => TestMethod<Schedule>(IsOpen, false);

        public PrimitiveTest<Date> IsToday => TestMethod<Date>(IsDate, false);
        public PrimitiveTest<Date> NineMonthsPast => TestMethod<Date>(NineMonthsPastDate, false);
        public bool IsDate(Date date) => date.Equals(Month, Day);
        public bool PastDate(Date date) => date.Month < Month || (date.Month == Month && date.Day < Day);
        public int YearsSince(Date date, int year) => Year - year + (PastDate(date) ? 1 : 0);
        public int YearsSince(TimePoint timePoint) => Year - (timePoint.Year - 1) +
            timePoint.IsNextCalendarYear(CalendarFromClock(_clock));

        public bool NineMonthsPastDate(Date date) => DaysSince(date) >= NineMonths;

        // using byte for Since because all base calls to since are small enough to use byte
        private static byte Since(byte current, byte previous, byte max) => (byte)((max + (current - previous)) % max);
        private byte MonthsSince(Month prevMonth) => Since((byte)Month, (byte)prevMonth, Months);
        // MonthsSince(Date) relies on prevDate being previous, not only does Since assume this
        // order but future Dates would give -1 for the same month, 0 for one month in the future, etc
        private byte MonthsSince(Date prevDate) => (byte)(MonthsSince(prevDate.Month) - (Day < prevDate.Day ? 1 : 0));
        private byte DaysSince(byte prevDay) => Since(Day, prevDay, DaysPerMonth);
        private ushort DaysSince(Date prevDate) => (ushort)(MonthsSince(prevDate) * DaysPerMonth + DaysSince(prevDate.Day));

        public override string ToString() =>
            $"{DayOfWeek} {TimeOfDay} - {Month} {Day.SuffixedDate()}, {Year}";
    }
}