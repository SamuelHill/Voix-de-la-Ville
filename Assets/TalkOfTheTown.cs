using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using TED.Tables;
using TED.Utilities;
using UnityEngine;
using static TED.Language;

public class TalkOfTheTown {
    public static Simulation Simulation = null!;
    public static Time Time;
    private bool _firstTick;

    #region CSV/TXT file helpers and CSV parsing functions
    private const string DataPath = "../TalkOfTheTown/Assets/Data/";
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
    private static object ParseColor(string htmlColorString) => ColorUtility.TryParseHtmlString(htmlColorString, out var color) ? color : Color.white;
    private static object ParseVector2Int(string vector2String) => IntArrayToVector((from i in vector2String.Split(',') select int.Parse(i)).ToArray());
    private static Vector2Int IntArrayToVector(IReadOnlyList<int> intArray) => new (intArray[0], intArray[1]);
    private static object ParseDate(string dateString) => Date.FromString(dateString);
    private static object ParseSexuality(string sexualityString) => Sexuality.FromString(sexualityString);
    private static object ParseSchedule(string scheduleString) => Schedule.FromString(scheduleString);
    #endregion

    #region Constructors
    public TalkOfTheTown() {
        Time = new Time();
        CsvReader.DeclareParser(typeof(Vector2Int), ParseVector2Int);
        CsvReader.DeclareParser(typeof(Date), ParseDate);
        CsvReader.DeclareParser(typeof(Sexuality), ParseSexuality);
        CsvReader.DeclareParser(typeof(Person), ParsePerson);
        CsvReader.DeclareParser(typeof(Location), ParseLocation);
        CsvReader.DeclareParser(typeof(Schedule), ParseSchedule);
        CsvReader.DeclareParser(typeof(Color), ParseColor);
        // Share the same Randomize Seed for now...
        TED.Utilities.Random.Rng = new System.Random(Randomize.Seed);
    }
    public TalkOfTheTown(int year) : this() => Time = new Time(year);
    public TalkOfTheTown(int year, ushort tick) : this() => Time = new Time(year, tick);
    #endregion

    #region public Tables
    public TablePredicate<Person, int, Date, Sex, Sexuality, VitalStatus> Agents;
    public GeneralIndex<(Person, int, Date, Sex, Sexuality, VitalStatus), VitalStatus> AgentsVitalStatusIndex;
    public TablePredicate<Person, Vocation, sbyte> Aptitude;
    public TablePredicate<Person, Facet, sbyte> Personality;
    public TablePredicate<Person, Person> Couples;
    public TablePredicate<Person, Person> Parents;
    public TablePredicate<LocationType, LocationCategories, Accessibility, DailyOperation, Schedule> LocationInformation;
    public TablePredicate<LocationCategories, Color> CategoryColors;
    public TablePredicate<LocationType, Color> LocationColors;
    public KeyIndex<(LocationType, Color), LocationType> LocationColorsIndex;
    public TablePredicate<Location, Vector2Int, int, Date> Locations;
    public KeyIndex<(Location, Vector2Int, int, Date), Vector2Int> LocationsPositionIndex;
    public TablePredicate<Location, Vector2Int, int, Date> NewLocations;
    public TablePredicate<Location, Vector2Int, int, Date> VacatedLocations;
    public TablePredicate<Vector2Int> UsedLots;
    public TablePredicate<Vocation, Person, Location> Vocations;
    public TablePredicate<Person, Location> Homes;
    public TablePredicate<Person, Location> WhereTheyAt;
    public GeneralIndex<(Person, Location), Location> WhereTheyAtLocationIndex;
    public TablePredicate REPL;
    #endregion

    public void SetREPL(string query) => REPL = Simulation.Repl.Query("REPL", query);

