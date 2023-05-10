using static Randomize;
using System;
using System.Linq;
using TED;
using TED.Primitives;
using static TED.Language;
using System.Collections.Generic;

public enum Month { January, February, March, April, May, June, 
    July, August, September, October, November, December }
public enum DayOfWeek { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
public enum TimeOfDay { AM, PM }
public enum Schedules { Everyday, Weekdays, ClosedSunday, 
    ClosedMonday, ClosedTuesday, ClosedWednesday, 
    ClosedThursday, ClosedFriday, ClosedSaturday, 
    MondayToThursday, TuesdayToFriday, ThursdayToSunday }
public enum DailyOperation { Morning, Evening, AllDay }

public static class ByteExtensions {
    public static string SuffixedDate(this byte num) {
        var number = num.ToString();
        return number.EndsWith("11") ? number + "th" : 
               number.EndsWith("12") ? number + "th" :
               number.EndsWith("13") ? number + "th" :
               number.EndsWith("1") ? number + "st" :
               number.EndsWith("2") ? number + "nd" :
               number.EndsWith("3") ? number + "rd" :
               number + "th"; }
}

public class Time {
    private ushort _clock = 1; // no day zero... makes the % math easier
    private uint _year; // internal way to keep track of years that have ticked past, starts at 0
    private readonly int _offset; // int year for pretty dates, optional

    #region Constants and pseudo-constants
    private static byte EnumLength(Type enumType) => (byte)Enum.GetNames(enumType).Length;
    public static readonly byte Months = EnumLength(typeof(Month)); // 12
    public static readonly byte DaysOfWeek = EnumLength(typeof(DayOfWeek)); // 7
    public static readonly byte TimesOfDay = EnumLength(typeof(TimeOfDay)); // 2
    public const byte NumWeeksPerMonth = 4;
    public static readonly byte DaysPerMonth = (byte)(DaysOfWeek * NumWeeksPerMonth); // 28
    public static readonly byte TicksPerMonth = (byte)(DaysPerMonth * TimesOfDay); // 56
    public static readonly ushort NumTicks = (ushort)(Months * TicksPerMonth); // 672
    #endregion

    #region Constructors (& tick calc helpers)
    public Time() { }
    public Time(int year) => _offset = year; // can be negative
    public Time(int year, ushort tick) : this(year) {
        if (tick > NumTicks || tick == 0)
            throw new ArgumentException("clock must be in range 1 to 672");
        _clock = tick; }
    public Time(int year, Month month, byte day, TimeOfDay time) : this(year, CalcClockTick(new Date(month, day), time)) {}
    public static ushort CalcClockTick(Date date, TimeOfDay time) => CalcClockTick(date.Month, date.Day, time);
    public static ushort CalcClockTick(Month month, byte day, TimeOfDay time) =>
        (ushort)((byte)month * TicksPerMonth + (day - 1) * TimesOfDay + (byte)time + 1);
    #endregion

    public void Tick() {
        _clock++;
        if (_clock <= NumTicks) return;
        _year++;
        _clock = 1; }

    public Function<T> GetProperty<T>(string property) => GetMember<T>(this, property);
    public PrimitiveTest TestProperty(string property) => TestMember(this, property);

    private static byte Since(byte current, byte past, byte max) => (byte)((current - past + max) % max);
    // using byte for since because all base calls to since are small enough to just use byte

