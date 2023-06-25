using System;
using System.Numerics;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Randomize;

    public record Personality {
        private readonly Vector<sbyte> _personality;

        public Personality() {
            var personality = new sbyte[Enum.GetValues(typeof(Facet)).Length];
            for (var i = 0; i < personality.Length; i++) personality[i] = SByteBellCurve();
            _personality = new Vector<sbyte>(personality);
        }

        public sbyte Facet(Facet facet) => _personality[(int)facet];

        public int Compatibility(Personality otherPersonality) => 
            Vector.Dot(Vector.Subtract(_personality, otherPersonality._personality), Vector<sbyte>.One);
    }
}
