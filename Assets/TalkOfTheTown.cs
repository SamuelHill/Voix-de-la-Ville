using System;
using System.Collections.Generic;
using System.Linq;
using TED;
using TED.Interpreter;
using TED.Tables;
using TED.Utilities;
using UnityEngine;
using static TED.Language;

using AgentRow = System.ValueTuple<Person, int, Date, Sex, Sexuality, VitalStatus>;
using BirthRow = System.ValueTuple<Person, Person, Sex, Person>;
using ColorUtility = UnityEngine.ColorUtility;

public class TalkOfTheTown {
    public static Simulation Simulation = null!;
    public static Time Time;

    // TODO : Move these to TED
    public static Goal Goals(params Goal[] goals) =>
        goals.Length == 0 ? null : goals.Aggregate((current, goal) => current & goal);
    public static Goal NonZero(Goal goal) => !!goal;
    public static Goal NonZero(params Goal[] goals) => NonZero(Goals(goals));

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
    public TablePredicate<Person> Buried;

    public GeneralIndex<AgentRow, VitalStatus> AgentsVitalStatusIndex;
    public KeyIndex<(LocationType, Color), LocationType> LocationColorsIndex;
    public KeyIndex<(Location, LocationType, Vector2Int, int, Date), Vector2Int> LocationsPositionIndex;
    public GeneralIndex<(Person, ActionType, Location), Location> WhereTheyAtLocationIndex;
    #endregion

    public void InitSimulator() {
        Simulation = new Simulation("Talk of the Town");

        // ReSharper disable InconsistentNaming
        // variables just so happen to follow c# var name norms, still disabling InconsistentNaming because
        // Tables, despite being local variables, will still be capitalized for style/identification purposes.

        #region Functions/PrimitiveTests
        var SByteBellCurve = Method(Randomize.SByteBellCurve);
        var Distance = Method<Vector2Int, Vector2Int, int>(Town.Distance);
        var CurrentYear = Time.Property<int>(nameof(Time.Year));
        var CurrentDate = Time.Property<Date>(nameof(Time.Date));
        var CurrentTimeOfDay = Time.Property<TimeOfDay>(nameof(Time.TimeOfDay));
        var IsAM = Time.TestProperty(nameof(Time.IsAM));
        var IsDate = TestMethod<Date>(Time.IsDate);
        var PastDate = TestMethod<Date>(Time.PastDate);
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
        var previousAge      = (Var<int>)"previousAge";
        var founded          = (Var<int>)"founded";
        var founded2         = (Var<int>)"founded2";
        var conception       = (Var<Date>)"conception";
        var dateOfBirth      = (Var<Date>)"dateOfBirth";
        var opening          = (Var<Date>)"opening";
        var opening2         = (Var<Date>)"opening2";

        var location         = (Var<Location>)"location";
        var otherLocation    = (Var<Location>)"otherLocation";
        var position         = (Var<Vector2Int>)"position";
        var otherPosition    = (Var<Vector2Int>)"otherPosition";
        var locationType     = (Var<LocationType>)"locationType";
        var locationCategory = (Var<LocationCategories>)"locationCategory";
        var operation        = (Var<DailyOperation>)"operation";
        var timeOfDay        = (Var<TimeOfDay>)"timeOfDay";
        var schedule         = (Var<Schedule>)"schedule";
        var color            = (Var<Color>)"color";
        var positions        = (Var<int>)"positions";
        var locationName     = (Var<string>)"locationName";

        var actionType       = (Var<ActionType>)"actionType";
        var distance         = (Var<int>)"distance";
        var count            = (Var<int>)"count";
        var util             = (Var<int>)"util";
        var state            = (Var<bool>)"state";
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
        
        #region Agents setup and state logic:
        Agents = Predicate("Agents", person.Key, age, dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed);
        AgentsVitalStatusIndex = (GeneralIndex<AgentRow, VitalStatus>)Agents.IndexFor(vitalStatus, false);

        // Dead and Alive definitions:
        var Dead = Definition("Dead", person)
            .Is(Agents[person, __, __, __, __, VitalStatus.Dead]);
        var Alive = Definition("Alive", person)
            .Is(Agents[person, __, __, __, __, VitalStatus.Alive]);
        // special case Alive check where we also bind age
        var Age = Definition("Age", person, age)
            .Is(Agents[person, age, __, __, __, VitalStatus.Alive]);

        // Set Dead condition:
        Agents.Set(person, vitalStatus, VitalStatus.Dead)
            .If(Age, age > 60, Prob[Time.PerMonth(0.003f)]);
        var JustDied = Predicate("JustDied", person).If(Agents.Set(person, vitalStatus));
        #endregion

        #region Sexual Attraction:
        // special case Alive check where we also bind sex
        var PersonSex = Definition("PersonSex", person, sex)
            .Is(Agents[person, __, __, sex, __, VitalStatus.Alive]);
        // special case Alive check where we also bind sexuality
        var PersonSexuality = Definition("PersonSexuality", person, sexuality)
            .Is(Agents[person, __, __, __, sexuality, VitalStatus.Alive]);

        var SexualityAttracted = Test<Sexuality, Sex>(
            "SexualityAttracted", (se, s) => se.IsAttracted(s));

        var AttractedSexuality = Definition("AttractedSexuality", person, partner)
            .Is(PersonSexuality[person, sexuality], PersonSex[partner, sexOfPartner], SexualityAttracted[sexuality, sexOfPartner],
                PersonSexuality[partner, sexualOfPartner], PersonSex[person, sex], SexualityAttracted[sexualOfPartner, sex]);
        #endregion

        #region Primordial Beings initialize:
        var PrimordialBeings = FromCsv("PrimordialBeings",
            Csv("agents"), person, age, dateOfBirth, sex, sexuality);

        Agents.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeings);
        Personality.Initially[person, facet, SByteBellCurve].Where(PrimordialBeings, Facets);
        Aptitude.Initially[person, job, SByteBellCurve].Where(PrimordialBeings, Jobs);
        #endregion

