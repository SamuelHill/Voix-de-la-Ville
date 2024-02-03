using System;
using System.Linq;
using TED;
using TED.Primitives;
using VdlV.Utilities;
using VdlV.ValueTypes;

namespace VdlV.Simulator {
    using static Favorability;
    using static Randomize;
    using static Sexuality;

    /// <summary>
    /// Functions for creating new people (Person constructor, random adult age and sex)
    /// as well as the math behind fertility rates. Also holds miscellaneous TED wrappers
    /// for several Person and Sexuality based C# functions.
    /// </summary>
    public static class Sims {
        public static Function<string, string, Person> NewPerson {
            get {
                var rng = MakeRng();
                return new Function<string, string, Person>(nameof(NewPerson), 
                    (first, last) => new Person(first, last, rng), false);
            }
        }

        public static readonly Function<Person, string> Surname = 
            new(nameof(Surname), p => p.LastName);

        public static readonly Function<int, int> Incr = new(nameof(Incr), i => i + 1);
        
        public static Function<int> RandomAdultAge {
            get {
                var rng = MakeRng();
                return new Function<int>(nameof(RandomAdultAge),
                    () => Integer(18, 72, rng), false);
            }
        }
        
        public static Function<Sex> RandomSex {
            get {
                var rng = MakeRng();
                return new Function<Sex>(nameof(RandomSex), 
                    () => (Sex)BooleanInt(rng), false);
            }
        }

        public static Function<Sex, Sexuality> RandomSexuality {
            get {
                var rng = MakeRng();
                return new Function<Sex, Sexuality>(nameof(RandomSexuality),
                    sex => Random(sex, rng), false);
            }
        }

        #region Fertility Math
        internal static void FertilityCurve(float t) {
            if (t is < 0 or > 1) return; // for t between 0 and 1,
            const int x0 = 60;
            const int x1 = 30;
            const int x2 = 60;
            const int x3 = 0;
            var xAxes = (x0, x1, x2, x3);
            const int y0 = 0;
            const int y1 = 0;
            const int y2 = 1;
            const float y3 = 0.95f;
            var yAxes = (y0, y1, y2, y3);

            // https://www.desmos.com/calculator/cahqdxeshd
            static float BezierCurveSingleAxis(float t, (float _0, float _1, float _2, float _3) axis) =>
                (1 - t) * ((1 - t) * ((1 - t) * axis._0 + t * axis._1) + t * ((1 - t) * axis._1 + t * axis._2)) +
                t * ((1 - t) * ((1 - t) * axis._1 + t * axis._2) + t * ((1 - t) * axis._2 + t * axis._3));

            // https://www.symbolab.com/solver/simplify-calculator/simplify
            static float SimplifiedSingleAxis(float t, (float _0, float _1, float _2, float _3) axis) => axis._0 +
                3 * MathF.Pow(t, 2) * axis._2 - MathF.Pow(t, 3) * axis._0 - 3 * MathF.Pow(t, 3) * axis._2 +
                3 * t * axis._1 + 3 * MathF.Pow(t, 3) * axis._1 + MathF.Pow(t, 3) * axis._3 +
                3 * MathF.Pow(t, 2) * axis._0 - 6 * MathF.Pow(t, 2) * axis._1 - 3 * t * axis._0;

            static float SimpleX(float t) => -150 * MathF.Pow(t, 3) + 180 * MathF.Pow(t, 2) - 90 * t + 60;

            // For X we want to solve for t as the X axis correlates to age; given some age get t and
            // then use that t value to get the y coordinate (fertility). This is not possible with the
            // .NET math libs and even a hookup to wolfram for solving would be extremely compute intensive.
            static float SimpleXSolveForT(float t) => -30 * (t - 1) * (5 * MathF.Pow(t, 2) - t + 2);
            // Instead, as the values of x that I want to solve for t are just the integer ages (really only
            // 0-60 as I cutoff the bezier function at 60) I can precompute these solutions to t and store
            // them in an array where age is the index.

            // This function acts as mathematical notation for my process to get the fertility function -
            // these statements are a C#-esk way of reminding myself all of these equations are equivalent
            var bezierCoordinateForT = (x: BezierCurveSingleAxis(t, xAxes), y: BezierCurveSingleAxis(t, yAxes));
            var simplifiedCoordinateForT = (x: SimplifiedSingleAxis(t, xAxes), y: SimplifiedSingleAxis(t, yAxes));
            var simpleCoordinateForT = (x: SimpleX(t), y: SimpleY(t));
            simpleCoordinateForT.x = SimpleXSolveForT(t);
            _ = bezierCoordinateForT == simplifiedCoordinateForT;
            _ = simplifiedCoordinateForT == simpleCoordinateForT;
        }

