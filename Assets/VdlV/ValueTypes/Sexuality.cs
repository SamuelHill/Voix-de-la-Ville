using System;
using System.Collections.Generic;
using VdlV.Utilities;

namespace VdlV.ValueTypes {
    using static Randomize;
    using static Sex;
    using static SexualityName;

    /// <summary>
    /// Sexuality of a Person. Preference is binary and applied to each sex independently.
    /// With two Sexes and binary attraction we get 4 possible sexualities - these 4 possibilities are
    /// named in the SexualityName enum. Bi (both true) and ace (both false) do not care about the Sex
    /// of themselves, but to be able to differentiate hetero and homosexuality Sex needs to be considered.
    /// As such, when assigning Sexuality - which is only done randomly - Sex is passed in. However, when
    /// outputting a Sexuality the name will only be one of the 4 SexualityNames (not reflecting the
    /// relationship to Sex).
    /// </summary>
    public readonly struct Sexuality : IComparable<Sexuality>, IEquatable<Sexuality> {
        /// <summary>5% chance of Random(Sex) assigning Asexual (false, false)</summary>
        private const float AsexualOccurrenceRate = 0.05F;
        /// <summary>15% chance of Random(Sex) assigning Bisexual (true, true)</summary>
        private const float BisexualOccurrenceRate = 0.15F;
        /// <summary>10% chance of Random(Sex) assigning Attraction to same as Sex</summary>
        private const float HomosexualOccurrenceRate = 0.1F;
        /// <summary>70% chance of Random(Sex) assigning Attraction to opposite of Sex</summary>
        private const float NonHeteroOccurrenceRate =
            HomosexualOccurrenceRate + BisexualOccurrenceRate + AsexualOccurrenceRate;

        /// <summary>Attraction to Sex.Female</summary>
        private bool FemaleAttraction { get; }

        /// <summary>Attraction to Sex.Male</summary>
        private bool MaleAttraction { get; }

        /// <summary>SexualityName equivalent to internal attractions</summary>
        private SexualityName Name => MaleAttraction && FemaleAttraction ? Bisexual :
                                      FemaleAttraction ? FemalePreference :
                                      MaleAttraction ? MalePreference : Asexual;

        /// <param name="femaleAttraction">Attraction to Sex.Female</param>
        /// <param name="maleAttraction">Attraction to Sex.Male</param>
        private Sexuality(bool femaleAttraction, bool maleAttraction) {
            FemaleAttraction = femaleAttraction;
            MaleAttraction = maleAttraction;
        }

        /// <summary>Is this Sexuality attracted to the Sex of potentialPartner?</summary>
        /// <returns>whether or not person is attracted to a potential partner</returns>
        public bool IsAttracted(Sex potentialPartner) => (potentialPartner == Male && MaleAttraction) ||
                                                         (potentialPartner == Female && FemaleAttraction);

        /// <returns>Sexuality(false, false)</returns>
        private static Sexuality Ace() => new(false, false);

        /// <returns>Sexuality(true, false)</returns>
        private static Sexuality FemalePref() => new(true, false);

        /// <returns>Sexuality(false, true)</returns>
        private static Sexuality MalePref() => new(false, true);

        /// <returns>Sexuality(true, true)</returns>
        private static Sexuality Bi() => new(true, true);

        /// <summary>List (indexed by SexualityName Enum) of the 4 possible constructors</summary>
        private static readonly List<Func<Sexuality>> SexualityFromName =
            new() { Ace, FemalePref, MalePref, Bi };

        /// <param name="sex">Sex of the individual being assigned this Sexuality</param>
        /// <returns>Sexuality with attraction to opposite of Sex</returns>
        private static Sexuality Heterosexual(Sex sex) => sex == Female ? MalePref() : FemalePref();

        /// <param name="sex">Sex of the individual being assigned this Sexuality</param>
        /// <returns>Sexuality with attraction to same as Sex</returns>
        private static Sexuality Homosexual(Sex sex) => sex == Male ? MalePref() : FemalePref();

        /// <summary>
        /// Randomly assign a Sexuality given a Probability (1.0f), the OccurrenceRates of possible Sexualities,
        /// and the Sex of the individual being assigned this Sexuality.
        /// </summary>
        /// <param name="sex">Sex of the individual being assigned this Sexuality</param>
        /// <returns>new Sexuality</returns>
        public static Sexuality Random(Sex sex) {
            return Probability() switch {
                >= NonHeteroOccurrenceRate => Heterosexual(sex),
                >= BisexualOccurrenceRate + AsexualOccurrenceRate => Homosexual(sex),
                >= AsexualOccurrenceRate => Bi(),
                _ => Ace()
            };
        }

        /// <returns>SexualityName associated with this Sexuality as a string</returns>
        public override string ToString() => Name.ToString();
        /// <summary>
        /// For use by CsvReader. Takes a string, try's parsing as a SexualityName, returns the associated Sexuality.
        /// </summary>
        public static Sexuality FromString(string sexualityString) {
            Enum.TryParse<SexualityName>(sexualityString, out var sexualities);
            return SexualityFromName[(int)sexualities]();
        }

        // Compare and Equality interfacing:
        public int CompareTo(Sexuality other) {
            var femaleAttractionComparison = FemaleAttraction.CompareTo(other.FemaleAttraction);
            return femaleAttractionComparison != 0 ? femaleAttractionComparison : 
                       MaleAttraction.CompareTo(other.MaleAttraction);
        }
        public bool Equals(Sexuality other) =>
            other.FemaleAttraction == FemaleAttraction && other.MaleAttraction == MaleAttraction;
    }
}
