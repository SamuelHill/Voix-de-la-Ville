using System.Collections.Generic;
using UnityEngine;

namespace TotT.Utilities {
    public class GridComparer : EqualityComparer<Vector2Int> {
        public override bool Equals(Vector2Int x, Vector2Int y) => x == y;
        public override int GetHashCode(Vector2Int v) => (v.x * 40 + v.y) * 2;
    }
}