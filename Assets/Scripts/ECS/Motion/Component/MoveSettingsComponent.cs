using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct MoveSettings : IComponentData {

    public float  speed;
    public float  defaultSpeed;
    public float  startDelay;
    public bool   isMoving;
    public bool   targetCellBlocked;
    public float3 targetCellPos;
}

[Serializable]
[RequiresEntityConversion]
public class MoveSettingsComponent : MonoBehaviour, IConvertGameObjectToEntity {

    public bool isMoving;

    [Range(0f, 1000f)]
    public float speed;

    [Tooltip("Delay before character will can start to move. Used after full stop")]
    [Range(0.1f, 0.9f)]
    public float startDelay = 0.2f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var ms = new MoveSettings() {
            speed      = speed,
            startDelay = startDelay,
            isMoving   = isMoving,
            defaultSpeed = speed
        };
        dstManager.AddComponentData(entity, ms);
    }

}