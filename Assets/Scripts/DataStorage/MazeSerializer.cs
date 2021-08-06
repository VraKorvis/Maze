using System;
using System.Collections.Generic;
using System.IO;
using Maze.Data.Storages;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeSerializer : ScriptableObject {

    public int currentLvl;

    //TODO 
    //to optimise
    //to reduce count empty cell, just to record not empty
    public static void MazeToJson(int lvl) {
        MazeData gd   = new MazeData();
        var      grid = GameObject.Find("Grid");
        if (grid == null) {
            throw new Exception("Grid not found");
        }

        var layers = grid.GetComponentsInChildren<Tilemap>();
        layers[0].CompressBounds();
        layers[0].ResizeBounds();
        for (int i = 0; i < layers.Length - 1; i++) {
            List<TileD> layerSer = new List<TileD>();
            Tilemap     tm       = layers[i];
            BoundsInt   bounds   = tm.cellBounds;
            for (int n = tm.origin.x; n < bounds.xMax; n++) {
                for (int p = tm.origin.y; p < bounds.yMax; p++) {
                    var localPlace = new Vector3Int(n, p, (int) tm.transform.position.y);

                    if (tm.HasTile(localPlace)) {
                        TileBase tb = tm.GetTile<TileBase>(localPlace);
                        TileD    td = new TileD(localPlace, tb.name);
                        layerSer.Add(td);
                    }
                }
            }

            LayerTilemap l = new LayerTilemap();
            l.tiles = layerSer.ToArray();
            gd.layers.Add(l);
            gd.layers[i].order = GetLayer(tm);
        }

        string jsonData = JsonUtility.ToJson(gd);

        string fullPath = Path.Combine(Application.streamingAssetsPath, StringConstant.PATH_LEVELS, lvl.ToString());
        string fullName = Path.Combine(fullPath, StringConstant.JSON_MAZE                          + lvl);

        if (!Directory.Exists(fullPath)) {
            Directory.CreateDirectory(fullPath);
        }

        if (!File.Exists(fullName)) {
            File.Create(fullName).Dispose();
        }

        File.WriteAllText(fullName, jsonData);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    private static LayerOrderType GetLayer(Tilemap tm) {
        LayerOrderType type;
        try {
            Enum.TryParse(tm.name, out type);
        } catch (ArgumentException e) {
            var values = Enum.GetValues(typeof(LayerOrderType));
            Console.WriteLine(
                "Layers of grid do not match the name of LayerType. Please make sure what layer name must match one of the LayerType:   " + values);
            Console.WriteLine(e);
            throw;
        }

        return type;
    }

    public static void JsonToMaze(int lvl) {
        //TextAsset levelAsset = Resources.Load<TextAsset>(StringConstant.LEVELS + lvl);
        
        string fullPath = Path.Combine(Application.streamingAssetsPath, StringConstant.PATH_LEVELS, lvl.ToString());
        string fullName = Path.Combine(fullPath, StringConstant.JSON_MAZE + lvl);
        string json = String.Empty;
        using (File.OpenRead(fullName)) {
            json = File.ReadAllText(fullName);
        }

        MazeData gd = JsonUtility.FromJson<MazeData>(json);

        var grid = GameObject.Find("Grid");
        if (grid == null) {
            throw new Exception("Grid not found");
        }

        var layers = grid.GetComponentsInChildren<Tilemap>();
        for (int i = 0; i < gd.layers.Count - 1; i++) {
            Tilemap tilemap = layers[i]?.GetComponent<Tilemap>();

            if (tilemap != null) {
                tilemap.ClearAllTiles();

                TileD[] tiles = gd.layers[i].tiles;

                tilemap.gameObject.GetComponent<TilemapRenderer>().sortingOrder = (int) gd.layers[i].order;
                TileBase tile = null;
                foreach (var t in tiles) {
                    if (tile == null) {
                        string name = String.Empty;
                        string end  = "(Clone)";
                        if (t.nameTile.EndsWith(end)) {
                            name = t.nameTile.Substring(0, t.nameTile.Length - end.Length);
                        }

                        var rt = Resources.Load(StringConstant.RULE_TILE_PATH + name) as TileBase;
                        tile = Instantiate<TileBase>(rt);
                    }

                    tilemap.SetTile(t.coord, tile);
                }

                tilemap.RefreshAllTiles();
            }
        }
    }

    public static void ClearGrid() {
        Tilemap[] layer = FindObjectsOfType<Tilemap>();
        for (int i = 0; i < layer.Length; i++) {
            layer[i].ClearAllTiles();
        }
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Maze/SerializerMazeSO")]
    public static void CreateSerializerMaze() {
        string path = EditorUtility.SaveFilePanelInProject("Save SerializerMaze", "SerializerMaze", "asset", "Save SerializerMaze", "Assets/Data");
        if (path == "") {
            return;
        }

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MazeSerializer>(), path);
    }
#endif

}