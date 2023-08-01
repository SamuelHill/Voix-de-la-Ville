namespace VdlV.Time {
    /// <summary>
    /// Names of some common schedules. Not used as a value type in simulation, instead this
    /// enum is used for easy ToString and FromString (from CSVs) for the Schedule type.
    /// </summary>
    public enum ScheduleName {
        Everyday,
        Weekdays,
        ClosedSunday,
        ClosedMonday,
        ClosedTuesday,
        ClosedWednesday,
        ClosedThursday,
        ClosedFriday,
        ClosedSaturday,
        MondayToThursday,
        TuesdayToFriday,
        ThursdayToSunday
    }
}
