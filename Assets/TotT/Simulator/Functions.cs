using TED;
using TED.Primitives;
using TotT.Time;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace TotT.Simulator {
    using static Sexuality;
    using static Calendar;
    using static Randomize;

    /// <summary>TED.Functions and TED.PrimitiveTests wrappers for Utility and ValueType functionality.</summary>
    /// <remarks>Predicates (includes Functions and Primitive tests) are TitleCase.</remarks>
    public static class Functions {
        // ReSharper disable InconsistentNaming
        public static readonly Function<string, string, Person> NewPerson = Method<string, string, Person>(Sims.NewPerson, false);
        public static readonly Function<Person, string> Surname = new(nameof(Surname), p => p.LastName);

        public static readonly Function<int, float> FertilityRate = Method<int, float>(Sims.FertilityRate);
        public static readonly Function<int> RandomAdultAge = Method(Sims.RandomAdultAge, false);
        public static readonly Function<Sex> RandomSex = Method(Sims.RandomSex, false);
        public static readonly Function<Sex, Sexuality> RandomSexuality = new(nameof(RandomSexuality), Random, false);

        public static readonly PrimitiveTest<Sexuality, Sex> AttractedTo = new(nameof(AttractedTo), (sexuality, s) => sexuality.IsAttracted(s));
        public static readonly Function<Person, Person, int> Similarity = new(nameof(Similarity), (p1, p2) => p1.Similarity(p2));
        public static readonly Function<Person, Person, int> Compatibility = new(nameof(Compatibility), (p1, p2) => p1.Compatibility(p2));

        public static readonly Function<Favorability> Favorable = Method(Sims.Favorable, false);

        // Should we have Actions? this doesn't neatly fit Function or PrimitiveTest patterns
        public static readonly PrimitiveTest<Person, Person> TakeLastName = new(nameof(TakeLastName), (person, other) => person.TakeLastName(other));

        public static readonly Function<Vector2Int, Vector2Int, int> Distance = Method<Vector2Int, Vector2Int, int>(Town.Distance);
        public static readonly Function<string, Location> NewLocation = Method<string, Location>(Town.NewLocation, false);
        public static readonly Function<int, Vector2Int> RandomLot = Method<int, Vector2Int>(Town.RandomLot, false);

        public static readonly Function<Date> RandomDate = new(nameof(RandomDate), Random, false);
        public static readonly Function<TimePoint, Date> TimePointToDate = new(nameof(TimePointToDate), t => t);
        public static readonly Function<Date, int, TimePoint> DateAgeToTimePoint = new(nameof(DateAgeToTimePoint), TimePointFromDateAndAge);

        public static readonly Function<int> RandomNormal = Method(BellCurve, false);
        public static readonly Function<sbyte> RandomNormalSByte = Method(SByteBellCurve, false);
        public static readonly Function<float> RandomNormalFloat = Method(FloatBellCurve, false);
        
        public static readonly Function<int, int> Incr = new(nameof(Incr), i => i + 1);
    }
}
