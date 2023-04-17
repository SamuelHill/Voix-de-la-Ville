using System;
using System.Collections.Generic;
using System.Linq;

public static class Randomize {
    public const int Seed = 349571286;
    // ReSharper disable InconsistentNaming
    private static readonly Random RNG = new(Seed);
    // ReSharper restore InconsistentNaming
    private static readonly uint[] Primes = {
        1u, 2u, 3u, 5u, 7u, 11u, 13u, 17u, 19u, 23u, 29u, 31u, 37u,
        41u, 43u, 53u, 59u, 61u, 67u, 71u, 73u, 79u, 83u, 89u, 97u };
    private static readonly int[] HighPrimes = new int[100];

    static Randomize() {
        var index = 0;
        for (var i = 1; i < HighPrimes.Length; i++) {
            if (index < Primes.Length - 1 && i > Primes[index + 1]) index++;
            HighPrimes[i] = index; }
    }

    public static int Integer(int high) => RNG.Next(high + 1);
    public static int Integer(int low, int high) => RNG.Next(low, high + 1);
    public static int BooleanInt() => Integer(0, 1);
    public static bool Boolean() => BooleanInt() == 1;

    public static int BellCurve(int low, int high) =>
        Enumerable.Range(0, 11).Aggregate((value, _) =>
            value + Integer(0, (high + Math.Abs(low)) / 10)) + low;
    public static int NormalScore() => BellCurve(-50, 50);

    public static sbyte SByte(sbyte high) => (sbyte)RNG.Next(high + 1);
    public static sbyte SByte(sbyte low, sbyte high) => (sbyte)RNG.Next(low, high + 1);
    public static sbyte SByteBellCurve() => (sbyte)BellCurve(-50, 50);
    public static byte Byte(byte high) => (byte)RNG.Next(high + 1);
    public static byte Byte(byte low, byte high) => (byte)RNG.Next(low, high + 1);

    public static float Float(float min, float max) => (float)RNG.NextDouble() * (max - min) + min;
    public static float Float(float max) => Float(0.0F, max);
    public static float Probability() => Float(1.0F);

    // instead of Randomize.Probability()... allows for more or less than 100% sum of actions...
    public static T RandomFromDict<T>(Dictionary<T, float> dict) where T : notnull {
        var floatMax = dict.Values.Sum();
        var randomFloat = Float(dict.Values.Sum());
        foreach (var kvp in dict) {
            floatMax -= kvp.Value;
            if (randomFloat > floatMax) return kvp.Key; }
        return default!; // should not get here unless some of the kvp Values are negative.
    }

    public static T RandomElement<T>(this IList<T> list) {
        var size = list.Count;
        System.Diagnostics.Debug.Assert(size > 0,
            "cannot choose random element from empty list");
        return list[RNG.Next(size)];
    }

    public static T[] Shuffle<T>(this IList<T> sequence) {
        var result = sequence.ToArray();
        for (var i = result.Length - 1; i > 0; i--) {
            var index = RNG.Next(i + 1);
            (result[index], result[i]) = (result[i], result[index]); }
        return result;
    }

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