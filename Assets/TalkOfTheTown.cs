using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using TED.Primitives;
using TED.Tables;
using TED.Utilities;
using UnityEngine;
using static TED.Language;

public class TalkOfTheTown {
    public static Simulation Simulation = null!;
    public static Time Time;
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

    #region public Tables
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

    public void InitSimulator() {
        Simulation = new Simulation("Talk of the Town");
        _firstTick = true;

        // ReSharper disable InconsistentNaming
        #region Functions
        var GetTimeOfDay = Time.GetProperty<TimeOfDay>(nameof(Time.TimeOfDay));
        var GetYear = Time.GetProperty<int>(nameof(Time.Year));
        var GetDate = Time.GetProperty<Date>(nameof(Time.Date));
        var YearsSince = Method<Date, int, int>(Time.YearsSince);
        var RandomSex = Method(Sims.RandomSex);
        var RandomSexuality = Function<Sex, Sexuality>("RandomSexuality", Sexuality.Random);
        var NewPerson = Method<string, string, Person>(Sims.NewPerson);
        var RandomDate = Function("RandomDate", Date.Random);
        var RandomAdultAge = Method(Sims.RandomAdultAge);
        var FertilityRate = Method<int, float>(Sims.FertilityRate);
        var Surname = Function<Person, string>("Surname", p => p.LastName);
        var GetFacet = Function<Person, Facet, sbyte>("GetFacet", 
            (p, f) => p.GetFacet(f));
        var GetVocation = Function<Person, Vocation, sbyte>("GetVocation", 
            (p, v) => p.GetVocation(v));
        var GetLocationType = Function<Location, LocationType>("GetLocationType", 
            l => l.Type);
        var NumLots = Function("NumLots", () => UsedLots.Length);
        var RandomLot = Method<uint, Vector2Int>(TownToTalkAbout.RandomLot);
        var NewLocation = Method<string, LocationType, Location>(Town.NewLocation);
        var Distance = Method<Vector2Int, Vector2Int, int>(Town.Distance);
        #endregion

        #region PrimitiveTests
        var IsNotFirstTick = Test("IsNotFirstTick", () => !_firstTick);
        var IsAttracted = Test<Sexuality, Sex>("IsAttracted", 
            (sexuality, sex) => sexuality.IsAttracted(sex));
        var IsOpen = TestMethod<DailyOperation, TimeOfDay>(Town.IsOpen);
        var IsAccessible = TestMethod<Accessibility, bool, bool>(Town.IsAccessible);
        var IsDate = TestMethod<Date>(Time.IsDate);
        var IsMonday = Time.TestProperty(nameof(Time.IsMonday));
        var IsTuesday = Time.TestProperty(nameof(Time.IsTuesday));
        var IsWednesday = Time.TestProperty(nameof(Time.IsWednesday));
        var IsThursday = Time.TestProperty(nameof(Time.IsThursday));
        var IsFriday = Time.TestProperty(nameof(Time.IsFriday));
        var IsSaturday = Time.TestProperty(nameof(Time.IsSaturday));
        var IsSunday = Time.TestProperty(nameof(Time.IsSunday));
        var IsVacant = Test<Vector2Int>("IsVacant", 
            vec => UsedLots.All(v => v != vec)); 
        var IsLocationType = Test<Location, LocationType>("IsLocationType", 
            (location, locationType) => location.Type == locationType);
        #endregion

        // variables just so happen to follow c# var name norms, still disabling InconsistentNaming
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
        var personalityScore = (Var<sbyte>)"personalityScore";
        var vocationScore    = (Var<sbyte>)"vocationScore";
        #endregion

        Simulation.BeginPredicates();

        // Tables, despite being local variables, will still be capitalized for style/identification purposes.
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
        NewLocations[location, position, GetYear, GetDate].If(FreeLot[position], 
            location == NewLocation["Test", LocationType.House], Prob[Time.PerWeek(0.8f)], IsNotFirstTick);
        Locations.Accumulates(NewLocations);
        UsedLots.If(Locations);

        Homes = Predicate("Homes", occupant.Key, location.Indexed);
        Homes.Unique = true;
        Homes.Add.If(Agents[occupant, age, dateOfBirth, sex, sexuality], age > 18,
            Locations, IsLocationType[location, LocationType.House], !Homes[occupant, home],
            !Homes[person, location] | (Homes[person, location] & IsFamily[person, occupant]));

        Vocations = Predicate("Vocation", job, employee, location);

        //var Aptitude = Definition("Aptitude", person, job, vocationScore).Is(Agents, Alive[person], vocationScore == GetVocation[person, job]);
        var BestForJob = Predicate("BestForDaycareWork", person).If(Agents, Alive[person], Maximal(person, vocationScore, vocationScore == GetVocation[person, Vocation.DaycareProvider]));

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