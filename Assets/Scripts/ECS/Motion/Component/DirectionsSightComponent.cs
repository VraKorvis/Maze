using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct DirectionsSight : IComponentData {
    public float2 VectorDirection;
}

public class DirectionsSightComponent : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new DirectionsSight());
    }
}