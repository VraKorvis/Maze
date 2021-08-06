using System;
using UnityEngine;

[Serializable]
public struct Sheet {
    [Tooltip("Filename for save csv file")]
    public string filename;
    [Tooltip("id google sheet (first list in sheet always 0)")]
    public long   id;
}