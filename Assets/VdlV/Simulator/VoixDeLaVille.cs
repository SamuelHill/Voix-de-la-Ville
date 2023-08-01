using System.Collections.Generic;
using System.IO;
using TED;
using TED.Interpreter;
using TED.Tables;
using TED.Utilities;
using VdlV.Simulog;
using VdlV.TextGenerator;
using VdlV.Time;
using VdlV.Unity;
using VdlV.Utilities;
using VdlV.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace VdlV.Simulator {
    using WhereTheyAtIndex = GeneralIndex<(Person, ActionType, Location), Location>;
    using static ActionType;
    using static BusinessStatus;
    using static Favorability;
    using static InteractionType;
    using static LocationType;
    using static Month;
    using static Sex;
    using static TimeOfDay;
    using static VitalStatus;
    // "Utils" (Utilities and Time)
    using static BindingList;    // Parameters for name generation
    using static Calendar;       // Prob per interval type
    using static Clock;          // All current time functions
    using TimePoint = TimePoint; // (clock hides TimePoint with a method, this fixes that)
    using TimeOfDay = TimeOfDay; // (clock hides TimeOfDay)
    using static CsvParsing;     // DeclareParsers
    using static File;           // Performance CSV output
    using static Generators;     // Name generation
    using static Randomize;      // Seed and RandomElement
    // GUI/graphics
    using static Color;
    using static GUIManager;       // Colorizers and Pop tables
    using static SimulationGraphs; // Visualize___ functions
    // The following offload static components of the TED code...
    using static Sims; // C# function hookups to TED predicates
    using static Town; // C# function hookups to TED predicates
    using static StaticTables; // non dynamic tables - classic datalog EDB
    using static Variables;    // named variables
    // TED Meta language hookup
    using static SimuLang;

    public static class VoixDeLaVille {
        private const int Seed = 349571286;
        public static Simulation Simulation = null!;
        public static bool RecordingPerformance;
        private static readonly List<(uint, uint, float)> PerformanceData = new();

        static VoixDeLaVille() {
            DeclareParsers(); // Parsers used in the FromCsv calls in InitStaticTables
            Seed(Seed, Seed);
            BindGlobal(TownName, PossibleTownName.Random);
            BindGlobal(RandomNumber, "");
            SetDefaultColorizers();
            SetDescriptionMethods();
            if (RecordingPerformance) {
                using var file = CreateText("PerformanceData.csv");
            }
        }

        // ReSharper disable InconsistentNaming
        // Tables, despite being local or private variables, will be capitalized for style/identification purposes.

        #region Tables and Indexers for GUI and Graph visuals
        public static TablePredicate<Person, int, Date, Sex, Sexuality, VitalStatus> CharacterAttributes;
        public static Event<Person, Person, InteractionType> Interaction;
        public static TablePredicate<Person, ActionType, Location> WhereTheyAt;
        public static TablePredicate<Person, Location> Home;
        public static TablePredicate<Person, Person> Parent;
        public static AffinityRelationship<Person, Person> Friend;
        public static AffinityRelationship<Person, Person> Enemy;
        public static AffinityRelationship<Person, Person> Romantic;
        public static TablePredicate<Vocation, Person, Location, TimeOfDay> Employment;
        public static KeyIndex<(Vocation, Person, Location, TimeOfDay), Person> EmploymentIndex;
        public static TablePredicate<Vector2Int, Location, LocationType> CreatedLocation;
        public static TablePredicate<Vector2Int> VacatedLocation;
        public static TablePredicate<Person> Buried;
        public static KeyIndex<(bool, int), bool> PopulationCountIndex;
        public static WhereTheyAtIndex WhereTheyAtLocationIndex;
        public static KeyIndex<(Vector2Int, Location, LocationType, TimePoint), Vector2Int> LocationsPositionIndex;
        public static ColumnAccessor<LocationType, Location> LocationToType;
        #endregion

        public static void InitSimulator() {
            Simulation = new Simulation("Talk of the Town");
            Simulation.Exceptions.Colorize(_ => red);
            Simulation.Problems.Colorize(_ => red);
            Simulation.BeginPredicates();
            InitStaticTables();

            // ************************************ Characters ************************************

            var Character = Exists("Character", person, age, dateOfBirth.Indexed, 
                                   sex.Indexed, sexuality, vitalStatus.Indexed, birthday)
               .InitiallyWhere(PrimordialBeing[person, age, dateOfBirth, __, __], TimeOfBirth[dateOfBirth, age, birthday]);
            Character.Attributes.Initially[person, age, dateOfBirth, sex, sexuality, Alive].Where(PrimordialBeing);

            CharacterAttributes = Character.Attributes;
            Character.Attributes.Colorize(vitalStatus);
            Character.Attributes.TableButton("Random friend network", VisualizeRandomFriendNetwork);
            Character.Attributes.TableButton("Full network", VisualizeFullSocialNetwork);

            #region Character helpers (Definitions)
            // TODO : Replace naming helpers with TextGenerator
            var RandomFirstName = Definition("RandomFirstName", sex, firstName);
            RandomFirstName[Male, firstName].If(RandomElement(MaleNames, firstName));
            RandomFirstName[Female, firstName].If(RandomElement(FemaleNames, firstName));
            // Surname here is only being used to facilitate A naming convention for last names (currently paternal lineage)
            var RandomPerson = Definition("RandomPerson", sex, person)
               .Is(RandomFirstName, RandomElement(Surnames, lastName), NewPerson[firstName, lastName, person]);

            PopulationCountIndex = Character.CountIndex;
            var Population = Definition("Population", count).Is(Character.Count[true, count]);

            // Check to vital status - same as also saying Character[person] but with less extraneous referencing
            var Age = Definition("Age", person, age)
               .Is(Character.Attributes[person, age, __, __, __, Alive]);
            var Man = Definition("Men", man)
               .Is(Character.Attributes[man, age, __, Male, __, Alive], age >= 18);
            var Woman = Definition("Women", woman)
               .Is(Character.Attributes[woman, age, __, Female, __, Alive], age >= 18);
            var PersonSex = Definition("PersonSex", person, sex)
               .Is(Character.Attributes[person, __, __, sex, __, Alive]);
            var PersonSexuality = Definition("PersonSexuality", person, sexuality)
               .Is(Character.Attributes[person, __, __, __, sexuality, Alive]);
            #endregion

            var Aptitude = Character.FeaturesMultiple("Aptitude", job.Indexed, aptitude);
            Aptitude.Initially[person, job, RandomNormalSByte].Where(PrimordialBeing, Jobs);
            Aptitude.Add[person, job, RandomNormalSByte].If(Character.Add, Jobs);

            Character.EndWhen(Age, age > 60, PerMonth(0.003f))
                     .EndCauses(Set(Character.Attributes, person, vitalStatus, Dead));

            var Drifter = Predicate("Drifter", person, sex, sexuality);
            Drifter[person, RandomSex, sexuality].If(PerYear(0.05f), RandomPerson, RandomSexuality[sex, sexuality]);
            Character.StartWithTime(Drifter, TimeOfBirth[RandomDate, RandomAdultAge, birthday])
                     .StartWithCauses(Add(Character.Attributes[person, YearsOld[birthday], 
                                                               TimePointToDate[birthday], sex, sexuality, Alive]).If(Drifter));

            // *********************************** Relationships **********************************
            // TODO : Primordial Relationships?
            // TODO : Married couples - last name changes

            var Charge = Affinity("Charge", pairing, person, otherPerson, charge).Decay(0.8f);
            Friend = Charge.Relationship(nameof(Friend), state, 5000, 4000);
            Enemy = Charge.Relationship(nameof(Enemy), state, -6000, -3000);

            var Spark = Affinity("Spark", pairing, person, otherPerson, spark).Decay(0.1f);
            Romantic = Spark.Relationship(nameof(Romantic), state, 7000, 6000);

            var MutualFriendship = Definition("MutualFriendship", person, otherPerson)
               .Is(Friend[person, otherPerson], Friend[otherPerson, person]);
            var MutualRomanticInterest = Definition("MutualRomanticInterest", person, otherPerson)
               .Is(Romantic[person, otherPerson], Romantic[otherPerson, person]);

            var Lover = ExclusiveRelationship("Lover", symmetricPair, person, otherPerson, state)
                       .StartWhen(MutualRomanticInterest, MutualFriendship)
                       .EndWhen(Character.End, Character[otherPerson]);

            Parent = Predicate("Parent", parent.Indexed, child.Indexed);
            Parent.TableButton("Visualize", VisualizeFamilies);
            var FamilialRelation = Definition("FamilialRelation", person, otherPerson)
               .Is(Parent[person, otherPerson] | Parent[otherPerson, person]); // only immediate family

            var SexualAttraction = Definition("SexualAttraction", person, partner)
               .Is(PersonSexuality[person, sexuality], PersonSex[partner, sexOfPartner], AttractedTo[sexuality, sexOfPartner],
                   PersonSexuality[partner, sexualOfPartner], PersonSex[person, sex], AttractedTo[sexualOfPartner, sex],
                   !FamilialRelation[person, partner]);

            // ************************************ Procreation ***********************************
            // TODO : Limit PotentialProcreation by interactions
            // TODO : Limit PotentialProcreation by Personality
            // TODO : Limit PotentialProcreation by time since last birth/number of children

            var Embryo = Exists("Embryo", child, woman.Indexed, man, sex, conception.Indexed);
            var Pregnant = Definition("Pregnant", woman)
               .Is(Embryo.Attributes[child, woman, __, __, __], Embryo[child]);

            var PotentialProcreation = Predicate("PotentialProcreation", woman, man.Indexed);
            var Procreation = Event("Procreation", woman, man, sex, child)
               .OccursWhen(PotentialProcreation[woman, man], RandomSex[sex], RandomFirstName, NewPerson[firstName, Surname[man], child]);

            Embryo.StartWhen(Procreation)
                  .StartCauses(Add(Embryo.Attributes[child, woman, man, sex, CurrentDate]).If(Procreation))
                  .EndWhen(Embryo[child], Embryo.Attributes, NineMonthsPast[conception], Prob[0.8f])
                  .EndCauses(Add(Parent[parent, child]).If(Embryo.Attributes[child, parent, __, __, __]),
                             Add(Parent[parent, child]).If(Embryo.Attributes[child, __, parent, __, __]));

            Character.StartWhen(Embryo.End[person]);
            Character.StartCauses(Add(CharacterAttributes[person, 0, CurrentDate, sex, sexuality, Alive])
                                     .If(Embryo.End[person], Embryo.Attributes[person, __, __, sex, __], RandomSexuality[sex, sexuality]));
            // Increment age once per birthday (in the AM, if you weren't just born)
            CharacterAttributes.Set(person, age, num)
                 .If(CharacterAttributes[person, age, dateOfBirth, __, __, Alive],
                     CurrentlyMorning, IsToday[dateOfBirth], !Embryo.End[person], Incr[age, num]);

            // ************************************ Locations *************************************

            var Place = Exists("Place", location, locationType.Indexed, 
                locationCategory.Indexed, position.Indexed, businessStatus.Indexed, founding).InitiallyWhere(PrimordialLocation);
            Place.Attributes.Initially[location, locationType, locationCategory, position, InBusiness]
                         .Where(PrimordialLocation, LocationInformation);

            var PlaceInBusiness = Definition("PlaceInBusiness", location)
               .Is(Place.Attributes[location, __, __, __, InBusiness]);
            var IsVacant = Definition("IsVacant", position)
               .Is(!Place.Attributes[__, __, __, position, InBusiness]);
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
               .If(Place.Attributes[location, locationType, __, position, InBusiness], 
                   Place[location, true, founding, TimePoint.Eschaton]);
            DisplayLocations.Colorize(location);
            LocationsPositionIndex = DisplayLocations.KeyIndex(position);
            LocationToType = Place.Attributes.Accessor(location, locationType);
            Place.Attributes.Colorize(location);
            #endregion

            Place.StartWhen(CreatedLocation)
                 .StartCauses(Add(Place.Attributes[location, locationType, locationCategory, position, InBusiness])
                                 .If(CreatedLocation, LocationInformation))
                 .EndWhen(Place.Attributes, !In(locationType, permanentLocationTypes),
                          Place, YearsOld[founding, age], age > 40, PerYear(0.1f))
                 .EndCauses(Set(Place.Attributes, location, businessStatus, OutOfBusiness));

            var NumLocations = CountsBy("NumLocations", Place.Attributes, locationType, count);
            var CategoryCount = CountsBy("CategoryCount", Place.Attributes, locationCategory, count);
            var AvailableCategory = Predicate("AvailableCategory", locationCategory).If(CategoryCount);

            // ************************************** Housing *************************************
            // TODO : Include ApartmentComplex locations in Housing logic
            // TODO : Include Inn locations in Housing logic - drifters start at an Inn

            Home = Predicate("Home", occupant.Key, location.Indexed);
            Home.Unique = true;
            Home.TableButton("Visualize", VisualizeHomes);

            var Occupancy = Counts("Occupancy", location, occupant)
                .By(Place.Attributes[location, House, __, __, InBusiness], Home);
            var Unoccupied = Predicate("Unoccupied", location)
                .If(Place.Attributes[location, House, __, __, InBusiness], !Home[__, location]);
            var UnderOccupied = Predicate("UnderOccupied", location);
            UnderOccupied.If(Occupancy.Count, count <= 5);
            UnderOccupied.If(Unoccupied);

            var Unhoused = Predicate("Unhoused", person).If(Character[person], !Home[person, __]);

            // Using this to randomly assign one house per primordial person...
            var PrimordialHouse = Predicate("PrimordialHouse", location)
                .If(PrimordialLocation[location, House, __, __]);
            Home.Initially.Where(PrimordialBeing[occupant, __, __, __, __],
                RandomElement(PrimordialHouse, location));

            Home.Add.If(Embryo.End[occupant], Embryo.Attributes[occupant, woman, __, __, __], Home[woman, location]); // Move in with mom
            // If no UnderOccupied homes this wont get set...
            Home.Add.If(Drifter[occupant, __, __], RandomElement(UnderOccupied, location));
            Home.Add.If(Unhoused[occupant], RandomElement(UnderOccupied, location));

            Home.Set(occupant, location).If(Character.End[occupant],
                Place.Attributes[location, Cemetery, __, __, InBusiness]);
            var BuriedAt = Predicate("BuriedAt", occupant, location)
                .If(Place.Attributes[location, Cemetery, __, __, InBusiness], Home);
            Buried = Predicate("Buried", person).If(BuriedAt[person, __]);

            var ForeclosedUpon = Event("ForeclosedUpon", occupant)
                                .OccursWhen(Home, Place.End)
                                .Causes(Set(Home, occupant, location, location)
                                           .If(RandomElement(UnderOccupied, location)));

            // Distance per person makes most sense when measured from either where the person is,
            // or where they live. This handles the latter:
            var DistanceFromHome = Definition("DistanceFromHome", person, location, distance)
                .Is(Place.Attributes[location, __, __, position, InBusiness],
                    Home[person, otherLocation],
                    Place.Attributes[otherLocation, __, __, otherPosition, InBusiness],
                    Distance[position, otherPosition, distance]);

            var WantToMove = Predicate("WantToMove", person).If(Home[person, location], Occupancy.Count, count > 7);

            var CanMoveToFamily = Assign("CanMoveToFamily", person, location)
               .When(WantToMove, Home[person, otherLocation], FamilialRelation, 
                     Home[otherPerson, location], location != otherLocation, Occupancy.Count, count < 7);
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
            Employment.TableButton("Visualize", VisualizeJobs);

            var EmploymentStatus = Predicate("EmploymentStatus", employee.Key, state.Indexed);
            EmploymentStatus.Add[employee, true].If(Employment.Add);
            EmploymentStatus.Set(employee, state, false).If(Character.End[employee], EmploymentStatus[employee, true]);
            EmploymentStatus.Set(employee, state, false).If(Place.End[location], Employment, EmploymentStatus[employee, true]);

            var StillEmployed = Definition("StillEmployed", person).Is(EmploymentStatus[person, true]);

            var JobToFill = Predicate("JobToFill", location, job)
                .If(CurrentTimeOfDay[timeOfDay], PlaceInBusiness, Place.Attributes, VocationShift,
                    PositionsPerJob, Count(Employment & EmploymentStatus[employee, true]) < positions);

            Drifter[person, RandomSex, sexuality].If(JobToFill, PerMonth(0.5f), RandomPerson, RandomSexuality[sex, sexuality]);

            var BestCandidate = Predicate("BestCandidate", person, job, location)
                .If(JobToFill, Maximal(person, aptitude, And[Age, !StillEmployed[person], age >= 18, Aptitude]));
            var CandidateForJob = Assign("CandidateForJob", person, job).When(BestCandidate);
            var CandidatePerJob = Assign("CandidatePerJob", person, location).When(CandidateForJob.Assignments, BestCandidate);

            Employment.Add[job, person, location, CurrentTimeOfDay].If(CandidatePerJob.Assignments, CandidateForJob.Assignments);

            // ********************************** New Locations ***********************************

            #region New Location Macros
            Goal OnlyLocationOfType(Goal goal, LocationType locType, bool onlyOne) => onlyOne ?
                !Place.Attributes[__, locType, __, __, InBusiness] & goal : goal;

            void LocationFromGenerator(LocationType locType, bool onlyOne, params Goal[] readyToAdd) =>
                NewPlace[LocationNames[locType].GenerateRandom, locType]
                   .If(OnlyLocationOfType(And[readyToAdd], locType, onlyOne));

            void OnlyLocation(LocationType locType, params Goal[] readyToAdd) =>
                LocationFromGenerator(locType, true, readyToAdd);

            void OnlyWhenPopOf(LocationType locType, int population) => 
                OnlyLocation(locType, Population[count], count >= population);

            void MultipleLocations(LocationType locType, params Goal[] readyToAdd) =>
                LocationFromGenerator(locType, false, readyToAdd);

            void PerCapita(LocationType locType, int perPopulation) {
                OnlyWhenPopOf(locType, perPopulation);
                MultipleLocations(locType, Population[count], 
                                  NumLocations[locType, num], count / perPopulation > num);
            }
            #endregion

            // Need to decide on the position in NewPlace if we want to name based on position
            // NewPlace[locationName, LocationType.House]
            //    .If(Once[WantToMove[__] | Unhoused[__]],
            //        NamePosition[position, locationName]); 

            // Start with, and always keep one in business
            OnlyLocation(PostOffice, True);
            OnlyLocation(Park, True);
            OnlyLocation(CarpentryCompany, True);
            // Spawn once, as needed
            OnlyLocation(Cemetery, Once[And[Age, age >= 60]]); // before anyone can die
            OnlyLocation(DayCare, Count(Age & (age < 6)) > 5);
            OnlyLocation(School, Count(Age & (age >= 5) & (age < 18)) > 5);
            // Spawn once a competent enough individual comes along
            OnlyLocation(DoctorOffice, Once[And[Aptitude[person, Vocation.Doctor, aptitude], aptitude > 20, Age, age > 30]]);
            OnlyLocation(DentistOffice, Once[And[Aptitude[person, Vocation.Dentist, aptitude], aptitude > 22, Age, age > 30]]);
            // Spawn once the population reaches num
            OnlyWhenPopOf(LumberMill, 50);
            OnlyWhenPopOf(Brewery, 80);
            OnlyWhenPopOf(CommunityCenter, 100);
            OnlyWhenPopOf(JewelryShop, 170);
            OnlyWhenPopOf(CityHall, 200);
            OnlyWhenPopOf(TattooParlor, 220);
            OnlyWhenPopOf(Hospital, 240);
            OnlyWhenPopOf(DepartmentStore, 300);

            // Houses whenever there is a need for housing
            MultipleLocations(House, Once[WantToMove[__] | Unhoused[__]]);
            // Spawn once per population density
            PerCapita(Bar, 35);
            PerCapita(Diner, 55);
            PerCapita(Bakery, 125);
            PerCapita(ButcherShop, 170);
            PerCapita(CandyShop, 250);
            PerCapita(GroceryStore, 95);
            PerCapita(GeneralStore, 150);
            PerCapita(Barbershop, 250);
            PerCapita(TailorShop, 100);
            PerCapita(Farm, 160);
            PerCapita(Orchard, 220);

            // ************************************* Movement: ************************************
            // TODO : NoOneToVisit change action assignment to StayingIn
            // TODO : NeedDayCare but not GoingToSchool follow a non-working parent
            // TODO : Couple movements - i.e. GoingOutForDateNight

            var OpenLocationType = Predicate("OpenLocationType", locationType)
                .If(LocationInformation, CurrentlyOperating[operation], CurrentlyOpen[schedule]);
            var OpenForBusiness = Predicate("OpenForBusiness", actionType.Indexed, location)
               .If(ActionToCategory, AvailableCategory, OpenLocationType, PlaceInBusiness, Place.Attributes);
            var ActionCount = CountsBy("ActionCount", OpenForBusiness, actionType, count);
            var AvailableAction = Predicate("AvailableAction", actionType)
               .If(ActionCount[actionType, __]);
            var IndividualActions = Predicate("IndividualActions", actionType)
               .If(AvailableAction, !In(actionType, new[] { GoingToSchool, GoingOutForDateNight }));
            var DrifterActions = Predicate("DrifterActions", actionType)
               .If(IndividualActions, !In(actionType, new[] { StayingIn, Visiting }));

            var NeedSchooling = Predicate("NeedSchooling", person).If(Age, age < 18, age > 6);
            var NeedDayCare = Predicate("NeedDayCare", person).If(Age, age <= 6);

            var AtSchool = Predicate("AtSchool", person, location)
                .If(OpenLocationType[DayCare], NeedDayCare, Place.Attributes[location, DayCare, __, __, InBusiness])
                .If(OpenLocationType[School], NeedSchooling, Place.Attributes[location, School, __, __, InBusiness]);

            var Working = Predicate("Working", person, location)
                .If(OpenLocationType, PlaceInBusiness, Place.Attributes, StillEmployed,
                    Employment[__, person, location, CurrentTimeOfDay]);

            var AvailableIndividual = Predicate("AvailableIndividual", person)
               .If(Character[person], !Working[person, __], !AtSchool[person, __]);

            var AvailablePair = Predicate("AvailablePair", symmetricPair, person, otherPerson)
               .If(AvailableAction[GoingOutForDateNight], AvailableIndividual[person], AvailableIndividual[otherPerson], 
                   person != otherPerson, MutualRomanticInterest[person, otherPerson], 
                   SymmetricTuple<Person>.NewSymmetricTuple[person, otherPerson, symmetricPair],
                   SymmetricTuple<Person>.InOrder[symmetricPair, person, otherPerson]);

            var ScoredPairing = Predicate("ScoredPairing", symmetricPair, rate);
            ScoredPairing[symmetricPair, 0.25f].If(AvailablePair, Lover[symmetricPair, __, __, true]);
            ScoredPairing[symmetricPair, 0.1f].If(AvailablePair, !Lover[symmetricPair, __, __, true]);

            var tempToMatch = Predicate("tempToMatch", person, otherPerson, score);
            tempToMatch.If(ScoredPairing, Prob[rate], AvailablePair, RandomNormal[num], score == (num + 50));
            var dateGoers = MatchGreedily("dateGoers", tempToMatch);

            var DateLocations = Predicate("DateLocations", location)
               .If(OpenForBusiness[GoingOutForDateNight, location]);
            var LocationOfDates = Predicate("LocationOfDates", person, otherPerson, location)
               .If(dateGoers, RandomElement(DateLocations, location));

            var NeedsActionAssignment = Predicate("NeedsActionAssignment", person)
               .If(AvailableIndividual, !dateGoers[person, __], !dateGoers[__, person]);
            var UnhousedActionAssignment = Predicate("UnhousedActionAssignment", person)
                .If(NeedsActionAssignment, Unhoused).If(Character.StartWith[person, __]);
            var RandomActionAssign = Predicate("RandomActionAssign", person, actionType)
                .If(NeedsActionAssignment, !UnhousedActionAssignment[person], RandomElement(IndividualActions, actionType))
                .If(UnhousedActionAssignment, RandomElement(DrifterActions, actionType));

            var VisitingFriend = Assign("VisitingFriend", person, otherPerson)
               .When(RandomActionAssign[person, Visiting], MutualFriendship, !Unhoused[otherPerson]);
            var NoOneToVisit = Predicate("NoOneToVisit", person)
               .If(RandomActionAssign[person, Visiting], !VisitingFriend[person, __]);

            var LocationByActionAssign = Predicate("LocationByActionAssign", person, location);
            LocationByActionAssign.If(RandomActionAssign[person, StayingIn], Home[person, location]);
            LocationByActionAssign.If(NoOneToVisit, Home[person, location]);
            LocationByActionAssign.If(VisitingFriend.Assignments, Home[otherPerson, location]);
            // Choose the closest location with the action type assigned
            LocationByActionAssign.If(RandomActionAssign, !UnhousedActionAssignment[person], !In(actionType, new[] { StayingIn, Visiting }), 
                                      Minimal(location, distance, OpenForBusiness & DistanceFromHome[person, location, distance]));
            LocationByActionAssign.If(RandomActionAssign, UnhousedActionAssignment,
                                      Minimal(location, distance, OpenForBusiness & RandomNormal[distance]));

            WhereTheyAt = Predicate("WhereTheyAt", person.Key, actionType.Indexed, location.Indexed);
            WhereTheyAt.Colorize(location);
            WhereTheyAt.TableButton("Snapshot", VisualizeWhereTheyAt);
            WhereTheyAtLocationIndex = (WhereTheyAtIndex)WhereTheyAt.IndexFor(location, false);
            WhereTheyAt[person, GoingToSchool, location].If(AtSchool);
            WhereTheyAt[person, GoingToWork, location].If(Working);
            WhereTheyAt[person, GoingOutForDateNight, location].If(LocationOfDates[person, __, location]);
            WhereTheyAt[person, GoingOutForDateNight, location].If(LocationOfDates[__, person, location]);
            WhereTheyAt.If(RandomActionAssign, LocationByActionAssign);

            // This relies on the mom having a house when the baby is born
            WhereTheyAt[person, StayingIn, location].If(Character.Start[person], 
                                                        Embryo.Attributes[person, woman, __, __, __], Home[woman, location]);

            //WhereTheyAt.Problem("Not everyone moved").If(Character[person], !WhereTheyAt[person, __, __]);

            // *********************************** Interactions: **********************************

            var NotWorking = Predicate("NotWorking", person.Key, location.Indexed)
               .If(WhereTheyAt[person, actionType, location], 
                   !In(actionType, new[] { GoingToWork, GoingToSchool, GoingOutForDateNight }), Age, age >= 18);
            var InteractionPair = Predicate("InteractionPair", person, otherPerson);
            InteractionPair.If(NotWorking[person, location], NotWorking[otherPerson, location], person != otherPerson);
            InteractionPair.If(Working[person, location], Working[otherPerson, location], person != otherPerson);

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

            var InteractFavorability = Predicate("InteractFavorability", 
                person.Indexed, otherPerson.Indexed, favorability);
            InteractFavorability[person, otherPerson, Favorable].If(PotentialInteracts.Matches);

            var IsPlatonic = Predicate("IsPlatonic", person, otherPerson, favorability)
                            .If(InteractFavorability, !SexualAttraction[person, otherPerson])
                            .If(InteractFavorability, SexualAttraction[person, otherPerson], Prob[0.25f]);
            var IsRomantic = Predicate("IsRomantic", person, otherPerson, favorability)
               .If(InteractFavorability, !IsPlatonic[person, otherPerson, __]);

            TableGoal PlatonicFavorability(Favorability favorability) => IsPlatonic[person, otherPerson, favorability];
            TableGoal RomanticFavorability(Favorability favorability) => IsRomantic[person, otherPerson, favorability];

            Interaction = Event("Interaction", person.Indexed, otherPerson.Indexed, interactionType.Indexed);
            Interaction.TableButton("Snapshot", VisualizeInteractions);

            TableGoal InteractionOfType(InteractionType interaction) => Interaction[person, otherPerson, interaction];

            InteractionOfType(Empathizing).If(PlatonicFavorability(MostPositive), Friend[person, otherPerson]);
            InteractionOfType(Assisting).If(PlatonicFavorability(MostPositive), !Friend[person, otherPerson]);
            InteractionOfType(Complimenting).If(PlatonicFavorability(Positive));
            InteractionOfType(Chatting).If(PlatonicFavorability(Neutral));
            InteractionOfType(Insulting).If(PlatonicFavorability(Negative), !Enemy[person, otherPerson]);
            InteractionOfType(Arguing).If(PlatonicFavorability(MostNegative), !Enemy[person, otherPerson]);
            InteractionOfType(Fighting).If(PlatonicFavorability(Negative), Enemy[person, otherPerson]);
            InteractionOfType(Dueling).If(PlatonicFavorability(MostNegative), Enemy[person, otherPerson]);

            InteractionOfType(Courting).If(RomanticFavorability(MostPositive));
            InteractionOfType(Flirting).If(RomanticFavorability(Positive));
            InteractionOfType(Chatting).If(RomanticFavorability(Neutral));
            InteractionOfType(Negging).If(RomanticFavorability(Negative));
            InteractionOfType(Insulting).If(RomanticFavorability(MostNegative));

            PotentialProcreation.If(dateGoers[woman, man] | dateGoers[man, woman],
                                    Woman, Man, !Pregnant[woman], Age[woman, age], Prob[FertilityRate[age]]);

            InteractionOfType(Snogging).If(dateGoers[person, otherPerson],
                                           Once[!PotentialProcreation[person, otherPerson] | !PotentialProcreation[otherPerson, person]]);
            InteractionOfType(Snogging).If(dateGoers[otherPerson, person], 
                Once[!PotentialProcreation[person, otherPerson] | !PotentialProcreation[otherPerson, person]]);
            InteractionOfType(Procreating).If(PotentialProcreation[person, otherPerson]);
            InteractionOfType(Procreating).If(PotentialProcreation[otherPerson, person]);

            Charge.UpdateWhen(InteractionOfType(Empathizing), charge == 900);
            Charge.UpdateWhen(InteractionOfType(Assisting), charge == 800);
            Charge.UpdateWhen(InteractionOfType(Complimenting), charge == 300);
            Charge.UpdateWhen(InteractionOfType(Chatting), charge == 80);
            Charge.UpdateWhen(InteractionOfType(Insulting), charge == -250);
            Charge.UpdateWhen(InteractionOfType(Arguing), charge == -750);
            Charge.UpdateWhen(InteractionOfType(Fighting), charge == -900);
            Charge.UpdateWhen(InteractionOfType(Dueling), charge == -1200);

            Spark.UpdateWhen(InteractionOfType(Procreating), spark == 1500);
            Spark.UpdateWhen(InteractionOfType(Snogging), spark == 1200);
            Spark.UpdateWhen(InteractionOfType(Courting), spark == 900);
            Spark.UpdateWhen(InteractionOfType(Flirting), spark == 150);
            Spark.UpdateWhen(InteractionOfType(Negging), spark == -120);
            Spark.UpdateWhen(InteractionOfType(Insulting), IsRomantic, spark == -750);

            // ************************************ END TABLES ************************************
            // ReSharper restore InconsistentNaming
            Simulation.EndPredicates();
            DataflowVisualizer.MakeGraph(Simulation, "Visualizations/Dataflow.dot");
            UpdateFlowVisualizer.MakeGraph(Simulation, "Visualizations/UpdateFlow.dot");
            Simulation.Update(); // optional, not necessary to call Update after EndPredicates
            Simulation.CheckForProblems = true;
        }

        public static void UpdateSimulator() {
#if ParallelUpdate
            if (update == null) LoopSimulator();
#else
            Tick();
            if (RecordingPerformance) {
                PerformanceData.Add((WhereTheyAt.Length, ClockTick - InitialClockTick, Simulation.RuleExecutionTime));
                if (Day() == 1 && Month() == January) {
                    using var file = AppendText("PerformanceData.csv");
                    foreach ((var population, var clock, var execution) in PerformanceData)
                        file.WriteLine($"{population}, {clock}, {execution}");
                    PerformanceData.Clear();
                }
            }
            Simulation.Update();
            PopTableIfNewActivity(Simulation.Problems);
            PopTableIfNewActivity(Simulation.Exceptions);
#endif
        }

#if ParallelUpdate
        private static Task update;

        static void LoopSimulator(){
            Clock.Tick();
            Simulation.Update();
            update = Simulation.UpdateAsync().ContinueWith((_) => LoopSimulator());
        }
#endif
    }
}