        // TODO : Better util for couples - facet similarity or score based on facet logic (> X, score + 100)
        // TODO : Married couples separate from ProcreativePair - last name changes in 'nickname' like table
        // TODO : Limit PotentialPairings by interactions (avoids performance hit for batch selection)
        // TODO : Non-monogamous pairings (needs above limitation as well) - alter NewProcreativePair too...
        #region Couples (for procreation):
        var Men = Predicate("Men", man).If(
            Agents[man, age, __, Sex.Male, __, VitalStatus.Alive], age >= 18);
        var Women = Predicate("Women", woman).If(
            Agents[woman, age, __, Sex.Female, __, VitalStatus.Alive], age >= 18);

        var ProcreativePair = Predicate("ProcreativePair", woman.Indexed, man.Indexed);
        ProcreativePair.Unique = true;
        var NewProcreativePair = Predicate("NewProcreativePair", woman, man);

        var Parents = Predicate("Parents", parent, child);
        var FamilialRelation = Definition("FamilialRelation", person, otherPerson)
            .Is(Parents[person, otherPerson] | Parents[otherPerson, person]); // only immediate family

        var PotentialPairings = Predicate("PotentialPairings", woman.Indexed, man.Indexed)
            .If(Women, !ProcreativePair[woman, __],
                Men, !ProcreativePair[__, man],
                AttractedSexuality[woman, man], !FamilialRelation[woman, man]);

        // Batch Selection from Market approach
        var NormalScore = Method(Randomize.NormalScore);
        var ScoredPairings = Predicate("ScoredPairings", woman.Indexed, man.Indexed, util)
            .If(PotentialPairings, util == NormalScore);
        var BestPairForWomen = Predicate("BestPairForWomen", woman, man, util)
            .If(Women, Maximal((man, util), util, ScoredPairings));
        var BestPairForBoth = Predicate("BestPairForBoth", woman, man)
            .If(BestPairForWomen[__, man, __], Maximal(woman, util, BestPairForWomen));

