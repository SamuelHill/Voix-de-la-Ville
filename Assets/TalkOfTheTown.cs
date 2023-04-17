using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using TED.Primitives;
using TED.Tables;
using TED.Utilities;
using UnityEngine;
using static TED.Language;

public static unsafe class FromPointer<T> where T : unmanaged {
    public static Func<T> Get(T variable) {
        var pointer = &variable;
        return () => *pointer; }
    public static Func<bool> Is(bool variable) {
        var pointer = &variable;
        return () => *pointer; }
    public static Func<bool> Not(bool variable) {
        var pointer = &variable;
        return () => !*pointer; }
}

public class TalkOfTheTown {
    public static Simulation Simulation = null!;
    public static Time Time;
    public Sims SimsUtils;
    public Town TownToTalkAbout;
    public Color DefaultTileColor;
    private bool _firstTick;

    #region CSV/TXT file helpers and CSV parsing functions
    private const string DataPath = "../TalkOfTheTown/Assets/Data/";
    private static string Txt(string filename) => $"{DataPath}{filename}.txt";
    private static string Csv(string filename) => $"{DataPath}{filename}.csv";
    private object ParsePerson(string personString) {
        var potentialPerson = Person.FromString(personString);
        if (Agents == null) return potentialPerson;
        var potentialPeople = Agents.Where(a => a.Item1 == personString).Select(a => a.Item1).ToList();
        return potentialPeople.Any() ? potentialPeople.First() : potentialPerson; }
    private object ParseLocation(string locationString) {
        var potentialLocation = Location.FromString(locationString);
        if (Locations == null) return potentialLocation;
        var potentialLocations = Locations.Where(x => x.Item1 == potentialLocation).Select(x => x.Item1).ToList();
        return potentialLocations.Any() ? potentialLocations.First() : potentialLocation;  }
    private object ParseColor(string htmlColorString) => ColorUtility.TryParseHtmlString(htmlColorString, out var color) ? color : DefaultTileColor;
    private static object ParseVector2Int(string vector2String) => IntArrayToVector((from i in vector2String.Split(',') select int.Parse(i)).ToArray());
    private static Vector2Int IntArrayToVector(IReadOnlyList<int> intArray) => new (intArray[0], intArray[1]);
    private static object ParseDate(string dateString) => Date.FromString(dateString);
    private static object ParseSexuality(string sexualityString) => Sexuality.FromString(sexualityString);
    #endregion

    #region Constructors
    public TalkOfTheTown(Color defaultTileColor) {
        Time = new Time();
        TownToTalkAbout = new Town();
        SimsUtils = new Sims();
        DefaultTileColor = defaultTileColor;
        CsvReader.DeclareParser(typeof(Vector2Int), ParseVector2Int);
        CsvReader.DeclareParser(typeof(Date), ParseDate);
        CsvReader.DeclareParser(typeof(Sexuality), ParseSexuality);
        CsvReader.DeclareParser(typeof(Person), ParsePerson);
        CsvReader.DeclareParser(typeof(Location), ParseLocation);
        CsvReader.DeclareParser(typeof(Color), ParseColor);
        // Share the same Randomize Seed for now...
        TED.Utilities.Random.Rng = new System.Random(Randomize.Seed);
    }
    public TalkOfTheTown(Color defaultTileColor, int year) : this(defaultTileColor) => Time = new Time(year);
    public TalkOfTheTown(Color defaultTileColor, int year, ushort tick) : this(defaultTileColor) => Time = new Time(year, tick);
    #endregion

    #region "Extensional" Databases
    public TablePredicate<Person, int, Date, Sex, Sexuality> Agents;
    public TablePredicate<Person> Dead;
    public TablePredicate<Person, Person> Couples;
    public TablePredicate<Person, Person> Parents;
    public TablePredicate<LocationType, LocationCategories, Accessibility, DailyOperation> LocationInformation;
    public KeyIndex<(LocationType, LocationCategories, Accessibility, DailyOperation), LocationType> LocationTypeKeyInfo;
    public TablePredicate<LocationCategories, Color> CategoryColors;
    public TablePredicate<LocationType, Color> LocationColors;
    public KeyIndex<(LocationType, Color), LocationType> LocationColorsIndex;
    public TablePredicate<Location, Vector2Int, int, Date> Locations;
    public KeyIndex<(Location, Vector2Int, int, Date), Location> LocationsLocationIndex;
    public KeyIndex<(Location, Vector2Int, int, Date), Vector2Int> LocationsPositionIndex;
    public TablePredicate<Location, Vector2Int, int, Date> NewLocations;
    public TablePredicate<Location, Vector2Int, int, Date> VacatedLocations;
    public TablePredicate<Vector2Int> UsedLots;
    public TablePredicate<Vocation, Person, Location> Vocations;
    public TablePredicate<Person, Location> Homes;
    #endregion

