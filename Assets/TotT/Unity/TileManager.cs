using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TotT.Unity {
    using static Enumerable;
    using static Camera;
    using static Input;

    /// <summary>
    /// Handles display of locations in a TileMap (position and color) as well as the ability to click on a tile.
    /// </summary>
    public class TileManager {
        // for the tile to be able to change color:
        //      https://github.com/Unity-Technologies/2d-extras/issues/96
        private readonly Tile _occupiedLot;
        private readonly Vector2Int _townCenter; // offsets the town's location in terms of tiles on screen
        public readonly Tilemap Tilemap;
        private readonly TilemapRenderer _tilemapRenderer;
        private Vector3Int? _selectedTile;

        public TileManager(Tilemap tilemap, Vector2Int townCenter, Tile occupiedLot) {
            Tilemap = tilemap;
            _tilemapRenderer = Tilemap.GetComponent<TilemapRenderer>();
            _townCenter = townCenter;
            _occupiedLot = occupiedLot;
        }

        private Vector3Int LotToTile(Vector2Int lot) => (Vector3Int)(_townCenter + lot);
        private Vector2Int TileToLot(Vector3Int tile) => (Vector2Int)tile - _townCenter;

        // **************************************** Tiles *****************************************

        private bool SetTiles(Vector3Int[] tiles, TileBase[] tilesToSet) {
            if (tiles.Length == 0) return false;
            Tilemap.SetTiles(tiles, tilesToSet);
            return true;
        }
        private bool SetAllTiles(Vector3Int[] tiles, TileBase tileToSet) =>
            SetTiles(tiles, Repeat(tileToSet, tiles.Length).ToArray());

        private bool DeleteTiles(Vector3Int[] tiles) => SetAllTiles(tiles, null);
        public bool DeleteLots(IEnumerable<Vector2Int> lots) => DeleteTiles(lots.Select(LotToTile).ToArray());
        private bool OccupyTiles(Vector3Int[] tiles) => SetAllTiles(tiles, _occupiedLot);
        private bool OccupyLots(IEnumerable<Vector2Int> lots) => OccupyTiles(lots.Select(LotToTile).ToArray());

        // **************************************** Colors ****************************************

        private void SetColors(IEnumerable<(Vector3Int, Color)> tileColors) {
            foreach (var tileColor in tileColors) Tilemap.SetColor(tileColor.Item1, tileColor.Item2);
        }
        private bool ColorTiles(IReadOnlyCollection<(Vector3Int, Color)> tiles) {
            if (tiles.Count == 0) return false;
            SetColors(tiles);
            return true;
        }
        private bool ColorLots(IEnumerable<(Vector2Int, Color)> lots) =>
            ColorTiles(lots.Select(lot => (LotToTile(lot.Item1), lot.Item2)).ToArray());

        public bool OccupyAndColorLots((Vector2Int, Color)[] lots) =>
            OccupyLots(lots.Select(t => t.Item1)) && ColorLots(lots);

        // ************************************** Selection ***************************************

        public Vector2Int? SelectedLot => _selectedTile is null ? null : TileToLot((Vector3Int)_selectedTile);

        private Vector3Int MouseToTile() => Tilemap.WorldToCell(main.ScreenToWorldPoint(mousePosition));
        private bool TrySelectTile(out Vector3Int tile) {
            tile = MouseToTile();
            return Tilemap.HasTile(tile);
        }
        public void UpdateSelectedTile() => _selectedTile = TrySelectTile(out var tile) ? tile : null;

        public void SetVisibility(bool show) {
            if (_tilemapRenderer.enabled == show) return;
            _tilemapRenderer.enabled = show;
        }
    }
}
