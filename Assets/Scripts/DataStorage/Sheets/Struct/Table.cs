using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public struct Table {
    [Tooltip("Choose filename for save")]
    public string filename;
    [Tooltip("Choose folder for save")]
    public Object folderPath;
    [Tooltip("Choose CSV file for parse to JSON")]
    public Object  csvFile;
}