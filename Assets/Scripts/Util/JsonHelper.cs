using System;
using UnityEngine;

public class JsonHelper {

    public static T[] FromJson<T>(string jsonArray) {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(jsonArray);
        return wrapper.items;
    }

    public static T[] FromJsonWrapped<T>(string jsonArray) {
        jsonArray = WrapArray(jsonArray);
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(jsonArray);
        return wrapper.items;
    }
    
    public static T FromJsonSeq<T>(string jsonArray) {
        jsonArray = WrapArray(jsonArray);
        return JsonUtility.FromJson<T>(jsonArray);
    }

    private static string WrapArray(string jsonArray) { return "{ \"items\": " + jsonArray + "}"; }

    private static string RemoveWrap(string jsonArray) {
        var length = jsonArray.Length;
        jsonArray = jsonArray.Substring(9, length - 6);
        return jsonArray;
    }

    public static string ToJsonWithoutWrapped<T>(T[] array) {
        Wrapper<T> wrapper = new Wrapper<T> {
            items = array
        };
        return RemoveWrap(JsonUtility.ToJson(wrapper));
    }

    public static string ToJson<T>(T[] array) {
        Wrapper<T> wrapper = new Wrapper<T> {
            items = array
        };
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint) {
        Wrapper<T> wrapper = new Wrapper<T> {
            items = array
        };
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T> {
        public T[] items;

    }
}