        // Best in Market approach - less expensive but only adds one pair at a time
        var PairingBestOnce = Predicate("PairingBestOnce", woman, man)
            .If(Maximal((woman, man), util, ScoredPairings));

        NewProcreativePair.If(BestPairForBoth, !ProcreativePair[__, man], !ProcreativePair[woman, __]);
        ProcreativePair.Accumulates(NewProcreativePair);
        #endregion

        // TODO : Limit Procreate by interactions to make procreation be more "physical" (also limits birth rates)
        // TODO : Limit Procreate by Personality (family planning, could include likely-hood to use contraceptives)
        // TODO : Limit Procreate by time since last birth and total number of children with partner (Gestation table info)
        #region Birth and aging:
        var FertilityRate = Method<int, float>(Sims.FertilityRate);
        var RandomSex = Method(Sims.RandomSex);
        // Surname here is only being used to facilitate A naming convention for last names (currently paternal lineage)
        var Surname = Function<Person, string>("Surname", p => p.LastName);

        // Procreate Indexed by woman allows for multiple partners (in the same tick)
        var Procreate = Predicate("Procreate", woman.Indexed, man, sex, child);
        var ProcreateMan = Item2(man, Procreate);
        var ProcreateSex = Item3(sex, Procreate);
        var ProcreateChild = Item4(child, Procreate);
        var procreateRow = RowVariable(Procreate);
        // woman should be NonVar | man, sex, and child are all Var
        var RandomProcreate = Definition("RandomProcreate", 
            woman, man, sex, child).Is(RandomIndexedElement(Procreate, woman, procreateRow),
            ProcreateMan[procreateRow, man], ProcreateSex[procreateRow, sex], ProcreateChild[procreateRow, child]);

        // Gestation is that interstitial table between Procreate and birth. These are dependent events and
        // as such they need to both have data dependency but also Event dependency. Two Events are needed -
        // Procreate and birth. Could have some `pseudo` event (the rules for the gestation table are the
        // Procreate event) but this is less robust. E.g. to prevent multiple partners creating a gestation
        // event in the same tick the system would have to be designed such that only one pair of woman and man
        // have sex on a given tick. Additionally, to allow for set on this table, one column must be a key
        var Gestation = Predicate("Gestation", 
            woman.Indexed, man, sex, child.Key, conception, state.Indexed);
        var Pregnant = Predicate("Pregnant", woman)
            .If(Gestation[woman, __, __, __, __, true]);

        // The point of linking birth to gestation is to not have women getting pregnant again while pregnant,
        // a Procreate event is used to do this check (thus !Pregnant)
        Procreate.If(ProcreativePair[woman, man], !Pregnant[woman],
            Age[woman, age], Alive[man], Prob[FertilityRate[age]],
            sex == RandomSex, RandomFirstName, child == NewPerson[firstName, Surname[man]]);
        Gestation.Add[woman, man, sex, child, CurrentDate, true]
            .If(Count(Procreate[woman, __, __, __]) <= 1, Procreate);
        Gestation.Add[woman, man, sex, child, CurrentDate, true]
            .If(Count(Procreate[woman, __, __, __]) > 1, 
                Procreate[woman, __, __, __], RandomProcreate);
        
        // Need to alter the state of the gestation table when giving birth, otherwise birth after 9 months with 'labor'
        var BirthTo = Predicate("BirthTo", woman, man, sex, child);
        var NineMonthsPast = TestMethod<Date>(Time.NineMonthsPast);
        BirthTo.If(Gestation[woman, man, sex, child, conception, true], NineMonthsPast[conception], Prob[0.8f]);
        Gestation.Set(child, state, false).If(BirthTo);

