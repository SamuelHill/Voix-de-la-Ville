namespace TotT.ValueTypes {
    /// <summary>
    /// Highest order classification of Location. LocationCategory denotes shared services/offerings
    /// that the underlying LocationTypes may provide. The table that relates each LocationType to its
    /// LocationCategory is LocationInformation (see StaticTables).
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