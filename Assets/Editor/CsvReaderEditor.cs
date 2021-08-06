using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CsvReader))]
public class CsvReaderEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        var reader = (CsvReader) target;
        if (GUILayout.Button("CSV to JSON and save")) {
            reader.ToJson();
        }
    }
}