        // BirthTo has a column for the sex of the child to facilitate gendered naming, however, since there is no need to
        // determine the child's sexuality in BirthTo, a child has the sexuality established when they are added to Agents
        var RandomSexuality = Function<Sex, Sexuality>("RandomSexuality", Sexuality.Random);
        Agents.Add[person, 0, CurrentDate, sex, sexuality, VitalStatus.Alive].If(
            BirthTo[__, __, sex, person], sexuality == RandomSexuality[sex]);

        Parents.Add.If(BirthTo[parent, __, __, child]);
        Parents.Add.If(BirthTo[__, parent, __, child]);

        // Increment age once per birthday (in the AM, if you weren't just born)
        var WhenToAge = Definition("WhenToAge", person, age).Is(
            Agents[person, age, dateOfBirth, __, __, VitalStatus.Alive],
            IsAM, IsDate[dateOfBirth], !BirthTo[__, __, __, person]);
        Agents.Increment(person, age, WhenToAge);

        // And add anything else that is needed for a new agent:
        Personality.Add[person, facet, SByteBellCurve].If(Agents.Add, Facets);
        Aptitude.Add[person, job, SByteBellCurve].If(Agents.Add, Jobs);
        // Agents.Add handles both Birth and Drifters, if we want to make kids inherit modified values from
        // their parents then we will need separate cases for BirthTo[__, __, __, person] and drifters.
        #endregion

        // TODO : Cue drifters when new jobs need filling that the township can't meet requirements for
        #region Drifters - adults moving to town:
        var RandomDate = Function("RandomDate", Date.Random);
        var RandomAdultAge = Method(Sims.RandomAdultAge);
        var Drifter = Predicate("Drifter", person, sex, sexuality);
        Drifter[person, RandomSex, sexuality].If(Prob[Time.PerYear(0.05f)], 
            RandomPerson, sexuality == RandomSexuality[sex]);
        Agents.Add[person, RandomAdultAge, RandomDate, sex, sexuality, VitalStatus.Alive].If(Drifter);
        #endregion

        // TODO : All non-intimate relationships 

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

        var LocationOfCategory = Predicate("LocationOfCategory", location, locationCategory);
        LocationOfCategory.Initially.Where(LocationInformation, PrimordialLocations);
        LocationOfCategory.Add.If(LocationInformation, NewLocations);

        var AvailableCategories = Predicate("AvailableCategories", locationCategory);
        AvailableCategories.Initially.Where(LocationInformation, NonZero(LocationInformation, PrimordialLocations));
        AvailableCategories.Add.If(LocationOfCategory.Add, NonZero(!AvailableCategories[locationCategory], LocationOfCategory.Add));
        #endregion

        // TODO : Separate out the dead into a new table... involve removal ?
        // TODO : Include ApartmentComplex locations in Housing logic
        // TODO : Include Inn locations in Housing logic - drifters start at an Inn ?
        #region Housing:
        var Homes = Predicate("Homes", occupant.Key, location.Indexed);
        Homes.Unique = true;
        var Occupancy = Predicate("Occupancy", location, count)
            .If(Locations[location, LocationType.House, position, founded, opening], count == Count(Homes));
        var UnderOccupied = Predicate("UnderOccupied", location).If(Occupancy, count <= 5);

        // Using this to randomly assign one house per person...
        var PrimordialHouses = Predicate("PrimordialHouses", location)
            .If(PrimordialLocations[location, LocationType.House, position, founded, opening]);
        // Ideally this would involve some more complex assignment logic that would fill houses based on some Goal
        // e.g. Initialize Homes first based on primordial couples, then on all single agents
        Homes.Initially.Where(PrimordialBeings[occupant, age, dateOfBirth, sex, sexuality], RandomElement(PrimordialHouses, location));
        
        Homes.Add.If(BirthTo[man, woman, sex, occupant], Homes[woman, location]); // Move in with mom
        Homes.Add.If(Drifter[occupant, __, __], RandomElement(UnderOccupied, location));

