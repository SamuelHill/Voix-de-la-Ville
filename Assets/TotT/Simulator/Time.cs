using System;
using TED.Primitives;
using TED;
using TotT.Utilities;
using static TED.Language;

namespace TotT.Simulator {
    using ValueTypes; // easier than specifying to not use System.DayOfWeek
    
    // ReSharper disable MemberCanBePrivate.Global
    public class Time {
        private ushort _clock = 1; // no day zero... makes the % math more succinct
        private uint _year; // internal way to keep track of years, starts at 0
        private readonly int _offset; // int year for pretty dates, optional (can be -)

        public const byte Months = 12; // Enum.GetValues(typeof(Month)).Length;
        public const byte DaysOfWeek = 7; // Enum.GetValues(typeof(DayOfWeek)).Length;
        public const byte TimesOfDay = 2; // Enum.GetValues(typeof(TimeOfDay)).Length;
        public const byte NumWeeksPerMonth = 4;
        public const byte DaysPerMonth = DaysOfWeek * NumWeeksPerMonth; // 28
        public const byte TicksPerMonth = DaysPerMonth * TimesOfDay; // 56
        public const ushort NumTicks = Months * TicksPerMonth; // 672
        public const byte NineMonths = 9 * DaysPerMonth; // 252
        
        public Time(int year) => _offset = year;
        public Time(int year, ushort tick = 1) : this(year) => _clock = CheckTickInRange(tick);
        public Time(int year, Month month, byte day = 1, TimeOfDay time = TimeOfDay.AM) : 
            this(year) => _clock = CalcClockTick(month, CheckDayInRange(day), time);

        internal static byte CheckDayInRange(byte day) => day is > DaysPerMonth or 0 ? 
            throw new ArgumentException($"day not in range 1 to {DaysPerMonth}") : day;
        private static ushort CheckTickInRange(ushort tick) => tick is > NumTicks or 0 ? 
            throw new ArgumentException($"tick not in range 1 to {NumTicks}") : tick;

        // Reverse calculation of clock tick from month, day, and time
        private static ushort CalcClockTick(Month month, byte day, TimeOfDay time) =>
            (ushort)((byte)month * TicksPerMonth + (day - 1) * TimesOfDay + (byte)time + 1);

        // Normal calculations from clock tick to various values
        private static Month CalcMonth(ushort clock) => (Month)((clock - 1) / TicksPerMonth);
        private static byte CalcDay(ushort clock) => (byte)((clock - 1) % TicksPerMonth / TimesOfDay + 1);
        private static DayOfWeek CalcDayOfWeek(ushort clock) => (DayOfWeek)((clock - 1) / TimesOfDay % DaysOfWeek);
        private static TimeOfDay CalcTimeOfDay(ushort clock) => (TimeOfDay)((clock + 1) % TimesOfDay);

        public void Tick() {
            _clock++;
            if (_clock <= NumTicks) return;
            _year++;
            _clock = 1; }

        private Function<T> Property<T>(string property) => Member<T>(this, property, "Current", false);
        private PrimitiveTest TestProperty(string property) => TestMember(this, property, false);

        public Function<int> CurrentYear => Property<int>(nameof(Year));
        public Function<Date> CurrentDate => Property<Date>(nameof(Date));
        public Function<TimeOfDay> CurrentTimeOfDay => Property<TimeOfDay>(nameof(TimeOfDay));
        public PrimitiveTest CurrentlyMorning => TestProperty(nameof(IsAM));

        public int Year => (int)_year + _offset;
        public Month Month => CalcMonth(_clock);
        public byte Day => CalcDay(_clock);
        public Date Date => new(Month, Day);
        public DayOfWeek DayOfWeek => CalcDayOfWeek(_clock);
        public TimeOfDay TimeOfDay => CalcTimeOfDay(_clock);
        public bool IsAM => TimeOfDay == TimeOfDay.AM;
        public bool IsPM => TimeOfDay == TimeOfDay.PM;

        public PrimitiveTest<Date> IsToday => TestMethod<Date>(IsDate, false);
        public PrimitiveTest<Date> NineMonthsPast => TestMethod<Date>(NineMonthsPastDate, false);
        public PrimitiveTest<DailyOperation> CurrentlyOperating => TestMethod<DailyOperation>(InOperation, false);
        public PrimitiveTest<Schedule> CurrentlyOpen => TestMethod<Schedule>(IsOpen, false);

        public bool IsDate(Date date) => date.Equals(Month, Day);
        public bool IsOpen(Schedule schedule) => schedule.IsOpen(DayOfWeek);
        public bool InOperation(DailyOperation operation) =>
            operation is DailyOperation.AllDay ||
            (operation is DailyOperation.Morning && IsAM) ||
            (operation is DailyOperation.Evening && IsPM);
        public bool PastDate(Date date) => date.Month < Month || (date.Month == Month && date.Day < Day);
        public int YearsSince(Date date, int year) => Year - year + (PastDate(date) ? 1 : 0);
        public bool NineMonthsPastDate(Date date) => DaysSince(date) >= NineMonths;

        // using byte for Since because all base calls to since are small enough to use byte
        private static byte Since(byte current, byte previous, byte max) => (byte)((max + (current - previous)) % max);
        private byte MonthsSince(Month prevMonth) => Since((byte)Month, (byte)prevMonth, Months);
        // MonthsSince(Date) relies on prevDate being previous, not only does Since assume this
        // order but future Dates would give -1 for the same month, 0 for one month in the future, etc
        private byte MonthsSince(Date prevDate) => (byte)(MonthsSince(prevDate.Month) - (Day < prevDate.Day ? 1 : 0));
        private byte DaysSince(byte prevDay) => Since(Day, prevDay, DaysPerMonth);
        private ushort DaysSince(Date prevDate) => (ushort)(MonthsSince(prevDate) * DaysPerMonth + DaysSince(prevDate.Day));

        public static float PerDay(float chance) => chance / TimesOfDay;
        public static float PerWeek(float chance) => chance / (TimesOfDay * DaysOfWeek);
        public static float PerMonth(float chance) => chance / TicksPerMonth;
        public static float PerYear(float chance) => chance / NumTicks;

        public override string ToString() =>
            $"{DayOfWeek} {TimeOfDay} - {Month} {Day.SuffixedDate()}, {Year}";
    }
}