    #region Properties & Helper functions
    public int Year => (int)_year + _offset;
    #region Month
    internal static Month GetMonth(ushort clock) => (Month)((clock - 1) / TicksPerMonth);
    public Month Month => GetMonth(_clock);
    public byte MonthsSince(Month pastMonth) => Since((byte)(Month + 1), (byte)(pastMonth + 1), Months);
    #endregion
    #region Day
    internal static byte GetDay(ushort clock) => (byte)((clock - 1) % TicksPerMonth / TimesOfDay + 1);
    public byte Day => GetDay(_clock);
    public byte DaysSince(byte pastDay) => Since(Day, pastDay, DaysPerMonth);
    #endregion
    #region Date(s)
    public Date Date => new(Month, Day);
    public bool IsDate(Date date) => date.Equals(Month, Day);
    public bool PastDate(Date date) => (date.Month < Month) || (date.Month == Month && date.Day < Day);
    public int YearsSince(Date date, int year) => Year - year + (PastDate(date) ? 1 : 0);
    public ushort DaysSince(Date date) => (ushort)(MonthsSince(date.Month) * DaysPerMonth + DaysSince(date.Day));
    public bool NineMonthsPast(Date date) => DaysSince(date) >= 9 * DaysPerMonth;
    #endregion
    #region DayOfWeek
    internal static DayOfWeek GetDayOfWeek(ushort clock) => (DayOfWeek)((clock - 1) / TimesOfDay % DaysOfWeek);
    public DayOfWeek DayOfWeek => GetDayOfWeek(_clock);
    public bool IsMonday => DayOfWeek == DayOfWeek.Monday;
    public bool IsTuesday => DayOfWeek == DayOfWeek.Tuesday;
    public bool IsWednesday => DayOfWeek == DayOfWeek.Wednesday;
    public bool IsThursday => DayOfWeek == DayOfWeek.Thursday;
    public bool IsFriday => DayOfWeek == DayOfWeek.Friday;
    public bool IsSaturday => DayOfWeek == DayOfWeek.Saturday;
    public bool IsSunday => DayOfWeek == DayOfWeek.Sunday;
    public bool IsOpen(Schedule schedule) => schedule.OpenOn[(int)DayOfWeek];
    #endregion
    #region TimeOfDay
    internal static TimeOfDay GetTimeOfDay(ushort clock) => (TimeOfDay)((clock + 1) % TimesOfDay);
    public TimeOfDay TimeOfDay => GetTimeOfDay(_clock);
    public bool IsAM => TimeOfDay == TimeOfDay.AM;
    public bool IsPM => TimeOfDay == TimeOfDay.PM;
    public bool InOperation(DailyOperation operation) =>
        operation is DailyOperation.AllDay ||
        (operation is DailyOperation.Morning && IsAM) ||
        (operation is DailyOperation.Evening && IsPM);
    #endregion
    #endregion

    #region Probability helpers
    public static float PerDay(float chance) => chance / TimesOfDay;
    public static float PerWeek(float chance) => chance / (TimesOfDay * DaysOfWeek);
    public static float PerMonth(float chance) => chance / TicksPerMonth;
    public static float PerYear(float chance) => chance / NumTicks;
    #endregion

    public override string ToString() => 
        $"{DayOfWeek} {TimeOfDay} - {Month} {Day.SuffixedDate()}, {Year}";
}

public readonly struct Date {
    public Month Month { get; }
    public byte Day { get; }
    
    public Date(Month month, byte day) {
        if (day > Time.DaysPerMonth || day == 0)
            throw new ArgumentException("date must be in range 1 to 28");
        Month = month;
        Day = day; }

    internal static Month RandomMonth() =>
        Enum.GetValues(typeof(Month)).Cast<Month>().ToList().RandomElement();
    public static Date Random() => new(RandomMonth(), Byte(1, 28));

    public bool Equals(Month month, byte date) => month == Month && date == Day;
    public bool Equals(Date other) => Month == other.Month && Day == other.Day;
    public override bool Equals(object obj) => obj is Date other && Equals(other);
    public override int GetHashCode() => HashCode.Combine((int)Month, Day);
    public static bool operator ==(Date d1, Date d2) => d1.Month == d2.Month && d1.Day == d2.Day;
    public static bool operator !=(Date d1, Date d2) => !(d1 == d2);

    public override string ToString() => $"{(int)Month + 1}/{Day}";

    public static Date FromString(string dateString) {
        var date = dateString.Split('/');
        return new Date((Month)(int.Parse(date[0]) - 1), byte.Parse(date[1])); }
}

public readonly struct Schedule {
    public readonly bool[] OpenOn;

    public Schedule(bool[] openOn) {
        if (openOn.Length != Time.DaysOfWeek)
            throw new ArgumentException($"openOn must be a bool[] of length {Time.DaysOfWeek}");
        OpenOn = openOn; }
    public Schedule(Schedules schedule) : this(GetSchedule(schedule)) { }

    public static bool[] GetSchedule(Schedules schedule) => SchedulesMapping[(int)schedule];
    public static bool[][] SchedulesMapping = {
        new[] { true, true, true, true, true, true, true }, // indexed by Schedules Enum
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
        new[] { false, false, false, true, true, true, true }};

    private string DayOfWeekList() {
        var strings = new List<string>();
        for (var i = 0; i < Time.DaysOfWeek; i++) {
            if (OpenOn[i]) strings.Add(((DayOfWeek)i).ToString()); }
        return string.Join(", ", strings); }
    public override string ToString() {
        var schedule = Array.IndexOf(SchedulesMapping, OpenOn);
        return schedule != -1 ? ((Schedules)schedule).ToString() : DayOfWeekList(); }

    public static Schedule FromString(string scheduleString) {
        Enum.TryParse(scheduleString, out Schedules schedule);
        return new Schedule(schedule); }
}