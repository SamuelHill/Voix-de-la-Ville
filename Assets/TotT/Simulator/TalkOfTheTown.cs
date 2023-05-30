using System;
using System.Linq;
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
    using AgentRow = ValueTuple<Person, int, Date, Sex, Sexuality, VitalStatus>;
    using BirthRow = ValueTuple<Person, Person, Sex, Person>;
    using LocationRow = ValueTuple<Location, LocationType, Vector2Int, int, Date, LocationCategory>;
    using static CsvParsing; // DeclareParsers
    using static Functions;
    using static Randomize; // Seed and .RandomElement
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
        public TablePredicate<Location, LocationType, Vector2Int, int, Date> NewLocations;
        public TablePredicate<Location, LocationType, Vector2Int, int, Date> VacatedLocations;
        public TablePredicate<Person> Buried;
        public GeneralIndex<AgentRow, VitalStatus> AgentsVitalStatusIndex;
        public KeyIndex<LocationRow, Vector2Int> LocationsPositionIndex;
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

            // For Population calculation in GUI
            AgentsVitalStatusIndex = (GeneralIndex<AgentRow, VitalStatus>)Agents.IndexFor(vitalStatus, false);

            // Dead and Alive logic -
            var Alive = Definition("Alive", person)
                .Is(Agents[person, __, __, __, __, VitalStatus.Alive]);
            // special case Alive check where we also bind age
            var Age = Definition("Age", person, age)
                .Is(Agents[person, age, __, __, __, VitalStatus.Alive]);

            Agents.Set(person, vitalStatus, VitalStatus.Dead) // Dying
                .If(Age, age > 60, Prob[Time.PerMonth(0.003f)]);
            var JustDied = Predicate("JustDied", person)
                .If(Agents.Set(person, vitalStatus)); // Assumes only two vitalstatus's

            // Person Name helpers -
            var RandomFirstName = Definition("RandomFirstName", sex, firstName);
            RandomFirstName[Sex.Male, firstName].If(RandomElement(MaleNames, firstName));
            RandomFirstName[Sex.Female, firstName].If(RandomElement(FemaleNames, firstName));
            // Surname here is only being used to facilitate A naming convention for last names (currently paternal lineage)
            var RandomPerson = Definition("RandomPerson", sex, person)
                .Is(RandomFirstName, RandomElement(Surnames, lastName), person == NewPerson[firstName, lastName]);

            // Independent Agent creation (not birth based) - 
            var Drifter = Predicate("Drifter", person, sex, sexuality);
            Drifter[person, RandomSex, sexuality].If(Prob[Time.PerYear(0.05f)],
                RandomPerson, sexuality == RandomSexuality[sex]);
            Agents.Add[person, RandomAdultAge, RandomDate, sex, sexuality, VitalStatus.Alive].If(Drifter);

            // Add associated info that is needed for a new agent -
            Personality.Add[person, facet, RandomNormalSByte].If(Agents.Add, Facets);
            Aptitude.Add[person, job, RandomNormalSByte].If(Agents.Add, Jobs);
            // Agents.Add handles both Birth and Drifters, if we want to make kids inherit modified values from
            // their parents then we will need separate cases for BirthTo[__, __, __, person] and drifters.

            // ************************************** Couples *************************************
            // TODO : Better util for couples - facet similarity or score based on facet logic (> X, score + 100)
            // TODO : Married couples separate from ProcreativePair - last name changes in 'nickname' like table
            // TODO : Limit PotentialPairings by interactions (avoids performance hit for batch selection)
            // TODO : Non-monogamous pairings (needs above limitation as well) - alter NewProcreativePair too...
            // TODO : All non-intimate relationships 

            var PersonSex = Definition("PersonSex", person, sex)
                .Is(Agents[person, __, __, sex, __, VitalStatus.Alive]);
            var PersonSexuality = Definition("PersonSexuality", person, sexuality)
                .Is(Agents[person, __, __, __, sexuality, VitalStatus.Alive]);
            var AttractedSexuality = Definition("AttractedSexuality", person, partner)
                .Is(PersonSexuality[person, sexuality], PersonSex[partner, sexOfPartner], SexualityAttracted[sexuality, sexOfPartner],
                    PersonSexuality[partner, sexualOfPartner], PersonSex[person, sex], SexualityAttracted[sexualOfPartner, sex]);

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
            var ScoredPairings = Predicate("ScoredPairings", woman.Indexed, man.Indexed, util)
                .If(PotentialPairings, util == RandomNormal);
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

            // TODO : PotentialProcreate table
            // TODO : Limit Procreate by interactions to make procreation be more "physical" (also limits birth rates)
            // TODO : Limit Procreate by Personality (family planning, could include likely-hood to use contraceptives)
            // TODO : Limit Procreate by time since last birth and total number of children with partner (Gestation table info)
            #region Birth and aging:

            // Procreate Indexed by woman allows for multiple partners (in the same tick)
            var Procreate = Predicate("Procreate", woman.Indexed, man, sex, child);
            var ProcreateIndex = (GeneralIndex<BirthRow, Person>)Procreate.IndexFor(woman, false);
            var RandomProcreateByWoman = Function<Person, BirthRow>("RandomProcreateByWoman",
                p => ProcreateIndex.RowsMatching(p).ToList().RandomElement());
            var ProcreateMan = Item2(man, Procreate);
            var ProcreateSex = Item3(sex, Procreate);
            var ProcreateChild = Item4(child, Procreate);
            var procreateRow = RowVariable(Procreate);
            // woman should be NonVar | man, sex, and child are all Var
            var RandomProcreate = Definition("RandomProcreate",
                woman, man, sex, child).Is(procreateRow == RandomProcreateByWoman[woman],
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
            Gestation.Add[woman, man, sex, child, Time.CurrentDate, true]
                .If(Count(Procreate[woman, __, __, __]) <= 1, Procreate);
            Gestation.Add[woman, man, sex, child, Time.CurrentDate, true]
                .If(Count(Procreate[woman, __, __, __]) > 1,
                    Procreate[woman, __, __, __], RandomProcreate);

            // Need to alter the state of the gestation table when giving birth, otherwise birth after 9 months with 'labor'
            var BirthTo = Predicate("BirthTo", woman, man, sex, child);
            BirthTo.If(Gestation[woman, man, sex, child, conception, true], Time.NineMonthsPast[conception], Prob[0.8f]);
            Gestation.Set(child, state, false).If(BirthTo);

            // BirthTo has a column for the sex of the child to facilitate gendered naming, however, since there is no need to
            // determine the child's sexuality in BirthTo, a child has the sexuality established when they are added to Agents
            Agents.Add[person, 0, Time.CurrentDate, sex, sexuality, VitalStatus.Alive].If(
                BirthTo[__, __, sex, person], sexuality == RandomSexuality[sex]);

            Parents.Add.If(BirthTo[parent, __, __, child]);
            Parents.Add.If(BirthTo[__, parent, __, child]);

            // Increment age once per birthday (in the AM, if you weren't just born)
            var WhenToAge = Definition("WhenToAge", person, age).Is(
                Agents[person, age, dateOfBirth, __, __, VitalStatus.Alive],
                Time.CurrentlyMorning, Time.IsToday[dateOfBirth], !BirthTo[__, __, __, person]);
            Increment(Agents, person, age, WhenToAge);
            #endregion

            // ********************************* Locations: *********************************

            #region Location Tables:
            // These tables are used for adding and removing tiles from the tilemap in unity efficiently -
            // by not having to check over all Locations. VacatedLocations is currently UNUSED
            NewLocations = Predicate("NewLocations", location, locationType, position, founded, opening);
            VacatedLocations = Predicate("VacatedLocations", location, locationType, position, founded, opening);

            var Locations = Predicate("Locations", location.Key, locationType.Indexed,
                position.Key, founded, opening, locationCategory.Indexed);
            Locations.Initially.Where(PrimordialLocations, LocationInformation);
            Locations.Add.If(NewLocations, LocationInformation);
            LocationsPositionIndex = Locations.KeyIndex(position);
            var NumLots = Length("NumLots", Locations);

            // for efficient checks to see if a location category is present:
            var AvailableCategories = Predicate("AvailableCategories", locationCategory);
            AvailableCategories.Initially.Where(Once[Goals(LocationInformation, PrimordialLocations)]);
            AvailableCategories.Add.If(Once[Goals(Locations.Add, !AvailableCategories[locationCategory])]);
            #endregion

            // TODO : Separate out the dead into a new table... involve removal ?
            // TODO : Include ApartmentComplex locations in Housing logic
            // TODO : Include Inn locations in Housing logic - drifters start at an Inn ?
            #region Housing:
            var Homes = Predicate("Homes", occupant.Key, location.Indexed);
            Homes.Unique = true;
            var Occupancy = Predicate("Occupancy", location, count)
                .If(Locations[location, LocationType.House, __, __, __, __], count == Count(Homes));
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
                Locations[location, LocationType.Cemetery, __, __, __, __]);
            var BuriedAt = Predicate("BuriedAt", occupant, location)
                .If(Locations[location, LocationType.Cemetery, __, __, __, __], Homes);
            // with only the one cemetery for now, the follow will suffice for the GUI
            Buried = Predicate("Buried", person).If(BuriedAt[person, __]);

            // Distance per person makes most sense when measured from either where the person is,
            // or where they live. This handles the latter:
            var DistanceFromHome = Definition("DistanceFromHome", person, location, distance)
                .Is(Locations[location, __, position, __, __, __],
                    Homes[person, otherLocation],
                    Locations[otherLocation, __, otherPosition, __, __, __],
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

            var MovingIn = Predicate("MovingIn", person, location).If(Once[WantToMove[person]],
                RandomElement(WantToMove, person), RandomElement(UnderOccupied, location));

            Homes.Set(occupant, location).If(MovingIn[occupant, location]);
            #endregion

            #region New Location helper Functions and Definitions:
            // Helper functions and definitions for creating new locations at a valid lot in town
            var IsVacant = Definition("IsVacant", position)
                .Is(!Locations[__, __, position, __, __, __]);
            var FreeLot = Definition("FreeLot", position)
                .Is(position == RandomLot[NumLots], IsVacant[position]);
            #endregion

            #region New Location helper functions (meta-sub-expressions):
            // Base case - useful mainly for testing/rapid development (you only need one string/generating a list of names can come second)
            void AddNewNamedLocation(LocationType locType, string name, Goal readyToAdd) =>
                NewLocations[location, locType, position, Time.CurrentYear, Time.CurrentDate]
                    .If(FreeLot, Prob[Time.PerWeek(0.5f)], // Needs the random lot to be available & 'construction' isn't instantaneous
                    readyToAdd, location == NewLocation[name]); // otherwise, check the readyToAdd Goal and if it passes add a NewLocation

            // If you are only planning on adding a single location of the given type, this adds the check that
            // a location of locType doesn't already exist.
            void AddOneLocation(LocationType locType, string name, Goal readyToAdd) => AddNewNamedLocation(locType, name,
                !Locations[__, locType, __, __, __, __] & readyToAdd);

            // This is the more realistic use case with a list of names for a give type to choose from.
            void AddNewLocation(LocationType locType, TablePredicate<string> names, Goal readyToAdd) =>
                NewLocations[location, locType, position, Time.CurrentYear, Time.CurrentDate]
                    .If(FreeLot, Prob[Time.PerWeek(0.5f)], readyToAdd,
                    RandomElement(names, locationName), location == NewLocation[locationName]);
            #endregion

            // TODO : Add more new locations for each location type
            #region New Location Logic:
            AddNewLocation(LocationType.House, HouseNames, !!WantToMove[person]);
            // Currently the following only happens with drifters - everyone starts housed
            AddNewLocation(LocationType.House, HouseNames,
                Count(Homes[person, location] & Alive[person]) < Count(Alive));

            AddOneLocation(LocationType.Hospital, "St. Asmodeus",
                Once[Goals(Aptitude[person, Vocation.Doctor, aptitude], aptitude > 15, Age, age > 21)]);

            AddOneLocation(LocationType.Cemetery, "The Old Cemetery",
                Once[Goals(Alive, Age, age >= 60)]);

            AddOneLocation(LocationType.DayCare, "Pumpkin Preschool",
                Count(Age & (age < 6)) > 5);

            AddOneLocation(LocationType.School, "Talk of the Township High",
                Count(Age & (age >= 5) & (age < 18)) > 5);
            #endregion

            // ********************************* Vocations: *********************************
            
            var Vocations = Predicate("Vocations", job.Indexed, employee, location.Indexed, timeOfDay.Indexed);

            var JobsToFill = Predicate("JobsToFill", location, job)
                .If(timeOfDay == Time.CurrentTimeOfDay, Locations, VocationShifts,
                    PositionsPerJob, Count(Vocations) < positions);

            var Candidates = Predicate("Candidates", person, job, location)
                .If(JobsToFill, Maximal(person, aptitude, Goals(Alive[person],
                    !Vocations[__, person, __, __], Age, age > 18, Aptitude)));

            Vocations.Add[job, person, location, Time.CurrentTimeOfDay].If(Candidates);

            // ********************************** Movement: *********************************
            
            var AvailableActions = Predicate("AvailableActions", actionType)
                .If(ActionToCategory, AvailableCategories);
            // for more complex scheduling include an extra table of non-default schedule/operation per location
            var OpenLocationTypes = Predicate("OpenLocationTypes", locationType)
                .If(LocationInformation, Time.CurrentlyOperating[operation], Time.CurrentlyOpen[schedule]);

            #region Schooling:
            var Kids = Predicate("Kids", person).If(Alive, Age, age < 18);
            var NeedsSchooling = Predicate("NeedsSchooling", person).If(Kids, Age, age > 6);
            var NeedsDayCare = Predicate("NeedsDayCare", person).If(Kids, !NeedsSchooling[person]);

            var GoingToSchool = Predicate("GoingToSchool", person, location).If(
                AvailableActions[ActionType.GoingToSchool], OpenLocationTypes[LocationType.School],
                Locations[location, LocationType.School, __, __, __, __], // only expecting one location...
                NeedsSchooling);
            var GoingToDayCare = Predicate("GoingToDayCare", person, location).If(
                AvailableActions[ActionType.GoingToSchool], OpenLocationTypes[LocationType.DayCare],
                Locations[location, LocationType.DayCare, __, __, __, __], // only expecting one location...
                NeedsDayCare);
            #endregion

            #region Working:
            var GoingToWork = Predicate("GoingToWork", person, location)
                .If(Vocations[__, person, location, Time.CurrentTimeOfDay], OpenLocationTypes, Locations);
            #endregion

            // TODO : Visiting action choose location of relative or partner (future friends)
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

            // Choose the closest location with the action type assigned
            var OpenForBusinessByAction = Predicate("OpenForBusinessByAction", actionType, location)
                .If(ActionToCategory, AvailableCategories, OpenLocationTypes, Locations);
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
        } // optional, not necessary to call Update after EndPredicates

        public void UpdateSimulator() {
            Time.Tick();
            Simulation.Update();
        }
    }
}