    public void InitSimulator() {
        Simulation = new Simulation("Talk of the Town");
        _firstTick = true;

        // ReSharper disable InconsistentNaming
        #region Functions
        var GetYear = Time.GetProperty<int>(nameof(Time.Year));
        var GetDate = Time.GetProperty<Date>(nameof(Time.Date));
        var YearsSince = Method<Date, int, int>(Time.YearsSince);
        var RandomSex = Method(Sims.RandomSex);
        var RandomSexuality = Function<Sex, Sexuality>("RandomSexuality", Sexuality.Random);
        var NewPerson = Method<string, string, Person>(Sims.NewPerson);
        var RandomDate = Function("RandomDate", Date.Random);
        var RandomAdultAge = Method(Sims.RandomAdultAge);
        var FertilityRate = Method<int, float>(Sims.FertilityRate);
        var SByteBellCurve = Method(Randomize.SByteBellCurve);
        var Surname = Function<Person, string>("Surname", p => p.LastName);
        var GetLocationType = Function<Location, LocationType>("GetLocationType", l => l.Type);
        var NumLots = Function("NumLots", () => UsedLots.Length);
        var RandomLot = Method<uint, Vector2Int>(Town.RandomLot);
        var NewLocation = Method<string, LocationType, Location>(Town.NewLocation);
        var Distance = Method<Vector2Int, Vector2Int, int>(Town.Distance);
        #endregion

        #region PrimitiveTests
        var IsNotFirstTick = Test("IsNotFirstTick", () => !_firstTick);
        var IsAttracted = Test<Sexuality, Sex>("IsAttracted", (se, s) => se.IsAttracted(s));
        var InOperation = TestMethod<DailyOperation>(Time.InOperation);
        var IsOpen = TestMethod<Schedule>(Time.IsOpen);
        var IsAccessible = TestMethod<Accessibility, bool, bool>(Town.IsAccessible);
        var IsDate = TestMethod<Date>(Time.IsDate);
        var IsSunday = Time.TestProperty(nameof(Time.IsSunday));
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
        var vitalStatus      = (Var<VitalStatus>)"vitalStatus";
        var facet            = (Var<Facet>)"facet";
        var personality      = (Var<sbyte>)"personality";
        var location         = (Var<Location>)"location";
        var home             = (Var<Location>)"home";
        var position         = (Var<Vector2Int>)"position";
        var opening          = (Var<Date>)"opening";
        var founded          = (Var<int>)"founded";
        var locationType     = (Var<LocationType>)"locationType";
        var locationCategory = (Var<LocationCategories>)"locationCategory";
        var accessibility    = (Var<Accessibility>)"accessibility";
        var operation        = (Var<DailyOperation>)"operation";
        var schedule         = (Var<Schedule>)"schedule";
        var color            = (Var<Color>)"color";
        var job              = (Var<Vocation>)"job";
        var aptitude         = (Var<sbyte>)"aptitude";
        var positions        = (Var<int>)"positions";
        var employee         = (Var<Person>)"employee";
        var occupant         = (Var<Person>)"occupant";
        #endregion

        Simulation.BeginPredicates();

        // Tables, despite being local variables, will still be capitalized for style/identification purposes.
        var MaleNames = FromCsv("MaleNames", Csv("male_names"), firstName);
        var FemaleNames = FromCsv("FemaleNames", Csv("female_names"), firstName);
        var Surnames = FromCsv("Surnames", Csv("english_surnames"), lastName);

        var RandomFirstName = Definition("RandomFirstName", sex, firstName);
        RandomFirstName[Sex.Male, firstName].If(RandomElement(MaleNames, firstName));
        RandomFirstName[Sex.Female, firstName].If(RandomElement(FemaleNames, firstName));
        var RandomPerson = Definition("RandomPerson", sex, person);
        RandomPerson.Is(RandomFirstName, RandomElement(Surnames, lastName), person == NewPerson[firstName, lastName]);
        
        // ******************************************************************************
        // Agents:

        var PrimordialBeings = FromCsv("PrimordialBeings", 
            Csv("agents"), person, age, dateOfBirth, sex, sexuality);
        Agents = Predicate("Agents", person.Key, age, dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed);
        Agents.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeings);
        AgentsVitalStatusIndex = (GeneralIndex<(Person, int, Date, Sex, Sexuality, VitalStatus), VitalStatus>)Agents.IndexFor(vitalStatus, false);

