using System;
using System.Collections;
using System.IO;
using Maze.Data.Storages;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Networking;

public class SpiritSerializer : MonoBehaviour {
    public IEnumerator LoadMetaSpirit(int lvl) {
        string fullPath = Path.Combine(Application.streamingAssetsPath, StringConstant.PATH_LEVELS, lvl.ToString());
        string fullNameMeta = Path.Combine(fullPath, StringConstant.JSON_SPIRIT + StringConstant.JSON_META);
        string meta = String.Empty;
//            Debug.Log(fullNameMeta);
//            using (File.OpenRead(fullNameMeta)) {
//                meta = File.ReadAllText(fullNameMeta);
//            }

//         should fix CharacterSerializerEditor (couroutine)

        if (fullNameMeta.Contains("://") || fullNameMeta.Contains(":///")) {
            UnityWebRequest www = UnityWebRequest.Get(fullNameMeta);
            yield return www.SendWebRequest();

            meta = www.downloadHandler.text;
        }
        else {
            using (File.OpenRead(fullNameMeta)) {
                meta = File.ReadAllText(fullNameMeta);
            }
        }

//            Debug.Log(meta);
        Transform container = GameObject.FindGameObjectWithTag(StringConstant.SPIRIT_CONTAINER_TAG).transform;
        EnemySpiritsData ld = JsonUtility.FromJson<EnemySpiritsData>(meta);

        for (int i = 0; i < ld.spirits.Count; i++) {
//                Debug.Log(ld.spirits);
            StartCoroutine(LoadSpiritData(ld.spirits[i].guise, fullPath, container));
        }
    }

    private IEnumerator LoadSpiritData(Guise guise, string fullPath, Transform container) {
        string path = Path.Combine(fullPath, StringConstant.JSON_SPIRIT + guise);
        string json = String.Empty;

//            using (File.OpenRead(name)) {
//                json = File.ReadAllText(name);
//            }
//            Debug.Log(path);
        if (path.Contains("://") || path.Contains(":///")) {
            UnityWebRequest www = UnityWebRequest.Get(path);
            yield return www.SendWebRequest();
            json = www.downloadHandler.text;
        }
        else {
            using (File.OpenRead(path)) {
                json = File.ReadAllText(path);
            }
        }

//           Debug.Log("json: "+json);
        GameObject go = null;
        switch (guise) {
            case Guise.Circle:
                var circleSpirits = JsonHelper.FromJson<CircleSpiritData>(json);
//                Debug.Log(circleSpirits);
                go = Resources.Load(StringConstant.CIRCLE_SPIRIT) as GameObject;
                if (go == null) throw new Exception($"Spirit {guise} not found in the folder path {fullPath}");
                for (int i = 0; i < circleSpirits.Length; i++) {
                    var tmp = Instantiate(go, container, true);
                    tmp.GetComponent<MoveSettingsComponent>().speed = circleSpirits[i].speed;
                    tmp.GetComponent<CircleComponent>().radius = circleSpirits[i].radius;
                    tmp.GetComponent<CircleComponent>().Clockwise = circleSpirits[i].clockwise;
                    tmp.GetComponent<Transform>().position = circleSpirits[i].position;
                    tmp.AddComponent<ConvertToEntity>();
                }

                break;

            case Guise.FlatCurve:
                var flatCurveSpirits = JsonHelper.FromJson<FlatCurveSpiritData>(json);
                go = Resources.Load(StringConstant.FLAT_CURVE_SPIRIT) as GameObject;
                if (go == null) throw new Exception($"Spirit {guise} not found in the folder path {fullPath}");
                for (int i = 0; i < flatCurveSpirits.Length; i++) {
                    var tmp = Instantiate(go, container, true);
                    tmp.GetComponent<MoveSettingsComponent>().speed = flatCurveSpirits[i].speed;
                    tmp.GetComponent<FlatCurveRoseComponent>().radius = flatCurveSpirits[i].radius;
                    tmp.GetComponent<FlatCurveRoseComponent>().Clockwise = flatCurveSpirits[i].clockwise;
                    tmp.GetComponent<Transform>().position = flatCurveSpirits[i].position;
                    tmp.AddComponent<ConvertToEntity>();
                }

                break;
            case Guise.BackForth:
                var backForthSpirits = JsonHelper.FromJson<BackForthSpiritData>(json);
                go = Resources.Load(StringConstant.BACK_FORTH_SPIRIT) as GameObject;
                if (go == null) throw new Exception($"Spirit {guise} not found in the folder path {fullPath}");
                for (int i = 0; i < backForthSpirits.Length; i++) {
                    var tmp = Instantiate(go, container, true);
                    tmp.GetComponent<HeadingComponent>().direction = backForthSpirits[i].direction;
                    tmp.GetComponent<MoveSettingsComponent>().speed = backForthSpirits[i].speed;
                    var bf = tmp.GetComponent<BackForthComponent>();
                    bf.cellCountToFirst = backForthSpirits[i].cellCountToFirst;
                    bf.cellCountToSecond = backForthSpirits[i].cellCountToSecond;
                    tmp.GetComponent<Transform>().position = backForthSpirits[i].position;
                    tmp.AddComponent<ConvertToEntity>();
                }

                break;

            case Guise.HauntingMask:
                var hauntings = JsonHelper.FromJson<HauntingSpirit>(json);
                go = Resources.Load(StringConstant.HAUNTING_SPIRIT) as GameObject;
                if (go == null) throw new Exception($"Spirit {guise} not found in the folder path {fullPath}");
                for (int i = 0; i < hauntings.Length; i++) {
                    var tmp = Instantiate(go, container, true);
                    tmp.GetComponent<MoveSettingsComponent>().speed = hauntings[i].speed;
                    tmp.GetComponent<Transform>().position = hauntings[i].position;
                    tmp.AddComponent<ConvertToEntity>();
                }

                break;
            case Guise.Simple: break;
        }
    }
}