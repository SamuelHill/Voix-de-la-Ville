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
        public TablePredicate<Vector2Int, Location, LocationType, TimePoint> NewLocations;
        public TablePredicate<Vector2Int> VacatedLocations;
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
            var Agents = Predicate("Agents", person.Key, 
                age, dateOfBirth.Indexed, sex.Indexed, sexuality, vitalStatus.Indexed);
            Agents.Initially[person, age, dateOfBirth, sex, sexuality, VitalStatus.Alive].Where(PrimordialBeings);
            // ditto for agents associated tables -
            var Personality = Predicate("Personality", person.Indexed, facet.Indexed, personality);
            Personality.Initially[person, facet, RandomNormalSByte].Where(PrimordialBeings, Facets);
            var Aptitude = Predicate("Aptitude", person.Indexed, job.Indexed, aptitude);
            Aptitude.Initially[person, job, RandomNormalSByte].Where(PrimordialBeings, Jobs);

            var PopulationCount = CountsBy("PopulationCount", Agents, vitalStatus, count);
            PopulationCount.IndexByKey(vitalStatus);
            PopulationCountIndex = (KeyIndex<(VitalStatus, int), VitalStatus>)PopulationCount.IndexFor(vitalStatus, true);

            var Alive = Definition("Alive", person)
                .Is(Agents[person, __, __, __, __, VitalStatus.Alive]);
            // special case Alive check where we also bind age
            var Age = Definition("Age", person, age)
                .Is(Agents[person, age, __, __, __, VitalStatus.Alive]);

            Agents.Set(person, vitalStatus, VitalStatus.Dead) // Dying
                .If(Age, age > 60, PerMonth(0.003f));
            var JustDied = Predicate("JustDied", person)
                .If(Agents.Set(person, vitalStatus)); // Assumes set only used for dying (two vitalStatus values)

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
            Agents.Add[person, RandomAdultAge, RandomDate, sex, sexuality, VitalStatus.Alive].If(Drifter);

            // Add associated info that is needed for a new agent -
            Personality.Add[person, facet, RandomNormalSByte].If(Agents.Add, Facets);
            Aptitude.Add[person, job, RandomNormalSByte].If(Agents.Add, Jobs);
            // Agents.Add handles both Birth and Drifters, if we want to make kids inherit modified values from
            // their parents then we will need separate cases for BirthTo[__, __, __, person] and drifters.

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
            var Parents = Predicate("Parents", parent, child);
            var FamilialRelation = Definition("FamilialRelation", person, otherPerson)
                .Is(Parents[person, otherPerson] | Parents[otherPerson, person]); // only immediate family

            var PersonSex = Definition("PersonSex", person, sex)
                .Is(Agents[person, __, __, sex, __, VitalStatus.Alive]);
            var PersonSexuality = Definition("PersonSexuality", person, sexuality)
                .Is(Agents[person, __, __, __, sexuality, VitalStatus.Alive]);
            var SexualAttraction = Definition("SexualAttraction", person, partner)
                .Is(PersonSexuality[person, sexuality], PersonSex[partner, sexOfPartner], SexualityAttracted[sexuality, sexOfPartner],
                    PersonSexuality[partner, sexualOfPartner], PersonSex[person, sex], SexualityAttracted[sexualOfPartner, sex]);
            
            var Men = Predicate("Men", man).If(
                Agents[man, age, __, Sex.Male, __, VitalStatus.Alive], age >= 18);
            var Women = Predicate("Women", woman).If(
                Agents[woman, age, __, Sex.Female, __, VitalStatus.Alive], age >= 18);

            var ProcreativePair = Predicate("ProcreativePair", woman.Indexed, man.Indexed);
            ProcreativePair.Unique = true;
            var PotentialPairings = Predicate("PotentialPairings", woman.Indexed, man.Indexed)
                .If(Women, !ProcreativePair[woman, __], Men, !ProcreativePair[__, man],
                    SexualAttraction[woman, man], !FamilialRelation[woman, man]);
            var ScoredPairings = Predicate("ScoredPairings", woman.Indexed, man.Indexed, score);
            ScoredPairings[woman, man, RandomNormalFloat].If(PotentialPairings);
            var PairForProcreate = MatchGreedily("PairForProcreate", ScoredPairings);
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

            Agents.Add[person, 0, Time.CurrentDate, sex, sexuality, VitalStatus.Alive].If(
                BirthTo[__, __, sex, person], RandomSexuality[sex, sexuality]);

            Parents.Add.If(BirthTo[parent, __, __, child]);
            Parents.Add.If(BirthTo[__, parent, __, child]);

            // Increment age once per birthday (in the AM, if you weren't just born)
            var WhenToAge = Definition("WhenToAge", person, age).Is(
                Agents[person, age, dateOfBirth, __, __, VitalStatus.Alive],
                Time.CurrentlyMorning, Time.IsToday[dateOfBirth], !BirthTo[__, __, __, person]);
            Increment(Agents, person, age, WhenToAge);

            // ************************************ Locations *************************************

            var Locations = Predicate("Locations", location.Key, 
                locationType.Indexed, locationCategory.Indexed, position.Indexed, founding, businessStatus.Indexed);
            Locations.Initially[location, locationType, locationCategory, position, founding, BusinessStatus.InBusiness]
                     .Where(PrimordialLocations, LocationInformation);

            // NewLocations is used for both adding tiles to the TileMap in Unity efficiently
            // (not checking every row of the Locations table) and for collecting new locations
            // with the minimal amount of information needed (excludes derivable columns).
            NewLocations = Predicate("NewLocations", position, location, locationType, founding);
            Locations.Add[location, locationType, locationCategory, position, founding, BusinessStatus.InBusiness]
                     .If(NewLocations, LocationInformation);

            var InBusiness = Definition("InBusiness", location)
               .Is(Locations[location, __, __, __, __, BusinessStatus.InBusiness]);
            
            Locations.Set(location, businessStatus, BusinessStatus.OutOfBusiness)
                     .If(Locations, 
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
               .If(Locations.Set(location, businessStatus));

            // VacatedLocations is used for removing tiles from the TileMap
            VacatedLocations = Predicate("VacatedLocations", position);
            VacatedLocations.If(Locations.Set(location, businessStatus), Locations);

            // For tile hover info strings we only want the locations in business, and since we will be
            // using this table for GUI interactions we need to be able to access a location by position.
            // Each lot in town can only hold one active location so this works out nicely.
            var DisplayLocations = Predicate("DisplayLocations", 
                position.Key, location, locationType,  founding)
               .If(Locations[location, locationType, __, position, founding, BusinessStatus.InBusiness]);
            LocationsPositionIndex = DisplayLocations.KeyIndex(position);

            // Helper functions and definitions for creating new locations at a valid lot in town
            var NumLots = Length("NumLots", Locations); // NumLots helps expand town borders
            var IsVacant = Definition("IsVacant", position)
                .Is(!Locations[__, __, __, position, __, BusinessStatus.InBusiness]);
            var FreeLot = Definition("FreeLot", position)
                .Is(RandomLot[NumLots, position], IsVacant[position]);


            var CategoryCount = CountsBy("CategoryCount", Locations, locationCategory, count);
            var AvailableCategories = Predicate("AvailableCategories", locationCategory).If(CategoryCount);
            

            var AvailableActions = Predicate("AvailableActions", actionType)
                .If(ActionToCategory, AvailableCategories); // need one to one from this, too many staying in and visiting entries currently

            // ************************************** Housing *************************************
            // TODO : Include ApartmentComplex locations in Housing logic
            // TODO : Include Inn locations in Housing logic - drifters start at an Inn ?
            // TODO : Families move in and out of houses together
            // TODO : Initialize Homes first based on primordial couples, then on all single agents

            var Homes = Predicate("Homes", occupant.Key, location.Indexed);
            Homes.Unique = true;
            var RealHomes = Predicate("RealHomes", occupant, location.Indexed)
                .If(Locations[location, LocationType.House, __, __, __, BusinessStatus.InBusiness], Homes);
            var Occupancy = CountsBy("Occupancy", RealHomes, location, count);
            var Unoccupied = Predicate("Unoccupied", location)
                .If(Locations[location, LocationType.House, __, __, __, BusinessStatus.InBusiness], !Homes[__, location]);
            var UnderOccupied = Predicate("UnderOccupied", location);
            UnderOccupied.If(Occupancy, count <= 5);
            UnderOccupied.If(Unoccupied);

            // Using this to randomly assign one house per primordial person...
            var PrimordialHouses = Predicate("PrimordialHouses", location)
                .If(PrimordialLocations[location, LocationType.House, __, __]);
            Homes.Initially.Where(PrimordialBeings[occupant, __, __, __, __], 
                RandomElement(PrimordialHouses, location));

            Homes.Add.If(BirthTo[man, woman, sex, occupant], Homes[woman, location]); // Move in with mom
            Homes.Add.If(Drifter[occupant, __, __], RandomElement(UnderOccupied, location));

            Homes.Set(occupant, location).If(JustDied[occupant],
                Locations[location, LocationType.Cemetery, __, __, __, BusinessStatus.InBusiness]);
            var BuriedAt = Predicate("BuriedAt", occupant, location)
                .If(Locations[location, LocationType.Cemetery, __, __, __, BusinessStatus.InBusiness], Homes);
            // with only the one cemetery for now, the follow will suffice for the GUI
            Buried = Predicate("Buried", person).If(BuriedAt[person, __]);

            var ForeclosedUpon = Predicate("ForeclosedUpon", occupant).If(Homes, JustClosed);
            Homes.Set(occupant, location).If(ForeclosedUpon, RandomElement(UnderOccupied, location));

            // Distance per person makes most sense when measured from either where the person is,
            // or where they live. This handles the latter:
            var DistanceFromHome = Definition("DistanceFromHome", person, location, distance)
                .Is(Locations[location, __, __, position, __, BusinessStatus.InBusiness],
                    Homes[person, otherLocation],
                    Locations[otherLocation, __, __, otherPosition, __, BusinessStatus.InBusiness],
                    Distance[position, otherPosition, distance]);
            
            var WantToMove = Predicate("WantToMove", person)
                .If(Homes[person, location], Occupancy, count >= 8);
            var MovingIn = Predicate("MovingIn", person, location)
                .If(Once[WantToMove[__]], RandomElement(WantToMove, person), RandomElement(UnderOccupied, location));
            Homes.Set(occupant, location).If(MovingIn[occupant, location]);

            // ********************************** New Locations ***********************************
            // TODO : Add more new locations for each location type
            
            // Needs the random lot to be available & 'construction' isn't instantaneous
            void AddOneLocation(LocationType locType, string name, Goal readyToAdd) => 
                NewLocations[position, location, locType, Time.CurrentTimePoint].If(FreeLot, // assign position and check
                    PerWeek(0.5f), !Locations[__, locType, __, __, __, BusinessStatus.InBusiness],
                    readyToAdd, NewLocation[name, location]);

            void AddNewLocation(LocationType locType, TablePredicate<string> names, Goal readyToAdd) =>
                NewLocations[position, location, locType, Time.CurrentTimePoint].If(FreeLot, 
                    PerWeek(0.5f), readyToAdd, RandomElement(names, locationName), NewLocation[locationName, location]);

            AddNewLocation(LocationType.House, HouseNames, !!WantToMove[person]);
            //AddNewLocation(LocationType.House, HouseNames, Once[WantToMove[__]]);
            // Currently the following only happens with drifters - everyone starts housed
            AddNewLocation(LocationType.House, HouseNames, Count(Homes[person, location] & Alive[person]) < Count(Alive));

            AddOneLocation(LocationType.Hospital, "St. Asmodeus",
                Once[Goals(Aptitude[person, Vocation.Doctor, aptitude], aptitude > 15, Age, age > 21)]);

            AddOneLocation(LocationType.Cemetery, "The Old Cemetery", Once[Goals(Alive, Age, age >= 60)]);

            AddOneLocation(LocationType.DayCare, "Pumpkin Preschool", Count(Age & (age < 6)) > 5);

            AddOneLocation(LocationType.School, "Talk of the Township High", Count(Age & (age >= 5) & (age < 18)) > 5);

            AddOneLocation(LocationType.CityHall, "Big City Hall", Goals(PopulationCount[VitalStatus.Alive, count], count > 200));

            AddOneLocation(LocationType.GeneralStore, "Big Box Store", Goals(PopulationCount[VitalStatus.Alive, count], count > 150));

            AddOneLocation(LocationType.Bar, "Triple Crossing", Goals(PopulationCount[VitalStatus.Alive, count], count > 125));

            AddOneLocation(LocationType.GroceryStore, "Trader Jewels", Goals(PopulationCount[VitalStatus.Alive, count], count > 100));

            AddOneLocation(LocationType.TattooParlor, "Heroes and Ghosts", Goals(PopulationCount[VitalStatus.Alive, count], count > 250));

            // ************************************ Vocations: ************************************

            var Vocations = Predicate("Vocations", job.Indexed, employee.Key, location.Indexed, timeOfDay.Indexed);

            var VocationStatus = Predicate("VocationStatus", employee.Key, state.Indexed);
            VocationStatus.Add[employee, true].If(Vocations.Add);
            VocationStatus.Set(employee, state, false).If(JustDied[employee], VocationStatus[employee, true]);

            var StillEmployed = Definition("StillEmployed", person).Is(VocationStatus[person, true]);

            var JobsToFill = Predicate("JobsToFill", location, job)
                .If(Time.CurrentTimeOfDay[timeOfDay], InBusiness, Locations, VocationShifts,
                    PositionsPerJob, Count(Vocations & VocationStatus[employee, true]) < positions);

            var Candidates = Predicate("Candidates", person, job, location)
                .If(JobsToFill, Maximal(person, aptitude, 
                                        Goals(Alive, !StillEmployed[person], Age, age >= 18, Aptitude)));

            var CandidatesToFilter = Predicate("CandidatesToFilter", person.Indexed, job).If(Candidates);
            var FilteredCandidates = AssignRandomly("FilteredCandidates", CandidatesToFilter);
            var SelectedCandidates = Predicate("SelectedCandidates", 
                person, job, location).If(FilteredCandidates, Candidates);

            Vocations.Add[job, person, location, Time.CurrentTimeOfDay].If(SelectedCandidates);

            // ************************************* Movement: ************************************
            // TODO : Visiting action choose location of relative or partner (future friends)
            // TODO : Couple movements
            // TODO : Babies not in daycare follow mom

            // for more complex scheduling include an extra table of non-default schedule/operation per location
            var OpenLocationTypes = Predicate("OpenLocationTypes", locationType)
                .If(LocationInformation, Time.CurrentlyOperating[operation], Time.CurrentlyOpen[schedule]);
            
            var Kids = Predicate("Kids", person).If(Alive, Age, age < 18);
            var NeedsSchooling = Predicate("NeedsSchooling", person).If(Kids, Age, age > 6);
            var NeedsDayCare = Predicate("NeedsDayCare", person).If(Kids, !NeedsSchooling[person]);

            var GoingToSchool = Predicate("GoingToSchool", person, location).If(
                AvailableActions[ActionType.GoingToSchool], OpenLocationTypes[LocationType.School],
                Locations[location, LocationType.School, __, __, __, BusinessStatus.InBusiness],
                NeedsSchooling);
            var GoingToDayCare = Predicate("GoingToDayCare", person, location).If(
                AvailableActions[ActionType.GoingToSchool], OpenLocationTypes[LocationType.DayCare],
                Locations[location, LocationType.DayCare, __, __, __, BusinessStatus.InBusiness],
                NeedsDayCare);
            
            var GoingToWork = Predicate("GoingToWork", person, location)
                .If(OpenLocationTypes, InBusiness, Locations, StillEmployed, Vocations[__, person, location, Time.CurrentTimeOfDay]);

            var WhereTheyAt = Predicate("WhereTheyAt", person.Key, actionType.Indexed, location.Indexed);
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

            // Choose the closest location with the action type assigned
            var OpenForBusinessByAction = Predicate("OpenForBusinessByAction", actionType, location)
                .If(ActionToCategory, AvailableCategories, OpenLocationTypes, InBusiness, Locations);
            LocationByActionAssign.If(RandomActionAssign, actionType != ActionType.StayingIn, actionType != ActionType.Visiting,
                Minimal(location, distance, OpenForBusinessByAction & DistanceFromHome[person, location, distance]));

            WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToSchool);
            WhereTheyAt[person, ActionType.GoingToSchool, location].If(GoingToDayCare);
            WhereTheyAt[person, ActionType.GoingToWork, location].If(GoingToWork);
            WhereTheyAt.If(RandomActionAssign, LocationByActionAssign);

            // *********************************** Interactions: **********************************

            var Interactions = Predicate("Interactions", person.Indexed, otherPerson.Indexed, interactionType.Indexed);

            var NotWorking = Predicate("NotWorking", person.Key, location.Indexed)
               .If(WhereTheyAt[person, actionType, location], actionType != ActionType.GoingToWork);

            var PotentialInteractions = Predicate("PotentialInteractions", person, otherPerson);
            PotentialInteractions.If(NotWorking[person, location], NotWorking[otherPerson, location]);

            var ScoredInteractions = Predicate("ScoredInteractions", person.Indexed, otherPerson.Indexed, score);
            ScoredInteractions[person, otherPerson, RandomNormalFloat].If(PotentialInteractions);

            var PositiveInteractions = Predicate("PositiveInteractions", 
                person.Indexed, otherPerson.Indexed).If(ScoredInteractions, score > 10);
            var NeutralInteractions = Predicate("NeutralInteractions", 
                person.Indexed, otherPerson.Indexed).If(ScoredInteractions, score >= -15, score <= 10);
            var NegativeInteractions = Predicate("NegativeInteractions", 
                person.Indexed, otherPerson.Indexed).If(ScoredInteractions, score < -15);

            var ChosenPositiveInteractions = AssignRandomly("ChosenPositiveInteractions", PositiveInteractions);
            var ChosenNeutralInteractions = AssignRandomly("ChosenNeutralInteractions", NeutralInteractions);
            var ChosenNegativeInteractions = AssignRandomly("ChosenNegativeInteractions", NegativeInteractions);

            Interactions[person, partner, InteractionType.Flirting].If(ChosenPositiveInteractions[person, partner], SexualAttraction);
            Interactions[person, partner, InteractionType.Assisting].If(ChosenPositiveInteractions[person, partner], !SexualAttraction[person, partner]);
            Interactions[person, otherPerson, InteractionType.Chatting].If(ChosenNeutralInteractions);
            Interactions[person, otherPerson, InteractionType.Arguing].If(ChosenNegativeInteractions);

            // ************************************ END TABLES ************************************
            // ReSharper restore InconsistentNaming
            Simulation.EndPredicates();
            DataflowVisualizer.MakeGraph(Simulation, "TotT.dot");
            UpdateFlowVisualizer.MakeGraph(Simulation, "TotTupdate.dot");
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