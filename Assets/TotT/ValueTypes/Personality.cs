using System;
using System.Numerics;
using TotT.Utilities;

namespace TotT.ValueTypes {
    using static Randomize;

    public record Personality {
        private readonly sbyte[] _personality;
        private readonly Vector<sbyte> _personalityVector;

        public Personality() {
            _personality = new sbyte[Enum.GetValues(typeof(Facet)).Length];
            for (var i = 0; i < _personality.Length; i++) _personality[i] = SByteBellCurve();
            _personalityVector = new Vector<sbyte>(_personality);
        }

        public sbyte Facet(Facet facet) => _personality[(int)facet];

        public int Compatibility(Personality otherPersonality) => 
            Vector.Dot(Vector.Subtract(_personalityVector, 
                                       otherPersonality._personalityVector), 
                       Vector<sbyte>.One);

        public int Similarity(Personality otherPersonality) {
            var diff = 0;
            for (var i = 0; i < _personality.Length; i++) 
                diff += 100 - Math.Abs(_personality[i] - otherPersonality._personality[i]);
            return diff;
        }
    }
}
