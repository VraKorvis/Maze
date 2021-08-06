using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public static class FindChildExtension {
    public static Vector3[] GetChildsPositionsWithTag(this Transform transform, string tagName) {
        Func<Transform, bool> predicate = tag => tag.tag == tagName;
        return transform.GetComponentsInChildren<Transform>().Where((t) => t.tag == tagName).Select(t => t.position).ToArray();
    }

    public static Transform[] GetChildsTransformsWithTag(this Transform transform, string tagName) {
        Func<Transform, bool> predicate = tag => tag.tag == tagName;
        return transform.GetComponentsInChildren<Transform>().Where(predicate).ToArray();
    }

    public static Transform GetChildTransformsWithTag(this Transform transform, string tagName) {
        Func<Transform, bool> predicate = tag => tag.tag == tagName;
        return transform.GetComponentsInChildren<Transform>().Where(predicate).Single<Transform>();
    }

    //java language
    //public static Vector3[] GetChildsWithTag(this Transform transform, string tagName) {
    //    return transform.GetComponentsInChildren<Transform>()
    //        .filter(t->t.tag.equals(tagName)) 
    //        .map(t->t.transform).toList();  
    //}

}