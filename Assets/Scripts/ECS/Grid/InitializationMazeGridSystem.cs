using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Tilemaps;

[UpdateInGroup(typeof(InitializationGroup))]
[UpdateBefore(typeof(UpdateCellIndexSystem))]
[DisableAutoCreation]
public class InitializationMazeGridSystem : SystemBase {
    protected override void OnUpdate() {
        int2 bound = new int2(0, 0);
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var grid = MazeBootstrap.Instance.grid;
        var ground = MazeBootstrap.Instance.ground;

        var layers = grid.GetComponentsInChildren<Tilemap>();

        foreach (var layer in layers) {
            layer.CompressBounds();
            layer.ResizeBounds();
        }

        var dimX = layers[0].cellBounds.size.x;
        var dimY = layers[0].cellBounds.size.y;

        var archetype =
            EntityManager.CreateArchetype(ComponentType.ReadOnly<GridBuffer>(), ComponentType.ReadOnly<GridTag>());
        var entity = em.CreateEntity(archetype);
        var mazeBuffer = em.GetBuffer<GridBuffer>(entity);

        int sizeMap = dimX * dimY;
        var tiles = new NativeArray<GridBuffer>(sizeMap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        int indexX = 0;
        int indexY = 0;

        float3 firstTile = new float3(0, 0, 0);

        //TODO now parse 2 layer(ground and wall), if add layer(example add trap or some ground type tile) need upgrade method
        foreach (var pos in ground.cellBounds.allPositionsWithin) {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            Vector3 worldPos = ground.GetCellCenterWorld(localPlace);

            CellType type = CellType.Empty;
            for (int i = 0; i < layers.Length; i++) {
                if (layers[i].name.Equals("Ground") || layers[i].name.Equals("Wall")) {
                    if (layers[i].HasTile(localPlace)) {
                        type = GridUtils.GetTileType(layers[i].name);
                    }
                }
            }

            int index = GridUtils.CoordToIndex(indexX, indexY, dimX);
            if (index == 0) {
                firstTile = worldPos;
            }

            int2 coord = GridUtils.WorldToCellCoord(firstTile, worldPos);
            // TODO grid bound
            bound.x = math.max(bound.x, coord.x);
            bound.y = math.max(bound.y, coord.y);
            indexX++;
            if (indexX == dimX) {
                indexX = 0;
                indexY++;
            }

            GridBuffer tile = new GridBuffer() {
                Index = index,
                Type = type,
                WorldPos = worldPos,
                coord = coord
            };
            tiles[index] = tile;
        }

        mazeBuffer.AddRange(tiles);
        tiles.Dispose();
        MazeBootstrap.Instance.bound = bound;
    }
}