        Homes.Set(occupant, location).If(JustDied[occupant],
            Locations[location, LocationType.Cemetery, __, __, __]);
        var BuriedAt = Predicate("BuriedAt", occupant, location)
            .If(Locations[location, LocationType.Cemetery, __, __, __], Homes);
        // with only the one cemetery for now, the follow will suffice for the GUI
        Buried = Predicate("Buried", person).If(BuriedAt[person, __]);

        // Distance per person makes most sense when measured from either where the person is,
        // or where they live. This handles the latter:
        var DistanceFromHome = Definition("DistanceFromHome", person, location, distance)
            .Is(Locations[location, __, position, __, __], 
                Homes[person, otherLocation],
                Locations[otherLocation, __, otherPosition, __, __], 
                distance == Distance[position, otherPosition]);
        #endregion

        // TODO : More "logic" behind who moves in and out of houses
        #region Moving houses:
        var WantToMove = Predicate("WantToMove", person).If(Homes[person, location], Occupancy, count >= 8);
        
        // var LivingWithFamily = Predicate("LivingWithFamily", person)
        //     .If(Homes[person, location], FamilialRelation, Homes[otherPerson, location]);
        // var FamilyHome = Predicate("FamilyHome", location)
        //     .If(Homes[person, location], FamilialRelation, Homes[otherPerson, location]);
        // var LivingAlone = Predicate("LivingAlone", person)
        //     .If(Homes[person, location], !Homes[__, location]);
        // var MoveToFamily = Predicate("MoveToFamily", person).If(WantToMove, !LivingWithFamily[person]);

        var MovingIn = Predicate("MovingIn", person, location).If(NonZero(WantToMove[person]),
            RandomElement(WantToMove, person), RandomElement(UnderOccupied, location));

        Homes.Set(occupant, location).If(MovingIn[occupant, location]);
        #endregion

        #region New Location helper Functions and Definitions:
        // Title case string and make a Location object
        var NewLocation = Method<string, Location>(Town.NewLocation);
        
        // Helper functions and definitions for creating new locations at a valid lot in town
        var RandomLot = Method<uint, Vector2Int>(Town.RandomLot);
        var NumLots = Length("NumLots", Locations);
        var IsVacant = Definition("IsVacant", position)
            .Is(!Locations[__, __, position, __, __]);
        var FreeLot = Definition("FreeLot", position)
            .Is(position == RandomLot[NumLots], IsVacant[position]);
        #endregion

        #region New Location helper functions (meta-sub-expressions):
        // Base case - useful mainly for testing/rapid development (you only need one string/generating a list of names can come second)
        void AddNewNamedLocation(LocationType locType, string name, Goal readyToAdd) =>
            NewLocations[location, locType, position, CurrentYear, CurrentDate]
                .If(FreeLot, Prob[Time.PerWeek(0.5f)], // Needs the random lot to be available & 'construction' isn't instantaneous
                readyToAdd, location == NewLocation[name]); // otherwise, check the readyToAdd Goal and if it passes add a NewLocation

        // If you are only planning on adding a single location of the given type, this adds the check that
        // a location of locType doesn't already exist.
        void AddOneLocation(LocationType locType, string name, Goal readyToAdd) => AddNewNamedLocation(locType, name, 
            !Locations[otherLocation, locType, otherPosition, founded2, opening2] & readyToAdd);

        // This is the more realistic use case with a list of names for a give type to choose from.
        void AddNewLocation(LocationType locType, TablePredicate<string> names, Goal readyToAdd) =>
            NewLocations[location, locType, position, CurrentYear, CurrentDate]
                .If(FreeLot, Prob[Time.PerWeek(0.5f)], readyToAdd,
                RandomElement(names, locationName), location == NewLocation[locationName]);
        #endregion

