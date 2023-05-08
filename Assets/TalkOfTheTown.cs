using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TED;
using TED.Interpreter;
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

    #region Tables and Indexers - for GUI
    public TablePredicate<Person, int, Date, Sex, Sexuality, VitalStatus> Agents;
    public TablePredicate<Location, LocationType, Vector2Int, int, Date> PrimordialLocations;
    public TablePredicate<Location, LocationType, Vector2Int, int, Date> Locations;
    public TablePredicate<Location, LocationType, Vector2Int, int, Date> NewLocations;
    public TablePredicate<Location, LocationType, Vector2Int, int, Date> VacatedLocations;
    public TablePredicate<LocationType, LocationCategories, DailyOperation, Schedule> LocationInformation;
    public TablePredicate<LocationType, Color> LocationColors;
    public GeneralIndex<AgentRow, VitalStatus> AgentsVitalStatusIndex;
    public KeyIndex<(LocationType, Color), LocationType> LocationColorsIndex;
    public KeyIndex<(Location, LocationType, Vector2Int, int, Date), Vector2Int> LocationsPositionIndex;
    public GeneralIndex<(Person, ActionType, Location), Location> WhereTheyAtLocationIndex;
    #endregion

    public void InitSimulator() {
        Simulation = new Simulation("Talk of the Town");
        _firstTick = true;  // only used to prevent new buildings on the first tick as of now

        // ReSharper disable InconsistentNaming
        // variables just so happen to follow c# var name norms, still disabling InconsistentNaming because
        // Tables, despite being local variables, will still be capitalized for style/identification purposes.

        #region Functions/PrimitiveTests
        var SByteBellCurve = Method(Randomize.SByteBellCurve);
        var Distance = Method<Vector2Int, Vector2Int, int>(Town.Distance);
        var GetYear = Time.GetProperty<int>(nameof(Time.Year));
        var GetDate = Time.GetProperty<Date>(nameof(Time.Date));
        var GetTimeOfDay = Time.GetProperty<TimeOfDay>(nameof(Time.TimeOfDay));
        var IsSunday = Time.TestProperty(nameof(Time.IsSunday));
        var IsDate = TestMethod<Date>(Time.IsDate);
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
        var otherJob         = (Var<Vocation>)"otherJob";
        var aptitude         = (Var<sbyte>)"aptitude";

        var age              = (Var<int>)"age";
        var founded          = (Var<int>)"founded";
        var founded2         = (Var<int>)"founded2";
        var dateOfBirth      = (Var<Date>)"dateOfBirth";
        var opening          = (Var<Date>)"opening";
        var opening2         = (Var<Date>)"opening2";

        var location         = (Var<Location>)"location";
        var otherLocation    = (Var<Location>)"otherLocation";
        var position         = (Var<Vector2Int>)"position";
        var otherPosition    = (Var<Vector2Int>)"otherPosition";
        var locationType     = (Var<LocationType>)"locationType";
        var otherLocType     = (Var<LocationType>)"otherLocType";
        var locationCategory = (Var<LocationCategories>)"locationCategory";
        var operation        = (Var<DailyOperation>)"operation";
        var timeOfDay        = (Var<TimeOfDay>)"timeOfDay";
        var schedule         = (Var<Schedule>)"schedule";
        var color            = (Var<Color>)"color";
        var positions        = (Var<int>)"positions";
        var locationName     = (Var<string>)"locationName";
        
        var count            = (Var<int>)"count";

        var actionType       = (Var<ActionType>)"actionType";
        var distance         = (Var<int>)"distance";
        #endregion

        Simulation.BeginPredicates();

        // *********************************** Agents: **********************************

        #region Names and new Person helpers:
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

        #region Person traits - Personality and Aptitude:
        var Facets = Predicate("Facets", facet);
        Facets.AddRows(Enum.GetValues(typeof(Facet)).Cast<Facet>());
        var Personality = Predicate("Personality", person.Indexed, facet.Indexed, personality);

        var Jobs = Predicate("Jobs", job);
        Jobs.AddRows(Enum.GetValues(typeof(Vocation)).Cast<Vocation>());
        var Aptitude = Predicate("Aptitude", person.Indexed, job.Indexed, aptitude);
        #endregion

        // TODO : Use set to age agents, might need function that uses KeyIndex to get age and increment
        #region Agents setup and Death (Set) logic:
        Agents = Predicate("Agents", person.Key, age, dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed);
        AgentsVitalStatusIndex = (GeneralIndex<AgentRow, VitalStatus>)Agents.IndexFor(vitalStatus, false);

        // Dead and Alive definitions:
        var Dead = Definition("Dead", person)
            .Is(Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Dead]);
        var Alive = Definition("Alive", person)
            .Is(Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive]);
        // Set Dead condition:
        Agents.Set(person, vitalStatus, VitalStatus.Dead)
            .If(Alive, Agents, age > 60, Prob[Time.PerMonth(0.01f)]);

        // Agent helper definitions
        var Age = Definition("Age", person, age)
            .Is(Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive]);
        #endregion

        #region Primordial Beings initialize:
        var PrimordialBeings = FromCsv("PrimordialBeings",
            Csv("agents"), person, age, dateOfBirth, sex, sexuality);

        Agents.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeings);
        Personality.Initially[person, facet, SByteBellCurve].Where(PrimordialBeings, Facets);
        Aptitude.Initially[person, job, SByteBellCurve].Where(PrimordialBeings, Jobs);
        #endregion

        // TODO : Implement primordial couples
        // TODO : Improve NewCouple logic. Use IsAttracted.
        #region Couples (for procreation):
        var Men = Predicate("Men", person).If(
            Agents[person, age, dateOfBirth, Sex.Male, sexuality, VitalStatus.Alive], age >= 18);
        var Women = Predicate("Women", person).If(
            Agents[person, age, dateOfBirth, Sex.Female, sexuality, VitalStatus.Alive], age >= 18);

        var Couples = Predicate("Couples", person, partner);
        Couples.Unique = true;
        var NewCouples = Predicate("NewCouples", person, partner);
        NewCouples.Unique = true;

        var IsAttracted = Test<Sexuality, Sex>("IsAttracted", (se, s) => se.IsAttracted(s));

        NewCouples.If(Women[person], RandomElement(Men, partner), Prob[0.5f], !Couples[person, man], !Couples[woman, partner]);
        Couples.Accumulates(NewCouples);
        #endregion

        #region Birth:
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

        #region Family:
        var Parents = Predicate("Parents", parent, child);
        Parents.Add.If(BirthTo[parent, person, sex, child]);
        Parents.Add.If(BirthTo[person, parent, sex, child]);

        var IsFamily = Definition("IsFamily", person, otherPerson).Is(
            Couples[person, otherPerson] | Couples[otherPerson, person] |
            (Parents[person, otherPerson] & Agents[otherPerson, age, dateOfBirth, sex, sexuality, VitalStatus.Alive] & age <= 18) |
            (Parents[otherPerson, person] & Agents[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive] & age <= 18));
        #endregion

        #region Drifters - adults moving to town:
        // Using a Definition to wrap RandomPerson and RandomSexuality
        var RandomDate = Function("RandomDate", Date.Random);
        var RandomAdultAge = Method(Sims.RandomAdultAge);
        Agents.Add[person, RandomAdultAge, RandomDate, RandomSex, sexuality, VitalStatus.Alive]
            .If(Prob[Time.PerYear(0.001f)],
                RandomPerson, sexuality == RandomSexuality[sex]);
        #endregion

        // ********************************* Locations: *********************************

        #region Location Information Tables:
        // Namely used for figuring out where a character can go (time and privacy based)
        LocationInformation = FromCsv("LocationInformation", Csv("locationInformation"),
            locationType.Key, locationCategory.Indexed, operation, schedule);

        // Location Colors:
        var CategoryColors = FromCsv("CategoryColors", 
            Csv("locationColors"), locationCategory.Key, color);
        LocationColors = Predicate("LocationColors", locationType.Key, color);
        LocationColors.Unique = true;
        LocationColors.Initially.Where(LocationInformation, CategoryColors);
        LocationColorsIndex = LocationColors.KeyIndex(locationType);
        #endregion

        #region Location Tables:
        // These tables are used for adding and removing tiles from the tilemap in unity efficiently -
        // by not having to check over all Locations. VacatedLocations is currently UNUSED
        PrimordialLocations = FromCsv("PrimordialLocations", Csv("locations"), 
            location, locationType, position, founded, opening);
        NewLocations = Predicate("NewLocations", location, locationType, position, founded, opening);
        VacatedLocations = Predicate("VacatedLocations", location, locationType, position, founded, opening);

        // This is how Locations gets built from the above tables (relational connections)
        Locations = Predicate("Locations", location.Key, locationType.Indexed, position.Key, founded, opening);
        Locations.Initially.Where(PrimordialLocations);
        Locations.Accumulates(NewLocations);
        LocationsPositionIndex = Locations.KeyIndex(position);

        // Locations Information tables and definitions:
        var LocationsOfCategory = Predicate("LocationsOfCategory", 
            location, locationCategory).If(LocationInformation, Locations);
        var AnyInCategory = Definition("AnyInCategory", locationCategory)
            .Is(!!LocationsOfCategory[location, locationCategory]); // same as Count(X) > 0 ?
        #endregion

        // TODO : Include ApartmentComplex locations in Housing logic
        #region Housing:
        var Homes = Predicate("Homes", occupant.Key, location.Indexed);
        Homes.Unique = true;

        // Using this to randomly assign one house per person...
        var PrimordialHouses = Predicate("PrimordialHouses", location)
            .If(PrimordialLocations[location, LocationType.House, position, founded, opening]);
        // Ideally this would involve some more complex assignment logic that would fill houses based on some Goal
        // e.g. Initialize Homes first based on primordial couples, then on all single agents
        Homes.Initially.Where(PrimordialBeings[occupant, age, dateOfBirth, sex, sexuality], RandomElement(PrimordialHouses, location));
        Homes.Add.If(BirthTo[man, woman, sex, occupant], Homes[woman, location]); // Move in with mom

        // Distance per person makes most sense when measured from either where the person is,
        // or where they live. This handles the latter:
        var DistanceFromHome = Definition("DistanceFromHome", person, location, distance)
            .Is(Locations[location, locationType, position, founded, opening], 
                Homes[person, otherLocation],
                Locations[otherLocation, otherLocType, otherPosition, founded2, opening2], 
                distance == Distance[position, otherPosition]);
        #endregion

        #region Moving houses:
        var Occupancy = Predicate("Occupancy", location, count)
            .If(Locations[location, LocationType.House, position, founded, opening], count == Count(Homes));
        var UnderOccupied = Predicate("UnderOccupied", location).If(Occupancy, count < 5);
        var WantToMove = Predicate("WantToMove", person).If(Homes[person, location], Occupancy, count > 5);

        var MovingIn = Predicate("MovingIn", person, location).If(!!WantToMove[person], 
            RandomElement(WantToMove, person), RandomElement(UnderOccupied, location));

        Homes.Set(occupant, location).If(MovingIn[occupant, location]);
        #endregion

        #region New Location helper functions and definitions:
        // Title case string and make a Location object
        var NewLocation = Method<string, Location>(Town.NewLocation);
        
        // Helper functions and definitions for creating new locations at a valid lot in town
        var RandomLot = Method<uint, Vector2Int>(Town.RandomLot);
        var NumLots = Length("NumLots", Locations);
        var IsVacant = Definition("IsVacant", position)
            .Is(!Locations[location, locationType, position, founded, opening]);
        var FreeLot = Definition("FreeLot", position)
            .Is(position == RandomLot[NumLots], IsVacant[position]);

        // IsNotFirstTick is meant to prevent the addition of locations on the first tick as tiles are only
        // added to the tilemap from PrimordialLocations on the first tick... Also to prevent attempting to bind
        // with Count(Homes) before Homes is initialized - not a problem if Homes comes before this
        var IsNotFirstTick = Test("IsNotFirstTick", () => !_firstTick);
        #endregion

        // TODO : Add more new locations for each location type
        #region New Location Logic:
        // Base case - useful mainly for testing/rapid development (you only need one string/generating a list of names can come second)
        void AddNewNamedLocation(LocationType locType, string name, Goal readyToAdd) =>
            NewLocations[location, locType, position, GetYear, GetDate].If(IsNotFirstTick,
                FreeLot, Prob[Time.PerWeek(0.5f)], // Needs the random lot to be available & 'construction' isn't instantaneous
                readyToAdd, location == NewLocation[name]); // otherwise, check the readyToAdd Goal and if it passes add a NewLocation

        // If you are only planning on adding a single location of the given type, this adds the check that
        // a location of locType doesn't already exist.
        void AddOneLocation(LocationType locType, string name, Goal readyToAdd) => AddNewNamedLocation(locType, name, 
            !Locations[otherLocation, locType, otherPosition, founded2, opening2] & readyToAdd);

        // This is the more realistic use case with a list of names for a give type to choose from.
        void AddNewLocation(LocationType locType, TablePredicate<string> names, Goal readyToAdd) =>
            NewLocations[location, locType, position, GetYear, GetDate].If(IsNotFirstTick,
                FreeLot, Prob[Time.PerWeek(0.5f)], readyToAdd, 
                RandomElement(names, locationName), location == NewLocation[locationName]);

        AddNewNamedLocation(LocationType.House, "Tumbleweed Ranch", // currently this only happens with drifters - everyone starts housed
            Count(Homes[person, location] & Alive[person]) < Count(Alive)); // need more house names...

        AddNewNamedLocation(LocationType.House, "Ian's place", !!WantToMove[person]); // need more house names...

        // TODO : enhance Goal writing - more default support/comma unpacking
        AddOneLocation(LocationType.Hospital, "St. Asmodeus",
            !!(Aptitude[person, Vocation.Doctor, aptitude] & (aptitude > 15) & Age & (age > 21)));

        AddOneLocation(LocationType.DayCare, "Pumpkin Preschool",
            Count(Age & (age < 6)) > 5);

        AddOneLocation(LocationType.School, "Talk of the Township High",
            Count(Age & (age >= 5) & (age < 18)) > 5);
        #endregion

        // ********************************* Vocations: *********************************

        #region Vocation Info:
        var VocationLocations = FromCsv("VocationLocations",
            Csv("vocationLocations"), job.Indexed, locationType.Indexed);
        var PositionsPerJob = FromCsv("PositionsPerJob", // positions is per time of day
            Csv("positionsPerJob"), job.Key, positions);
        var OperatingTimes = FromCsv("OperatingTimes", 
            Csv("operatingTimes"), timeOfDay, operation);

        var VocationShifts = Predicate("VocationShifts", locationType.Indexed, job.Indexed, timeOfDay)
            .Initially.Where(VocationLocations, LocationInformation, OperatingTimes);
        #endregion

        #region Vocations:
        var Vocations = Predicate("Vocations", job, employee, location, timeOfDay);

        var JobsToFill = Predicate("JobsToFill", location, job)
            .If(timeOfDay == GetTimeOfDay, Locations, VocationShifts,
                PositionsPerJob, Count(Vocations) < positions);

        var Candidates = Predicate("Candidates", person, job, location)
            .If(JobsToFill, Maximal(person, aptitude, 
                Alive[person] & !Vocations[otherJob, person, otherLocation, timeOfDay] & Age & (age > 18) & Aptitude));

        Vocations.Add[job, person, location, GetTimeOfDay].If(IsNotFirstTick, Candidates);
        #endregion

        // ********************************** Movement: *********************************

        #region Action Info:
        var ActionToCategory = FromCsv("ActionToCategory",
            Csv("actionCategories"), actionType, locationCategory);
        var AvailableActions = Predicate("AvailableActions", actionType)
            .If(ActionToCategory, AnyInCategory);
        #endregion

        #region Operation and Open logic:
        var InOperation = TestMethod<DailyOperation>(Time.InOperation);
        var IsOpen = TestMethod<Schedule>(Time.IsOpen);
        var OpenForBusiness = Predicate("OpenForBusiness", location).If(Locations,
            LocationInformation, InOperation[operation], IsOpen[schedule]);
        var OpenForBusinessByAction = Predicate("OpenForBusinessByAction", actionType, location)
            .If(ActionToCategory, LocationsOfCategory, OpenForBusiness);
        #endregion

        #region Schooling:
        var Kids = Predicate("Kids", person).If(Alive, Age, age < 18);
        var NeedsSchooling = Predicate("NeedsSchooling", person).If(Kids, Age, age > 6);
        var NeedsDayCare = Predicate("NeedsDayCare", person).If(Kids, !NeedsSchooling[person]);

        var GoingToSchool = Predicate("GoingToSchool", person, location).If(
            AvailableActions[ActionType.GoingToSchool], OpenForBusiness,
            Locations[location, LocationType.School, position, founded, opening], // only expecting one location...
            NeedsSchooling);
        var GoingToDayCare = Predicate("GoingToDayCare", person, location).If(
            AvailableActions[ActionType.GoingToSchool], OpenForBusiness,
            Locations[location, LocationType.DayCare, position, founded, opening], // only expecting one location...
            NeedsDayCare);
        #endregion

        #region Working:
        var GoingToWork = Predicate("GoingToWork", person, location)
            .If(Vocations[job, person, location, GetTimeOfDay], OpenForBusiness);
        #endregion

        // TODO : Couple movements
        // TODO : Babies not in daycare follow mom
        #region Daily Movements:
        var WhereTheyAt = Predicate("WhereTheyAt", person.Key, actionType, location.Indexed);
        WhereTheyAt.Unique = true;
        WhereTheyAtLocationIndex = (GeneralIndex<(Person, ActionType, Location), Location>)WhereTheyAt.IndexFor(location, false);

        var AdultActions = Predicate("AdultActions", actionType)
            .If(AvailableActions, actionType != ActionType.GoingToSchool);
        var NeedsActionAssignment = Predicate("NeedsActionAssignment", person).If(Alive,
            !GoingToWork[person, location],
            !GoingToDayCare[person, location],
            !GoingToSchool[person, location]);
        var RandomActionAssign = Predicate("RandomActionAssign", person, actionType).If(NeedsActionAssignment,
            RandomElement(AdultActions, actionType));

        var LocationByActionAssign = Predicate("LocationByActionAssign", person, location);
        LocationByActionAssign.If(RandomActionAssign[person, ActionType.StayingIn], Homes[person, location]);
        LocationByActionAssign.If(RandomActionAssign[person, ActionType.Visiting], Homes[person, location]);
        LocationByActionAssign.If(RandomActionAssign, actionType != ActionType.StayingIn, actionType != ActionType.Visiting,
            Minimal(location, distance, OpenForBusinessByAction & DistanceFromHome[person, location, distance]));

        WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToSchool);
        WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToDayCare);
        WhereTheyAt[person, ActionType.GoingToWork, location].If(GoingToWork);
        WhereTheyAt.If(RandomActionAssign, LocationByActionAssign);
        #endregion

        // ReSharper restore InconsistentNaming
        Simulation.EndPredicates();
        DataflowVisualizer.MakeGraph(Simulation, "TotT.dot");
        Simulation.Update();
        _firstTick = false;
    }

    public void UpdateSimulator() {
        Time.Tick();
        Simulation.Update();
        UpdateRows();
    }

    public void UpdateRows() {
        // Age a person on the first tick of a given date
        if (Time.IsAM) {
            Agents.UpdateRows((ref (Person _, int age, Date dateOfBirth, 
                Sex __, Sexuality ___, VitalStatus ____) agent) => {
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