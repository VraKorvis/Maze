using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
    
    public void LoadScene(int n) {
        MazeBootstrap.Instance?.DestroyEntityWorld();
        SceneManager.LoadScene(n);
    }
}