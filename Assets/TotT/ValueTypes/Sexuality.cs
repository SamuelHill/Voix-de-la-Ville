using System;
using System.Collections.Generic;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Randomize;

    public readonly struct Sexuality {
        // Occurrence Rate out of a probability of 100% (1.00f), these percentages guide the
        // likely-hood of the associated sexuality being returned in the Random function..
        private const float AsexualOccurrenceRate = 0.05F;
        private const float BisexualOccurrenceRate = 0.15F;
        private const float HomosexualOccurrenceRate = 0.1F;
        private const float NonHeteroOccurrenceRate = 
            HomosexualOccurrenceRate + BisexualOccurrenceRate + AsexualOccurrenceRate;

        private bool FemaleAttraction { get; }
        private bool MaleAttraction { get; }
        private bool IsBi => MaleAttraction && FemaleAttraction;
        // Do not currently need an IsAce function as there is no need to check if someone is
        // asexual in isolation yet. Only add if reasoning about asexual individuals directly
        // as opposed to using Sexuality only to drive attraction...

        private SexualityName Name => IsBi ? SexualityName.Bisexual :
            FemaleAttraction ? SexualityName.FemalePreference :
            MaleAttraction ? SexualityName.MalePreference :
            SexualityName.Asexual;

        private Sexuality(bool femaleAttraction, bool maleAttraction) {
            FemaleAttraction = femaleAttraction;
            MaleAttraction = maleAttraction; }

        public bool IsAttracted(Sex potentialPartner) =>
            (potentialPartner == Sex.Male && MaleAttraction) ||
            (potentialPartner == Sex.Female && FemaleAttraction);

        private static Sexuality Asexual() => new(false, false);
        private static Sexuality Bisexual() => new(true, true);
        private static Sexuality FemalePref() => new(true, false);
        private static Sexuality MalePref() => new(false, true);
        
        // indexed by SexualityName Enum
        private static readonly List<Func<Sexuality>> SexualityFromName = new()
            { Asexual, FemalePref, MalePref, Bisexual };

        private static Sexuality Heterosexual(Sex sex) => sex == Sex.Female ? MalePref() : FemalePref();
        private static Sexuality Homosexual(Sex sex) => sex == Sex.Male ? MalePref() : FemalePref();

        public static Sexuality Random(Sex sex) => Probability() switch {
            >= NonHeteroOccurrenceRate => Heterosexual(sex),
            >= BisexualOccurrenceRate + AsexualOccurrenceRate => Homosexual(sex),
            >= AsexualOccurrenceRate => Bisexual(), _ => Asexual() };

        public override string ToString() => Name.ToString();
        public static Sexuality FromString(string sexualityString) {
            Enum.TryParse<SexualityName>(sexualityString, out var sexualities);
            return SexualityFromName[(int)sexualities](); }
    }
}