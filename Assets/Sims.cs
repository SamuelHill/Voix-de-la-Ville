using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using static Randomize;

public class Sims {
    private static readonly CultureInfo CultureInfo = Thread.CurrentThread.CurrentCulture;
    private static readonly TextInfo TextInfo = CultureInfo.TextInfo;
    private readonly int _maxAge;

    public Sims(int maxNewAdultAge = 79) => _maxAge = maxNewAdultAge;

    public int RandomAdultAge() => Integer(18, _maxAge);

    // Ignoring Other Sex for now...
    public static Sex RandomSex() => (Sex)BooleanInt();

    public static Person NewPerson(string first, string last) =>
        new(TextInfo.ToTitleCase(first), TextInfo.ToTitleCase(last));
}

public enum Sex { Male, Female, Other }

public enum Sexualities { Asexual, FemalePreference, MalePreference, Bisexual }

public struct Sexuality {
    public bool MaleAttraction;
    public bool FemaleAttraction;

    private const float HomosexualityIncidence = 0.1F;
    private const float BisexualityIncidence = 0.15F;
    private const float AsexualIncidence = 0.05F;

    public Sexuality(bool men, bool women) {
        MaleAttraction = men;
        FemaleAttraction = women; }

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


public class Person {
    public string FirstName;
    public string LastName;
    private string FullName => FirstName + " " + LastName;

    private readonly sbyte[] _personalityScores = new sbyte[Enum.GetValues(typeof(Facet)).Length];
    public sbyte GetFacet(Facet facet) => _personalityScores[(int)facet];

    private readonly sbyte[] _occupationalAptness = new sbyte[Enum.GetValues(typeof(Vocation)).Length];
    public sbyte GetVocation(Vocation vocations) => _occupationalAptness[(int)vocations];

    public Person(string firstName, string lastName) {
        FirstName = firstName;
        LastName = lastName;
        for (var i = 0; i < _personalityScores.Length; i++)
            _personalityScores[i] = SByteBellCurve();
        for (var i = 0; i < _occupationalAptness.Length; i++)
            _occupationalAptness[i] = SByteBellCurve(); }
    
    public override bool Equals(object obj) => obj is not null && ReferenceEquals(this, obj);
    public override int GetHashCode() => HashCode.Combine(_personalityScores, _occupationalAptness);
    public static bool operator ==([NotNull] Person p, string potentialName) => p.FullName == potentialName;
    public static bool operator !=([NotNull] Person p, string potentialName) => !(p == potentialName);

    public override string ToString() => FullName;
    public string DebugString() {
        var scores = _personalityScores.ToList();
        var aptness = _occupationalAptness.ToList();
        return FullName + " : Highest; " + (Facet)scores.IndexOf(_personalityScores.Max()) +
                          ", Lowest; " + (Facet)scores.IndexOf(_personalityScores.Min()) +
                          ", Highest; " + (Vocation)aptness.IndexOf(_occupationalAptness.Max()) +
                          ", Lowest; " + (Vocation)aptness.IndexOf(_occupationalAptness.Min()); }

    public static Person FromString(string personName) {
        var person = personName.Split(' ');
        return new Person(person[0], person[1]); }
}

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
