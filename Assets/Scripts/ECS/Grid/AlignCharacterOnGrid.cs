using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

[ExecuteAlways]
public class AlignCharacterOnGrid : MonoBehaviour {
    public GameObject container;
    [SerializeField] 
    public List<GameObject> gameObjects = new List<GameObject>();

    [SerializeField]
    public Grid grid;
    [SerializeField] 
    public Tilemap ground;
    [SerializeField] 
    public Tilemap wall;
    [SerializeField] 
    public Tilemap mask;

    public TileBase highlightTile;
    
    public static AlignCharacterOnGrid Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        
    }

    public void AlignObject() {
        gameObjects.Clear();
        container = GameObject.Find("AllGameObjects");
        var all = container.GetComponentsInChildren<Transform>();

        for (int i = 1; i < all.Length; i++) {
            gameObjects.Add(all[i].gameObject);
        }

        for (int i = 0; i < gameObjects.Count; i++) {
            Vector3    pos              = gameObjects[i].transform.position;
            Vector3Int posInCell        = ground.WorldToCell(pos);
            Vector3    centerPosToWorld = ground.GetCellCenterWorld(posInCell);
            gameObjects[i].transform.position = centerPosToWorld;
        }
    }
}