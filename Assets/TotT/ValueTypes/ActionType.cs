namespace TotT.ValueTypes {
    /// <summary>
    /// Movement categories. GoingToWork is a special case that handles going to different Locations not
    /// for the services provided or to hang out but to do some Vocation at the Location. All other ActionTypes
    /// correlate to a LocationCategory, the table containing these relations is ActionToCategory (see StaticTables).
    /// </summary>
    public enum ActionType {
        GoingToWork,
        GoingToSchool,
        StayingIn,
        Visiting,
        EnjoyingAmenity,
        Administrating,
        AddressingHealth,
        GoingOutToEatOrDrink,
        CommercialShopping,
        MarketShopping,
        PersonalShopping,
        GoingOutForDateNight
    }
}
