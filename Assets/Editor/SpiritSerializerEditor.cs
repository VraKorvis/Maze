using System.Collections.Generic;
using System.IO;
using Maze.Data.Storages;
using UnityEditor;
using UnityEngine;

public class SpiritSerializerEditor : ScriptableObject {

    public int level;
    
    public static void SpiritsToJson(int lvl) {
            string fullPath = Path.Combine(Application.streamingAssetsPath, StringConstant.PATH_LEVELS, lvl.ToString());
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }

            EnemySpiritsData ld = new EnemySpiritsData {
                lvl     = lvl,
                spirits = new List<SpiritData>(),
            };

            Dictionary<Guise, List<GameObject>> spiritsOnScene = FindSpiritsOnScene();
            foreach (KeyValuePair<Guise, List<GameObject>> pair in spiritsOnScene) {
                var guise   = pair.Key;
                var spirits = pair.Value;
                ld.spirits.Add(new SpiritData() {
                    guise = guise,
                    count = spirits.Count
                });
                SaveToJson(guise, spirits, fullPath);
            }

            string meta_spirit  = JsonUtility.ToJson(ld);
            string fullNameMeta = Path.Combine(fullPath, StringConstant.JSON_SPIRIT + StringConstant.JSON_META);

            if (!File.Exists(fullNameMeta)) {
                File.Create(fullNameMeta).Dispose();
            }

            File.WriteAllText(fullNameMeta, meta_spirit);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
    
        private static Dictionary<Guise, List<GameObject>> FindSpiritsOnScene() {
            Dictionary<Guise, List<GameObject>> spirits     = new Dictionary<Guise, List<GameObject>>();
            GameObject[]                        gameObjects = GameObject.FindGameObjectsWithTag(StringConstant.SPIRIT_TAG);
            foreach (var go in gameObjects) {
                var type = go.GetComponent<SpiritComponent>();
                if (spirits.TryGetValue(type.guise, out var list)) {
                    list.Add(go);
                } else {
                    spirits.Add(type.guise, new List<GameObject> {go});
                }
            }

            return spirits;
        }

        private static void SaveToJson(Guise guise, List<GameObject> list, string fullPath) {
            string spirit   = string.Empty;
            string fullName = Path.Combine(fullPath, StringConstant.JSON_SPIRIT + guise);
            if (!File.Exists(fullName)) {
                File.Create(fullName).Dispose();
            }

            switch (guise) {
                case Guise.Circle:
                    var circleSpirits = new CircleSpiritData[list.Count];
                    for (int i = 0; i < circleSpirits.Length; i++) {
                        circleSpirits[i].speed     = list[i].GetComponent<MoveSettingsComponent>().speed;
                        circleSpirits[i].radius    = list[i].GetComponent<CircleComponent>().radius;
                        circleSpirits[i].clockwise = list[i].GetComponent<CircleComponent>().Clockwise;
                        circleSpirits[i].position  = list[i].GetComponent<Transform>().position;
                    }

                    spirit = JsonHelper.ToJson(circleSpirits);
                    break;
                case Guise.FlatCurve:
                    var flatCurveSpirits = new FlatCurveSpiritData[list.Count];
                    for (int i = 0; i < flatCurveSpirits.Length; i++) {
                        flatCurveSpirits[i].speed     = list[i].GetComponent<MoveSettingsComponent>().speed;
                        flatCurveSpirits[i].radius    = list[i].GetComponent<FlatCurveRoseComponent>().radius;
                        flatCurveSpirits[i].clockwise = list[i].GetComponent<FlatCurveRoseComponent>().Clockwise;
                        flatCurveSpirits[i].position  = list[i].GetComponent<Transform>().position;
                    }

                    spirit = JsonHelper.ToJson(flatCurveSpirits);
                    break;
                case Guise.BackForth:
                    var backForthSpirits = new BackForthSpiritData[list.Count];
                    for (int i = 0; i < backForthSpirits.Length; i++) {
                        backForthSpirits[i].direction = list[i].GetComponent<HeadingComponent>().direction;
                        backForthSpirits[i].speed     = list[i].GetComponent<MoveSettingsComponent>().speed;
                        var bf = list[i].GetComponent<BackForthComponent>();
                        backForthSpirits[i].cellCountToFirst  = bf.cellCountToFirst;
                        backForthSpirits[i].cellCountToSecond = bf.cellCountToSecond;
                        backForthSpirits[i].position          = list[i].GetComponent<Transform>().position;
                    }

                    spirit = JsonHelper.ToJson(backForthSpirits);
                    break;
                case Guise.HauntingMask:
                    var boss = new HauntingSpirit[list.Count];
                    for (int i = 0; i < boss.Length; i++) {
                        boss[i].speed    = list[i].GetComponent<MoveSettingsComponent>().speed;
                        boss[i].position = list[i].GetComponent<Transform>().position;
                    }

                    spirit = JsonHelper.ToJson(boss);
                    break;
                case Guise.Simple: break;
            }
            File.WriteAllText(fullName, spirit);
        }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Maze/SerializerSpirits")]
    public static void CreateSerializerMaze() {
        string path = EditorUtility.SaveFilePanelInProject("Save SerializerSpirits",
            "SerializerSpirits",
            "asset",
            "Save SerializerSpirits",
            "Assets/Data");
        if (path == "") {
            return;
        }
        AssetDatabase.CreateAsset(CreateInstance<SpiritSerializerEditor>(), path);
    }
#endif
    
}