        // The only part of the FertilityCurve math that needs to be pulled out as a standalone function:
        private static float SimpleY(float t) => -2.05f * MathF.Pow(t, 3) + 3 * MathF.Pow(t, 2);

        // See wolfram alpha's equation solver, using approximations (not exact forms):
        // https://www.wolframalpha.com/input?i=equation+solver with `-30(t-1)(5t^2-t+2) = 18` as the equation to solve
        private static readonly float[] SolvedTForAge = {
            1, 0.99440f, 0.98870f, 0.98290f, 0.97699f, 0.97098f, 0.96486f, 0.95860f, 0.95222f, 0.94571f,
            0.93906f, 0.93227f, 0.92531f, 0.91820f, 0.91090f, 0.90343f, 0.89575f, 0.88787f, 0.87976f, 0.87141f,
            0.86281f, 0.85392f, 0.84474f, 0.83522f, 0.82535f, 0.81509f, 0.80441f, 0.79324f, 0.78155f, 0.76927f,
            0.75632f, 0.74262f, 0.72804f, 0.71246f, 0.69568f, 0.67749f, 0.65758f, 0.63554f, 0.61081f, 0.58260f,
            0.54978f, 0.51087f, 0.46444f, 0.41110f, 0.35625f, 0.30676f, 0.26496f, 0.22990f, 0.20000f, 0.17399f,
            0.15095f, 0.13025f, 0.11141f, 0.09409f, 0.07806f, 0.06310f, 0.04906f, 0.03582f, 0.02328f, 0.01136f,
            0
        };

        private static readonly float[] FertilityForAges = (from t in SolvedTForAge select SimpleY(t)).ToArray();

        private static float FertilityForAge(int age) => age >= FertilityForAges.Length ? 0 : FertilityForAges[age];
        #endregion

        public static readonly Function<int, float> FertilityRate = new(nameof(FertilityRate), FertilityForAge);
        
        public static Function<Favorability> Favorable {
            get {
                var rng = MakeRng();
                return new Function<Favorability>(nameof(Favorable), 
                    () => BellCurve(rng) switch {
                        > 18 => MostPositive,
                        > 8 => Positive,
                        >= -8 => Neutral,
                        >= -18 => Negative,
                        _ => MostNegative
                    }, false);
            }
        }
        
        public static readonly PrimitiveTest<Sexuality, Sex> AttractedTo = new(nameof(AttractedTo), 
            (sexuality, sex) => sexuality.IsAttracted(sex));
        
        public static readonly PrimitiveTest<Person, Person> TakeLastName = new(nameof(TakeLastName), 
            (person1, person2) => person1.TakeLastName(person2));
        
        public static readonly Function<Person, Person, int> Similarity = new(nameof(Similarity), 
            (person1, person2) => person1.Similarity(person2));
        
        public static readonly Function<Person, Person, int> Difference = new(nameof(Difference), 
            (person1, person2) => person1.Difference(person2));
        
        public static readonly Function<Person, Facet, sbyte> PersonalityFacet = new(nameof(PersonalityFacet), 
            (person, facet) => person.Facet(facet));
    }
}
