using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct BackForth : IComponentData {
    public float3 firstPoint;
    public float3 secondPoint;
    public float distanceBetween;
}

[Serializable]
[RequiresEntityConversion]
//[RequireComponent(typeof(MoveSettings), typeof(SpiritComponent))]
public class BackForthComponent : MonoBehaviour, ICheckCorrectnessPathMovement, IConvertGameObjectToEntity {
    [Tooltip("Count of cell to first point (Up/right direction)")]
    public int cellCountToFirst;

    [Tooltip("Count of cell to second point (Down/left direction)")]
    public int cellCountToSecond;

    [Tooltip("Type of orientation for moving")]
    private WayOrient orient;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var moveSettings = GetComponent<HeadingComponent>();
        if (moveSettings == null) {
            throw new Exception(
                $"No found 'MoveSettingsComponent'. Pls add 'MoveSettingsComponent' to {gameObject.name}");
        }

        var alignInstance = Singleton<AlignCharacterOnGrid>.Instance;
        var grid = alignInstance.grid;

        Vector3 currPos = transform.position;
        Vector3Int cellPos = grid.WorldToCell(currPos);

        SetOrientation();

        var offsetFirst = NextCellPos(orient) * cellCountToFirst;
        var offsetSecond = NextCellPos(orient) * cellCountToSecond;

        Vector3Int firstBoundCell = cellPos + offsetFirst;
        Vector3Int secondBoundCell = cellPos - offsetSecond;

        var data = new BackForth();
        var cellCenterWorld1S = grid.GetCellCenterWorld(firstBoundCell);
        var cellCenterWorld2D = grid.GetCellCenterWorld(secondBoundCell);
        if (moveSettings.direction == MoveDirection.DOWN || moveSettings.direction == MoveDirection.LEFT) {
            data.firstPoint = cellCenterWorld2D;
            data.secondPoint = cellCenterWorld1S;
        }
        else {
            data.firstPoint = cellCenterWorld1S;
            data.secondPoint = cellCenterWorld2D;
        }

        data.distanceBetween = math.length(data.firstPoint - data.secondPoint);

        dstManager.AddComponentData(entity, data);
        dstManager.AddComponentData(entity, new CellMovementTag());
        dstManager.AddComponentData(entity, new IndicesForwardCellTemp());
        dstManager.AddComponentData(entity, new PathRequestAgent());
        dstManager.AddComponentData(entity, new PathAgentStatus());
        dstManager.AddBuffer<Waypoint>(entity);
    }

    private void SetOrientation() {
        var moveSettings = GetComponent<HeadingComponent>();
        if (moveSettings == null) {
            throw new Exception(
                $"No found 'MoveSettingsComponent'. Pls add 'MoveSettingsComponent' to {gameObject.name}");
        }

        if (moveSettings.direction == MoveDirection.UP || moveSettings.direction == MoveDirection.DOWN) {
            orient = WayOrient.Vertical;
        }
        else {
            orient = WayOrient.Horizontal;
        }
    }

    public bool CheckCorrectnessPathMovement() {
        var alignInstance = Singleton<AlignCharacterOnGrid>.Instance;
        var grid = alignInstance.grid;
        Vector3Int tilePosForCheck = grid.WorldToCell(transform.position);
        SetOrientation();
        var offset = NextCellPos(orient);

        var red = new Color32(255, 0, 0, 70);
        var green = new Color32(0, 255, 0, 70);

        if (alignInstance.wall.HasTile(tilePosForCheck)) {
            alignInstance.mask.SetTile(tilePosForCheck, alignInstance.highlightTile);
            alignInstance.mask.SetTileFlags(tilePosForCheck, TileFlags.None);
            alignInstance.mask.SetColor(tilePosForCheck, red);
            return false;
        }

        for (int i = 0; i < 2; i++) {
            int len = cellCountToFirst;
            if (i == 1) {
                tilePosForCheck = grid.WorldToCell(transform.position);
                offset *= -1;
                len = cellCountToSecond;
            }

            for (int j = 0; j < len; j++) {
                tilePosForCheck = tilePosForCheck + offset;
                alignInstance.mask.SetTile(tilePosForCheck, alignInstance.highlightTile);
                alignInstance.mask.SetTileFlags(tilePosForCheck, TileFlags.None);
                alignInstance.mask.SetColor(tilePosForCheck, green);

                if (alignInstance.wall.HasTile(tilePosForCheck)) {
                    alignInstance.mask.SetColor(tilePosForCheck, red);
                    return false;
                }
            }
        }

        return true;
    }

    private Vector3Int NextCellPos(WayOrient orientation) {
        switch (orientation) {
            case WayOrient.Vertical:
                return new Vector3Int(0, 1, 0);
            case WayOrient.Horizontal:
                return new Vector3Int(1, 0, 0);
        }

        return new Vector3Int(0, 0, 0);
    }

    private void OnValidate() {
        GetComponent<SpiritComponent>().guise = Guise.BackForth;
    }
}