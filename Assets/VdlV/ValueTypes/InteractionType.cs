namespace VdlV.ValueTypes {
    /// <summary>
    /// Types of interactions between two people.
    /// </summary>
    public enum InteractionType {
        Empathizing,   // Friend most positive
        Assisting,     // Most positive
        Complimenting, // Positive
        Chatting,      // Neutral (Enemy most positive)
        Insulting,     // Negative (Enemy positive, Romantic most negative)
        Arguing,       // Most negative (Enemy neutral)
        Fighting,      // Enemy negative
        Dueling,       // Enemy most negative
        Negging,       // Negative romantic
        Flirting,      // Positive romantic
        Courting,      // Most positive romantic
        Snogging,      // Dating
        Procreating,   // Snogging advanced
        Murdering,
        Discussing,
    }
}