        var Dead = Definition("Dead", person).Is(Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Dead]);
        var Alive = Definition("Alive", person).Is(Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive]);
        Agents.Set(person, vitalStatus, VitalStatus.Dead).If(Alive, Agents, age > 60, Prob[Time.PerMonth(0.001f)]);

        var Facets = Predicate("Facets", facet);
        Facets.AddRows(Enum.GetValues(typeof(Facet)).Cast<Facet>());
        Personality = Predicate("Personality", person.Indexed, facet.Indexed, personality);
        Personality.Initially[person, facet, SByteBellCurve].Where(PrimordialBeings, Facets);

        var Jobs = Predicate("Jobs", job);
        Jobs.AddRows(Enum.GetValues(typeof(Vocation)).Cast<Vocation>());
        Aptitude = Predicate("Aptitude", person.Indexed, job.Indexed, aptitude);
        Aptitude.Initially[person, job, SByteBellCurve].Where(PrimordialBeings, Jobs);

        var Man = Predicate("Man", person).If(
            Agents[person, age, dateOfBirth, Sex.Male, sexuality, VitalStatus.Alive], age >= 18);
        var Woman = Predicate("Woman", person).If(
            Agents[person, age, dateOfBirth, Sex.Female, sexuality, VitalStatus.Alive], age >= 18);

        Couples = Predicate("Couples", person, partner);
        Couples.Unique = true;
        var NewCouples = Predicate("NewCouples", person, partner);
        NewCouples.Unique = true;
        NewCouples.If(Woman[person], RandomElement(Man, partner), Prob[0.5f],
            !Couples[person, man], !Couples[woman, partner]);
        Couples.Accumulates(NewCouples);

        var BirthTo = Predicate("Birth", woman, man, sex, child).If(
            Couples[woman, man], sex == RandomSex, Agents[woman, age, dateOfBirth, Sex.Female, sexuality, VitalStatus.Alive],
            Prob[FertilityRate[age]], RandomFirstName, child == NewPerson[firstName, Surname[man]]);
        Parents = Predicate("Parents", parent, child);
        Parents.Add.If(BirthTo[parent, person, sex, child]);
        Parents.Add.If(BirthTo[person, parent, sex, child]);
        Agents.Add[person, -1, GetDate, sex, sexuality, VitalStatus.Alive].If(
            BirthTo[man, woman, sex, person], sexuality == RandomSexuality[sex]);
        Personality.Add[person, facet, SByteBellCurve].If(BirthTo[man, woman, sex, person], Facets);
        Aptitude.Add[person, job, SByteBellCurve].If(BirthTo[man, woman, sex, person], Jobs);

        var IsFamily = Definition("IsFamily", person, otherPerson).Is(
            Couples[person, otherPerson] | Couples[otherPerson, person] | 
            (Parents[person, otherPerson] & Agents[otherPerson, age, dateOfBirth, sex, sexuality, VitalStatus.Alive] & age <= 18) |
            (Parents[otherPerson, person] & Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive] & age <= 18));

        // ******************************************************************************
        // LOCATIONS:

        LocationInformation = FromCsv("LocationInformation", Csv("locationInformation"), 
            locationType.Key, locationCategory.Indexed, accessibility, operation, schedule);
        CategoryColors = FromCsv("CategoryColors", Csv("locationColors"), locationCategory.Key, color);
        LocationColors = Predicate("LocationColors", locationType.Key, color);
        LocationColors.Unique = true;
        LocationColors.Initially.Where(LocationInformation, CategoryColors);
        LocationColorsIndex = LocationColors.KeyIndex(locationType);
        
        var VocationLocations = FromCsv("VocationLocations", 
            Csv("vocationLocations"), job.Indexed, locationType.Indexed);
        var PositionsPerJob = FromCsv("PositionsPerJob", 
            Csv("positionsPerJob"), job.Key, positions);

        Locations = FromCsv("Locations", Csv("locations"), location.Key, position.Key, founded, opening);
        LocationsPositionIndex = Locations.KeyIndex(position);
        NewLocations = Predicate("NewLocations", location, position, founded, opening);
        VacatedLocations = Predicate("VacatedLocations", location, position, founded, opening);  // UNUSED
        UsedLots = Predicate("UsedLots", position);
        UsedLots.Unique = true;

        var IsVacant = Definition("IsVacant", position).Is(!UsedLots[position]);
        var FreeLot = Definition("FreeLot", position).Is(position == RandomLot[NumLots], IsVacant[position]);
        NewLocations[location, position, GetYear, GetDate].If(FreeLot,
            // , Count(Homes) <= Count(Agents)
            location == NewLocation["Test", LocationType.House], Prob[Time.PerWeek(0.8f)], IsNotFirstTick);
        Locations.Accumulates(NewLocations);
        UsedLots.If(Locations);
        var AllPlaces = Predicate("AllPlaces", location).If(Locations);

        Vocations = Predicate("Vocations", job, employee, location);
        var BestForJob = Definition("BestForJob", person, job).Is(Maximal(person, aptitude, Aptitude));
        var OnShift = Predicate("OnShift", person, job, location);
        var OpenForBusiness = Predicate("OpenForBusiness", location).If(Locations,
            locationType == GetLocationType[location], LocationInformation, InOperation[operation], IsOpen[schedule]);

        Homes = Predicate("Homes", occupant.Key, location.Indexed);
        Homes.Unique = true;
        Homes.Initially.Where(PrimordialBeings[occupant, age, dateOfBirth, sex, sexuality], Locations, IsLocationType[location, LocationType.House]);
        // | !Homes[person, location] here means babies fall back on moving into an empty house if their parents are homeless...?
        Homes.Add.If(BirthTo[man, woman, sex, occupant], Locations, IsLocationType[location, LocationType.House],
            (Homes[person, location] & IsFamily[person, occupant]) | !Homes[person, location]);

        WhereTheyAt = Predicate("WhereTheyAt", person.Key, location.Indexed);
        WhereTheyAtLocationIndex = (GeneralIndex<(Person, Location), Location>)WhereTheyAt.IndexFor(location, false);
        var Homebodies = Predicate("Homebodies", person);
        Homebodies.If(Agents, Alive);
        WhereTheyAt.If(Homebodies, RandomElement(AllPlaces, location));

        // ReSharper restore InconsistentNaming

        Simulation.EndPredicates();
        DataflowVisualizer.MakeGraph(Simulation, "TotT.dot");
        Simulation.Update();
        _firstTick = false;
        Time.Tick();
    }

    public void UpdateSimulator() {
        Simulation.Update();
        UpdateRows();
        Time.Tick();
    }

    public void UpdateRows() {
        // Age a person on the first tick of a given date
        if (Time.IsAM) {
            Agents.UpdateRows((ref (Person _, int age, Date dateOfBirth, Sex __, Sexuality ___, VitalStatus ____) agent) => {
                if (Time.IsDate(agent.dateOfBirth) && agent.age >= 0) agent.age++;
            });
        }
        // However, if they were born on today's date (as denoted with the age -1), set the age to 0 in the last tick of the given date
        else if (Time.IsPM) {
            Agents.UpdateRows((ref (Person _, int age, Date dateOfBirth, Sex __, Sexuality ___, VitalStatus ____) agent) => {
                if (Time.IsDate(agent.dateOfBirth) && agent.age < 0) agent.age = 0;
            });
        }
    }
}