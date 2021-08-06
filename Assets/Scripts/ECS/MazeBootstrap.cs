using System;
using System.Collections;
using System.Collections.Generic;
using Maze.Data.Storages;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeBootstrap : MonoBehaviour {
    public static MazeBootstrap Instance;

    [SerializeField]
    private AbilityView abilityButs;

    public Grid grid;
    public Tilemap ground;

    [HideInInspector]
    public int dimX;

    [HideInInspector]
    public int dimY;

    [HideInInspector]
    public int size;

    [HideInInspector]
    public int2 bound;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        if (grid == null) {
            grid = GameObject.Find("Grid").GetComponent<Grid>();
        }

        if (ground == null) {
            ground = GameObject.Find("Ground").GetComponent<Tilemap>();
        }

        LoadMaze();
        LoadCharacters();
        LoadSpirits();
        InitAbilityButtons();
        InitCellPositions();
    }

    private void OnDestroy() {
        // if (GridUtils.Directions.IsCreated) GridUtils.Directions.Dispose();
    }

    private void InitCellPositions() {
        var updateCellIndexSys = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UpdateCellIndexSystem>();
        updateCellIndexSys.Update();
    }

    private void LoadSpirits() {
        StartCoroutine(GetComponent<SpiritSerializer>().LoadMetaSpirit(0));
    }

    private void LoadCharacters() {
        //        ShamanData data = new ShamanData();
        //        data = JsonUtility.FromJson<ShamanData>(json); 

        //var shaman = CharacterFactory.LoadShaman();
        //        var moveData = shaman.GetComponent<MoveSettingsComponent>();
        //       // moveData.speed = data.speed;
        //        var heading = shaman.GetComponent<HeadingComponent>();
        //        heading.direction = MoveDirection.UP;
        // Instantiate();

        //       GameObject goSham = GameObject.FindWithTag("Player");
        //       goSham.AddComponent<ConvertToEntity>();
    }

    private void LoadMaze() {
        //  MazeSerializer.JsonToMaze(0);
        //Parse maze, create cell buffer entity

        ground.CompressBounds();
        ground.ResizeBounds();
        var cellBounds = ground.cellBounds;
        dimX = cellBounds.size.x;
        dimY = cellBounds.size.y;
        size = dimX * dimY;
        InitializationMaze();
        MortonZCurveTest();
    }

    public void DestroyEntityWorld() {
        var destroyWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DestroyMazeWorldSystem>();
        destroyWorld.Update();
    }

    private void InitializationMaze() {
        var init = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<InitializationMazeGridSystem>();
        init.Update();
    }

    private void InitAbilityButtons() {
        abilityButs.Init();
    }

    public void Spawner(int count) {
        // var pathfs = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PathFindingSystem>();
        //pathfs.Enabled = false;
        var spawnPathFindSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SpawnPathFindSystem>();
        spawnPathFindSystem.count = count;
        spawnPathFindSystem.Update();
    }
    
    public void RemoveAllHauntingMobs() {
        var destroyHauntingSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DestroyHauntingSystem>();
        destroyHauntingSystem.Update();
    }

    private void MortonZCurveTest() {
        // test morton z curve
        int2 a = new int2(4, 4);
        int2 b = new int2(4, 5);
        int2 c = new int2(5, 4);
        int2 d = new int2(5, 5);

        var a1 = MortonZCurve.WorldPosToMortonIndex(a);
        var b1 = MortonZCurve.WorldPosToMortonIndex(b);
        var c1 = MortonZCurve.WorldPosToMortonIndex(c);
        var d1 = MortonZCurve.WorldPosToMortonIndex(d);
        // Debug.Log(a1 +" "+ b1 +" "+ c1 +" "+ d1);
    }
}