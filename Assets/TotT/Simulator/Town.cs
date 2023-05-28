using System;
using TotT.Utilities;
using TotT.ValueTypes;
using UnityEngine;

namespace TotT.Simulator {
    using static Randomize;

    public static class Town {
        private static Vector2Int _max = new(10, 10);
        private static Vector2Int _min = new(-10, -10);

        private static int GridSpaces() => (Math.Abs(_min.x) + _max.x) * (Math.Abs(_min.y) + _max.y);
        private static void ExpandAllSides() { _min.x--; _max.x++; _min.y--; _max.y++; }
        private static Vector2Int RandomLot() => new(Integer(_min.x, _max.x), Integer(_min.y, _max.y));
        public static Vector2Int RandomLot(uint lotCount) {
            if (lotCount * 2 >= GridSpaces()) ExpandAllSides();
            return RandomLot(); }

        private static int EuclideanSquare(int x1, int y1, int x2, int y2) => (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        public static int Distance(Vector2Int loc1, Vector2Int loc2) => EuclideanSquare(loc1.x, loc1.y, loc2.x, loc2.y);

        public static Location NewLocation(string name) => new(name);
    }
}