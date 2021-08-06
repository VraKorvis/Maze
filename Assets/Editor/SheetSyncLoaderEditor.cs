using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SheetSyncLoader))]
public class SheetSyncLoaderEditor : Editor{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        var loader = (SheetSyncLoader) target;
        if (GUILayout.Button("Load")) {
            loader.Sync();
        }
    }
}