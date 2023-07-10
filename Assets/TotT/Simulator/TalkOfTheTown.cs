using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using GraphVisualization;
using TED;
using TED.Interpreter;
using TED.Tables;
using TED.Utilities;
using TotT.Simulog;
using TotT.TextGenerator;
using TotT.Unity;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace TotT.Simulator {
    using static Calendar;   // Prob per interval type
    using static CsvParsing; // DeclareParsers
    using static Generators; // Name Generation
    using static SimpleCFG;  // Name Generation
    using static GUIManager; // Colorize Extension
    using static Randomize;  // Seed and RandomElement
    // The following offload static components of a TED program...
    using static Functions;    // C# function hookups to TED predicates
    using static StaticTables; // non dynamic tables - classic datalog EDB
    using static Variables;    // named variables
    // TED Meta language hookup
    using static SimuLang;

    public class TalkOfTheTown {
        public static Simulation Simulation = null!;
        public static Time Time;
        private const int Seed = 349571286;
        private readonly string TownName;

        private TalkOfTheTown() {
            DeclareParsers();
            Seed(Seed, Seed);
            Time = new Time();
            TownName = PossibleTownName.Random;
            BindingList.BindGlobal(Generators.TownName, TownName);
            TEDGraphVisualization.SetTableDescription();
            Graph.SetDescriptionMethod<Person>(PersonDescription);
        }
        public TalkOfTheTown(ushort tick = 1) : this() => Time = new Time(tick);
        public TalkOfTheTown(Month month, byte day = 1, TimeOfDay time = TimeOfDay.AM) :
            this() => Time = new Time(month, day, time);

        // ReSharper disable InconsistentNaming
        // Tables, despite being local or private variables, will be capitalized for style/identification purposes.

        #region Tables and Indexers for GUI and Graph visuals
        private TablePredicate<Person, int, Date, Sex, Sexuality, VitalStatus> CharacterAttributes;
        private TablePredicate<Person, Person, InteractionType> Interaction;
        private TablePredicate<Person, ActionType, Location> WhereTheyAt;
        private TablePredicate<Person, Location> Home;
        private TablePredicate<Person, Person> Parent;
        private AffinityRelationship<Person, Person> Friend;
        private AffinityRelationship<Person, Person> Enemy;
        private AffinityRelationship<Person, Person> Romantic;
        private TablePredicate<Vocation, Person, Location, TimeOfDay> Employment;
        private KeyIndex<(Vocation, Person, Location, TimeOfDay), Person> EmploymentIndex;
        public TablePredicate<Vector2Int, Location, LocationType> CreatedLocation;
        public TablePredicate<Vector2Int> VacatedLocation;
        public TablePredicate<Person> Buried;
        public KeyIndex<(bool, int), bool> PopulationCountIndex;
        public GeneralIndex<(Person, ActionType, Location), Location> WhereTheyAtLocationIndex;
        public KeyIndex<(Vector2Int, Location, LocationType, TimePoint), Vector2Int> LocationsPositionIndex;
        private ColumnAccessor<LocationType, Location> LocationToType;
        #endregion

        #region Functions for GUI and Graph visuals
        private Color PlaceColor(Location place) => LocationColorsIndex[LocationToType[place]].Item2;

        private void DefaultColorizers() {
            SetDefaultColorizer<Location>(PlaceColor);
            SetDefaultColorizer<bool>(s => s ? Color.white : Color.gray);
            SetDefaultColorizer<VitalStatus>(s => s == VitalStatus.Alive ? Color.white : Color.gray);
            SetDefaultColorizer<BusinessStatus>(s => s == BusinessStatus.InBusiness ? Color.white : Color.gray);
        }
        #endregion

        public void InitSimulator() {
            Simulation = new Simulation("Talk of the Town");
            Simulation.Exceptions.Colorize(_ => Color.red);
            Simulation.Problems.Colorize(_ => Color.red);
            DefaultColorizers();
            Simulation.BeginPredicates();
            InitStaticTables();

            // ************************************ Characters ************************************

            var Character = Exists("Character", person, age, 
                dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed, birthday)
               .InitiallyWhere(PrimordialBeing[person, age, dateOfBirth, __, __], DateAgeToTimePoint[dateOfBirth, age, birthday]);
            Character.Attributes.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeing);

            CharacterAttributes = Character.Attributes;
            Character.Attributes.Colorize(vitalStatus);
            Character.Attributes.Button("Random friend network", VisualizeRandomFriendNetwork);
            Character.Attributes.Button("Full network", VisualizeFullSocialNetwork);

            #region Character Helpers
            // TODO : Replace naming helpers with TextGenerator
            var RandomFirstName = Definition("RandomFirstName", sex, firstName);
            RandomFirstName[Sex.Male, firstName].If(RandomElement(MaleNames, firstName));
            RandomFirstName[Sex.Female, firstName].If(RandomElement(FemaleNames, firstName));
            // Surname here is only being used to facilitate A naming convention for last names (currently paternal lineage)
            var RandomPerson = Definition("RandomPerson", sex, person)
               .Is(RandomFirstName, RandomElement(Surnames, lastName), NewPerson[firstName, lastName, person]);

            PopulationCountIndex = Character.CountIndex;
            var Population = Definition("Population", count).Is(Character.Count[true, count]);

            // Check to vital status - same as also saying Character[person] but with less extraneous referencing
            var Age = Definition("Age", person, age)
               .Is(Character.Attributes[person, age, __, __, __, VitalStatus.Alive]);
            var Man = Definition("Men", man)
               .Is(Character.Attributes[man, age, __, Sex.Male, __, VitalStatus.Alive], age >= 18);
            var Woman = Definition("Women", woman)
               .Is(Character.Attributes[woman, age, __, Sex.Female, __, VitalStatus.Alive], age >= 18);
            var PersonSex = Definition("PersonSex", person, sex)
               .Is(Character.Attributes[person, __, __, sex, __, VitalStatus.Alive]);
            var PersonSexuality = Definition("PersonSexuality", person, sexuality)
               .Is(Character.Attributes[person, __, __, __, sexuality, VitalStatus.Alive]);
            var SexualAttraction = Definition("SexualAttraction", person, partner)
               .Is(PersonSexuality[person, sexuality], PersonSex[partner, sexOfPartner], AttractedTo[sexuality, sexOfPartner],
                   PersonSexuality[partner, sexualOfPartner], PersonSex[person, sex], AttractedTo[sexualOfPartner, sex]);
            #endregion

            var Aptitude = Character.FeaturesMultiple("Aptitude", job.Indexed, aptitude);
            Aptitude.Initially[person, job, RandomNormalSByte].Where(PrimordialBeing, Jobs);
            Aptitude.Add[person, job, RandomNormalSByte].If(Character.Add, Jobs);

            Character.EndWhen(Age, age > 60, PerMonth(0.003f))
                     .EndCauses(Set(Character.Attributes, person, vitalStatus, VitalStatus.Dead));

            var Drifter = Predicate("Drifter", person, sex, sexuality);
            Drifter[person, RandomSex, sexuality].If(PerYear(0.05f), RandomPerson, RandomSexuality[sex, sexuality]);
            Character.StartWithTime(Drifter, DateAgeToTimePoint[RandomDate, RandomAdultAge, birthday])
                     .StartWithCauses(Add(Character.Attributes[person, Time.YearsOld[birthday], 
                                                               TimePointToDate[birthday], sex, sexuality, VitalStatus.Alive]).If(Drifter));

            // *********************************** Relationships **********************************
            // TODO : Primordial Relationships?
            // TODO : Married couples - last name changes

            var Charge = Affinity("Charge", pairing, person, otherPerson, charge).Decay(0.8f);
            Friend = Charge.Relationship("Friend", state, 5000, 4000);
            Enemy = Charge.Relationship("Enemy", state, -6000, -3000);

            var Spark = Affinity("Spark", pairing, person, otherPerson, spark).Decay(0.1f);
            Romantic = Spark.Relationship("Romantic", state, 7000, 6000);

            var MutualFriendship = Definition("MutualFriendship", person, otherPerson)
               .Is(Friend[person, otherPerson], Friend[otherPerson, person]);
            var MutualRomanticInterest = Definition("MutualRomanticInterest", person, otherPerson)
               .Is(Romantic[person, otherPerson], Romantic[otherPerson, person]);

            var Lover = ExclusiveRelationship("Lover", symmetricPair, person, otherPerson, state)
                       .StartWhen(MutualRomanticInterest, MutualFriendship)
                       .EndWhen(Character.End, Character[otherPerson]);

            Parent = Predicate("Parent", parent.Indexed, child.Indexed);
            Parent.Button("Visualize", VisualizeFamilies);
            var FamilialRelation = Definition("FamilialRelation", person, otherPerson)
               .Is(Parent[person, otherPerson] | Parent[otherPerson, person]); // only immediate family

            // ************************************ Procreation ***********************************
            // TODO : Limit PotentialProcreation by interactions
            // TODO : Limit PotentialProcreation by Personality
            // TODO : Limit PotentialProcreation by time since last birth/number of children

            var Embryo = Exists("Embryo", child, woman.Indexed, man, sex, conception.Indexed);
            var Pregnant = Definition("Pregnant", woman)
               .Is(Embryo.Attributes[child, woman, __, __, __], Embryo[child]);

            var PotentialProcreation = Assign("PotentialProcreation", woman, man.Indexed)
               .When(MutualRomanticInterest[woman, man], Woman, Man, !FamilialRelation[woman, man], 
                   !Pregnant[woman], Age[woman, age], Prob[FertilityRate[age]]);
            var Procreation = Event("Procreation", woman, man, sex, child)
               .OccursWhen(PotentialProcreation.Assignments[woman, man], RandomSex[sex], RandomFirstName, NewPerson[firstName, Surname[man], child]);

            Embryo.StartWhen(Procreation)
                  .StartCauses(Add(Embryo.Attributes[child, woman, man, sex, Time.CurrentDate]).If(Procreation))
                  .EndWhen(Embryo[child], Embryo.Attributes, Time.NineMonthsPast[conception], Prob[0.8f])
                  .EndCauses(Add(Parent[parent, child]).If(Embryo.Attributes[child, parent, __, __, __]),
                             Add(Parent[parent, child]).If(Embryo.Attributes[child, __, parent, __, __]));

            Character.StartWhen(Embryo.End[person]);
            Character.StartCauses(Add(CharacterAttributes[person, 0, Time.CurrentDate, sex, sexuality, VitalStatus.Alive])
                                     .If(Embryo.End[person], Embryo.Attributes[person, __, __, sex, __], RandomSexuality[sex, sexuality]));
            // Increment age once per birthday (in the AM, if you weren't just born)
            CharacterAttributes.Set(person, age, num)
                 .If(CharacterAttributes[person, age, dateOfBirth, __, __, VitalStatus.Alive],
                     Time.CurrentlyMorning, Time.IsToday[dateOfBirth], !Embryo.End[person], Incr[age, num]);

            // ************************************ Locations *************************************

            var Place = Exists("Place", location, locationType.Indexed, 
                locationCategory.Indexed, position.Indexed, businessStatus.Indexed, founding).InitiallyWhere(PrimordialLocation);
            Place.Attributes.Initially[location, locationType, locationCategory, position, BusinessStatus.InBusiness]
                         .Where(PrimordialLocation, LocationInformation);

            var InBusiness = Definition("InBusiness", location)
               .Is(Place.Attributes[location, __, __, __, BusinessStatus.InBusiness]);
            var IsVacant = Definition("IsVacant", position)
               .Is(!Place.Attributes[__, __, __, position, BusinessStatus.InBusiness]);
            var FreeLot = Definition("FreeLot", position)
               .Is(Place.Count[true, count], RandomLot[count, position], IsVacant[position]);

            // NewPlace event drives CreatedLocation table
            var NewPlace = Event("NewPlace", locationName, locationType);

            #region Tilemap/GUI helpers
            // CreatedLocation is used for both adding tiles to the TileMap in Unity efficiently
            // (not checking every row of the Locations table) and for collecting new locations
            // with the minimal amount of information needed (excludes derivable columns).
            CreatedLocation = Predicate("CreatedLocation", position, location, locationType);
            CreatedLocation.If(NewPlace, FreeLot, PerWeek(0.5f), NewLocation[locationName, location]);
            CreatedLocation.Colorize(location);
            // VacatedLocation is used for removing tiles from the TileMap
            VacatedLocation = Predicate("VacatedLocation", position);
            VacatedLocation.If(Place.End, Place.Attributes);

            // For tile hover info strings we only want the locations in business, and since we will be
            // using this table for GUI interactions we need to be able to access a location by position.
            // Each lot in town can only hold one active location so this works out nicely.
            // DisplayLocations is a collection - not intended to be used as a predicate - thus the plural naming
            var DisplayLocations = Predicate("DisplayLocations", 
                position.Key, location, locationType, founding)
               .If(Place.Attributes[location, locationType, __, position, BusinessStatus.InBusiness], 
                   Place[location, true, founding, TimePoint.Eschaton]);
            DisplayLocations.Colorize(location);
            LocationsPositionIndex = DisplayLocations.KeyIndex(position);
            LocationToType = Place.Attributes.Accessor(location, locationType);
            Place.Attributes.Colorize(location);
            #endregion

            Place.StartWhen(CreatedLocation)
                 .StartCauses(Add(Place.Attributes[location, locationType, locationCategory,
                                                position, BusinessStatus.InBusiness])
                                 .If(CreatedLocation, LocationInformation))
                 .EndWhen(Place.Attributes, !In(locationType, permanentLocationTypes),
                          Place, Time.YearsOld[founding, age], age > 40, PerYear(0.1f))
                 .EndCauses(Set(Place.Attributes, location, businessStatus, BusinessStatus.OutOfBusiness));

            var NumLocations = CountsBy("NumLocations", Place.Attributes, locationType, count);
            var CategoryCount = CountsBy("CategoryCount", Place.Attributes, locationCategory, count);
            var AvailableCategory = Predicate("AvailableCategory", locationCategory).If(CategoryCount);
            var AvailableAction = Predicate("AvailableAction", actionType).If(ActionToCategory, CategoryCount);

            // ************************************** Housing *************************************
            // TODO : Include ApartmentComplex locations in Housing logic
            // TODO : Include Inn locations in Housing logic - drifters start at an Inn

            Home = Predicate("Home", occupant.Key, location.Indexed);
            Home.Unique = true;
            Home.Button("Visualize", VisualizeHomes);

            var Occupancy = Counts("Occupancy", location, occupant)
                .By(Place.Attributes[location, LocationType.House, __, __, BusinessStatus.InBusiness], Home);
            var Unoccupied = Predicate("Unoccupied", location)
                .If(Place.Attributes[location, LocationType.House, __, __, BusinessStatus.InBusiness], !Home[__, location]);
            var UnderOccupied = Predicate("UnderOccupied", location);
            UnderOccupied.If(Occupancy.Count, count <= 5);
            UnderOccupied.If(Unoccupied);

            var Unhoused = Predicate("Unhoused", person).If(Character[person], !Home[person, __]);

            // Using this to randomly assign one house per primordial person...
            var PrimordialHouse = Predicate("PrimordialHouse", location)
                .If(PrimordialLocation[location, LocationType.House, __, __]);
            Home.Initially.Where(PrimordialBeing[occupant, __, __, __, __],
                RandomElement(PrimordialHouse, location));

            Home.Add.If(Embryo.End[occupant], Embryo.Attributes[occupant, woman, __, __, __], Home[woman, location]); // Move in with mom
            // If no UnderOccupied homes this wont get set...
            Home.Add.If(Drifter[occupant, __, __], RandomElement(UnderOccupied, location));
            Home.Add.If(Unhoused[occupant], RandomElement(UnderOccupied, location));

            Home.Set(occupant, location).If(Character.End[occupant],
                Place.Attributes[location, LocationType.Cemetery, __, __, BusinessStatus.InBusiness]);
            var BuriedAt = Predicate("BuriedAt", occupant, location)
                .If(Place.Attributes[location, LocationType.Cemetery, __, __, BusinessStatus.InBusiness], Home);
            Buried = Predicate("Buried", person).If(BuriedAt[person, __]);

            var ForeclosedUpon = Event("ForeclosedUpon", occupant)
                                .OccursWhen(Home, Place.End)
                                .Causes(Set(Home, occupant, location, location)
                                           .If(RandomElement(UnderOccupied, location)));

            // Distance per person makes most sense when measured from either where the person is,
            // or where they live. This handles the latter:
            var DistanceFromHome = Definition("DistanceFromHome", person, location, distance)
                .Is(Place.Attributes[location, __, __, position, BusinessStatus.InBusiness],
                    Home[person, otherLocation],
                    Place.Attributes[otherLocation, __, __, otherPosition, BusinessStatus.InBusiness],
                    Distance[position, otherPosition, distance]);

            var WantToMove = Predicate("WantToMove", person).If(Home[person, location], Occupancy.Count, count > 8);

            var CanMoveToFamily = Assign("CanMoveToFamily", person, location)
               .When(WantToMove, Home[person, otherLocation], FamilialRelation, 
                     Home[otherPerson, location], location != otherLocation, Occupancy.Count, count < 8);
            var SelectedToMoveToFamily = Predicate("SelectedToMoveToFamily", person).If(CanMoveToFamily.Assignments);

            var FamilyInHouse = Predicate("FamilyInHouse", person);
            FamilyInHouse.If(WantToMove, WantToMove[otherPerson], person != otherPerson,
                             Home[person, location], Home[otherPerson, location], FamilialRelation);
            var SelectedOutsiderToMove = Predicate("SelectedOutsiderToMove", person).If(WantToMove, !FamilyInHouse[person]);

            var MovingIn = Predicate("MovingIn", person, location);
            MovingIn.If(Once[SelectedToMoveToFamily[__]],
                        RandomElement(SelectedToMoveToFamily, person), CanMoveToFamily.Assignments);
            MovingIn.If(Once[SelectedOutsiderToMove[__]], !SelectedToMoveToFamily[__],
                        RandomElement(SelectedOutsiderToMove, person), RandomElement(UnderOccupied, location));
            MovingIn.If(Once[WantToMove[__]], !SelectedToMoveToFamily[__], !SelectedOutsiderToMove[__],
                        RandomElement(WantToMove, person), RandomElement(UnderOccupied, location));

            Home.Set(occupant, location).If(MovingIn[occupant, location]);

            // ************************************ Vocations: ************************************

            Employment = Predicate("Employment", job.Indexed, employee.Key, location.Indexed, timeOfDay.Indexed);
            EmploymentIndex = Employment.KeyIndex(employee);
            Employment.Colorize(location);
            Employment.Button("Visualize", VisualizeJobs);

            var EmploymentStatus = Predicate("EmploymentStatus", employee.Key, state.Indexed);
            EmploymentStatus.Add[employee, true].If(Employment.Add);
            EmploymentStatus.Set(employee, state, false).If(Character.End[employee], EmploymentStatus[employee, true]);
            EmploymentStatus.Set(employee, state, false).If(Place.End[location], Employment, EmploymentStatus[employee, true]);

            var StillEmployed = Definition("StillEmployed", person).Is(EmploymentStatus[person, true]);

            var JobToFill = Predicate("JobToFill", location, job)
                .If(Time.CurrentTimeOfDay[timeOfDay], InBusiness, Place.Attributes, VocationShift,
                    PositionsPerJob, Count(Employment & EmploymentStatus[employee, true]) < positions);

            Drifter[person, RandomSex, sexuality].If(JobToFill, PerMonth(0.5f), RandomPerson, RandomSexuality[sex, sexuality]);

            var BestCandidate = Predicate("BestCandidate", person, job, location)
                .If(JobToFill, Maximal(person, aptitude, And[Age, !StillEmployed[person], age >= 18, Aptitude]));

            var CandidateForJob = Assign("CandidateForJob", person, job).When(BestCandidate);
            var CandidatePerJob = Assign("CandidatePerJob", person, location).When(CandidateForJob.Assignments, BestCandidate);

            Employment.Add[job, person, location, Time.CurrentTimeOfDay].If(CandidatePerJob.Assignments, CandidateForJob.Assignments);

            // ********************************** New Locations ***********************************

            Goal UniqueLocationType(Goal goal, LocationType locType, bool unique) => unique ?
                !Place.Attributes[__, locType, __, __, BusinessStatus.InBusiness] & goal : goal;

            void NewLocationNamed(LocationType locType, string name, Goal readyToAdd, bool unique = true) =>
                NewPlace[name, locType].If(UniqueLocationType(readyToAdd, locType, unique));

            void NewLocationFromFunc(LocationType locType, Function<string> name, Goal readyToAdd, bool unique = true) =>
                NewPlace[name, locType].If(UniqueLocationType(readyToAdd, locType, unique));

            void NewLocationFromNames(LocationType locType, TablePredicate<string> names, Goal readyToAdd, bool unique = false) =>
                NewPlace[locationName, locType].If(RandomElement(names, locationName),
                                                   UniqueLocationType(readyToAdd, locType, unique));

            NewLocationFromNames(LocationType.House, HouseNames, Once[WantToMove[__] | Unhoused[__]]);

            NewLocationFromFunc(LocationType.Hospital, HospitalName.GenerateName,
                                Once[And[Aptitude[person, Vocation.Doctor, aptitude], aptitude > 15, Age, age > 21]]);

            NewLocationFromFunc(LocationType.Cemetery, CemeteryName.GenerateRandom, Once[And[Age, age >= 60]]); // before anyone can die

            NewLocationFromFunc(LocationType.DayCare, DaycareName.GenerateName, Count(Age & (age < 6)) > 5);

            NewLocationFromFunc(LocationType.School, HighSchoolName.GenerateRandom, Count(Age & (age >= 5) & (age < 18)) > 5);

            NewLocationNamed(LocationType.CityHall, "Big City Hall", And[Population[count], count > 150]);

            NewLocationNamed(LocationType.GeneralStore, "Big Box Store", And[Population[count], count > 50]);
            
            NewLocationNamed(LocationType.Bar, "The Black Sheep", And[Population[count], NumLocations[LocationType.Bar, num], count / 25 > num], false);
            NewLocationNamed(LocationType.Bar, "Triple Crossing", !Place.Attributes[__, LocationType.Bar, __, __, BusinessStatus.InBusiness]);

            NewLocationNamed(LocationType.GroceryStore, "Trader Jewels", And[Population[count], count > 75]);

            NewLocationNamed(LocationType.TattooParlor, "Heroes and Ghosts", And[Population[count], count > 120]);

            // ************************************* Movement: ************************************
            // TODO : Visiting choose location of relative or partner (if no friends)
            // TODO : NeedDayCare but not GoingToDayCare follow a non-working parent
            // TODO : Couple movements - i.e. GoingOutForDateNight

            // for more complex scheduling include an extra table of non-default schedule/operation per location
            var OpenLocationType = Predicate("OpenLocationType", locationType)
                .If(LocationInformation, Time.CurrentlyOperating[operation], Time.CurrentlyOpen[schedule]);

            var Kid = Predicate("Kid", person).If(Age, age < 18);
            var NeedSchooling = Predicate("NeedSchooling", person).If(Kid, Age, age > 6);
            var NeedDayCare = Predicate("NeedDayCare", person).If(Kid, !NeedSchooling[person]);

            var GoingToSchool = Predicate("GoingToSchool", person, location)
                               .If(AvailableAction[ActionType.GoingToSchool], OpenLocationType[LocationType.School],
                                   Place.Attributes[location, LocationType.School, __, __, BusinessStatus.InBusiness],
                                   NeedSchooling)
                               .If(AvailableAction[ActionType.GoingToSchool], OpenLocationType[LocationType.DayCare],
                                   Place.Attributes[location, LocationType.DayCare, __, __, BusinessStatus.InBusiness],
                                   NeedDayCare);

            var GoingToWork = Predicate("GoingToWork", person, location)
                .If(OpenLocationType, InBusiness, Place.Attributes, StillEmployed, Employment[__, person, location, Time.CurrentTimeOfDay]);

            WhereTheyAt = Predicate("WhereTheyAt", person.Key, actionType.Indexed, location.Indexed);
            WhereTheyAt.Unique = true;
            WhereTheyAt.Colorize(location);
            WhereTheyAtLocationIndex = (GeneralIndex<(Person, ActionType, Location), Location>)WhereTheyAt.IndexFor(location, false);
            WhereTheyAt.Button("Snapshot", VisualizeWhereTheyAt);

            var AdultAction = Predicate("AdultAction", actionType)
                .If(AvailableAction, !In(actionType, new[] { ActionType.GoingToSchool, ActionType.GoingOutForDateNight }));
            var NeedsActionAssignment = Predicate("NeedsActionAssignment", person).If(Character[person],
                !GoingToWork[person, __],
                !GoingToSchool[person, __]);
            var RandomActionAssign = Predicate("RandomActionAssign", person, actionType)
                .If(NeedsActionAssignment, RandomElement(AdultAction, actionType));

            var LocationByActionAssign = Predicate("LocationByActionAssign", person, location);
            LocationByActionAssign.If(RandomActionAssign[person, ActionType.StayingIn], Home[person, location]);

            var VisitingFriend = Assign("VisitingFriend", person, otherPerson)
               .When(RandomActionAssign[person, ActionType.Visiting], MutualFriendship);
            var NoOneToVisit = Predicate("NoOneToVisit", person)
               .If(RandomActionAssign[person, ActionType.Visiting], !VisitingFriend[person, __]);

            LocationByActionAssign.If(NoOneToVisit, Home[person, location]);
            LocationByActionAssign.If(VisitingFriend.Assignments, Home[otherPerson, location]);

            // Choose the closest location with the action type assigned
            var OpenForBusinessByAction = Predicate("OpenForBusinessByAction", actionType, location)
                .If(ActionToCategory, AvailableCategory, OpenLocationType, InBusiness, Place.Attributes);
            LocationByActionAssign.If(RandomActionAssign, !In(actionType, new[] { ActionType.StayingIn, ActionType.Visiting }),
                Minimal(location, distance, OpenForBusinessByAction & DistanceFromHome[person, location, distance]));

            WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToSchool);
            WhereTheyAt[person, ActionType.GoingToWork, location].If(GoingToWork);
            WhereTheyAt.If(RandomActionAssign, LocationByActionAssign);

            // *********************************** Interactions: **********************************

            var NotWorking = Predicate("NotWorking", person.Key, location.Indexed)
               .If(WhereTheyAt[person, actionType, location], actionType != ActionType.GoingToWork, !Kid[person]);

            var InteractionPair = Predicate("InteractionPair", person, otherPerson);
            InteractionPair.If(NotWorking[person, location], NotWorking[otherPerson, location], person != otherPerson);
            InteractionPair.If(GoingToWork[person, location], GoingToWork[otherPerson, location], person != otherPerson);

            var PotentialInteracts = MatchAsymmetric("PotentialInteracts", person, otherPerson, score)
               .When(InteractionPair, Friend[person, otherPerson], Romantic[person, otherPerson],
                     Similarity[person, otherPerson, num], score == num * 10)
               .When(InteractionPair, Friend[person, otherPerson], !Romantic[person, otherPerson],
                     Similarity[person, otherPerson, num], score == num * 5)
               .When(InteractionPair, !Friend[person, otherPerson], !Enemy[person, otherPerson], Romantic[person, otherPerson],
                     Similarity[person, otherPerson, num], score == num * 3)
               .When(InteractionPair, Enemy[person, otherPerson], Romantic[person, otherPerson],
                     Similarity[person, otherPerson, num], score == num * 2)
               .When(InteractionPair, Enemy[person, otherPerson], !Romantic[person, otherPerson],
                     Similarity[person, otherPerson, num], score == num / 2)
               .When(InteractionPair, !Friend[person, otherPerson], !Enemy[person, otherPerson], !Romantic[person, otherPerson],
                     Similarity[person, otherPerson, score]);

            var ScoredInteraction = Predicate("ScoredInteraction", person.Indexed, otherPerson.Indexed, score);
            ScoredInteraction[person, otherPerson, RandomNormal].If(PotentialInteracts.Matches);

            var PositiveInteraction = Predicate("PositiveInteraction",
                person.Indexed, otherPerson.Indexed).If(ScoredInteraction, score > 10);
            var NeutralInteraction = Predicate("NeutralInteraction",
                person.Indexed, otherPerson.Indexed).If(ScoredInteraction, score >= -15, score <= 10);
            var NegativeInteraction = Predicate("NegativeInteraction",
                person.Indexed, otherPerson.Indexed).If(ScoredInteraction, score < -15);

            Interaction = Predicate("Interaction", person.Indexed, otherPerson.Indexed, interactionType.Indexed);
            Interaction[person, partner, InteractionType.Flirting].If(PositiveInteraction[person, partner], SexualAttraction);
            Interaction[person, partner, InteractionType.Assisting].If(PositiveInteraction[person, partner], !SexualAttraction[person, partner]);
            Interaction[person, otherPerson, InteractionType.Chatting].If(NeutralInteraction);
            Interaction[person, otherPerson, InteractionType.Arguing].If(NegativeInteraction);
            Interaction.Button("Snapshot", VisualizeInteractions);

            Spark.UpdateWhen(Interaction[person, otherPerson, InteractionType.Flirting], spark == 900);
            Spark.UpdateWhen(Interaction[person, otherPerson, InteractionType.Arguing], SexualAttraction[person, otherPerson], spark == -700);

            Charge.UpdateWhen(Interaction[person, otherPerson, InteractionType.Assisting], charge == 800);
            Charge.UpdateWhen(Interaction[person, otherPerson, InteractionType.Chatting], charge == 80);
            Charge.UpdateWhen(Interaction[person, otherPerson, InteractionType.Arguing], charge == -700);

            // ************************************ END TABLES ************************************
            // ReSharper restore InconsistentNaming
            Simulation.EndPredicates();
            DataflowVisualizer.MakeGraph(Simulation, "Visualizations/Dataflow.dot");
            UpdateFlowVisualizer.MakeGraph(Simulation, "Visualizations/UpdateFlow.dot");
            Simulation.Update(); // optional, not necessary to call Update after EndPredicates
            Simulation.CheckForProblems = true;
        }

        #region Graph visualizations
        private void VisualizeHomes() {
            var g = new GraphViz<object>();
            foreach ((var person, var place) in Home) {
                var color = PlaceColor(place);
                if (!g.Nodes.Contains(place)) {
                    g.Nodes.Add(place);
                    g.NodeAttributes[place] = new Dictionary<string, object> { { "rgbcolor", color } };
                }
                g.AddEdge(new GraphViz<object>.Edge(person, place, true, null,
                                                    new Dictionary<string, object> { { "rgbcolor", color } }));
            }
            TEDGraphVisualization.ShowGraph(g);
        }

        private void VisualizeJobs() {
            var g = new GraphViz<object>();
            foreach (var job in Employment) {
                var place = job.Item3;
                var color = PlaceColor(place);
                if (!g.Nodes.Contains(place)) {
                    g.Nodes.Add(place);
                    g.NodeAttributes[place] = new Dictionary<string, object>() { { "rgbcolor", color } };
                }
                g.AddEdge(new GraphViz<object>.Edge(job.Item2, place, true, job.Item1.ToString(),
                                                    new Dictionary<string, object> { { "rgbcolor", color } }));
            }
            TEDGraphVisualization.ShowGraph(g);
        }

        private void VisualizeRandomFriendNetwork() => VisualizeFriendNetworkOf(
            CharacterAttributes.ColumnValueFromRowNumber(person)((uint)Integer(0, (int)CharacterAttributes.Length)));

        private void VisualizeFriendNetworkOf(Person p) {
            // ReSharper disable InconsistentNaming
            var FriendIndex = (GeneralIndex<((Person,Person), Person, Person,bool), Person>)Friend.IndexFor(person, false);
            var EnemyIndex = (GeneralIndex<((Person,Person), Person, Person,bool), Person>)Enemy.IndexFor(person, false);
            var RomanticInterestIndex = (GeneralIndex<((Person,Person), Person, Person,bool), Person>)Romantic.IndexFor(person, false);

            IEnumerable<(Person, string, string)> FriendsOf(Person person, GeneralIndex<((Person, Person), Person, Person, bool), Person> friendIndex)
                => friendIndex.RowsMatching(p).Select(r => (r.Item3, (string)null, "green"));
            IEnumerable<(Person, string, string)> EnemiesOf(Person person, GeneralIndex<((Person, Person), Person, Person, bool), Person> enemyIndex)
                => enemyIndex.RowsMatching(p).Select(r => (r.Item3, (string)null, "red"));
            IEnumerable<(Person, string, string)> RomanticInterestsOf(Person person, GeneralIndex<((Person, Person), Person, Person, bool), Person> romanticInterestIndex)
                => romanticInterestIndex.RowsMatching(p).Select(r => (r.Item3, (string)null, "blue"));
            IEnumerable<(Person, string, string)> ConnectionsOf(Person person) =>
                FriendsOf(person, FriendIndex).Concat(EnemiesOf(person, EnemyIndex)).Concat(RomanticInterestsOf(person, RomanticInterestIndex));

            var g = TEDGraphVisualization.TraceToDepth<object, Person>(1, p, ConnectionsOf);
            var people = g.Nodes.Cast<Person>().ToArray();
            foreach (var p2 in people) {
                if (!EmploymentIndex.ContainsKey(p2)) continue;
                (var job, _, var company, _) = EmploymentIndex[p2];
                var jobColor = PlaceColor(company);
                if (!g.Nodes.Contains(company)) {
                    g.AddNode(company);
                    g.NodeAttributes[company] = new Dictionary<string, object>() { { "rgbcolor", jobColor } };
                }
                g.AddEdge(new GraphViz<object>.Edge(p2, company, true,
                                                    job.ToString(), new Dictionary<string, object>() { { "rgbcolor", jobColor } }));
            }
            TEDGraphVisualization.ShowGraph(g);
            // ReSharper restore InconsistentNaming
        }

        private void VisualizeFullSocialNetwork() {
            var g = new GraphViz<object>();
            foreach (var r in Friend)
                g.AddEdge(new GraphViz<object>.Edge(r.Item2, r.Item3, true, null,
                    new Dictionary<string, object>() { { "color", "green"}}));
            foreach (var r in Enemy)
                g.AddEdge(new GraphViz<object>.Edge(r.Item2, r.Item3, true, null,
                    new Dictionary<string, object> { { "color", "red"}}));
            foreach (var r in Romantic)
                g.AddEdge(new GraphViz<object>.Edge(r.Item2, r.Item3, true, null,
                    new Dictionary<string, object> { { "color", "blue"}}));
            TEDGraphVisualization.ShowGraph(g);
        }

        private void VisualizeFamilies() {
            var g = new GraphViz<Person>();
            foreach (var p in Parent) {
                var parent = p.Item1;
                var child = p.Item2;
                if (!g.Nodes.Contains(parent)) g.AddNode(parent);
                if (!g.Nodes.Contains(child)) g.AddNode(child);
                g.AddEdge(new GraphViz<Person>.Edge(child, parent));
            }
            TEDGraphVisualization.ShowGraph(g);
        }

        private void VisualizeInteractions() {
            var g = new GraphViz<Person>();
            foreach (var row in Interaction)
                g.AddEdge(new GraphViz<Person>.Edge(
                              row.Item1, row.Item2, true, row.Item3.ToString(), 
                              new Dictionary<string, object> { { "color", row.Item3 switch { 
                                  InteractionType.Arguing => "red", 
                                  InteractionType.Assisting => "green", 
                                  InteractionType.Flirting => "blue", 
                                  InteractionType.Chatting => "yellow", 
                                  _ => "white"
                              } } }));
            TEDGraphVisualization.ShowGraph(g);
        }

        private void VisualizeWhereTheyAt() {
            var g = new GraphViz<object>();
            foreach (var row in WhereTheyAt) {
                var place = row.Item3;
                var color = PlaceColor(place);
                if (!g.Nodes.Contains(place)) {
                    g.Nodes.Add(place);
                    g.NodeAttributes[place] = new Dictionary<string, object>() { { "rgbcolor", color } };
                }
                g.AddEdge(new GraphViz<object>.Edge(row.Item1, place, true, row.Item2.ToString(),
                    new Dictionary<string, object>() {{ "rgbcolor", color }}));
            }
            TEDGraphVisualization.ShowGraph(g);
        }

        private string PersonDescription(Person p) {
            var b = new StringBuilder();
            var info = CharacterAttributes.KeyIndex(person)[p];
            var dead = info.Item6 == VitalStatus.Dead;
            var living = dead ? "Dead" : "Living";
            var job = "Unemployed";
            if (EmploymentIndex.ContainsKey(p)) job = EmploymentIndex[p].Item1.ToString();
            b.Append(dead ? "<color=grey>" : "");
            b.Append("<b>");
            b.Append(p.FullName);
            b.AppendLine("</b><size=24>");
            b.AppendFormat("{0} {1}, age: {2}\n", living, info.Item4.ToString().ToLower(), info.Item2);
            b.AppendLine(info.Item5.ToString());
            b.AppendLine(job);
            return b.ToString();
        }
        #endregion

        public static void UpdateSimulator() {
#if ParallelUpdate
            if (update == null) LoopSimulator();
#else
            Time.Tick();
            Simulation.Update();
            PopTableIfNewActivity(Simulation.Problems);
            PopTableIfNewActivity(Simulation.Exceptions);
#endif
        }

#if ParallelUpdate
        private static Task update;

        static void LoopSimulator(){
            Time.Tick();
            Simulation.Update();
            update = Simulation.UpdateAsync().ContinueWith((_) => LoopSimulator());
        }
#endif
    }
}