    #region Functions
    public Function<TimeOfDay> GetTimeOfDay => GetMember(() => Time.TimeOfDay);
    public Function<int> GetYear => GetMember(() => Time.Year);
    public Function<Date> GetDate => GetMember(() => Time.Date);
    public Function<Sex> RandomSex => Method(Sims.RandomSex);
    public Function<Sex, Sexuality> RandomSexuality => new("RandomSexuality", Sexuality.Random);
    public Function<string, string, Person> NewPerson => new(Sims.NewPerson);
    public Function<int> RandomAdultAge => Method(SimsUtils.RandomAdultAge);
    public Function<Person, string> Surname => new("Surname", p => p.LastName);
    public Function<Person, Facet, sbyte> GetFacet => new("GetFacet", (p, f) => p.GetFacet(f));
    public Function<Person, Vocation, sbyte> GetVocation => new("GetVocation", (p, v) => p.GetVocation(v));
    public Function<Location, LocationType> GetLocationType => new("GetLocationType", l => l.Type);
    public Function<uint> NumLots => Function("NumLots", () => UsedLots.Length);
    public Function<uint, Vector2Int> RandomLot => new(TownToTalkAbout.RandomLot);
    public Function<string, LocationType, Location> NewLocation => new(Town.NewLocation);
    public Function<Vector2Int, Vector2Int, int> Distance => new(Town.Distance);
    #endregion

    #region PrimitiveTests
    public PrimitiveTest<DailyOperation, TimeOfDay> IsOpen => new(Town.IsOpen);
    public PrimitiveTest<Accessibility, bool, bool> IsAccessible => new(Town.IsAccessible);
    public PrimitiveTest<Date> IsDate => new(Time.IsDate);
    public PrimitiveTest IsMonday => TestMember(() => Time.IsMonday);
    public PrimitiveTest IsTuesday => TestMember(() => Time.IsTuesday);
    public PrimitiveTest IsWednesday => TestMember(() => Time.IsWednesday);
    public PrimitiveTest IsThursday => TestMember(() => Time.IsThursday);
    public PrimitiveTest IsFriday => TestMember(() => Time.IsFriday);
    public PrimitiveTest IsSaturday => TestMember(() => Time.IsSaturday);
    public PrimitiveTest IsSunday => TestMember(() => Time.IsSunday);
    public PrimitiveTest IsNotFirstTick => new("IsNotFirstTick", () => !_firstTick);
    public PrimitiveTest<Vector2Int> IsVacant => new("IsVacant", 
        vec => UsedLots.All(v => v != vec));
    public PrimitiveTest<Location, LocationType> IsLocationType =>
        new("IsLocationType", (location, locationType) => location.Type == locationType);
    #endregion

