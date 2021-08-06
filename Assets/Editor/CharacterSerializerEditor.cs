using Maze.Data.Storages;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpiritSerializerEditor))]
public class CharacterSerializerEditor : Editor {
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var script = (SpiritSerializerEditor)target;

       if (GUILayout.Button("Json to spirits")) {
          // SpiritSerializerEditor.LoadSpirit(script.level);
        }

        if (GUILayout.Button("Spirits to json")) {
            SpiritSerializerEditor.SpiritsToJson(script.level);
        }

    }
}