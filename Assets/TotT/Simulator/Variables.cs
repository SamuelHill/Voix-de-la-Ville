using TED;
using TotT.ValueTypes;
using UnityEngine;

namespace TotT.Simulator {
    /// <summary>Commonly used TED.Vars for types used in this simulator.</summary>
    /// <remarks>Variables will be lowercase for style/identification purposes.</remarks>
    public static class Variables {
        // ReSharper disable InconsistentNaming
        public static readonly Var<Person> person = (Var<Person>)"person";
        public static readonly Var<Person> partner = (Var<Person>)"partner";
        public static readonly Var<Person> parent = (Var<Person>)"parent";
        public static readonly Var<Person> child = (Var<Person>)"child";
        public static readonly Var<Person> man = (Var<Person>)"man";
        public static readonly Var<Person> woman = (Var<Person>)"woman";
        public static readonly Var<Person> employee = (Var<Person>)"employee";
        public static readonly Var<Person> occupant = (Var<Person>)"occupant";
        public static readonly Var<Person> otherPerson = (Var<Person>)"otherPerson";
        public static readonly Var<string> firstName = (Var<string>)"firstName";
        public static readonly Var<string> lastName = (Var<string>)"lastName";

        public static readonly Var<Sex> sex = (Var<Sex>)"sex";
        public static readonly Var<Sex> sexOfPartner = (Var<Sex>)"sexOfPartner";
        public static readonly Var<Sexuality> sexuality = (Var<Sexuality>)"sexuality";
        public static readonly Var<Sexuality> sexualOfPartner = (Var<Sexuality>)"sexualityOfPartner";
        public static readonly Var<VitalStatus> vitalStatus = (Var<VitalStatus>)"vitalStatus";
        public static readonly Var<BusinessStatus> businessStatus = (Var<BusinessStatus>)"businessStatus";

        public static readonly Var<Facet> facet = (Var<Facet>)"facet";
        public static readonly Var<sbyte> personality = (Var<sbyte>)"personality";
        public static readonly Var<Vocation> job = (Var<Vocation>)"job";
        public static readonly Var<sbyte> aptitude = (Var<sbyte>)"aptitude";

        public static readonly Var<int> age = (Var<int>)"age";
        public static readonly Var<Date> conception = (Var<Date>)"conception";
        public static readonly Var<Date> dateOfBirth = (Var<Date>)"dateOfBirth";
        public static readonly Var<TimePoint> dob = (Var<TimePoint>)"dob";
        public static readonly Var<TimePoint> founding = (Var<TimePoint>)"founding";
        public static readonly Var<TimePoint> time = (Var<TimePoint>)"time";

        public static readonly Var<Location> location = (Var<Location>)"location";
        public static readonly Var<Location> otherLocation = (Var<Location>)"otherLocation";
        public static readonly Var<Vector2Int> position = (Var<Vector2Int>)"position";
        public static readonly Var<Vector2Int> otherPosition = (Var<Vector2Int>)"otherPosition";
        public static readonly Var<LocationType> locationType = (Var<LocationType>)"locationType";
        public static readonly Var<LocationCategory> locationCategory = (Var<LocationCategory>)"locationCategory";
        public static readonly Var<DailyOperation> operation = (Var<DailyOperation>)"operation";
        public static readonly Var<TimeOfDay> timeOfDay = (Var<TimeOfDay>)"timeOfDay";
        public static readonly Var<Schedule> schedule = (Var<Schedule>)"schedule";
        public static readonly Var<Color> color = (Var<Color>)"color";
        public static readonly Var<int> positions = (Var<int>)"positions";
        public static readonly Var<string> locationName = (Var<string>)"locationName";

        public static readonly Var<ActionType> actionType = (Var<ActionType>)"actionType";
        public static readonly Var<InteractionType> interactionType = (Var<InteractionType>)"interactionType";
        public static readonly Var<int> distance = (Var<int>)"distance";
        public static readonly Var<int> count = (Var<int>)"count";
        public static readonly Var<int> num = (Var<int>)"num";
        public static readonly Var<int> spark = (Var<int>)"spark";
        public static readonly Var<int> charge = (Var<int>)"charge";

        public static readonly Var<RelationshipId<Person>> pairing = (Var<RelationshipId<Person>>)"pairing";
        public static readonly Var<RelationshipId<Person>> otherPairing = (Var<RelationshipId<Person>>)"otherPairing";

        public static readonly Var<float> score = (Var<float>)"score";
        public static readonly Var<bool> state = (Var<bool>)"state";
    }
}