using System;
using TED;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;

namespace TotT.Simulator {
    using static Randomize;

    /// <summary>
    /// Location creation wrapper, distance calculations, and a random lot/position function
    /// </summary>
    // ReSharper disable InconsistentNaming
    #pragma warning disable IDE1006
    public static class Town {
        private static Vector2Int _max = new(8, 8);
        private static Vector2Int _min = new(-8, -8);

        private static Location newLocation(string name) => new(name);
        public static readonly Function<string, Location> NewLocation =
            new(nameof(NewLocation), newLocation, false);

        private static int GridSpaces() => (Math.Abs(_min.x) + _max.x) * (Math.Abs(_min.y) + _max.y);
        private static void ExpandAllSides() { _min.x--; _max.x++; _min.y--; _max.y++; }
        private static Vector2Int RandomLotWithinTown() => 
            new(Integer(_min.x, _max.x), Integer(_min.y, _max.y));
        private static Vector2Int RandomLotExpandWhenDense(int lotCount) {
            if (lotCount * 2 >= GridSpaces()) ExpandAllSides();
            return RandomLotWithinTown();
        }
        public static readonly Function<int, Vector2Int> RandomLot = 
            new(nameof(RandomLot), RandomLotExpandWhenDense, false);

        private static int EuclideanSquare(int x1, int y1, int x2, int y2) =>
            (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        private static int EuclideanDistance(Vector2Int loc1, Vector2Int loc2) => 
            EuclideanSquare(loc1.x, loc1.y, loc2.x, loc2.y);

        public static readonly Function<Vector2Int, Vector2Int, int> Distance = 
            new(nameof(Distance), EuclideanDistance);
    }
    #pragma warning restore IDE1006
}