        // TODO : Add more new locations for each location type
        #region New Location Logic:
        var HouseNames = FromCsv("HouseNames", Csv("house_names"), locationName);
        AddNewLocation(LocationType.House, HouseNames, !!WantToMove[person]);
        // Currently the following only happens with drifters - everyone starts housed
        AddNewLocation(LocationType.House, HouseNames, 
            Count(Homes[person, location] & Alive[person]) < Count(Alive));
        
        AddOneLocation(LocationType.Hospital, "St. Asmodeus",
            NonZero(Aptitude[person, Vocation.Doctor, aptitude], 
                aptitude > 15, Age, age > 21));

        AddOneLocation(LocationType.Cemetery, "The Old Cemetery", 
            NonZero(Alive, Age, age >= 60));

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
            .If(timeOfDay == CurrentTimeOfDay, Locations, VocationShifts,
                PositionsPerJob, Count(Vocations) < positions);

        var Candidates = Predicate("Candidates", person, job, location)
            .If(JobsToFill, Maximal(person, aptitude, Goals(Alive[person], 
                !Vocations[__, person, __, __], Age, age > 18, Aptitude)));

        Vocations.Add[job, person, location, CurrentTimeOfDay].If(Candidates);
        #endregion

        // ********************************** Movement: *********************************

        #region Action Info:
        var ActionToCategory = FromCsv("ActionToCategory",
            Csv("actionCategories"), actionType, locationCategory);
        var AvailableActions = Predicate("AvailableActions", actionType)
            .If(ActionToCategory, AvailableCategories);
        #endregion
        
        #region Operation and Open logic:
        var InOperation = TestMethod<DailyOperation>(Time.InOperation);
        var IsOpen = TestMethod<Schedule>(Time.IsOpen);
        // for better performance with larger numbers of locations, do a pre-calculation 
        // of the location types that are accessible as this is the only place that
        // the functions InOperation and IsOpen need to be called
        var OpenLocationTypes = Predicate("OpenLocationTypes", locationType)
            .If(LocationInformation, InOperation[operation], IsOpen[schedule]);
        OpenLocationTypes.ForceDynamic();
        var OpenForBusiness = Predicate("OpenForBusiness", location)
            .If(OpenLocationTypes, Locations); // for more complex scheduling have an extra table
        // of non-default schedule or operation per location to include in this Predicate
        var OpenForBusinessByAction = Predicate("OpenForBusinessByAction", actionType, location)
            .If(ActionToCategory, LocationOfCategory, OpenForBusiness);
        #endregion

        #region Schooling:
        var Kids = Predicate("Kids", person).If(Alive, Age, age < 18);
        var NeedsSchooling = Predicate("NeedsSchooling", person).If(Kids, Age, age > 6);
        var NeedsDayCare = Predicate("NeedsDayCare", person).If(Kids, !NeedsSchooling[person]);

        var GoingToSchool = Predicate("GoingToSchool", person, location).If(
            AvailableActions[ActionType.GoingToSchool], OpenForBusiness,
            Locations[location, LocationType.School, __, __, __], // only expecting one location...
            NeedsSchooling);
        var GoingToDayCare = Predicate("GoingToDayCare", person, location).If(
            AvailableActions[ActionType.GoingToSchool], OpenForBusiness,
            Locations[location, LocationType.DayCare, __, __, __], // only expecting one location...
            NeedsDayCare);
        #endregion

        #region Working:
        var GoingToWork = Predicate("GoingToWork", person, location)
            .If(Vocations[__, person, location, CurrentTimeOfDay], OpenForBusiness);
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
            !GoingToWork[person, __],
            !GoingToDayCare[person, __],
            !GoingToSchool[person, __]);
        var RandomActionAssign = Predicate("RandomActionAssign", person, actionType)
            .If(NeedsActionAssignment, RandomElement(AdultActions, actionType));

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
        Simulation.Update(); } // optional, not necessary to call Update after EndPredicates

    public void UpdateSimulator() {
        Time.Tick();
        Simulation.Update(); }
}