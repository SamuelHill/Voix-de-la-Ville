namespace TotT.ValueTypes {
    /// <summary>
    /// Denotes which period(s) of the day - TimeOfDay - a LocationType is open/operational. Due to this
    /// correlation with TimeOfDay (values of AM/PM) the operational periods are Morning, Evening, and AllDay.
    /// The table that relates each DailyOperation to its TimeOfDay(s) is _operatingTimes (see StaticTables).
    /// </summary>
    public enum DailyOperation { Morning, Evening, AllDay }
}
