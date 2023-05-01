using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using TED.Tables;
using TED.Utilities;
using UnityEngine;
using static TED.Language;

using AgentRow = System.ValueTuple<Person, int, Date, Sex, Sexuality, VitalStatus>;
using ColorUtility = UnityEngine.ColorUtility;

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
    public GeneralIndex<AgentRow, VitalStatus> AgentsVitalStatusIndex;
    public TablePredicate<Person, Vocation, sbyte> Aptitude;
    public TablePredicate<Person, Facet, sbyte> Personality;
    public TablePredicate<Person, Person> Couples;
    public TablePredicate<Person, Person> Parents;
    public TablePredicate<LocationType, LocationCategories, Accessibility, DailyOperation, Schedule> LocationInformation;
    public TablePredicate<LocationCategories, Color> CategoryColors;
    public TablePredicate<LocationType, Color> LocationColors;
    public KeyIndex<(LocationType, Color), LocationType> LocationColorsIndex;
    public TablePredicate<Location, Vector2Int, int, Date> PrimordialLocations;
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
        _firstTick = true;  // only used to prevent new buildings on the first tick as of now

        // ReSharper disable InconsistentNaming
        // variables just so happen to follow c# var name norms, still disabling InconsistentNaming because
        // Tables, despite being local variables, will still be capitalized for style/identification purposes.

        #region Functions/PrimitiveTests
        var SByteBellCurve = Method(Randomize.SByteBellCurve);
        var GetYear = Time.GetProperty<int>(nameof(Time.Year));
        var GetDate = Time.GetProperty<Date>(nameof(Time.Date));
        var IsLocationType = TestMethod<Location, LocationType>(Town.IsLocationType);
        var YearsSince = Method<Date, int, int>(Time.YearsSince);
        var Distance = Method<Vector2Int, Vector2Int, int>(Town.Distance);
        var IsDate = TestMethod<Date>(Time.IsDate);
        var IsSunday = Time.TestProperty(nameof(Time.IsSunday));
        #endregion

        #region Variables
        var person           = (Var<Person>)"person";
        var partner          = (Var<Person>)"partner";
        var parent           = (Var<Person>)"parent";
        var child            = (Var<Person>)"child";
        var man              = (Var<Person>)"man";
        var woman            = (Var<Person>)"woman";
        var employee         = (Var<Person>)"employee";
        var occupant         = (Var<Person>)"occupant";
        var otherPerson      = (Var<Person>)"otherPerson";
        var firstName        = (Var<string>)"firstName";
        var lastName         = (Var<string>)"lastName";

        var sex              = (Var<Sex>)"sex";
        var sexOfPartner     = (Var<Sex>)"sexOfPartner";
        var sexuality        = (Var<Sexuality>)"sexuality";
        var sexualOfPartner  = (Var<Sexuality>)"sexualityOfPartner";
        var vitalStatus      = (Var<VitalStatus>)"vitalStatus";
        var facet            = (Var<Facet>)"facet";
        var personality      = (Var<sbyte>)"personality";
        var job              = (Var<Vocation>)"job";
        var aptitude         = (Var<sbyte>)"aptitude";

        var age              = (Var<int>)"age";
        var founded          = (Var<int>)"founded";
        var dateOfBirth      = (Var<Date>)"dateOfBirth";
        var opening          = (Var<Date>)"opening";

        var location         = (Var<Location>)"location";
        var position         = (Var<Vector2Int>)"position";
        var locationType     = (Var<LocationType>)"locationType";
        var locationCategory = (Var<LocationCategories>)"locationCategory";
        var accessibility    = (Var<Accessibility>)"accessibility";
        var operation        = (Var<DailyOperation>)"operation";
        var schedule         = (Var<Schedule>)"schedule";
        var color            = (Var<Color>)"color";
        var positions        = (Var<int>)"positions";
        #endregion

        Simulation.BeginPredicates();

        // *********************************** Agents: **********************************

        #region Names and new Person helpers
        var MaleNames = FromCsv("MaleNames", Csv("male_names"), firstName);
        var FemaleNames = FromCsv("FemaleNames", Csv("female_names"), firstName);
        var Surnames = FromCsv("Surnames", Csv("english_surnames"), lastName);

        var RandomFirstName = Definition("RandomFirstName", sex, firstName);
        RandomFirstName[Sex.Male, firstName].If(RandomElement(MaleNames, firstName));
        RandomFirstName[Sex.Female, firstName].If(RandomElement(FemaleNames, firstName));

        var NewPerson = Method<string, string, Person>(Sims.NewPerson);
        var RandomPerson = Definition("RandomPerson", sex, person);
        RandomPerson.Is(RandomFirstName, RandomElement(Surnames, lastName), person == NewPerson[firstName, lastName]);
        #endregion

        #region Person traits - Personality and Aptitude
        var Facets = Predicate("Facets", facet);
        Facets.AddRows(Enum.GetValues(typeof(Facet)).Cast<Facet>());
        Personality = Predicate("Personality", person.Indexed, facet.Indexed, personality);

        var Jobs = Predicate("Jobs", job);
        Jobs.AddRows(Enum.GetValues(typeof(Vocation)).Cast<Vocation>());
        Aptitude = Predicate("Aptitude", person.Indexed, job.Indexed, aptitude);
        #endregion

        #region Agents setup and Death (Set) logic
        Agents = Predicate("Agents", person.Key, age, dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed);
        AgentsVitalStatusIndex = (GeneralIndex<AgentRow, VitalStatus>)Agents.IndexFor(vitalStatus, false);
        // Dead and Alive definitions:
        var Dead = Definition("Dead", person).Is(Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Dead]);
        var Alive = Definition("Alive", person).Is(Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive]);
        // Set Dead condition:
        Agents.Set(person, vitalStatus, VitalStatus.Dead).If(Alive, Agents, age > 60, Prob[Time.PerMonth(0.01f)]);
        #endregion

        #region Primordial Beings initialize
        var PrimordialBeings = FromCsv("PrimordialBeings",
            Csv("agents"), person, age, dateOfBirth, sex, sexuality);

        Agents.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeings);
        Personality.Initially[person, facet, SByteBellCurve].Where(PrimordialBeings, Facets);
        Aptitude.Initially[person, job, SByteBellCurve].Where(PrimordialBeings, Jobs);
        #endregion

        // TODO : implement primordial couples
        #region Couples (for procreation)
        var Men = Predicate("Men", person).If(
            Agents[person, age, dateOfBirth, Sex.Male, sexuality, VitalStatus.Alive], age >= 18);
        var Women = Predicate("Women", person).If(
            Agents[person, age, dateOfBirth, Sex.Female, sexuality, VitalStatus.Alive], age >= 18);

        Couples = Predicate("Couples", person, partner);
        Couples.Unique = true;
        var NewCouples = Predicate("NewCouples", person, partner);
        NewCouples.Unique = true;

        // TODO: Improve NewCouple logic. Use IsAttracted... Unless you want to have couples having kids with differing attractions
        var IsAttracted = Test<Sexuality, Sex>("IsAttracted", (se, s) => se.IsAttracted(s));

        NewCouples.If(Women[person], RandomElement(Men, partner), Prob[0.5f], !Couples[person, man], !Couples[woman, partner]);
        Couples.Accumulates(NewCouples);
        #endregion

        #region Birth
        var FertilityRate = Method<int, float>(Sims.FertilityRate);
        var RandomSex = Method(Sims.RandomSex);
        // Surname here is only being used to facilitate A naming convention for last names (currently paternal lineage)
        var Surname = Function<Person, string>("Surname", p => p.LastName);
        var BirthTo = Predicate("Birth", woman, man, sex, child).If(
            Couples[woman, man], sex == RandomSex, Agents[woman, age, dateOfBirth, Sex.Female, sexuality, VitalStatus.Alive],
            Prob[FertilityRate[age]], RandomFirstName, child == NewPerson[firstName, Surname[man]]);

        // BirthTo has a column for the sex of the child to facilitate gendered naming, however, since there is no need to
        // determine the child's sexuality in BirthTo, a child has the sexuality established when they are added to Agents
        var RandomSexuality = Function<Sex, Sexuality>("RandomSexuality", Sexuality.Random);
        Agents.Add[person, -1, GetDate, sex, sexuality, VitalStatus.Alive].If(
            BirthTo[man, woman, sex, person], sexuality == RandomSexuality[sex]);

        // And add anything else that is needed for a new agent:
        Personality.Add[person, facet, SByteBellCurve].If(BirthTo[man, woman, sex, person], Facets);
        Aptitude.Add[person, job, SByteBellCurve].If(BirthTo[man, woman, sex, person], Jobs);
        #endregion

        #region Family
        Parents = Predicate("Parents", parent, child);
        Parents.Add.If(BirthTo[parent, person, sex, child]);
        Parents.Add.If(BirthTo[person, parent, sex, child]);

        var IsFamily = Definition("IsFamily", person, otherPerson).Is(
            Couples[person, otherPerson] | Couples[otherPerson, person] |
            (Parents[person, otherPerson] & Agents[otherPerson, age, dateOfBirth, sex, sexuality, VitalStatus.Alive] & age <= 18) |
            (Parents[otherPerson, person] & Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive] & age <= 18));
        #endregion

        #region Drifters - adults moving to town
        // Using a Definition to wrap RandomPerson and RandomSexuality
        var RandomDate = Function("RandomDate", Date.Random);
        var RandomAdultAge = Method(Sims.RandomAdultAge);
        Agents.Add[person, RandomAdultAge, RandomDate, RandomSex, sexuality, VitalStatus.Alive]
            .If(Prob[Time.PerYear(0.00001f)],
                RandomPerson, sexuality == RandomSexuality[sex]);
        #endregion

        // ********************************* Locations: *********************************

        #region Location Information Tables:
        // Namely used for figuring out where a character can go (time and privacy based)
        LocationInformation = FromCsv("LocationInformation", Csv("locationInformation"),
            locationType.Key, locationCategory.Indexed, accessibility, operation, schedule);
        var VocationLocations = FromCsv("VocationLocations",
            Csv("vocationLocations"), job.Indexed, locationType.Indexed);
        var PositionsPerJob = FromCsv("PositionsPerJob",
            Csv("positionsPerJob"), job.Key, positions);

        // Location Colors:
        CategoryColors = FromCsv("CategoryColors", Csv("locationColors"), locationCategory.Key, color);
        LocationColors = Predicate("LocationColors", locationType.Key, color);
        LocationColors.Unique = true;
        LocationColors.Initially.Where(LocationInformation, CategoryColors);
        LocationColorsIndex = LocationColors.KeyIndex(locationType);
        #endregion

        #region Location Tables:
        // These tables are used for adding and removing tiles from the tilemap in unity efficiently -
        // by not having to check over all Locations. VacatedLocations is currently UNUSED
        PrimordialLocations = FromCsv("PrimordialLocations", Csv("locations"), 
            location, position, founded, opening);
        NewLocations = Predicate("NewLocations", location, position, founded, opening);
        VacatedLocations = Predicate("VacatedLocations", location, position, founded, opening);

        // This is how Locations gets built from the above tables (relational connections)
        Locations = Predicate("Locations", location.Key, position.Key, founded, opening);
        Locations.Initially.Where(PrimordialLocations);
        Locations.Accumulates(NewLocations);
        LocationsPositionIndex = Locations.KeyIndex(position);
        #endregion

        #region Housing:
        Homes = Predicate("Homes", occupant.Key, location.Indexed);
        Homes.Unique = true;
        // Using this to randomly assign one house per person...
        var PrimordialHouses = Predicate("PrimordialHouses", location)
            .If(PrimordialLocations, IsLocationType[location, LocationType.House]);
        // Ideally this would involve some more complex assignment logic that would fill houses based on some Goal
        // e.g. Initialize Homes first based on primordial couples, then on all single agents
        Homes.Initially.Where(PrimordialBeings[occupant, age, dateOfBirth, sex, sexuality], RandomElement(PrimordialHouses, location));
        Homes.Add.If(BirthTo[man, woman, sex, occupant], Homes[woman, location]); // Move in with mom
        #endregion

        #region New Location Logic:
        // UsedLots table lets us keep track of where locations are, feeds into the functions below:
        UsedLots = Predicate("UsedLots", position);
        UsedLots.Unique = true;
        UsedLots.If(Locations);

        // Helper functions and definitions for creating new locations at a valid lot in town
        var RandomLot = Method<uint, Vector2Int>(Town.RandomLot);
        var NumLots = Length("NumLots", UsedLots);
        var IsVacant = Definition("IsVacant", position).Is(!UsedLots[position]);
        var FreeLot = Definition("FreeLot", position).Is(position == RandomLot[NumLots], IsVacant[position]);
        var NewLocation = Method<string, LocationType, Location>(Town.NewLocation);

        // IsNotFirstTick is meant to prevent the addition of locations on the first tick as tiles are only
        // added to the tilemap from PrimordialLocations on the first tick... Also to prevent attempting to bind
        // with Count(Homes) before Homes is initialized - not a problem if Homes comes before this
        var IsNotFirstTick = Test("IsNotFirstTick", () => !_firstTick);

        // Location creation logic: (new houses as of now - and this should only happen if a drifter comes in)
        NewLocations[location, position, GetYear, GetDate].If(IsNotFirstTick,
            Count(Homes[person, location] & Alive[person]) < Count(Alive),
            Prob[Time.PerWeek(0.5f)],  // Also, construction isn't instantaneous
            FreeLot, location == NewLocation["Test", LocationType.House]);
        #endregion

        // TODO : Give people jobs (need to start adding more location types in the NewLocation logic)
        #region Vocations:
        Vocations = Predicate("Vocations", job, employee, location);
        var BestForJob = Definition("BestForJob", person, job).Is(Maximal(person, aptitude, Aptitude));
        var OnShift = Predicate("OnShift", person, job, location);
        #endregion

        // ********************************** Movement: *********************************

        // TODO : Incorporate Distance from housing...
        #region Daily Movements
        WhereTheyAt = Predicate("WhereTheyAt", person.Key, location.Indexed);
        WhereTheyAtLocationIndex = (GeneralIndex<(Person, Location), Location>)WhereTheyAt.IndexFor(location, false);

        var GetLocationType = Function<Location, LocationType>("GetLocationType", l => l.Type);
        var InOperation = TestMethod<DailyOperation>(Time.InOperation);
        var IsOpen = TestMethod<Schedule>(Time.IsOpen);
        var OpenForBusiness = Predicate("OpenForBusiness", location).If(Locations,
            locationType == GetLocationType[location], LocationInformation, InOperation[operation], IsOpen[schedule]);

        var Accessible = Definition("Accessible", person, location);
        Accessible[person, location].If(locationType == GetLocationType[location],  // public is always true...
            LocationInformation[locationType, locationCategory, Accessibility.Public, operation, schedule],
            Alive); // Only need alive to make sure rules are fully bound
        Accessible[person, location].If(locationType == GetLocationType[location], // private needs to live there OR be 'invited'
            LocationInformation[locationType, locationCategory, Accessibility.Private, operation, schedule],
            Homes[person, location] | (Homes[occupant, location] & IsFamily[person, occupant]));
        Accessible[person, location].If(locationType == GetLocationType[location], // NoTrespass related to employment
            LocationInformation[locationType, locationCategory, Accessibility.NoTrespass, operation, schedule],
            OnShift);
        
        WhereTheyAt.If(Alive, RandomElement(OpenForBusiness, location), Accessible);
        #endregion

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