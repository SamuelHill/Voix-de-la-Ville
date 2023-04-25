using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static Randomize;

public enum VitalStatus { Alive, Dead }
public enum Facet {
    AbstractInclined, ActivityLevel, Altruism, Ambition, AngerPropensity,
    AnxietyPropensity, ArtInclined, Assertiveness, Bashful, Bravery,
    CheerPropensity, Closeminded, Confidence, Cruelty, Curious,
    DepressionPropensity, Discord, DisdainAdvice, Dutifulness,
    EmotionallyObsessive, EnvyPropensity, ExcitementSeeking, Friendliness,
    Gratitude, Greed, Gregarioiusness, HatePropensity, Hopeful, Humor,
    Imagination, Immoderation, Immodesty, LovePropensity, LustPropensity,
    Orderliness, Perfectionist, Perseverance, Politeness, Pride, Privacy,
    Singleminded, StressVulnerability, SwayedByEmotions, Thoughtlessness,
    Tolerant, Trust, Vanity, Vengeful, Violent, Wastefulness }

public static class Sims {
    public static Person NewPerson(string first, string last) => new(Utils.Title(first), Utils.Title(last));
    public static int RandomAdultAge() => Integer(18, 79);
    public static Sex RandomSex() => (Sex)BooleanInt();

    #region Fertility Calculation Notes
    public static void FertilityCurve(float t) {
        // https://www.desmos.com/calculator/cahqdxeshd
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
        // using the above points in the four point bezier curve gives:
        var fertilityFunc = FourPointBezierCurve(t, xAxes, yAxes);
        // However, I want to solve for t given some x value and then get the y value from the given t
        // To do this I will want to simplify both equations:
        var simpleY = ApplyPointsToSimplified(yAxes);
        var sameY = simpleY == SimpleY;
        var simpleX = ApplyPointsToSimplified(xAxes);
        var sameX = simpleX == SimpleX && simpleX == SimpleXSolveForT;
        // and in the case of x, solve for t. This is not possible with just the .NET math libs
        // and even with a hookup to wolfram the solve operation would be rather compute intensive.
        // What I can do though, as the values of x that I want to solve for t are just the integer
        // ages (even then really only 0-60 as I cutoff the bezier function at 60) is precompute these
        // solutions to t and store them in an array where age is the index.
    }
    public static Vector2 FourPointBezierCurve(float t,
        (float _0, float _1, float _2, float _3) x,
        (float _0, float _1, float _2, float _3) y) => 
        new (BezierCurveSingleAxis(t, x), BezierCurveSingleAxis(t, y));
    public static float BezierCurveSingleAxis(float t, (float _0, float _1, float _2, float _3) axis) => 
        (1 - t)*((1 - t)*((1 - t)*axis._0 + t*axis._1) + t*((1 - t)*axis._1 + t*axis._2)) + 
                t*((1 - t)*((1 - t)*axis._1 + t*axis._2) + t*((1 - t)*axis._2 + t*axis._3));
    public static float SimplifiedSingleAxis(float t, (float _0, float _1, float _2, float _3) axis) =>
        axis._0 + 3*MathF.Pow(t, 2)*axis._2 - MathF.Pow(t, 3)*axis._0 - 3*MathF.Pow(t, 3)*axis._2 +
        3*t* axis._1 + 3*MathF.Pow(t, 3)*axis._1 + MathF.Pow(t, 3)*axis._3 + 3*MathF.Pow(t, 2)*axis._0 -
        6*MathF.Pow(t, 2)*axis._1 - 3*t*axis._0;
    public static Func<float, float> ApplyPointsToSimplified(
        (float _0, float _1, float _2, float _3) axis) => 
        t => SimplifiedSingleAxis(t, axis);

