using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class WallTile : Tile {
    [SerializeField] private Sprite[] wallSprite;
    [SerializeField] private Sprite preview;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        base.GetTileData(position, tilemap, ref tileData);

        StringBuilder autotileID = new StringBuilder();
        StringBuilder autoID = new StringBuilder();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if ((x==-1 && y==-1) || (x == -1 && y == 1) || (x == 1 && y == -1) || (x == 1 && y == 1)) continue;
                if (x != 0 || y != 0) {
                    Vector3Int nPos = new Vector3Int(position.x + x, position.y + y, position.z);
                    if (HasWall(tilemap, nPos)) {
                        autotileID.Append("W");
                        autoID.Append("1");

                    } else {
                        autotileID.Append("E");
                        autoID.Append("0");
                    }
                    
                }
            }
        }

        tileData.sprite = wallSprite[0];
        DrawTile(autotileID.ToString(), ref tileData);
    }

    private void DrawTile(string autotileID, ref TileData tileData) {
        switch (autotileID) {
            case AutotileID.Empty:
                tileData.sprite = wallSprite[(int) WallType.Empty];
                break;
            case AutotileID.Bot:
                tileData.sprite = wallSprite[(int) WallType.Bot];
                break;
            case AutotileID.Left:
                tileData.sprite = wallSprite[(int) WallType.Left];
                break;
            case AutotileID.Right:
                tileData.sprite = wallSprite[(int) WallType.Right];
                break;
            case AutotileID.Top:
                tileData.sprite = wallSprite[(int) WallType.Top];
                break;
            case AutotileID.BotRight:
                tileData.sprite = wallSprite[(int) WallType.BotRight];
                break;
            case AutotileID.BotLeft:
                tileData.sprite = wallSprite[(int) WallType.BotLeft];
                break;
            case AutotileID.BotTop:
                tileData.sprite = wallSprite[(int) WallType.BotTop];
                break;
            case AutotileID.RightLeft:
                tileData.sprite = wallSprite[(int) WallType.RightLeft];
                break;
            case AutotileID.RightTop:
                tileData.sprite = wallSprite[(int) WallType.RightTop];
                break;
            case AutotileID.LeftTop:
                tileData.sprite = wallSprite[(int) WallType.LeftTop];
                break;
            case AutotileID.BotRightLeft:
                tileData.sprite = wallSprite[(int)WallType.BotRightLeft];
                break;
            case AutotileID.BotRightTop:
                tileData.sprite = wallSprite[(int)WallType.BotRightTop];
                break;
            case AutotileID.BotLeftTop:
                tileData.sprite = wallSprite[(int)WallType.BotLeftTop];
                break;
            case AutotileID.RightLeftTop:
                tileData.sprite = wallSprite[(int)WallType.RightLeftTop];
                break;
            case AutotileID.All:
                tileData.sprite = wallSprite[(int)WallType.All];
                break;
        }
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap) {
        base.RefreshTile(position, tilemap);
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int nPos = new Vector3Int(position.x + x, position.y + y, position.z);

                if (HasWall(tilemap, nPos)) {
                    tilemap.RefreshTile(nPos);
                }
            }
        }
    }

    private bool HasWall(ITilemap tilemap, Vector3Int position) {
        return tilemap.GetTile(position) == this;
    }


#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/WallTile")]
    public static void CreateWallTile() {
        string path =
            EditorUtility.SaveFilePanelInProject("Save Walltile", "New Walltile", "asset", "Save walltile", "Assets");
        if (path == "") {
            return;
        }

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WallTile>(), path);
    }

#endif
}