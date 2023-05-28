using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TotT.Utilities {
    using TEDRandom = TED.Utilities.Random;

    public static class Randomize {
        // ReSharper disable once InconsistentNaming
        private static Random RNG;

        private static readonly uint[] Primes = { 
            1u, 2u, 3u, 5u, 7u, 11u, 13u, 17u, 19u, 23u, 29u, 31u, 37u, 
            41u, 43u, 53u, 59u, 61u, 67u, 71u, 73u, 79u, 83u, 89u, 97u };
        private static readonly int[] HighPrimes = new int[100];

        static Randomize() {
            var index = 0;
            for (var i = 1; i < HighPrimes.Length; i++) {
                if (index < Primes.Length - 1 && i > Primes[index + 1]) index++;
                HighPrimes[i] = index; }
            RNG = new Random(); }

        public static void Seed(int seed, int tedSeed) { Seed(seed); TEDSeed(tedSeed); }
        private static void Seed(int seed) => RNG = new Random(seed);
        // ReSharper disable once InconsistentNaming
        private static void TEDSeed(int seed) => TEDRandom.Rng = new Random(seed);

        // ReSharper disable once MemberCanBePrivate.Global
        public static int Integer(int high) => RNG.Next(high + 1);
        public static int Integer(int low, int high) => RNG.Next(low, high + 1);
        public static sbyte SByte(sbyte high) => (sbyte)Integer(high);
        public static sbyte SByte(sbyte low, sbyte high) => (sbyte)Integer(low, high);
        public static byte Byte(byte high) => (byte)Integer(high);
        public static byte Byte(byte low, byte high) => (byte)Integer(low, high);
        public static int BooleanInt() => Integer(0, 1);
        public static bool Boolean() => BooleanInt() == 1;

        // ReSharper disable once MemberCanBePrivate.Global
        public static float Float(float min, float max) => (float)RNG.NextDouble() * (max - min) + min;
        // ReSharper disable once MemberCanBePrivate.Global
        public static float Float(float max) => Float(0.0F, max);
        public static float Probability() => Float(1.0F);

        public static int BellCurve() => NormalDistribution(-50, 50);
        public static sbyte SByteBellCurve() => (sbyte)BellCurve();
        public static float FloatBellCurve() => NormalDistribution(-50f, 50f);

        private static int NormalDistribution(int low, int high, int numAggregate = 10) => // "Normal"
            Enumerable.Range(0, numAggregate + 1).Aggregate((value, _) =>
                value + Integer((high + Math.Abs(low)) / numAggregate)) + low;
        private static float NormalDistribution(float low, float high, int numAggregate = 10) =>
            Enumerable.Range(0, numAggregate + 1).Select(i => (float)i).Aggregate((value, _) =>
                value + Float((high + Math.Abs(low)) / numAggregate)) + low;

        // like Probability() but allows for more or less than 1.0f total options... (non-scaled probabilities)
        // with 1.0f float sum, this function will act much like a switch statement called on Probability()
        // wherein the key T will be returned if the "probability" is in the associated value float range
        public static T RandomFromDict<T>(Dictionary<T, float> dict) where T : notnull {
            var floatMax = dict.Values.Sum();
            var randomFloat = Float(dict.Values.Sum());
            foreach (var kvp in dict) {
                floatMax -= kvp.Value;
                if (randomFloat > floatMax) return kvp.Key; }
            // should only return default when some the kvp Values are negative.
            return default!; }

        public static T RandomElement<T>(this IList<T> list) {
            var size = list.Count;
            Debug.Assert(size > 0,
                "cannot choose random element from empty list");
            return list[RNG.Next(size)]; }

        public static T[] Shuffle<T>(this IEnumerable<T> sequence) {
            var result = sequence.ToArray();
            for (var i = result.Length - 1; i > 0; i--) {
                var index = RNG.Next(i + 1);
                (result[index], result[i]) = (result[i], result[index]); }
            return result; }

        public static IEnumerable<T> BadShuffle<T>(this IList<T> list) {
            var length = (uint)list.Count;
            if (length == 0) yield break;
            var position = RNG.Next() % length;
            var maxPrimeIndex = Primes.Length - 1;
            if (length < HighPrimes.Length) maxPrimeIndex = HighPrimes[length];
            var step = Primes[RNG.Next() % (maxPrimeIndex + 1)];
            for (uint i = 0; i < length; i++) {
                yield return list[(int)position];
                position = (position + step) % length; }
        }
    }
}