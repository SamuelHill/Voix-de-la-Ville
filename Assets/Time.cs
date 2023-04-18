using static Randomize;
using System;
using System.Linq;
using TED;
using TED.Primitives;
using static TED.Language;

public enum Month { January, February, March, April, May, June, July, August, September, October, November, December }
public enum DayOfWeek { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
public enum TimeOfDay { AM, PM }

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

    public Function<T> GetProperty<T>(string property) => GetMember<T>(typeof(Time), property);
    public PrimitiveTest TestProperty(string property) => TestMember(typeof(Time), property);

    #region Properties & Helper functions
    public int Year => (int)_year + _offset;
    #region Month
    internal static Month GetMonth(ushort clock) => (Month)((clock - 1) / TicksPerMonth);
    public Month Month => GetMonth(_clock);
    #endregion
    #region Day
    internal static byte GetDay(ushort clock) => (byte)((clock - 1) % TicksPerMonth / TimesOfDay + 1);
    public byte Day => GetDay(_clock);
    #endregion
    #region Date(s)
    public Date Date => new(Month, Day);
    public bool IsDate(Date date) => date.Equals(Month, Day);
    public bool PastDate(Date date) => (date.Month < Month) || (date.Month == Month && date.Day < Day);
    public int YearsSince(Date date, int year) => Year - year + (PastDate(date) ? 1 : 0);
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
    #endregion
    #region TimeOfDay
    internal static TimeOfDay GetTimeOfDay(ushort clock) => (TimeOfDay)((clock + 1) % TimesOfDay);
    public TimeOfDay TimeOfDay => GetTimeOfDay(_clock);
    public bool IsAM => TimeOfDay == TimeOfDay.AM;
    public bool IsPM => TimeOfDay == TimeOfDay.PM;
    #endregion
    #endregion

    #region Probability helpers
    public float PerDay(float chance) => chance / TimesOfDay;
    public float PerWeek(float chance) => chance / (TimesOfDay * DaysOfWeek);
    public float PerMonth(float chance) => chance / TicksPerMonth;
    public float PerYear(float chance) => chance / NumTicks;
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

    public static Date Random() =>
        new(Enum.GetValues(typeof(Month)).Cast<Month>().ToList().RandomElement(),
            Byte(1, 28));

    public bool Equals(Month month, byte date) => month == Month && date == Day;
    public bool Equals(Date other) => Month == other.Month && Day == other.Day;
    public override bool Equals(object obj) => obj is Date other && Equals(other);
    public override int GetHashCode() => HashCode.Combine((int)Month, Day);
    public static bool operator ==(Date d1, Date d2) => d1.Month == d2.Month && d1.Day == d2.Day;
    public static bool operator !=(Date d1, Date d2) => !(d1 == d2);

    public override string ToString() => $"{(int)Month + 1}/{Day}";

    // TODO - change the FromString to be "mm/dd" not "MMM,dd"
    public static Date FromString(string dateString) { // Month,date
        var date = dateString.Split(',');
        Enum.TryParse(date[0], out Month month);
        return new Date(month, byte.Parse(date[1])); }
}