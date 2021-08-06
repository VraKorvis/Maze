
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(AlignCharacterOnGrid), true)]
public class AlignCharacterOnGridEditor : Editor {
	private static GUIStyle ToggleButtonStyleNormal  = null;
	private static GUIStyle ToggleButtonStyleToggled = null;

	private bool toggleTxt = false;

	public override void OnInspectorGUI() {
		//base.OnInspectorGUI();
		DrawDefaultInspector();
		var script = (AlignCharacterOnGrid) serializedObject.targetObject;

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Check correctness location spirit on scene")) {
			Align(script);
			ClearHighlightMaskTilemap(script);
			CheckOutSpiritsPath(script);
		}

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Clear highlight")) {
			ClearHighlightMaskTilemap(script);
		}

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (ToggleButtonStyleNormal == null) {
			ToggleButtonStyleNormal                    = "Button";
			ToggleButtonStyleToggled                   = new GUIStyle(ToggleButtonStyleNormal);
			ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
		}

		toggleTxt = GUILayout.Toggle(toggleTxt, "TODO edit button", toggleTxt ? ToggleButtonStyleNormal : ToggleButtonStyleToggled);
		GUILayout.EndHorizontal();
	}

	private void CheckOutSpiritsPath(AlignCharacterOnGrid script) {
		foreach (var go in script.gameObjects) {
			var spirit = go.GetComponent<ICheckCorrectnessPathMovement>();
			if (spirit != null)
				if (!spirit.CheckCorrectnessPathMovement()) {
#if UNITY_EDITOR
					Debug.LogError("Enemy Path not correct. Pls check existence wall on enemy path");
#endif
				}
		}
	}

	private void ClearHighlightMaskTilemap(AlignCharacterOnGrid script) {
		var tmp = script.mask;
		foreach (var pos in tmp.cellBounds.allPositionsWithin) {
			Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
			if (tmp.HasTile(localPlace)) {
				tmp.SetTileFlags(localPlace, TileFlags.LockAll);
				tmp.SetTile(localPlace, null);
			}
		}
	}

	private void Align(AlignCharacterOnGrid script) {
		script.AlignObject();
	}
}