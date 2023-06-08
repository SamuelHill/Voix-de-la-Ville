using System.Collections.Generic;
using UnityEngine;

namespace TotT.Utilities {
    /// <summary>
    /// Allows for Vector2Int comparisons (with a non-shit hash function like Unity's default)
    /// </summary>
    public class GridComparer : EqualityComparer<Vector2Int> {
        public override bool Equals(Vector2Int x, Vector2Int y) => x == y;
        public override int GetHashCode(Vector2Int v) => (v.x * 80 + v.y) * 2;
    }
}
