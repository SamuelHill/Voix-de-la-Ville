﻿namespace VdlV.ValueTypes {
    /// <summary>
    /// Names of the various sexualities. Not used as a value type in simulation, instead this
    /// enum is used for easy ToString and FromString (from CSVs) for the Sexuality type.
    /// </summary>
    internal enum SexualityName {
        Asexual,
        FemalePreference,
        MalePreference,
        Bisexual
    }
}
