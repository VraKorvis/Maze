using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Heading : IComponentData {
    public int2 VectorDirection;
}

[Serializable]
[RequiresEntityConversion]
//[RequireComponent(typeof(MoveSettings))]
public class HeadingComponent : MonoBehaviour, IConvertGameObjectToEntity {
    [Tooltip("Direction of motion character")]
    public MoveDirection direction;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new Heading() {
            VectorDirection = GridUtils.GetDir(direction),
        });
    }
}