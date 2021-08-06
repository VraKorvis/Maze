using UnityEngine;
using System.Collections;

public class FileRWPlatformExtension  {

    public static string GetFriendlyFilesPath() {

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject applicationContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject path = applicationContext.Call<AndroidJavaObject>("getFilesDir");
        string filesPath = path.Call<string>("getCanonicalPath");
        return filesPath;
#endif
        return Application.persistentDataPath;
    }

    public static string GetFriendlyCachePath() {

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject applicationContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject path = applicationContext.Call<AndroidJavaObject>("getCacheDir");
        string filesPath = path.Call<string>("getCanonicalPath");
        return filesPath;
#endif
        return Application.temporaryCachePath;
    }
}
