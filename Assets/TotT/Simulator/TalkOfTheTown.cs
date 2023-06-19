using System;
using System.Threading.Tasks;
using TED;
using TED.Interpreter;
using TED.Tables;
using TED.Utilities;
using TotT.Simulog;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace TotT.Simulator {
    using LocationDisplayRow = ValueTuple<Vector2Int, Location, LocationType, TimePoint>;
    using static Calendar;   // Prob per interval type
    using static CsvParsing; // DeclareParsers
    using static Functions;
    using static Randomize;  // Seed and .RandomElement
    using static StaticTables;
    using static TEDHelpers; // Increment and Goals(params...)
    using static Variables;

    public class TalkOfTheTown {
        public static Simulation Simulation = null!;
        public static Time Time;
        private const int Seed = 349571286;

        private TalkOfTheTown() {
            DeclareParsers();
            Seed(Seed, Seed); }
        public TalkOfTheTown(int year) : this() => Time = new Time(year);
        public TalkOfTheTown(int year, ushort tick = 1) : this() => Time = new Time(year, tick);
        public TalkOfTheTown(int year, Month month, byte day = 1, TimeOfDay time = TimeOfDay.AM) : 
            this() => Time = new Time(year, month, day, time);

        // public Tables and Indexers for GUI purposes (see Unity.SimulationInfo for most uses)
        public TablePredicate<Vector2Int, Location, LocationType, TimePoint> NewLocation;
        public TablePredicate<Vector2Int> VacatedLocation;
        public TablePredicate<Person> Buried;
        public KeyIndex<(VitalStatus, int), VitalStatus> PopulationCountIndex;
        public KeyIndex<LocationDisplayRow, Vector2Int> LocationsPositionIndex;
        public GeneralIndex<(Person, ActionType, Location), Location> WhereTheyAtLocationIndex;

        public void InitSimulator() {
            Simulation = new Simulation("Talk of the Town");
            Simulation.BeginPredicates();
            InitStaticTables();
            // ReSharper disable InconsistentNaming
            // Tables, despite being local variables, will be capitalized for style/identification purposes.

            // ************************************** Agents **************************************
            // TODO : Cue drifters when new jobs need filling that the township can't meet requirements for

            // Primordial beings init -
            var Agent = Predicate("Agent", person.Key, 
                age, dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed);
            Agent.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeing);
            // ditto for agents associated tables -
            var Personality = Predicate("Personality", person.Indexed, facet.Indexed, personality);
            Personality.Initially[person, facet, RandomNormalSByte].Where(PrimordialBeing, Facets);
            var Aptitude = Predicate("Aptitude", person.Indexed, job.Indexed, aptitude);
            Aptitude.Initially[person, job, RandomNormalSByte].Where(PrimordialBeing, Jobs);

            var PopulationCount = CountsBy("PopulationCount", Agent, vitalStatus, count);
            PopulationCount.IndexByKey(vitalStatus);
            PopulationCountIndex = (KeyIndex<(VitalStatus, int), VitalStatus>)PopulationCount.IndexFor(vitalStatus, true);

            var Alive = Definition("Alive", person)
                .Is(Agent[person, __, __, __, __, VitalStatus.Alive]);
            // special case Alive check where we also bind age
            var Age = Definition("Age", person, age)
                .Is(Agent[person, age, __, __, __, VitalStatus.Alive]);

            Agent.Set(person, vitalStatus, VitalStatus.Dead) // Dying
                .If(Age, age > 60, PerMonth(0.003f));
            var JustDied = Predicate("JustDied", person)
                .If(Agent.Set(person, vitalStatus)); // Assumes set only used for dying (two vitalStatus values)

            // Person Name helpers -
            var RandomFirstName = Definition("RandomFirstName", sex, firstName);
            RandomFirstName[Sex.Male, firstName].If(RandomElement(MaleNames, firstName));
            RandomFirstName[Sex.Female, firstName].If(RandomElement(FemaleNames, firstName));
            // Surname here is only being used to facilitate A naming convention for last names (currently paternal lineage)
            var RandomPerson = Definition("RandomPerson", sex, person)
                .Is(RandomFirstName, RandomElement(Surnames, lastName), NewPerson[firstName, lastName, person]);

            // Independent Agent creation (not birth based) - 
            var Drifter = Predicate("Drifter", person, sex, sexuality);
            Drifter[person, RandomSex, sexuality].If(PerYear(0.05f),
                RandomPerson, RandomSexuality[sex, sexuality]);
            Agent.Add[person, RandomAdultAge, RandomDate, sex, sexuality, VitalStatus.Alive].If(Drifter);

            // Add associated info that is needed for a new agent -
            Personality.Add[person, facet, RandomNormalSByte].If(Agent.Add, Facets);
            Aptitude.Add[person, job, RandomNormalSByte].If(Agent.Add, Jobs);
            // Agents.Add handles both Birth and Drifters, if we want to make kids inherit modified values from
            // their parents then we will need separate cases for BirthTo[__, __, __, person] and drifters.

            // *********************************** Relationships **********************************

            var Spark = Predicate("Spark", pairing.Key, person.Indexed, otherPerson.Indexed, spark.Indexed);
            var Charge = Predicate("Charge", pairing.Key, person.Indexed, otherPerson.Indexed, charge.Indexed);

            var Friend = Predicate("Friend", pairing.Key, person.Indexed, otherPerson.Indexed, state.Indexed);
            Friend.Add[pairing, person, otherPerson, true]
                  .If(Charge[pairing, person, otherPerson, charge], 
                      charge > 5000, !Friend[pairing, person, otherPerson, true]);
            Friend.Set(pairing, state, false)
                  .If(Charge[pairing, person, otherPerson, charge],
                      charge < 4000, Friend[pairing, person, otherPerson, true]);
            Friend.Set(pairing, state, true)
                  .If(Charge[pairing, person, otherPerson, charge],
                      charge > 4000, Friend[pairing, person, otherPerson, false]);

            var MutualFriendship = Predicate("MutualFriendship", person.Indexed, otherPerson.Indexed);
            MutualFriendship.If(Friend[pairing, person, otherPerson, true], Friend[otherPairing, otherPerson, person, true], SortOrder[person, otherPerson]);


            // ************************************** Couples *************************************
            // TODO : Primordial couples?
            // TODO : Better util for couples - facet similarity or score based on facet logic (> X, score + 100)
            // TODO : Limit PotentialPairings by interactions (avoids performance hit for batch selection)
            // TODO : Non-monogamous PotentialPairings for PotentialProcreation (needs interaction based limitation as well)
            // TODO : Limit PotentialProcreation by interactions (realism)
            // TODO : Limit PotentialProcreation by Personality (family planning, could include likely-hood to use contraceptives)
            // TODO : Limit PotentialProcreation by time since last birth and total number of children with partner (Gestation table info)
            // TODO : All non-intimate relationships
            // TODO : Married couples separate from ProcreativePair - last name changes in 'nickname' like table

            // Need Parents before parenting logic to prevent procreative relationships between parents and kids
            var Parent = Predicate("Parent", parent, child);
            var FamilialRelation = Definition("FamilialRelation", person, otherPerson)
                .Is(Parent[person, otherPerson] | Parent[otherPerson, person]); // only immediate family

            var PersonSex = Definition("PersonSex", person, sex)
                .Is(Agent[person, __, __, sex, __, VitalStatus.Alive]);
            var PersonSexuality = Definition("PersonSexuality", person, sexuality)
                .Is(Agent[person, __, __, __, sexuality, VitalStatus.Alive]);
            var SexualAttraction = Definition("SexualAttraction", person, partner)
                .Is(PersonSexuality[person, sexuality], PersonSex[partner, sexOfPartner], SexualityAttracted[sexuality, sexOfPartner],
                    PersonSexuality[partner, sexualOfPartner], PersonSex[person, sex], SexualityAttracted[sexualOfPartner, sex]);
            
            var Man = Predicate("Men", man).If(
                Agent[man, age, __, Sex.Male, __, VitalStatus.Alive], age >= 18);
            var Woman = Predicate("Women", woman).If(
                Agent[woman, age, __, Sex.Female, __, VitalStatus.Alive], age >= 18);

            var ProcreativePair = Predicate("ProcreativePair", woman.Indexed, man.Indexed);
            ProcreativePair.Unique = true;
            var PotentialPairing = Predicate("PotentialPairing", woman.Indexed, man.Indexed)
                .If(Woman, !ProcreativePair[woman, __], Man, !ProcreativePair[__, man],
                    SexualAttraction[woman, man], !FamilialRelation[woman, man]);
            var ScoredPairing = Predicate("ScoredPairing", woman.Indexed, man.Indexed, score);
            ScoredPairing[woman, man, RandomNormalFloat].If(PotentialPairing);
            var PairForProcreate = MatchGreedily("PairForProcreate", ScoredPairing);
            ProcreativePair.Add.If(PairForProcreate);
            
            var PotentialProcreation = Predicate("PotentialProcreation", woman.Indexed, man);
            PotentialProcreation.Unique = true;
            var Gestation = Predicate("Gestation",
                woman.Indexed, man, sex, child.Key, conception, state.Indexed);
            var Pregnant = Predicate("Pregnant", woman)
                .If(Gestation[woman, __, __, __, __, true]);

            PotentialProcreation.If(ProcreativePair, !Pregnant[woman], Age[woman, age], Alive[man], Prob[FertilityRate[age]]);
            var SuccessfulProcreation = AssignRandomly("SuccessfulProcreation", PotentialProcreation);

            Gestation.Add[woman, man, sex, child, Time.CurrentDate, true]
                .If(SuccessfulProcreation[woman, man], RandomSex[sex], RandomFirstName, NewPerson[firstName, Surname[man], child]);

            var BirthTo = Predicate("BirthTo", woman, man, sex, child);
            BirthTo.If(Gestation[woman, man, sex, child, conception, true],
                Time.NineMonthsPast[conception], Prob[0.8f]); // Birth after 9 months with 'labor'
            Gestation.Set(child, state, false).If(BirthTo);

            Agent.Add[person, 0, Time.CurrentDate, sex, sexuality, VitalStatus.Alive].If(
                BirthTo[__, __, sex, person], RandomSexuality[sex, sexuality]);

            Parent.Add.If(BirthTo[parent, __, __, child]);
            Parent.Add.If(BirthTo[__, parent, __, child]);

            // Increment age once per birthday (in the AM, if you weren't just born)
            var WhenToAge = Definition("WhenToAge", person, age).Is(
                Agent[person, age, dateOfBirth, __, __, VitalStatus.Alive],
                Time.CurrentlyMorning, Time.IsToday[dateOfBirth], !BirthTo[__, __, __, person]);
            Increment(Agent, person, age, WhenToAge);

            // ************************************ Locations *************************************

            var Location = Predicate("Location", location.Key, 
                locationType.Indexed, locationCategory.Indexed, position.Indexed, founding, businessStatus.Indexed);
            Location.Initially[location, locationType, locationCategory, position, founding, BusinessStatus.InBusiness]
                     .Where(PrimordialLocation, LocationInformation);

            // NewLocation is used for both adding tiles to the TileMap in Unity efficiently
            // (not checking every row of the Locations table) and for collecting new locations
            // with the minimal amount of information needed (excludes derivable columns).
            NewLocation = Predicate("NewLocation", position, location, locationType, founding);
            Location.Add[location, locationType, locationCategory, position, founding, BusinessStatus.InBusiness]
                     .If(NewLocation, LocationInformation);

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
            // TODO : Include Inn locations in Housing logic - drifters start at an Inn if no available homes
            // TODO : Families move in and out of houses together
            // TODO : Initialize Homes first based on primordial couples, then on all single agents

            var Home = Predicate("Home", occupant.Key, location.Indexed);
            Home.Unique = true;
            var InBusinessHome = Predicate("InBusinessHome", occupant, location.Indexed)
                .If(Location[location, LocationType.House, __, __, __, BusinessStatus.InBusiness], Home);
            var Occupancy = CountsBy("Occupancy", InBusinessHome, location, count);
            var Unoccupied = Predicate("Unoccupied", location)
                .If(Location[location, LocationType.House, __, __, __, BusinessStatus.InBusiness], !Home[__, location]);
            var UnderOccupied = Predicate("UnderOccupied", location);
            UnderOccupied.If(Occupancy, count <= 5);
            UnderOccupied.If(Unoccupied);

            var Unhoused = Predicate("Unhoused", person).If(Alive, !Home[person, __]);

            // Using this to randomly assign one house per primordial person...
            var PrimordialHouse = Predicate("PrimordialHouse", location)
                .If(PrimordialLocation[location, LocationType.House, __, __]);
            Home.Initially.Where(PrimordialBeing[occupant, __, __, __, __], 
                RandomElement(PrimordialHouse, location));

            Home.Add.If(BirthTo[man, woman, sex, occupant], Home[woman, location]); // Move in with mom
            // If no UnderOccupied homes this wont get set...
            Home.Add.If(Drifter[occupant, __, __], RandomElement(UnderOccupied, location));
            Home.Add.If(Unhoused[occupant], RandomElement(UnderOccupied, location));

            Home.Set(occupant, location).If(JustDied[occupant],
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
            
            var WantToMove = Predicate("WantToMove", person)
                .If(Home[person, location], Occupancy, count >= 8);
            var MovingIn = Predicate("MovingIn", person, location)
                .If(Once[WantToMove[__]], RandomElement(WantToMove, person), RandomElement(UnderOccupied, location));
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

            AddLocationByCFG(LocationType.Hospital, HospitalName,
                           Once[Goals(Aptitude[person, Vocation.Doctor, aptitude], aptitude > 15, Age, age > 21)]);

            AddNamedLocation(LocationType.Cemetery, "The Old Cemetery", Once[Goals(Alive, Age, age >= 60)]); // before anyone can die

            AddLocationByCFG(LocationType.DayCare, DaycareName, Count(Age & (age < 6)) > 5);

            AddNamedLocation(LocationType.School, "Talk of the Township High", Count(Age & (age >= 5) & (age < 18)) > 5);

            AddNamedLocation(LocationType.CityHall, "Big City Hall", Goals(PopulationCount[VitalStatus.Alive, count], count > 200));

            AddNamedLocation(LocationType.GeneralStore, "Big Box Store", Goals(PopulationCount[VitalStatus.Alive, count], count > 150));

            AddNamedLocation(LocationType.Bar, "Triple Crossing", Goals(PopulationCount[VitalStatus.Alive, count], count > 125));

            AddNamedLocation(LocationType.GroceryStore, "Trader Jewels", Goals(PopulationCount[VitalStatus.Alive, count], count > 100));

            AddNamedLocation(LocationType.TattooParlor, "Heroes and Ghosts", Goals(PopulationCount[VitalStatus.Alive, count], count > 250));

            // ************************************ Vocations: ************************************

            var Employment = Predicate("Employment", job.Indexed, employee.Key, location.Indexed, timeOfDay.Indexed);

            var EmploymentStatus = Predicate("EmploymentStatus", employee.Key, state.Indexed);
            EmploymentStatus.Add[employee, true].If(Employment.Add);
            EmploymentStatus.Set(employee, state, false).If(JustDied[employee], EmploymentStatus[employee, true]);

            var StillEmployed = Definition("StillEmployed", person).Is(EmploymentStatus[person, true]);

            var JobToFill = Predicate("JobsToFill", location, job)
                .If(Time.CurrentTimeOfDay[timeOfDay], InBusiness, Location, VocationShift,
                    PositionsPerJob, Count(Employment & EmploymentStatus[employee, true]) < positions);

            var Candidate = Predicate("Candidate", person, job, location)
                .If(JobToFill, Maximal(person, aptitude, 
                                        Goals(Alive, !StillEmployed[person], Age, age >= 18, Aptitude)));

            var CandidateToFilter = Predicate("CandidateToFilter", person.Indexed, job).If(Candidate);
            var FilteredCandidate = AssignRandomly("FilteredCandidate", CandidateToFilter);
            var SelectedCandidate = Predicate("SelectedCandidates", 
                person, job, location).If(FilteredCandidate, Candidate);

            Employment.Add[job, person, location, Time.CurrentTimeOfDay].If(SelectedCandidate);

            // ************************************* Movement: ************************************
            // TODO : Visiting action choose location of relative or partner (future friends)
            // TODO : Babies not in daycare follow mom
            // TODO : Couple movements

            // for more complex scheduling include an extra table of non-default schedule/operation per location
            var OpenLocationType = Predicate("OpenLocationType", locationType)
                .If(LocationInformation, Time.CurrentlyOperating[operation], Time.CurrentlyOpen[schedule]);
            
            var Kid = Predicate("Kid", person).If(Alive, Age, age < 18);
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

            var WhereTheyAt = Predicate("WhereTheyAt", person.Key, actionType.Indexed, location.Indexed);
            WhereTheyAt.Unique = true;
            WhereTheyAtLocationIndex = (GeneralIndex<(Person, ActionType, Location), Location>)WhereTheyAt.IndexFor(location, false);

            var AdultAction = Predicate("AdultAction", actionType)
                .If(AvailableAction, actionType != ActionType.GoingToSchool);
            var NeedsActionAssignment = Predicate("NeedsActionAssignment", person).If(Alive,
                !GoingToWork[person, __],
                !GoingToDayCare[person, __],
                !GoingToSchool[person, __]);
            var RandomActionAssign = Predicate("RandomActionAssign", person, actionType)
                .If(NeedsActionAssignment, RandomElement(AdultAction, actionType));

            var LocationByActionAssign = Predicate("LocationByActionAssign", person, location);
            LocationByActionAssign.If(RandomActionAssign[person, ActionType.StayingIn], Home[person, location]);
            LocationByActionAssign.If(RandomActionAssign[person, ActionType.Visiting], Home[person, location]);

            // Choose the closest location with the action type assigned
            var OpenForBusinessByAction = Predicate("OpenForBusinessByAction", actionType, location)
                .If(ActionToCategory, AvailableCategory, OpenLocationType, InBusiness, Location);
            LocationByActionAssign.If(RandomActionAssign, actionType != ActionType.StayingIn, actionType != ActionType.Visiting,
                Minimal(location, distance, OpenForBusinessByAction & DistanceFromHome[person, location, distance]));

            WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToSchool);
            WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToDayCare);
            WhereTheyAt[person, ActionType.GoingToWork, location].If(GoingToWork);
            WhereTheyAt.If(RandomActionAssign, LocationByActionAssign);

            // *********************************** Interactions: **********************************

            var Interaction = Predicate("Interaction", person.Indexed, otherPerson.Indexed, interactionType.Indexed);

            var NotWorking = Predicate("NotWorking", person.Key, location.Indexed)
               .If(WhereTheyAt[person, actionType, location], actionType != ActionType.GoingToWork);

            var PotentialInteraction = Predicate("PotentialInteraction", person, otherPerson);
            PotentialInteraction.If(NotWorking[person, location], NotWorking[otherPerson, location], person != otherPerson);

            var ScoredInteraction = Predicate("ScoredInteraction", person.Indexed, otherPerson.Indexed, score);
            ScoredInteraction[person, otherPerson, RandomNormalFloat].If(PotentialInteraction);

            var PositiveInteraction = Predicate("PositiveInteraction", 
                person.Indexed, otherPerson.Indexed).If(ScoredInteraction, score > 10);
            var NeutralInteraction = Predicate("NeutralInteraction", 
                person.Indexed, otherPerson.Indexed).If(ScoredInteraction, score >= -15, score <= 10);
            var NegativeInteraction = Predicate("NegativeInteraction", 
                person.Indexed, otherPerson.Indexed).If(ScoredInteraction, score < -15);

            var ChosenPositiveInteraction = AssignRandomly("ChosenPositiveInteraction", PositiveInteraction);
            var ChosenNeutralInteraction = AssignRandomly("ChosenNeutralInteraction", NeutralInteraction);
            var ChosenNegativeInteraction = AssignRandomly("ChosenNegativeInteraction", NegativeInteraction);

            Interaction[person, partner, InteractionType.Flirting].If(ChosenPositiveInteraction[person, partner], SexualAttraction);
            Interaction[person, partner, InteractionType.Assisting].If(ChosenPositiveInteraction[person, partner], !SexualAttraction[person, partner]);
            Interaction[person, otherPerson, InteractionType.Chatting].If(ChosenNeutralInteraction);
            Interaction[person, otherPerson, InteractionType.Arguing].If(ChosenNegativeInteraction);

            Spark.Add[pairing, person, otherPerson, 1000].If(Interaction[person, otherPerson, InteractionType.Flirting], 
                                                             !Spark[__, person, otherPerson, __], NewRelationship[person, otherPerson, pairing]);
            Spark.Set(pairing, spark).If(Interaction[person, otherPerson, InteractionType.Flirting], 
                                                Spark[pairing, person, otherPerson, num], spark == num + 500);

            Charge.Add[pairing, person, otherPerson, 1000].If(Interaction[person, otherPerson, InteractionType.Assisting], 
                                                              !Charge[__, person, otherPerson, __], NewRelationship[person, otherPerson, pairing]);
            Charge.Set(pairing, charge).If(Interaction[person, otherPerson, InteractionType.Assisting],
                                           Charge[pairing, person, otherPerson, num], charge == num + 500);

            Charge.Add[pairing, person, otherPerson, 100].If(Interaction[person, otherPerson, InteractionType.Chatting],
                                                             !Charge[__, person, otherPerson, __], NewRelationship[person, otherPerson, pairing]);
            Charge.Set(pairing, charge).If(Interaction[person, otherPerson, InteractionType.Chatting], 
                                           Charge[pairing, person, otherPerson, num], charge == num + 50);

            Charge.Add[pairing, person, otherPerson, -900].If(Interaction[person, otherPerson, InteractionType.Arguing],
                                                             !Charge[__, person, otherPerson, __], NewRelationship[person, otherPerson, pairing]);
            Charge.Set(pairing, charge).If(Interaction[person, otherPerson, InteractionType.Arguing],
                                           Charge[pairing, person, otherPerson, num], charge == num - 450);

            Spark.Add[pairing, person, otherPerson, -900].If(Interaction[person, otherPerson, InteractionType.Arguing],
                                                             !Spark[__, person, otherPerson, __], NewRelationship[person, otherPerson, pairing]);
            Spark.Set(pairing, spark).If(Interaction[person, otherPerson, InteractionType.Arguing],
                                         Spark[pairing, person, otherPerson, num], spark == num - 450);

            Spark.Set(pairing, spark).If(Alive[person], Alive[otherPerson], !Interaction[person, otherPerson, __], 
                                         Spark[pairing, person, otherPerson, num], RegressToZero[num, spark]);

            Charge.Set(pairing, charge).If(Alive[person], Alive[otherPerson], !Interaction[person, otherPerson, __], 
                                           Charge[pairing, person, otherPerson, num], RegressToZero[num, charge]);

            // ************************************ END TABLES ************************************
            // ReSharper restore InconsistentNaming
            Simulation.EndPredicates();
            DataflowVisualizer.MakeGraph(Simulation, "Visualizations/Dataflow.dot");
            UpdateFlowVisualizer.MakeGraph(Simulation, "Visualizations/UpdateFlow.dot");
            Simulation.Update(); // optional, not necessary to call Update after EndPredicates
        } 

        public static void UpdateSimulator() {
#if ParallelUpdate
            if (update == null)
                LoopSimulator();
#else
            Time.Tick();
            Simulation.Update();
#endif
        }

#if ParallelUpdate
        private static Task update;
        
        static void LoopSimulator()
        {
            Time.Tick();
            Simulation.Update();
            update = Simulation.UpdateAsync().ContinueWith((_) => LoopSimulator());
        }
#endif
    }
}