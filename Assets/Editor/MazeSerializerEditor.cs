using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MazeSerializer))]
public class MazeSerializerEditor : Editor {
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var script = (MazeSerializer)target;

        if (GUILayout.Button("Json to maze")) {
            MazeSerializer.JsonToMaze(script.currentLvl);
            //if (Application.isPlaying) {
            //    script.JsonToMaze();
            //}
        }

        if (GUILayout.Button("Maze to json")) {
            MazeSerializer.MazeToJson(script.currentLvl);
            //if (Application.isPlaying) {
            //    script.MazeToJson();
            //}
        }

        if (GUILayout.Button("ClearTiles Attention!!!")) {
//            script.ClearGrid();
            //if (Application.isPlaying) {
            //    script.MazeToJson();
            //}
        }
    }

}
