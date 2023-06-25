﻿using System;
using TED;
using TED.Primitives;
using TotT.Simulog;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;
using static TED.Language;
using Function = TED.Function;

namespace TotT.Simulator {
    using static Randomize;

    /// <summary>TED.Functions and TED.PrimitiveTests wrappers for Utility and ValueType functionality.</summary>
    /// <remarks>Predicates (includes Functions and Primitive tests) are TitleCase.</remarks>
    public static class Functions {
        // ReSharper disable InconsistentNaming
        public static readonly Function<string, string, Person> NewPerson = Method<string, string, Person>(Sims.NewPerson, false);
        public static readonly Function<Person, string> Surname = Function<Person, string>(nameof(Surname), p => p.LastName);
        public static readonly Function<int, float> FertilityRate = Method<int, float>(Sims.FertilityRate);
        public static readonly Function<int> RandomAdultAge = Method(Sims.RandomAdultAge, false);
        public static readonly Function<Sex> RandomSex = Method(Sims.RandomSex, false);
        public static readonly Function<Sex, Sexuality> RandomSexuality = Function<Sex, Sexuality>(nameof(RandomSexuality), Sexuality.Random, false);
        public static readonly PrimitiveTest<Sexuality, Sex> SexualityAttracted = 
            Test<Sexuality, Sex>(nameof(SexualityAttracted), (se, s) => se.IsAttracted(s));
        public static readonly Function<Person, Person, int> Compatibility =
            Function<Person, Person, int>(nameof(Compatibility), (p1, p2) => p1.Compatibility(p2));

        // Should we have Actions? this doesn't neatly fit Function or PrimitiveTest patterns
        public static readonly PrimitiveTest<Person, Person> TakeLastName =
            Test<Person, Person>(nameof(TakeLastName), (person, other) => person.TakeLastName(other));

        public static readonly Function<Person, Person, OrderedPair<Person>> NewRelationship =
            Function<Person, Person, OrderedPair<Person>>(nameof(NewRelationship),
                (main, other) => new OrderedPair<Person>(main, other));

        public static readonly Function<Person, Person, ValueTuple<Person, Person>> PersonPair =
            Function<Person, Person, ValueTuple<Person, Person>>(nameof(PersonPair), (main, other) => (main, other));

        public static readonly Function<Vector2Int, Vector2Int, int> Distance = Method<Vector2Int, Vector2Int, int>(Town.Distance);
        public static readonly Function<string, Location> NewLocation = Method<string, Location>(Town.NewLocation, false);
        public static readonly Function<uint, Vector2Int> RandomLot = Method<uint, Vector2Int>(Town.RandomLot, false);

        public static readonly Function<Date> RandomDate = Function(nameof(RandomDate), Calendar.Random, false);
        public static readonly Function<TimePoint, Date> TimePointToDate = Function<TimePoint, Date>(nameof(TimePointToDate), t => t);
        public static readonly Function<Date, int, TimePoint> DateAgeToTimePoint = 
            Function<Date, int, TimePoint>(nameof(DateAgeToTimePoint), Calendar.TimePointFromDateAndAge, false);

        public static readonly Function<int> RandomNormal = Method(BellCurve, false);
        public static readonly Function<sbyte> RandomNormalSByte = Method(SByteBellCurve, false);
        public static readonly Function<float> RandomNormalFloat = Method(FloatBellCurve, false);

        public static readonly Function<int, int> RegressToZero = Function<int, int>(nameof(RegressToZero),
            num => num == 0 ? 0 : num > 0 ? num - 1 : num + 1);
        public static readonly Function<int, int> Incr = Function<int, int>(nameof(Incr), i => i + 1);
    }
}
