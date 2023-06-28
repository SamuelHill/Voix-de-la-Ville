using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        }
        public TalkOfTheTown(ushort tick = 1) : this() => Time = new Time(tick);
        public TalkOfTheTown(Month month, byte day = 1, TimeOfDay time = TimeOfDay.AM) :
            this() => Time = new Time(month, day, time);

        // ReSharper disable InconsistentNaming
        // Tables, despite being local or private variables, will be capitalized for style/identification purposes.

        #region Tables and Indexers for GUI and Graph visuals
        private TablePredicate<Person, int, Date, Sex, Sexuality, VitalStatus> CharacterFeatures;
        private TablePredicate<Person, Person, InteractionType> Interaction;
        private TablePredicate<Person, ActionType, Location> WhereTheyAt;
        private TablePredicate<Person, Location> Home;
        private TablePredicate<Person, Person> Parent;
        private AffinityRelationship<Person, Person> Friend;
        private AffinityRelationship<Person, Person> Enemy;
        private AffinityRelationship<Person, Person> RomanticInterest;
        private TablePredicate<Vocation, Person, Location, TimeOfDay> Employment;
        private KeyIndex<(Vocation, Person, Location, TimeOfDay), Person> EmploymentIndex;
        public TablePredicate<Vector2Int, Location, LocationType, TimePoint> NewLocation;
        public TablePredicate<Vector2Int> VacatedLocation;
        public TablePredicate<Person> Buried;
        public KeyIndex<(bool, int), bool> PopulationCountIndex;
        public GeneralIndex<(Person, ActionType, Location), Location> WhereTheyAtLocationIndex;
        public KeyIndex<(Vector2Int, Location, LocationType, TimePoint), Vector2Int> LocationsPositionIndex;
        private ColumnAccessor<LocationType, Location> LocationToType;

        // the only place LocationToType is used...
        private Color PlaceColor(Location place) => LocationColorsIndex[LocationToType[place]].Item2;
        #endregion

        public void InitSimulator() {
            Simulation = new Simulation("Talk of the Town");
            Simulation.Exceptions.Colorize(_ => Color.red);
            Simulation.Problems.Colorize(_ => Color.red);
            SetDefaultColorizer<Location>(PlaceColor);
            Simulation.BeginPredicates();
            InitStaticTables();

            // ************************************ Characters ************************************

            var Character = Exists("Character", person, birthday)
               .InitiallyWhere(PrimordialBeing[person, age, dateOfBirth, __, __], DateAgeToTimePoint[dateOfBirth, age, birthday]);
            CharacterFeatures = Character.Features(age, dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed);
            CharacterFeatures.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeing);
            CharacterFeatures.Colorize(vitalStatus, s => s == VitalStatus.Alive ? Color.white : Color.gray);
            CharacterFeatures.Button("Random friend network", VisualizeRandomFriendNetwork);
            CharacterFeatures.Button("Full network", VisualizeFullSocialNetwork);
            Graph.SetDescriptionMethod<Person>(PersonDescription);
            PopulationCountIndex = Character.CountIndex;

            #region Character Helpers
            // TODO : Replace naming helpers with TextGenerator
            var RandomFirstName = Definition("RandomFirstName", sex, firstName);
            RandomFirstName[Sex.Male, firstName].If(RandomElement(MaleNames, firstName));
            RandomFirstName[Sex.Female, firstName].If(RandomElement(FemaleNames, firstName));
            // Surname here is only being used to facilitate A naming convention for last names (currently paternal lineage)
            var RandomPerson = Definition("RandomPerson", sex, person)
               .Is(RandomFirstName, RandomElement(Surnames, lastName), NewPerson[firstName, lastName, person]);

            var Population = Definition("Population", count).Is(Character.Count[true, count]);
            // Check to vital status - same as also saying Character[person] but with less extraneous referencing
            var Age = Definition("Age", person, age)
               .Is(CharacterFeatures[person, age, __, __, __, VitalStatus.Alive]);
            var PersonSex = Definition("PersonSex", person, sex)
               .Is(CharacterFeatures[person, __, __, sex, __, VitalStatus.Alive]);
            var PersonSexuality = Definition("PersonSexuality", person, sexuality)
               .Is(CharacterFeatures[person, __, __, __, sexuality, VitalStatus.Alive]);
            var SexualAttraction = Definition("SexualAttraction", person, partner)
               .Is(PersonSexuality[person, sexuality], PersonSex[partner, sexOfPartner], AttractedTo[sexuality, sexOfPartner],
                   PersonSexuality[partner, sexualOfPartner], PersonSex[person, sex], AttractedTo[sexualOfPartner, sex]);

            // TODO : Check, Definitions vs Predicates
            var Man = Predicate("Men", man).If(
                CharacterFeatures[man, age, __, Sex.Male, __, VitalStatus.Alive], age >= 18);
            var Woman = Predicate("Women", woman).If(
                CharacterFeatures[woman, age, __, Sex.Female, __, VitalStatus.Alive], age >= 18);
            #endregion

            var Aptitude = Character.FeaturesMultiple("Aptitude", job.Indexed, aptitude);
            Aptitude.Initially[person, job, RandomNormalSByte].Where(PrimordialBeing, Jobs);
            Aptitude.Add[person, job, RandomNormalSByte].If(Character.Add, Jobs);

            Character.EndWhen(Age, age > 60, PerMonth(0.003f))
                     .EndCauses(Set(CharacterFeatures, person, vitalStatus, VitalStatus.Dead));

            var Drifter = Predicate("Drifter", person, sex, sexuality);
            Drifter[person, RandomSex, sexuality].If(PerYear(0.05f), RandomPerson, RandomSexuality[sex, sexuality]);
            Character.StartWithTime(Drifter, DateAgeToTimePoint[RandomDate, RandomAdultAge, birthday])
                     .StartWithCauses(Add(CharacterFeatures[person, Time.YearsOld[birthday],
                                                            TimePointToDate[birthday], sex, sexuality, VitalStatus.Alive]).If(Drifter));

            // *********************************** Relationships **********************************
            // TODO : Primordial Relationships?
            // TODO : Married couples - last name changes

            var Charge = Affinity("Charge", pairing, person, otherPerson, charge).Decay(0.8f);
            Friend = Charge.Relationship("Friend", state, 5000, 4000);
            var MutualFriendship = Predicate("MutualFriendship", person, otherPerson)
               .If(Friend[person, otherPerson], Friend[otherPerson, person]);
            Enemy = Charge.Relationship("Enemy", state, -6000, -3000);

            var Spark = Affinity("Spark", pairing, person, otherPerson, spark).Decay(0.1f);
            RomanticInterest = Spark.Relationship("RomanticInterest", state, 7000, 6000);
            var MutualRomanticInterest = Predicate("MutualRomanticInterest", person, otherPerson)
               .If(RomanticInterest[person, otherPerson], RomanticInterest[otherPerson, person]);

            Parent = Predicate("Parent", parent, child);
            Parent.Button("Visualize", VisualizeFamilies);
            var FamilialRelation = Definition("FamilialRelation", person, otherPerson)
               .Is(Parent[person, otherPerson] | Parent[otherPerson, person]); // only immediate family

            // ************************************ Procreation ***********************************
            // TODO : Limit PotentialProcreation by interactions (realism)
            // TODO : Limit PotentialProcreation by Personality (family planning, could include likely-hood to use contraceptives)
            // TODO : Limit PotentialProcreation by time since last birth and number of children with partner (Gestation table info)

            var Gestation = Predicate("Gestation", 
                woman.Indexed, man, sex, child.Key, conception, state.Indexed);
            var Pregnant = Predicate("Pregnant", woman)
                .If(Gestation[woman, __, __, __, __, true]);

            var PotentialProcreation = Predicate("PotentialProcreation", woman.Indexed, man);
            PotentialProcreation.Unique = true;
            var ProcreativePair = Predicate("ProcreativePair", woman.Indexed, man.Indexed)
               .If(MutualRomanticInterest[woman, man], Woman, Man, !FamilialRelation[woman, man]);
            PotentialProcreation.If(ProcreativePair, !Pregnant[woman], Age[woman, age], Character[man], Prob[FertilityRate[age]]);
            var SuccessfulProcreation = AssignRandomly("SuccessfulProcreation", PotentialProcreation);

            Gestation.Add[woman, man, sex, child, Time.CurrentDate, true]
                .If(SuccessfulProcreation[woman, man], RandomSex[sex], RandomFirstName, NewPerson[firstName, Surname[man], child]);

            var BirthTo = Predicate("BirthTo", woman, man, sex, child);
            BirthTo.If(Gestation[woman, man, sex, child, conception, true],
                Time.NineMonthsPast[conception], Prob[0.8f]); // Birth after 9 months with 'labor'
            Gestation.Set(child, state, false).If(BirthTo);

            Parent.Add.If(BirthTo[parent, __, __, child]);
            Parent.Add.If(BirthTo[__, parent, __, child]);

            Character.StartWhen(BirthTo[__, __, __, person]);
            Character.StartCauses(Add(CharacterFeatures[person, 0, Time.CurrentDate, sex, sexuality, VitalStatus.Alive])
                                     .If(BirthTo[__, __, sex, person], RandomSexuality[sex, sexuality]));
            // Increment age once per birthday (in the AM, if you weren't just born)
            CharacterFeatures.Set(person, age, num)
                 .If(CharacterFeatures[person, age, dateOfBirth, __, __, VitalStatus.Alive],
                     Time.CurrentlyMorning, Time.IsToday[dateOfBirth], !BirthTo[__, __, __, person], Incr[age, num]);

            // ************************************ Locations *************************************

            var Location = Predicate("Location", location.Key,
                locationType.Indexed, locationCategory.Indexed, position.Indexed, founding, businessStatus.Indexed);
            LocationToType = Location.Accessor(location, locationType);
            Location.Initially[location, locationType, locationCategory, position, founding, BusinessStatus.InBusiness]
                     .Where(PrimordialLocation, LocationInformation);
            Location.Colorize(location);

            // NewLocation is used for both adding tiles to the TileMap in Unity efficiently
            // (not checking every row of the Locations table) and for collecting new locations
            // with the minimal amount of information needed (excludes derivable columns).
            NewLocation = Predicate("NewLocation", position, location, locationType, founding);
            NewLocation.Colorize(location);
            Location.Add[location, locationType, locationCategory, position, founding, BusinessStatus.InBusiness]
                     .If(NewLocation, LocationInformation);
            Location.Add.Colorize(location);
            NewLocation.Colorize(location);

            var InBusiness = Definition("InBusiness", location)
               .Is(Location[location, __, __, __, __, BusinessStatus.InBusiness]);

            Location.Set(location, businessStatus, BusinessStatus.OutOfBusiness)
                     .If(Location,
                         !In(locationType, new[] {
                             LocationType.Cemetery,
                             LocationType.CityHall,
                             LocationType.DayCare,
                             LocationType.FireStation,
                             LocationType.Hospital,
                             LocationType.School
                         }),
                         Time.YearsOld[founding, age], age > 40, PerYear(0.1f));
            var JustClosed = Predicate("JustClosed", location)
               .If(Location.Set(location, businessStatus));

            // VacatedLocation is used for removing tiles from the TileMap
            VacatedLocation = Predicate("VacatedLocation", position);
            VacatedLocation.If(Location.Set(location, businessStatus), Location);

            // For tile hover info strings we only want the locations in business, and since we will be
            // using this table for GUI interactions we need to be able to access a location by position.
            // Each lot in town can only hold one active location so this works out nicely.
            var DisplayLocations = Predicate("DisplayLocations",
                position.Key, location, locationType,  founding)
               .If(Location[location, locationType, __, position, founding, BusinessStatus.InBusiness]);
            DisplayLocations.Colorize(location);
            // DisplayLocations is a collection - not intended to be used as a predicate - thus the plural naming
            LocationsPositionIndex = DisplayLocations.KeyIndex(position);

            // Helper functions and definitions for creating new locations at a valid lot in town
            var NumLots = Length("NumLots", Location); // NumLots helps expand town borders
            var IsVacant = Definition("IsVacant", position)
                .Is(!Location[__, __, __, position, __, BusinessStatus.InBusiness]);
            var FreeLot = Definition("FreeLot", position)
                .Is(RandomLot[NumLots, position], IsVacant[position]);

            var CategoryCount = CountsBy("CategoryCount", Location, locationCategory, count);
            var AvailableCategory = Predicate("AvailableCategory", locationCategory).If(CategoryCount);
            var AvailableAction = Predicate("AvailableAction", actionType)
                .If(ActionToCategory, AvailableCategory); // need one to one from this, too many staying in and visiting entries currently

            // ************************************** Housing *************************************
            // TODO : Include ApartmentComplex locations in Housing logic
            // TODO : Include Inn locations in Housing logic - drifters start at an Inn

            Home = Predicate("Home", occupant.Key, location.Indexed);
            Home.Unique = true;
            Home.Button("Visualize", VisualizeHomes);

            var InBusinessHome = Predicate("InBusinessHome", occupant, location.Indexed)
                .If(Location[location, LocationType.House, __, __, __, BusinessStatus.InBusiness], Home);
            var Occupancy = CountsBy("Occupancy", InBusinessHome, location, count);
            var Unoccupied = Predicate("Unoccupied", location)
                .If(Location[location, LocationType.House, __, __, __, BusinessStatus.InBusiness], !Home[__, location]);
            var UnderOccupied = Predicate("UnderOccupied", location);
            UnderOccupied.If(Occupancy, count <= 5);
            UnderOccupied.If(Unoccupied);

            var Unhoused = Predicate("Unhoused", person).If(Character[person], !Home[person, __]);

            // Using this to randomly assign one house per primordial person...
            var PrimordialHouse = Predicate("PrimordialHouse", location)
                .If(PrimordialLocation[location, LocationType.House, __, __]);
            Home.Initially.Where(PrimordialBeing[occupant, __, __, __, __],
                RandomElement(PrimordialHouse, location));

            Home.Add.If(BirthTo[man, woman, sex, occupant], Home[woman, location]); // Move in with mom
            // If no UnderOccupied homes this wont get set...
            Home.Add.If(Drifter[occupant, __, __], RandomElement(UnderOccupied, location));
            Home.Add.If(Unhoused[occupant], RandomElement(UnderOccupied, location));

            Home.Set(occupant, location).If(Character.End[occupant],
                Location[location, LocationType.Cemetery, __, __, __, BusinessStatus.InBusiness]);
            var BuriedAt = Predicate("BuriedAt", occupant, location)
                .If(Location[location, LocationType.Cemetery, __, __, __, BusinessStatus.InBusiness], Home);
            // with only the one cemetery for now, the follow will suffice for the GUI
            Buried = Predicate("Buried", person).If(BuriedAt[person, __]);

            var ForeclosedUpon = Predicate("ForeclosedUpon", occupant).If(Home, JustClosed);
            Home.Set(occupant, location).If(ForeclosedUpon, RandomElement(UnderOccupied, location));

            // Distance per person makes most sense when measured from either where the person is,
            // or where they live. This handles the latter:
            var DistanceFromHome = Definition("DistanceFromHome", person, location, distance)
                .Is(Location[location, __, __, position, __, BusinessStatus.InBusiness],
                    Home[person, otherLocation],
                    Location[otherLocation, __, __, otherPosition, __, BusinessStatus.InBusiness],
                    Distance[position, otherPosition, distance]);

            var WantToMove = Predicate("WantToMove", person).If(Home[person, location], Occupancy, count > 8);

            var CanMoveToFamily = Predicate("CanMoveToFamily", person.Indexed, location);
            CanMoveToFamily.If(WantToMove, Home[person, otherLocation], FamilialRelation,
                               Home[otherPerson, location], location != otherLocation, Occupancy, count < 8);
            var SelectedFamilyToMoveTo = AssignRandomly("SelectedFamilyToMoveTo", CanMoveToFamily);
            var SelectedToMoveToFamily = Predicate("SelectedToMoveToFamily", person).If(SelectedFamilyToMoveTo);

            var FamilyInHouse = Predicate("FamilyInHouse", person);
            FamilyInHouse.If(WantToMove, WantToMove[otherPerson], person != otherPerson,
                             Home[person, location], Home[otherPerson, location], FamilialRelation[person, otherPerson]);
            var SelectedOutsiderToMove = Predicate("SelectedOutsiderToMove", person).If(WantToMove, !FamilyInHouse[person]);

            var MovingIn = Predicate("MovingIn", person, location);
            MovingIn.If(Once[SelectedToMoveToFamily[__]],
                        RandomElement(SelectedToMoveToFamily, person), SelectedFamilyToMoveTo);
            MovingIn.If(Once[SelectedOutsiderToMove[__]], !SelectedToMoveToFamily[__],
                        RandomElement(SelectedOutsiderToMove, person), RandomElement(UnderOccupied, location));
            MovingIn.If(Once[WantToMove[__]], !SelectedToMoveToFamily[__], !SelectedOutsiderToMove[__],
                        RandomElement(WantToMove, person), RandomElement(UnderOccupied, location));

            Home.Set(occupant, location).If(MovingIn[occupant, location]);

            // ********************************** New Locations ***********************************

            // Needs the random lot to be available & 'construction' isn't instantaneous
            void NewLocationByType(LocationType locType, Goal readyToAdd, Goal newLocation, bool oneOfAKind) =>
                NewLocation[position, location, locType, Time.CurrentTimePoint]
                   .If(FreeLot, PerWeek(0.5f), oneOfAKind ?
                           !Location[__, locType, __, __, __, BusinessStatus.InBusiness] & readyToAdd : readyToAdd, newLocation);

            void AddNamedLocation(LocationType locType, string name, Goal readyToAdd) =>
                NewLocationByType(locType, readyToAdd, Functions.NewLocation[name, location], true);

            void AddLocationByCFG(LocationType locType, Function<string> name, Goal readyToAdd, bool oneOfAKind = true) =>
                NewLocationByType(locType, readyToAdd, Functions.NewLocation[name, location], oneOfAKind);

            void AddLocationFromNames(LocationType locType, TablePredicate<string> names, Goal readyToAdd, bool oneOfAKind = false) =>
                NewLocationByType(locType, readyToAdd, RandomElement(names, locationName) &
                                                       Functions.NewLocation[locationName, location], oneOfAKind);

            AddLocationFromNames(LocationType.House, HouseNames, Once[WantToMove[__]]);
            // Currently the following only happens with drifters - everyone starts housed
            AddLocationFromNames(LocationType.House, HouseNames, Once[Unhoused[__]]);

            AddLocationByCFG(LocationType.Hospital, HospitalName.GenerateName,
                           Once[Goals(Aptitude[person, Vocation.Doctor, aptitude], aptitude > 15, Age, age > 21)]);

            AddLocationByCFG(LocationType.Cemetery, CemeteryName.GenerateRandom, Once[Goals(Age, age >= 60)]); // before anyone can die

            AddLocationByCFG(LocationType.DayCare, DaycareName.GenerateName, Count(Age & (age < 6)) > 5);

            AddLocationByCFG(LocationType.School, HighSchoolName.GenerateRandom, Count(Age & (age >= 5) & (age < 18)) > 5);

            AddNamedLocation(LocationType.CityHall, "Big City Hall", Goals(Population[count], count > 200));

            AddNamedLocation(LocationType.GeneralStore, "Big Box Store", Goals(Population[count], count > 150));

            AddNamedLocation(LocationType.Bar, "Triple Crossing", Goals(Population[count], count > 125));

            AddNamedLocation(LocationType.GroceryStore, "Trader Jewels", Goals(Population[count], count > 100));

            AddNamedLocation(LocationType.TattooParlor, "Heroes and Ghosts", Goals(Population[count], count > 250));

            // ************************************ Vocations: ************************************

            Employment = Predicate("Employment", job.Indexed, employee.Key, location.Indexed, timeOfDay.Indexed);
            EmploymentIndex = Employment.KeyIndex(employee);
            Employment.Colorize(location);
            Employment.Button("Visualize", VisualizeJobs);

            var EmploymentStatus = Predicate("EmploymentStatus", employee.Key, state.Indexed);
            EmploymentStatus.Add[employee, true].If(Employment.Add);
            EmploymentStatus.Set(employee, state, false).If(Character.End[employee], EmploymentStatus[employee, true]);
            EmploymentStatus.Set(employee, state, false).If(JustClosed[location], Employment, EmploymentStatus[employee, true]);

            var StillEmployed = Definition("StillEmployed", person).Is(EmploymentStatus[person, true]);

            var JobToFill = Predicate("JobToFill", location, job)
                .If(Time.CurrentTimeOfDay[timeOfDay], InBusiness, Location, VocationShift,
                    PositionsPerJob, Count(Employment & EmploymentStatus[employee, true]) < positions);

            Drifter[person, RandomSex, sexuality].If(JobToFill, PerMonth(0.5f), RandomPerson, RandomSexuality[sex, sexuality]);

            var BestCandidate = Predicate("BestCandidate", person, job, location)
                .If(JobToFill, Maximal(person, aptitude,
                                        Goals(Age, !StillEmployed[person], age >= 18, Aptitude)));

            var CandidateForJob = Predicate("CandidateForJob", person.Indexed, job).If(BestCandidate);
            var OneJobPerCandidate = AssignRandomly("OneJobPerCandidate", CandidateForJob);

            var CandidatePerJobForLocation = Predicate("CandidatePerJobForLocation",
                                                       person.Indexed, location).If(OneJobPerCandidate, BestCandidate);
            var OneCandidatePerJobPerLocation = AssignRandomly("OneCandidatePerJobPerLocation", CandidatePerJobForLocation);

            Employment.Add[job, person, location, Time.CurrentTimeOfDay].If(OneCandidatePerJobPerLocation, OneJobPerCandidate);

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

            var GoingToSchool = Predicate("GoingToSchool", person, location).If(
                AvailableAction[ActionType.GoingToSchool], OpenLocationType[LocationType.School],
                Location[location, LocationType.School, __, __, __, BusinessStatus.InBusiness],
                NeedSchooling);
            var GoingToDayCare = Predicate("GoingToDayCare", person, location).If(
                AvailableAction[ActionType.GoingToSchool], OpenLocationType[LocationType.DayCare],
                Location[location, LocationType.DayCare, __, __, __, BusinessStatus.InBusiness],
                NeedDayCare);

            var GoingToWork = Predicate("GoingToWork", person, location)
                .If(OpenLocationType, InBusiness, Location, StillEmployed, Employment[__, person, location, Time.CurrentTimeOfDay]);

            WhereTheyAt = Predicate("WhereTheyAt", person.Key, actionType.Indexed, location.Indexed);
            WhereTheyAt.Unique = true;
            WhereTheyAt.Colorize(location);
            WhereTheyAtLocationIndex = (GeneralIndex<(Person, ActionType, Location), Location>)WhereTheyAt.IndexFor(location, false);
            WhereTheyAt.Button("Snapshot", VisualizeWhereTheyAt);

            var AdultAction = Predicate("AdultAction", actionType)
                .If(AvailableAction, !In(actionType, new[] { ActionType.GoingToSchool, ActionType.GoingOutForDateNight }));
            var NeedsActionAssignment = Predicate("NeedsActionAssignment", person).If(Character[person],
                !GoingToWork[person, __],
                !GoingToDayCare[person, __],
                !GoingToSchool[person, __]);
            var RandomActionAssign = Predicate("RandomActionAssign", person, actionType)
                .If(NeedsActionAssignment, RandomElement(AdultAction, actionType));

            var LocationByActionAssign = Predicate("LocationByActionAssign", person, location);
            LocationByActionAssign.If(RandomActionAssign[person, ActionType.StayingIn], Home[person, location]);

            var VisitingFriend = Predicate("VisitingFriend", person.Indexed, otherPerson);
            VisitingFriend.If(RandomActionAssign[person, ActionType.Visiting], MutualFriendship);
            var SelectedFriendToVisit = AssignRandomly("SelectedFriendToVisit", VisitingFriend);
            var NoOneToVisit = Predicate("NoOneToVisit", person)
               .If(RandomActionAssign[person, ActionType.Visiting], !VisitingFriend[person, __]);

            LocationByActionAssign.If(NoOneToVisit, Home[person, location]);
            LocationByActionAssign.If(SelectedFriendToVisit, Home[otherPerson, location]);

            // Choose the closest location with the action type assigned
            var OpenForBusinessByAction = Predicate("OpenForBusinessByAction", actionType, location)
                .If(ActionToCategory, AvailableCategory, OpenLocationType, InBusiness, Location);
            LocationByActionAssign.If(RandomActionAssign, !In(actionType, new[] { ActionType.StayingIn, ActionType.Visiting }),
                Minimal(location, distance, OpenForBusinessByAction & DistanceFromHome[person, location, distance]));

            WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToSchool);
            WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToDayCare);
            WhereTheyAt[person, ActionType.GoingToWork, location].If(GoingToWork);
            WhereTheyAt.If(RandomActionAssign, LocationByActionAssign);

            // *********************************** Interactions: **********************************

            var NotWorking = Predicate("NotWorking", person.Key, location.Indexed)
               .If(WhereTheyAt[person, actionType, location],
                   !In(actionType, new[] { ActionType.GoingToWork, ActionType.GoingToSchool }));

            var InteractionPair = Predicate("InteractionPair", person, otherPerson);
            InteractionPair.If(NotWorking[person, location], NotWorking[otherPerson, location], person != otherPerson);
            InteractionPair.If(GoingToWork[person, location], GoingToWork[otherPerson, location], person != otherPerson);

            var PotentialInteraction = Predicate("PotentialInteraction", person, otherPerson, score);
            PotentialInteraction.If(InteractionPair,
                                    Friend[__, person, otherPerson, true],
                                    RomanticInterest[__, person, otherPerson, true],
                                    Similarity[person, otherPerson, num], score == num * 10);
            PotentialInteraction.If(InteractionPair,
                                    Friend[__, person, otherPerson, true],
                                    !RomanticInterest[__, person, otherPerson, true],
                                    Similarity[person, otherPerson, num], score == num * 5);
            PotentialInteraction.If(InteractionPair,
                                    !Friend[__, person, otherPerson, true],
                                    !Enemy[__, person, otherPerson, true],
                                    RomanticInterest[__, person, otherPerson, true],
                                    Similarity[person, otherPerson, num], score == num * 3);
            PotentialInteraction.If(InteractionPair,
                                    Enemy[__, person, otherPerson, true],
                                    RomanticInterest[__, person, otherPerson, true],
                                    Similarity[person, otherPerson, num], score == num * 2);
            PotentialInteraction.If(InteractionPair,
                                    Enemy[__, person, otherPerson, true],
                                    !RomanticInterest[__, person, otherPerson, true],
                                    Similarity[person, otherPerson, num], score == num / 2);
            PotentialInteraction.If(InteractionPair,
                                    !Friend[__, person, otherPerson, true],
                                    !Enemy[__, person, otherPerson, true],
                                    !RomanticInterest[__, person, otherPerson, true],
                                    Similarity[person, otherPerson, score]);
            var SelectedInteractionPair = MatchGreedilyAsymmetric("SelectedInteractionPair", PotentialInteraction);

            var ScoredInteraction = Predicate("ScoredInteraction", person.Indexed, otherPerson.Indexed, score);
            ScoredInteraction[person, otherPerson, RandomNormal].If(SelectedInteractionPair);
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
            CharacterFeatures.ColumnValueFromRowNumber(person)((uint)Integer(0, (int)CharacterFeatures.Length)));

        private void VisualizeFriendNetworkOf(Person p) {
            // ReSharper disable InconsistentNaming
            var FriendIndex = (GeneralIndex<((Person,Person), Person, Person,bool), Person>)Friend.IndexFor(person, false);
            var EnemyIndex = (GeneralIndex<((Person,Person), Person, Person,bool), Person>)Enemy.IndexFor(person, false);
            var RomanticInterestIndex = (GeneralIndex<((Person,Person), Person, Person,bool), Person>)RomanticInterest.IndexFor(person, false);

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
            foreach (var r in RomanticInterest)
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
            var info = CharacterFeatures.KeyIndex(person)[p];
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
