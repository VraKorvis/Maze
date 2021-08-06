using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[ExecuteAlways]
public class SheetSyncLoader : MonoBehaviour {

    public string SheetID;
    public Sheet[] Sheets;

    [SerializeField]
    private Object folderPath;

    public void Sync() {
        StopAllCoroutines();
        StartCoroutine(SyncCoroutine());
    }

    private IEnumerator SyncCoroutine() {
        
#if UNITY_EDITOR
        var folder = AssetDatabase.GetAssetPath(folderPath);
        Debug.Log("<color=grey>Sync started, please wait for confirmation message...</color>");
        var wwwSheets = new List<WWW>();
        
        foreach (var sheet in Sheets) {
            var url = string.Format(StringConstant.URL_PATTERN, SheetID, sheet.id);
            Debug.LogFormat("Downloading: {0}...", url);
            wwwSheets.Add(new WWW(url));
        }

        foreach (var wwwSheet in wwwSheets) {
            if (!wwwSheet.isDone) {
                yield return wwwSheet;
            }
            if (wwwSheet.error == null) {
                var sheet = Sheets.Single(e => wwwSheet.url == string.Format(StringConstant.URL_PATTERN, SheetID, e.id));
                var path  = Path.Combine(folder, sheet.filename + ".csv");

                File.WriteAllBytes(path, wwwSheet.bytes);
                AssetDatabase.Refresh();
                Debug.LogFormat("Sheet id {0} downloaded to {1}", sheet.id, path);
            } else {
                throw new Exception(wwwSheet.error);
            }
        }
        Debug.Log("<color=green>Localization successfully synced!</color>");
#endif
        yield return null;

    }

}