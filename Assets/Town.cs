using System;
using UnityEngine;

public enum LocationCategories {
    Accommodation, Administration, Amenity, ChildCare, Commerce, Eatery, Food, Health, Industry, Personal }
public enum LocationType {
    ApartmentComplex, Bakery, Bank, Bar, Barbershop, Brewery, ButcherShop, CandyShop, CarpentryCompany,
    CityHall, ClothingStore, CommunityCenter, Dairy, DayCare, DentistOffice, DepartmentStore, Diner,
    Distillery, DoctorOffice, Farm, FireStation, Foundry, FurnitureStore, GeneralStore, GroceryStore,
    HardwareStore, Hospital, House, Inn, JewelryShop, LumberMill, OptometryClinic, Orchard, Park, Pharmacy,
    PoliceStation, PostOffice, Quarry, Restaurant, School, ShoemakerShop, TailorShop, TattooParlor, }
public enum Vocation {
    Architect, Baker, BankTeller, Barber, Barkeeper, Bartender, Bottler, Brewer, Builder,
    BusDriver, Busser, Butcher, Carpenter, Cashier, Clothier, Cook, Cooper, DaycareProvider,
    Dentist, Dishwasher, Distiller, Doctor, Dressmaker, Farmer, Farmhand, FireChief, Firefighter,
    Grocer, Groundskeeper, Innkeeper, Janitor, Jeweler, Joiner, Laborer, Maid, Mayor, Milkman,
    Millworker, Mortician, Nurse, Optometrist, Painter, Pharmacist, Plumber, PoliceChief,
    PoliceOfficer, PostalWorker, Principal, Puddler, Quarryman, Seamstress, Secretary, Shoemaker,
    Stocker, Stonecutter, Surgeon, Tailor, TattooArtist, Teacher, Turner, Waiter, Woodworker, }

public static class Town {
    public static Vector2Int Max = new(10, 10);
    public static Vector2Int Min = new(-10, -10);

    private static int GridSpaces() => (Math.Abs(Min.x) + Max.x) * (Math.Abs(Min.y) + Max.y);
    private static void ExpandAllSides() { Min.x--; Max.x++; Min.y--; Max.y++; }
    private static Vector2Int RandomLot() => new(Randomize.Integer(Min.x, Max.x), Randomize.Integer(Min.y, Max.y));
    public static Vector2Int RandomLot(uint lotCount) {
        if (lotCount * 2 >= GridSpaces())
            ExpandAllSides();
        return RandomLot(); }

    private static int EuclideanSquare(int x1, int y1, int x2, int y2) => (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
    public static int Distance(Vector2Int loc1, Vector2Int loc2) => EuclideanSquare(loc1.x, loc1.y, loc2.x, loc2.y);

    public static Location NewLocation(string name) => new(name);
}

public class Location {
    private readonly Guid _id;
    public string Name;

    internal Location() => _id = Guid.NewGuid();
    public Location(string name) : this() => Name = Utils.Title(name);

    public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
    public override int GetHashCode() => _id.GetHashCode();
    public static bool operator ==(Location l, string potentialName) => l != null && l.Name == potentialName;
    public static bool operator !=(Location l, string potentialName) => !(l == potentialName);

    public override string ToString() => Name;
    public static Location FromString(string locationString) => new(locationString);
}
