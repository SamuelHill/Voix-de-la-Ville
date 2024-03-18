using System;
using TED;
using VdlV.Utilities;
using VdlV.ValueTypes;
using UnityEngine;
using Random = System.Random;

namespace VdlV.Simulator {
    using static Randomize;

    /// <summary>
    /// Location creation wrapper, distance calculations, and a random lot/position function
    /// </summary>
    public static class Town {
        public static readonly Function<string, Location> NewLocation =
            new(nameof(NewLocation), name => new Location(name), false);

        private static Vector2Int _max = new(6, 6);
        private static Vector2Int _min = new(-6, -6);

        //interlock.increment(ref Vector.x)

        private static int GridSpaces() => (Math.Abs(_min.x) + _max.x) * (Math.Abs(_min.y) + _max.y);
        private static void ExpandAllSides() { _min.x--; _max.x++; _min.y--; _max.y++; }
        private static Vector2Int RandomLotWithinTown(Random rng) => 
            new(Integer(_min.x, _max.x, rng), Integer(_min.y, _max.y, rng));
        private static Vector2Int RandomLotExpandWhenDense(int lotCount, Random rng) {
            if (lotCount * 2 >= GridSpaces()) ExpandAllSides();
            return RandomLotWithinTown(rng);
        }

        public static Function<int, Vector2Int> RandomLot {
            get {
                var rng = MakeRng();
                return new Function<int, Vector2Int>(nameof(RandomLot), 
                    lotCount => RandomLotExpandWhenDense(lotCount, rng), false);
            }
        }

        private static int EuclideanSquare(int x1, int y1, int x2, int y2) =>
            (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        private static int EuclideanDistance(Vector2Int loc1, Vector2Int loc2) => 
            EuclideanSquare(loc1.x, loc1.y, loc2.x, loc2.y);

        public static readonly Function<Vector2Int, Vector2Int, int> Distance = 
            new(nameof(Distance), EuclideanDistance);
    }
}
