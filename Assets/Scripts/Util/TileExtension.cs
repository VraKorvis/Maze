using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public static class TileExtension {

    public static TileData GetTileDataFromTile(this Tile tile) {
        var tileData = new TileData();
        tileData.sprite       = tile.sprite;
        tileData.color        = tile.color;
        tileData.transform    = tile.transform;
        tileData.gameObject   = tile.gameObject;
        tileData.flags        = tile.flags;
        tileData.colliderType = tile.colliderType;
        return tileData;
    }

    /// <summary>
    /// Check existence tile
    /// </summary>
    /// <param name="tilemap where want check existence tile"></param>
    /// <param name="targetTile - verifiable tile"></param>
    /// <returns></returns>
    public static TileBase HasTile(this Tilemap tilemap, Vector2 targetTile) { return tilemap.GetTile(tilemap.WorldToCell(targetTile)); }

    public static T[] GetTiles<T>(this Tilemap tilemap) where T : TileBase {
        List<T> tiles = new List<T>();

        for (int y = tilemap.origin.y; y < (tilemap.origin.y + tilemap.size.y); y++) {
            for (int x = tilemap.origin.x; x < (tilemap.origin.x + tilemap.size.x); x++) {
                T tile = tilemap.GetTile<T>(new Vector3Int(x, y, 0));
                if (tile != null) {
                    tiles.Add(tile);
                }
            }
        }

        return tiles.ToArray();
    }

    public static Vector3 GetWorldPos(this Grid grid, Tilemap tilemap, Vector3Int pos) {
        var tmp = grid.GetCellCenterWorld(pos);
        return tmp;
    }
}