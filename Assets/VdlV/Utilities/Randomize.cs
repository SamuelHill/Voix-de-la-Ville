using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TED;
using TEDRandom = TED.Utilities.Random;

namespace VdlV.Utilities {
    using static Enumerable;
    using static Math;

    /// <summary>
    /// Maps over System.Random.Next for basic types and provides pseudo-normal bell curve
    /// random scoring and various random element/shuffle functions.
    /// </summary>
    // ReSharper disable MemberCanBePrivate.Global
    public static class Randomize {
        public static readonly int[] Primes = new[]
        {
            1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 53, 59, 61, 67, 71, 73, 79,
            83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173,
            179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281,
            283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409,
            419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541
        };

        public static int RandomPrime(Random r) => RandomElement(Primes, r);

        private const int PseudoNormalNumAggregations = 10;

        public const sbyte BellCurveMax = 50;
        public const sbyte BellCurveMin = -50;
        public const sbyte BellCurveRange = BellCurveMax - BellCurveMin;

        public static readonly Random RngForInitialization = MakeRng();

        public static void Seed(int seed) => TEDRandom.SetGlobalSeed(seed);
        public static Random MakeRng() => TEDRandom.MakeRng();
        
        public static int Integer(int high, Random rng) => rng.Next(high + 1);
        public static int Integer(int low, int high, Random rng) => rng.Next(low, high + 1);

        public static sbyte SByte(sbyte high, Random rng) => (sbyte)Integer(high, rng);
        public static sbyte SByte(sbyte low, sbyte high, Random rng) => (sbyte)Integer(low, high, rng);
        public static byte Byte(byte high, Random rng) => (byte)Integer(high, rng);
        public static byte Byte(byte low, byte high, Random rng) => (byte)Integer(low, high, rng);
        public static int BooleanInt(Random rng) => Integer(0, 1, rng);
        public static bool Boolean(Random rng) => BooleanInt(rng) == 1;

        public static float Float(float min, float max, Random rng) => (float)rng.NextDouble() * (max - min) + min;
        public static float Float(float max, Random rng) => Float(0.0F, max, rng);
        public static float Probability(Random rng) => Float(1.0F, rng);
        public static double Double(double max, Random rng) => rng.NextDouble() * max;

        private static int NormalDistribution(int low, int high, Random rng) =>
            Range(0, PseudoNormalNumAggregations + 1).Aggregate((value, _) => 
            value + Integer((high + Abs(low)) / PseudoNormalNumAggregations, rng)) + low;

        private static float NormalDistribution(float low, float high, Random rng) =>
            Range(0, PseudoNormalNumAggregations + 1).Select(i => (float)i).Aggregate((value, _) =>
                value + Float((high + Abs(low)) / PseudoNormalNumAggregations, rng)) + low;

        public static int BellCurve(Random rng) => NormalDistribution(BellCurveMin, BellCurveMax, rng);
        public static sbyte SByteBellCurve(Random rng) => (sbyte)BellCurve(rng);
        public static float FloatBellCurve(Random rng) => NormalDistribution((float)BellCurveMin, BellCurveMax, rng);
        
        public static Function<int> RandomNormal {
            get {
                var rng = MakeRng(); 
                return new Function<int>(nameof(RandomNormal), 
                    () => BellCurve(rng), false);
            }
        }
        public static Function<sbyte> RandomNormalSByte {
            get {
                var rng = MakeRng(); 
                return new Function<sbyte>(nameof(RandomNormalSByte), 
                    () => SByteBellCurve(rng), false);
            }
        }
        public static Function<float> RandomNormalFloat {
            get {
                var rng = MakeRng();
                return new Function<float>(nameof(RandomNormalFloat), 
                    () => FloatBellCurve(rng), false);
            }
        }

        // like Probability() but allows for more or less than 1.0f total options... (non-scaled probabilities)
        // with 1.0f float sum, this function will act much like a switch statement called on Probability()
        // wherein the key T will be returned if the "probability" is in the associated value float range
        public static T RandomFromDict<T>(this Dictionary<T, float> dict, Random rng) where T : notnull {
            var floatMax = dict.Values.Sum();
            var randomFloat = Float(dict.Values.Sum(), rng);
            foreach (var kvp in dict) {
                floatMax -= kvp.Value;
                if (randomFloat > floatMax) return kvp.Key;
            }
            // should only return default when some of the kvp Values are negative..
            return default!;
        }

        public static T RandomElement<T>(this IList<T> list, Random rng) {
            var size = list.Count;
            Debug.Assert(size > 0,
                         "cannot choose random element from empty list");
            return list[rng.Next(size)];
        }

        public static T[] Shuffle<T>(this IEnumerable<T> sequence, Random rng) {
            var result = sequence.ToArray();
            for (var i = result.Length - 1; i > 0; i--) {
                var index = rng.Next(i + 1);
                (result[index], result[i]) = (result[i], result[index]);
            }
            return result;
        }
    }
}
