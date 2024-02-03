using System;
using System.Linq;
using System.Numerics;
using VdlV.Utilities;

namespace VdlV.ValueTypes {
    using static Enum;
    using static Vector;
    using static Randomize;

    public record Personality {
        private static readonly int FacetCount = GetValues(typeof(Facet)).Length;
        // Vector<sbyte> has a Count of 16 on most modern CPUs (128bits, 256 is possible but I can't say which
        // Vector type to use - .Net 7 lets you say Vector64, Vector128, Vector256)...
        private static readonly int VectorCount = Vector<sbyte>.Count;
        private static readonly int NumVectors = FacetCount / VectorCount;

        [SerializeOnSave] private readonly sbyte[] _personality;
        // To store the scores for all 50 personality facets we need 3 Vector<sbyte>s for 48 of those elements,
        // The last two can't go into a normal Vector<sbyte> so either a Vector2 is needed (with 2 floats -
        // I'm assuming this is a Vector64 internally?) or two facets have to be ignored...
        [SerializeOnSave] private readonly Vector<sbyte>[] _personalityVectors;

        private Personality() {}

        public Personality(Random rng) {
            _personality = new sbyte[FacetCount];
            for (var i = 0; i < FacetCount; i++) _personality[i] = SByteBellCurve(rng);
            _personalityVectors = new Vector<sbyte>[NumVectors];
            for (var i = 0; i < NumVectors; i++)
                _personalityVectors[i] = new Vector<sbyte>(_personality, i * VectorCount);
        }

        public sbyte Facet(Facet facet) => _personality[(int)facet];

        #region Vector<sbyte> processing
        private static readonly Vector<sbyte> One = Vector<sbyte>.One;
        private static readonly Vector<sbyte> Range = Multiply(BellCurveRange, One);

        private static int Sum(Vector<sbyte> a) => Dot(a, One);
        private static Vector<sbyte> AbsoluteDifference(Vector<sbyte> a, Vector<sbyte> b) => Abs(Subtract(a, b));
        private static Vector<sbyte> RangeInversion(Vector<sbyte> a) => Subtract(Range, a);

        private static int Difference(Vector<sbyte> a, Vector<sbyte> b) => Sum(AbsoluteDifference(a, b));
        private static int Similarity(Vector<sbyte> a, Vector<sbyte> b) => Sum(RangeInversion(AbsoluteDifference(a, b)));
        #endregion

        public int Difference(Personality other) => 
            _personalityVectors.Select((vector, i) => Difference(vector, other._personalityVectors[i])).Sum();
        public int Similarity(Personality other) =>
            _personalityVectors.Select((vector, i) => Similarity(vector, other._personalityVectors[i])).Sum();
    }
}