    public void InitSimulator() {
        Simulation = new Simulation("Talk of the Town");
        _firstTick = true;

        Simulation.BeginPredicates();

        // Vars, just so happen to fit the naming conventions for local variables, i.e. lowercase.
        #region Variables
        var person           = (Var<Person>)"person";
        var partner          = (Var<Person>)"partner";
        var parent           = (Var<Person>)"parent";
        var child            = (Var<Person>)"child";
        var man              = (Var<Person>)"man";
        var woman            = (Var<Person>)"woman";
        var otherPerson      = (Var<Person>)"otherPerson";
        var firstName        = (Var<string>)"firstName";
        var lastName         = (Var<string>)"lastName";
        var age              = (Var<int>)"age";
        var dateOfBirth      = (Var<Date>)"dateOfBirth";
        var sex              = (Var<Sex>)"sex";
        var sexOfPartner     = (Var<Sex>)"sexOfPartner";
        var sexuality        = (Var<Sexuality>)"sexuality";
        var sexualOfPartner  = (Var<Sexuality>)"sexualityOfPartner";
        var location         = (Var<Location>)"location";
        var home             = (Var<Location>)"home";
        var position         = (Var<Vector2Int>)"position";
        var opening          = (Var<Date>)"opening";
        var founded          = (Var<int>)"founded";
        var locationType     = (Var<LocationType>)"locationType";
        var locationCategory = (Var<LocationCategories>)"locationCategory";
        var accessibility    = (Var<Accessibility>)"accessibility";
        var operation        = (Var<DailyOperation>)"operation";
        var color            = (Var<Color>)"color";
        var openSunday       = (Var<bool>)"openSunday";
        var openMonday       = (Var<bool>)"openMonday";
        var openTuesday      = (Var<bool>)"openTuesday";
        var openWednesday    = (Var<bool>)"openWednesday";
        var openThursday     = (Var<bool>)"openThursday";
        var openFriday       = (Var<bool>)"openFriday";
        var openSaturday     = (Var<bool>)"openSaturday";
        var job              = (Var<Vocation>)"job";
        var positions        = (Var<int>)"positions";
        var employee         = (Var<Person>)"employee";
        var occupant         = (Var<Person>)"occupant";
        #endregion
        // Tables, despite being local variables, will still be capitalized for style/identification purposes.
        // ReSharper disable InconsistentNaming

        var MaleNames = FromCsv("MaleNames", Txt("male_names"), firstName);
        var FemaleNames = FromCsv("FemaleNames", Txt("female_names"), firstName);
        var Surnames = FromCsv("Surnames", Txt("english_surnames"), lastName);
        var RandomFirstName = Predicate("RandomFirstName", sex, firstName);
        RandomFirstName[Sex.Male, firstName].If(RandomElement(MaleNames, firstName));
        RandomFirstName[Sex.Female, firstName].If(RandomElement(FemaleNames, firstName));
        var RandomPerson = Predicate("RandomPerson", sex, person);
        RandomPerson.If(RandomFirstName, RandomElement(Surnames, lastName), person == NewPerson[firstName, lastName]);

        Agents = FromCsv("Agents", Csv("agents"), person.Key, age, dateOfBirth.Indexed, sex.Indexed, sexuality);
        Dead = Predicate("Dead", person);
        var Alive = Definition("Alive", person).Is(Not[Dead[person]]);
        var Died = Predicate("Died", person).If(Agents, age > 60,
            Prob[Time.PerMonth(0.001f)], Alive[person]);
        Dead.Accumulates(Died);

        var Man = Predicate("Man", person).If(
            Agents[person, age, dateOfBirth, Sex.Male, sexuality], Alive[person], age >= 18);
        var Woman = Predicate("Woman", person).If(
            Agents[person, age, dateOfBirth, Sex.Female, sexuality], Alive[person], age >= 18);

        Couples = Predicate("Couples", person, partner);
        Couples.Unique = true;
        var NewCouples = Predicate("NewCouples", person, partner);
        NewCouples.Unique = true;
        NewCouples.If(Woman[person], RandomElement(Man, partner), Prob[0.5f],
            !Couples[person, man], !Couples[woman, partner]);
        Couples.Accumulates(NewCouples);

        var BirthTo = Predicate("Birth", woman, man, sex, child).If(
            Couples[woman, man], sex == RandomSex,
            Prob[Time.PerYear(0.3f)], RandomFirstName, 
            child == NewPerson[firstName, Surname[man]]);
        Parents = Predicate("Parents", parent, child);
        Parents.Add.If(BirthTo[parent, person, sex, child]);
        Parents.Add.If(BirthTo[person, parent, sex, child]);
        Agents.Add[person, -1, GetDate, sex, sexuality].If(
            BirthTo[man, woman, sex, person], sexuality == RandomSexuality[sex]);

        var IsFamily = Definition("IsFamily", person, otherPerson).Is(
            Couples[person, otherPerson] | Couples[otherPerson, person] | 
            (Parents[person, otherPerson] & Agents[otherPerson, age, dateOfBirth, sex, sexuality] & age <= 18) |
            (Parents[otherPerson, person] & Agents[person, age, dateOfBirth, sex, sexuality] & age <= 18));

        LocationInformation = FromCsv("LocationInformation", Csv("locationInformation"), 
            locationType.Key, locationCategory.Indexed, accessibility, operation);
        LocationTypeKeyInfo = LocationInformation.KeyIndex(locationType);
        CategoryColors = FromCsv("CategoryColors", Csv("locationColors"), locationCategory.Key, color);
        LocationColors = Predicate("LocationColors", locationType.Key, color);
        LocationColors.Unique = true;
        LocationColors.Add.If(LocationInformation, CategoryColors);
        LocationColorsIndex = LocationColors.KeyIndex(locationType);

        var OperatingSchedule = FromCsv("OperatingSchedule", 
            Csv("operatingSchedule"), locationType.Key, 
            openSunday, openMonday, openTuesday, openWednesday, 
            openThursday, openFriday, openSaturday);
        var VocationLocations = FromCsv("VocationLocations", 
            Csv("vocationLocations"), job, locationType);
        var PositionsPerJob = FromCsv("PositionsPerJob", 
            Csv("positionsPerJob"), job.Key, positions);

        Locations = FromCsv("Locations", Csv("locations"), location, position.Key, founded, opening);
        //LocationsLocationIndex = Locations.KeyIndex(location);
        LocationsPositionIndex = Locations.KeyIndex(position);
        NewLocations = Predicate("NewLocations", location, position, founded, opening);
        VacatedLocations = Predicate("VacatedLocations", location, position, founded, opening);  // UNUSED
        UsedLots = Predicate("UsedLots", position);
        UsedLots.Unique = true;

        var FreeLot = Definition("FreeLot", position).Is(position == RandomLot[NumLots], IsVacant[position]);
        NewLocations.If(FreeLot[position], 
            location == NewLocation["Test", LocationType.House], 
            founded == GetYear, opening == GetDate, Prob[Time.PerWeek(0.8f)], IsNotFirstTick);
        Locations.Accumulates(NewLocations);
        UsedLots.If(Locations);

        Homes = Predicate("Homes", occupant.Key, location.Indexed);
        Homes.Unique = true;
        Homes.Add.If(Agents[occupant, age, dateOfBirth, sex, sexuality], age > 18,
            Locations, IsLocationType[location, LocationType.House], !Homes[occupant, home],
            !Homes[person, location] | (Homes[person, location] & IsFamily[person, occupant]));


        Vocations = Predicate("Vocation", job, employee, location);

        var OnShift = Predicate("OnShift", person, job, location);

        var OpenForBusiness = Predicate("OpenForBusiness", location);
        OpenForBusiness.If(Locations, locationType == GetLocationType[location],
            LocationInformation, IsOpen[operation, GetTimeOfDay], OperatingSchedule,
            (openMonday == true & IsMonday) |
            (openTuesday == true & IsTuesday) | 
            (openWednesday == true & IsWednesday) | 
            (openThursday == true & IsThursday) |
            (openFriday == true & IsFriday) | 
            (openSaturday == true & IsSaturday) | 
            (openSunday == true & IsSunday));

        // ReSharper restore InconsistentNaming

        Simulation.EndPredicates();
        Simulation.Update();

        _firstTick = false;
        Time.Tick(); // Call this so that these initial table builds get proper update treatment.
    }

    public void UpdateSimulator() {
        Simulation.Update();
        UpdateRows();
        Time.Tick();
    }

    public void UpdateRows() {
        // Age a person on the first tick of a given date
        if (Time.IsAM) {
            Agents.UpdateRows((ref (Person _, int age, Date dateOfBirth, Sex __, Sexuality ___) agent) => {
                if (Time.IsDate(agent.dateOfBirth) && agent.age >= 0) agent.age++;
            });
        }
        // However, if they were born on today's date (as denoted with the age -1), set the age to 0 in the last tick of the given date
        else if (Time.IsPM) {
            Agents.UpdateRows((ref (Person _, int age, Date dateOfBirth, Sex __, Sexuality ___) agent) => {
                if (Time.IsDate(agent.dateOfBirth) && agent.age < 0) agent.age = 0;
            });
        }
    }

}
