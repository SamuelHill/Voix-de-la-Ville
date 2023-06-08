namespace TotT.ValueTypes {
    /// <summary>
    /// Denotes shared services/offerings that a set of LocationTypes may provide. Highest order
    /// classification of Location. The table that relates each LocationType to its LocationCategory
    /// is LocationInformation (see StaticTables).
    /// </summary>
    public enum LocationCategory {
        Accommodation,
        Administration,
        Amenity,
        ChildCare,
        Commerce,
        Eatery,
        Food,
        Health,
        Industry,
        Personal
    }
}
