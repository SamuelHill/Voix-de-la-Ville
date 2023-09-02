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
        private static readonly uint[] Primes = {
            1u,  2u,  3u,  5u,  7u,  11u, 13u, 17u, 19u, 23u,
            29u, 31u, 37u, 41u, 43u, 53u, 59u, 61u, 67u, 71u,
            73u, 79u, 83u, 89u, 97u };
        private static readonly int[] HighPrimes = new int[100];

        static Randomize() {
            var index = 0;
            for (var i = 1; i < HighPrimes.Length; i++) {
                if (index < Primes.Length - 1 && i > Primes[index + 1]) index++;
                HighPrimes[i] = index;
            }
        }

        public static void Seed(int seed) => TEDRandom.SetGlobalSeed(seed);
        public static Random MakeRng() => TEDRandom.MakeRng();

        public static readonly Random RngForInitialization = MakeRng();
        
        public static int Integer(Random rng, int high) => rng.Next(high + 1);
        public static int Integer(Random rng, int low, int high) => rng.Next(low, high + 1);

        public static sbyte SByte(Random rng, sbyte high) => (sbyte)Integer(rng, high);
        public static sbyte SByte(Random rng, sbyte low, sbyte high) => (sbyte)Integer(rng, low, high);
        public static byte Byte(Random rng, byte high) => (byte)Integer(rng, high);
        public static byte Byte(Random rng, byte low, byte high) => (byte)Integer(rng, low, high);
        public static int BooleanInt(Random rng) => Integer(rng, 0, 1);
        public static bool Boolean(Random rng) => BooleanInt(rng) == 1;

        public static float Float(Random rng, float min, float max) => (float)rng.NextDouble() * (max - min) + min;
        public static float Float(Random rng, float max) => Float(rng, 0.0F, max);
        public static float Probability(Random rng) => Float(rng, 1.0F);
        public static double Double(Random rng, double max) => rng.NextDouble() * max;

        private static int NormalDistribution(Random rng, int low, int high, int numAggregate = 10) => // "Normal"
            Range(0, numAggregate + 1).Aggregate((value, _) => 
                value + Integer(rng, (high + Abs(low)) / numAggregate)) + low;

        private static float NormalDistribution(Random rng, float low, float high, int numAggregate = 10) =>
            Range(0, numAggregate + 1).Select(i => (float)i).Aggregate((value, _) =>
                value + Float(rng, (high + Abs(low)) / numAggregate)) + low;

        public static int BellCurve(Random rng) => NormalDistribution(rng, -50, 50);
        public static sbyte SByteBellCurve(Random rng) => (sbyte)BellCurve(rng);
        public static float FloatBellCurve(Random rng) => NormalDistribution(rng, -50f, 50f);
        
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
        public static T RandomFromDict<T>(Random rng, Dictionary<T, float> dict) where T : notnull {
            var floatMax = dict.Values.Sum();
            var randomFloat = Float(rng, dict.Values.Sum());
            foreach (var kvp in dict) {
                floatMax -= kvp.Value;
                if (randomFloat > floatMax) return kvp.Key;
            }
            // should only return default when some the kvp Values are negative.
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

        public static IEnumerable<T> BadShuffle<T>(this IList<T> list, Random rng) {
            var length = (uint)list.Count;
            if (length == 0) yield break;
            var position = rng.Next() % length;
            var maxPrimeIndex = Primes.Length - 1;
            if (length < HighPrimes.Length) maxPrimeIndex = HighPrimes[length];
            var step = Primes[rng.Next() % (maxPrimeIndex + 1)];
            for (uint i = 0; i < length; i++) {
                yield return list[(int)position];
                position = (position + step) % length;
            }
        }
    }
}