    // https://www.symbolab.com/solver/simplify-calculator/simplify
    public static float SimpleX(float t) => -150*MathF.Pow(t, 3) + 180*MathF.Pow(t, 2) - 90*t + 60;
    public static float SimpleXSolveForT(float t) => -30*(t - 1)*(5*MathF.Pow(t, 2) - t + 2);
    #endregion
    #region Fertility Math and precompute arrays
    // ReSharper disable once InconsistentNaming
    // See wolfram alpha's equation solver, using approximations (not exact forms):
    // https://www.wolframalpha.com/input?i=equation+solver&assumption=%7B%22F%22%2C+%22SolveEquation%22%2C+%22solveequation%22%7D+-%3E%22-30%28t-1%29%285t%5E2-t%2B2%29+%3D+18%22
    private static readonly float[] tForAge = { 1,
        0.99440f, 0.98870f, 0.98290f, 0.97699f, 0.97098f,
        0.96486f, 0.95860f, 0.95222f, 0.94571f, 0.93906f,
        0.93227f, 0.92531f, 0.91820f, 0.91090f, 0.90343f, // zero out above this? age < 15
        0.89575f, 0.88787f, 0.87976f, 0.87141f, 0.86281f,
        0.85392f, 0.84474f, 0.83522f, 0.82535f, 0.81509f,
        0.80441f, 0.79324f, 0.78155f, 0.76927f, 0.75632f,
        0.74262f, 0.72804f, 0.71246f, 0.69568f, 0.67749f,
        0.65758f, 0.63554f, 0.61081f, 0.58260f, 0.54978f,
        0.51087f, 0.46444f, 0.41110f, 0.35625f, 0.30676f,
        0.26496f, 0.22990f, 0.20000f, 0.17399f, 0.15095f,
        0.13025f, 0.11141f, 0.09409f, 0.07806f, 0.06310f,
        0.04906f, 0.03582f, 0.02328f, 0.01136f, 0 };
    public static float SimpleY(float t) => -2.05f * MathF.Pow(t, 3) + 3 * MathF.Pow(t, 2);
    private static readonly float[] FertilityForAge = 
        (from t in tForAge select Time.PerYear(SimpleY(t))).ToArray();
    #endregion
    public static float FertilityRate(int age) => age >= FertilityForAge.Length ? 0 : FertilityForAge[age];
}

public class Person {
    private readonly Guid _id;
    public string FirstName;
    public string LastName;

    public string FullName => FirstName + " " + LastName;

    public Person(string firstName, string lastName) {
        _id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName; }

    public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
    public override int GetHashCode() => _id.GetHashCode();
    public static bool operator ==(Person p, string potentialName) => p != null && p.FullName == potentialName;
    public static bool operator !=(Person p, string potentialName) => !(p == potentialName);

    public override string ToString() => FullName;

    public static Person FromString(string personName) {
        var person = personName.Split(' ');
        return new Person(person[0], person[1]); }
}

public enum Sex { Male, Female, Other }

public enum Sexualities { Asexual, FemalePreference, MalePreference, Bisexual }

public readonly struct Sexuality {
    public bool MaleAttraction { get; }
    public bool FemaleAttraction { get; }

    private const float HomosexualityIncidence = 0.1F;
    private const float BisexualityIncidence = 0.15F;
    private const float AsexualIncidence = 0.05F;

    public Sexuality(bool maleAttraction, bool femaleAttraction) {
        MaleAttraction = maleAttraction;
        FemaleAttraction = femaleAttraction; }
    public static Sexuality Asexual() => new(false, false);
    public static Sexuality FemalePreference() => new(false, true);
    public static Sexuality MalePreference() => new(true, false);
    public static Sexuality Bisexual() => new(true, true);

    public static Dictionary<Sexualities, Func<Sexuality>> GetSexuality = new() {
        {Sexualities.Asexual, Asexual},
        {Sexualities.FemalePreference, FemalePreference},
        {Sexualities.MalePreference, MalePreference},
        {Sexualities.Bisexual, Bisexual} };
    public Sexualities GetSexualities() => MaleAttraction && FemaleAttraction ? Sexualities.Bisexual :
        MaleAttraction ? Sexualities.MalePreference : FemaleAttraction ? Sexualities.FemalePreference : Sexualities.Asexual;

    public static Sexuality Homosexual(Sex sex) => sex == Sex.Male ? MalePreference() : FemalePreference();
    public static Sexuality Heterosexual(Sex sex) => sex == Sex.Female ? MalePreference() : FemalePreference();
    public static Sexuality Random(Sex sex) => Probability() switch {
        >= HomosexualityIncidence + BisexualityIncidence + AsexualIncidence => Heterosexual(sex),
        >= BisexualityIncidence + AsexualIncidence => Homosexual(sex),
        >= AsexualIncidence => Bisexual(),
        _ => Asexual() };

    public bool IsAttracted(Sex potentialPartner) =>
        (potentialPartner == Sex.Male && MaleAttraction) ||
        (potentialPartner == Sex.Female && FemaleAttraction);

    public override string ToString() => GetSexualities().ToString();
    public static Sexuality FromString(string sexualityString) {
        Enum.TryParse<Sexualities>(sexualityString, out var sexualities);
        return GetSexuality[sexualities](); }
}
